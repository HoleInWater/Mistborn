/* SteelPush.cs
 *
 * PURPOSE:
 * Implements the Steel Allomancy ability (Coinshot) – push metal objects away.
 *
 * FLARE INTEGRATION:
 * ==================
 * Uses the single shared FlareManager intensity (1–10, scroll wheel).
 * Force scales smoothly with FlareManager.FlareMultiplier.
 *
 * CONTROLS:
 * - Left Ctrl    → Toggle burning ON / OFF (via FlareManager)
 * - Scroll wheel → Adjust shared intensity 1–10 (via FlareManager)
 * - E key        → Execute push (requires burning to be active)
 * - F key        → Steel bubble radial push (requires burning)
 * - E release    → Stop burning Steel locally
 *
 * BURN REQUIREMENT:
 * FlareManager.Instance.IsBurning must be true (Left Ctrl toggled on) before
 * E or F will do anything. Same gate used by IronPull.
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[PlayerComponent("Allomancy Metals", order: 10)]
public class SteelPush : MonoBehaviour
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private float CurrentFlareMultiplier =>
        FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

    /// <summary>True only when the player has Steel burning toggled on.</summary>
    private bool IsBurning =>
        FlareManager.Instance != null && FlareManager.Instance.IsBurning;

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Push Settings")]
    public float minDistance        = 1f;
    public float maxRange           = 30f;
    public float metalCostPerSecond = 0.5f;  // MAG: 20 min/charge; ~0.5 per push at ~1 push/6s
    public float pushCooldown       = 0.2f;

    [Header("Push Physics — PHYSICS-MATH-BOOK.md Section 2")]
    [Tooltip("Recoil speed when pushing anchored metal (launches player)")]
    public float pushSpeed = 25f;
    [Tooltip("Max recoil speed cap")]
    public float maxRecoilSpeed = 20f;
    [Tooltip("Speed applied to loose objects when pushed")]
    public float loosePushForce = 35f;
    [Tooltip("Stronger push at close range")]
    public bool inverseDistanceScaling = true;

    [Header("Flare Scaling")]
    [Tooltip("Metal cost multiplier at full intensity.")]
    [Range(1f, 5f)]
    public float flaringMetalCostMultiplier = 3f;

    [Tooltip("Skill mastery bonus.")]
    [Range(1f, 2f)]
    public float masteryBonus = 1f;

    [Header("References")]
    public Camera         playerCamera;
    public LayerMask      metalLayer;
    public Allomancer     allomancer;
    public Rigidbody      playerRigidbody;
    public Transform      chestTransform;
    public MetalSelector  metalSelector;

    [Header("Visual Effects")]
    public float shakeMagnitude      = 0.1f;
    public float shakeDuration       = 0.1f;
    public float shakeForceThreshold = 100f;
    public bool  enablePushScreenTint = true;
    public Color weakPushTint   = new Color(0f, 1f, 0f, 0.1f);
    public Color mediumPushTint = new Color(1f, 1f, 0f, 0.2f);
    public Color strongPushTint = new Color(1f, 0f, 0f, 0.3f);
    public float pushTintDuration = 0.2f;

    [Header("Flaring Vignette")]
    public UnityEngine.UI.Image vignetteImage;
    public Color flaringColor         = new Color(1f, 0.2f, 0f, 0.3f);
    public float vignettePulseDuration = 0.5f;
    public float vignetteMaxAlpha     = 0.3f;

    [Header("UI")]
    public UnityEngine.UI.Image crosshairImage;
    public Color metalInRangeColor = Color.green;
    public Color noMetalColor      = Color.white;

    [Header("Push Prediction")]
    public bool  enablePushPrediction = true;
    public Color predictionColor      = new Color(1f, 1f, 0f, 0.5f);
    public int   predictionPoints     = 20;
    public float predictionTimeStep   = 0.1f;
    public bool  showPredictionOnHold = true;

    [Header("Steel Bubble")]
    public bool enableSteelBubble = true;
    // [AGENT REVIEW] Dynamically bound based on primary/secondary state
    public KeyCode steelBubbleKey => GetAbility2Key();

    private KeyCode GetAbility1Key()
    {
        if (metalSelector != null)
        {
            if (metalSelector.GetPrimaryMetal() == AllomancySkill.MetalType.Steel)
                return Keybinds.Ability1;   // E = primary slot
            if (metalSelector.GetSecondaryMetal() == AllomancySkill.MetalType.Steel)
                return Keybinds.Ability2;   // Q = secondary slot
            return KeyCode.None;             // Steel not equipped in either slot
        }
        return Keybinds.SteelPush;           // fallback if no selector
    }
    private KeyCode GetAbility2Key()
    {
        if (metalSelector != null)
        {
            if (metalSelector.GetPrimaryMetal()   == AllomancySkill.MetalType.Steel) return Keybinds.Ability3;  // F
            if (metalSelector.GetSecondaryMetal() == AllomancySkill.MetalType.Steel) return Keybinds.Ability4;  // V
            return KeyCode.None;
        }
        return Keybinds.Ability3;  // no selector — default F
    }

    public float steelBubbleRadius              = 2.5f;
    public float steelBubbleForce               = 50f;
    public float steelBubbleCooldown            = 0.5f;
    public float steelBubbleMetalCostMultiplier = 1.5f;

    [Header("Impulse Mode")]
    public float impulseMassThreshold = 5f;
    public float impulseCalibration   = 0.000917f;

    [Header("Debug")]
    public bool debugPushOperations = false;
    public bool debugCalibration    = false;

    // ── Private State ─────────────────────────────────────────────────────────

    private float cooldownTimer            = 0f;
    private float steelBubbleCooldownTimer = 0f;

    private RaycastHit       currentTargetHit;
    private AllomanticTarget currentTarget;
    private Rigidbody        currentTargetRigidbody;
    private bool             hasCurrentTarget = false;
    private bool             metalInRange     = false;

    private Coroutine    vignetteCoroutine;
    private Coroutine    pushTintCoroutine;
    private Color        currentPushTint = Color.clear;
    private LineRenderer predictionLine;
    private bool         isPredictionActive = false;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    void Start()
    {
        if (playerRigidbody == null) playerRigidbody = GetComponentInParent<Rigidbody>();
        if (playerCamera    == null) playerCamera    = Camera.main;
        if (allomancer      == null) allomancer      = GetComponentInParent<Allomancer>();

        if (metalSelector == null) metalSelector = GetComponentInParent<MetalSelector>();

        if (chestTransform == null)
        {
            Transform p = GetComponentInParent<Transform>();
            if (p != null)
            {
                Transform chest = p.Find("Chest") ?? p.Find("ChestBone")
                               ?? p.Find("Spine2") ?? p.Find("Torso");
                chestTransform = chest != null ? chest : p;
            }
        }

        CreatePredictionLine();
    }

    void Update()
    {
        if (cooldownTimer > 0f)            cooldownTimer            -= Time.deltaTime;
        if (steelBubbleCooldownTimer > 0f) steelBubbleCooldownTimer -= Time.deltaTime;

        UpdateTargetedMetal();

        // ── E key: single-click push impulse ──────────────────────────────────
        // Requires:
        //   1. FlareManager.IsBurning — player must have Left Ctrl toggled on
        //   2. Metal reserve > 0
        //   3. A valid metal target in range
        //   4. Cooldown elapsed
        KeyCode pushKey = GetAbility1Key();
        if (pushKey != KeyCode.None && Input.GetKeyDown(pushKey) && cooldownTimer <= 0f)
        {
            if (!IsBurning)
            {
                // Silently block — burning is off.
            }
            else if (allomancer == null || allomancer.GetMetalReserve(AllomancySkill.MetalType.Steel) <= 0)
            {
                // Steel reserve empty.
            }
            else if (hasCurrentTarget)
            {
                PushMetals();
                allomancer.DrainMetal(AllomancySkill.MetalType.Steel, metalCostPerSecond);
                cooldownTimer = pushCooldown;
                if (IsBurning) StartFlaringVignette();
            }
        }

        // ── Steel Bubble: radial push (F = primary, V = secondary, requires burn session) ──
        KeyCode bubbleKey = steelBubbleKey;
        bool bubblePressed = bubbleKey != KeyCode.None && Input.GetKeyDown(bubbleKey);

        if (enableSteelBubble && bubblePressed && IsBurning && steelBubbleCooldownTimer <= 0f)
        {
            if (allomancer != null && allomancer.GetMetalReserve(AllomancySkill.MetalType.Steel) > 0)
            {
                PushMetalsInBubble();
                // Drain scales with flare — higher intensity = more metal consumed
                allomancer.DrainMetal(AllomancySkill.MetalType.Steel,
                    metalCostPerSecond * steelBubbleMetalCostMultiplier * CurrentFlareMultiplier);
                steelBubbleCooldownTimer = steelBubbleCooldown;
            }
        }

        UpdatePrediction();
        UpdateCrosshairColor();
    }

    // ── Target Detection ──────────────────────────────────────────────────────

    private Renderer lastHighlightedRenderer;
    private Color    originalColor;

    private float targetScanTimer = 0f;
    private const float TARGET_SCAN_INTERVAL = 0.1f;

    void UpdateTargetedMetal()
    {
        targetScanTimer -= Time.deltaTime;
        if (targetScanTimer > 0f) return;
        targetScanTimer = TARGET_SCAN_INTERVAL;

        hasCurrentTarget       = false;
        currentTarget          = null;
        currentTargetRigidbody = null;
        metalInRange           = false;

        if (playerRigidbody == null) return;
        LayerMask targetLayer = metalLayer != 0 ? metalLayer : LayerMask.GetMask("Metal");
        Collider[] hits = Physics.OverlapSphere(playerRigidbody.position, maxRange, targetLayer);
        if (hits.Length == 0) return;

        Vector3 camForward = playerCamera != null ? playerCamera.transform.forward : transform.forward;
        float bestScore = float.MinValue;

        foreach (var hit in hits)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null || rb == playerRigidbody) continue;

            AllomanticTarget at = hit.GetComponentInParent<AllomanticTarget>();
            if (at != null && !at.canBePushed) continue;

            Vector3 toTarget = rb.position - playerRigidbody.position;
            float dist = toTarget.magnitude;
            if (dist < minDistance) continue;

            float alignment = Vector3.Dot(camForward, toTarget.normalized);
            float score     = alignment - (dist / maxRange);

            if (score > bestScore)
            {
                bestScore              = score;
                currentTargetRigidbody = rb;
                currentTarget          = at;
                hasCurrentTarget       = true;
                metalInRange           = true;
            }
        }
    }

    // ── Push ──────────────────────────────────────────────────────────────────

    void PushMetals()
    {
        if (playerRigidbody == null || !hasCurrentTarget || currentTargetRigidbody == null) return;

        Rigidbody        targetRb = currentTargetRigidbody;
        AllomanticTarget target   = currentTarget;

        if (targetRb == playerRigidbody)           return;
        if (target != null && !target.canBePushed) return;

        float   distance      = Vector3.Distance(playerRigidbody.position, targetRb.position);
        Vector3 pushDirection = (targetRb.position - playerRigidbody.position).normalized;
        bool    isAnchored    = (target != null && target.isAnchored) || targetRb.isKinematic;

        float flare        = CurrentFlareMultiplier;
        float distanceMult = inverseDistanceScaling
            ? Mathf.Clamp(maxRange / Mathf.Max(distance, minDistance), 0.5f, 3f)
            : 1f;

        if (isAnchored)
        {
            float   recoilSpeed = pushSpeed * flare * distanceMult;
            Vector3 recoil      = -pushDirection * Mathf.Min(recoilSpeed, maxRecoilSpeed * flare);
            playerRigidbody.AddForce(recoil, ForceMode.VelocityChange);
        }
        else
        {
            float playerMass = playerRigidbody.mass;
            float targetMass = targetRb.mass;
            float totalMass  = playerMass + targetMass;
            float pushMag    = loosePushForce * flare * distanceMult;

            float objectSpeed = Mathf.Min(pushMag * (playerMass / totalMass), loosePushForce * 3f);
            targetRb.AddForce(pushDirection * objectSpeed, ForceMode.VelocityChange);

            float playerSpeed = Mathf.Min(pushMag * (targetMass / totalMass), maxRecoilSpeed);
            playerRigidbody.AddForce(-pushDirection * playerSpeed, ForceMode.VelocityChange);
        }

        CameraShakeManager.Instance?.Shake(shakeDuration, shakeMagnitude);
        SoundManager.Instance?.PlayPushSound();

        Vector3 chestPos = chestTransform != null ? chestTransform.position : transform.position;
        PushPullTrail.Instance?.ShowPushTrail(chestPos, targetRb.position);
    }

    void PushMetalsInBubble()
    {
        float flareMult = CurrentFlareMultiplier;

        // Radius and force both scale with flare intensity
        float radius = steelBubbleRadius * flareMult;
        LayerMask targetLayer = metalLayer != 0 ? metalLayer : LayerMask.GetMask("Metal");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, targetLayer);
        foreach (var hitCollider in hitColliders)
        {
            Rigidbody rb = hitCollider.attachedRigidbody;
            if (rb != null && rb != playerRigidbody)
            {
                Vector3 dir = rb.position - transform.position;
                rb.AddForce(dir.normalized * steelBubbleForce * flareMult, ForceMode.Impulse);
            }
        }

        CameraShakeManager.Instance?.Shake(shakeDuration * flareMult, shakeMagnitude * flareMult);
    }

    void DrainMetal(float multiplier)
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(AllomancySkill.MetalType.Steel, metalCostPerSecond * multiplier * Time.deltaTime);
    }

    // ── Visuals ───────────────────────────────────────────────────────────────

    void StartFlaringVignette()
    {
        if (vignetteCoroutine != null) StopCoroutine(vignetteCoroutine);
        vignetteCoroutine = StartCoroutine(PulseVignette());
    }

    IEnumerator PulseVignette()
    {
        if (vignetteImage == null) yield break;
        vignetteImage.color = flaringColor;
        float elapsed = 0f;
        while (elapsed < vignettePulseDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(vignetteMaxAlpha, 0f, elapsed / vignettePulseDuration);
            vignetteImage.color = new Color(flaringColor.r, flaringColor.g, flaringColor.b, alpha);
            yield return null;
        }
    }

    void UpdateCrosshairColor()
    {
        if (crosshairImage == null) return;
        crosshairImage.color = metalInRange ? metalInRangeColor : noMetalColor;
    }

    void CreatePredictionLine()
    {
        predictionLine = gameObject.AddComponent<LineRenderer>();
        predictionLine.startWidth = 0.05f;
        predictionLine.endWidth   = 0.01f;
        predictionLine.material   = new Material(Shader.Find("Sprites/Default"));
        predictionLine.startColor = Color.blue;
        predictionLine.endColor   = new Color(0, 0, 1, 0.5f);
        predictionLine.positionCount = 0;
    }

    void UpdatePrediction()
    {
        if (predictionLine == null) return;

        if (hasCurrentTarget && currentTargetRigidbody != null)
        {
            predictionLine.positionCount = 2;
            predictionLine.SetPosition(0, chestTransform != null ? chestTransform.position : transform.position);
            predictionLine.SetPosition(1, currentTargetRigidbody.position);
        }
        else
        {
            predictionLine.positionCount = 0;
        }
    }
}
