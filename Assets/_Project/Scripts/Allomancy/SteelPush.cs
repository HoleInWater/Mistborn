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
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SteelPush : MonoBehaviour
{
    // ── Helpers ───────────────────────────────────────────────────────────────


    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Push Settings")]
    public float minDistance       = 1f;
    public float maxRange          = 30f;
    public float metalCostPerSecond= 2f;
    public float pushCooldown      = 0.2f;

    [Header("Push Physics — PHYSICS-MATH-BOOK.md Section 2")]
    [Tooltip("Recoil speed when pushing anchored metal (launches player)")]
    public float pushSpeed = 60f;
    [Tooltip("Max recoil speed cap")]
    public float maxRecoilSpeed = 50f;
    [Tooltip("Speed applied to loose objects when pushed")]
    public float loosePushForce = 80f;
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
    public Camera     playerCamera;
    public LayerMask  metalLayer;
    public Allomancer allomancer;
    public Rigidbody  playerRigidbody;
    public Transform  chestTransform;

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
    public Color flaringColor        = new Color(1f, 0.2f, 0f, 0.3f);
    public float vignettePulseDuration = 0.5f;
    public float vignetteMaxAlpha    = 0.3f;

    [Header("UI")]
    public UnityEngine.UI.Image crosshairImage;
    public Color metalInRangeColor = Color.green;
    public Color noMetalColor      = Color.white;

    [Header("Push Prediction")]
    public bool  enablePushPrediction  = true;
    public Color predictionColor       = new Color(1f, 1f, 0f, 0.5f);
    public int   predictionPoints      = 20;
    public float predictionTimeStep    = 0.1f;
    public bool  showPredictionOnHold  = true;

    [Header("Steel Bubble")]
    public bool    enableSteelBubble             = true;
    // [AGENT REVIEW] Dynamically bound based on primary/secondary state
    public KeyCode steelBubbleKey => GetAbility2Key();

    private KeyCode GetAbility1Key()
    {
        // E always pushes — no metal selector dependency
        return KeyCode.E;
    }

    private KeyCode GetAbility2Key()
    {
        // G key for steel bubble (F is parry/block)
        if (allomancer == null) return KeyCode.G;
        var selector = allomancer.GetComponent<MetalSelector>();
        if (selector == null) return KeyCode.G;
        if (selector.GetPrimaryMetal() == AllomancySkill.MetalType.Steel) return KeyCode.G;
        if (selector.GetSecondaryMetal() == AllomancySkill.MetalType.Steel) return KeyCode.V;
        return KeyCode.None;
    }

    public float   steelBubbleRadius             = 2.5f;
    public float   steelBubbleForce              = 50f;
    public float   steelBubbleCooldown           = 0.5f;
    public float   steelBubbleMetalCostMultiplier= 1.5f;

    [Header("Impulse Mode")]
    public float impulseMassThreshold = 5f;
    public float impulseCalibration   = 0.000917f;

    [Header("Debug")]
    public bool debugPushOperations = false;
    public bool debugCalibration = false;

    // ── Private State ─────────────────────────────────────────────────────────
    // No burn state — push works on click without needing to "burn" first

    private bool IsFlaring =>
        FlareManager.Instance != null && FlareManager.Instance.IsFlaring;

    private float FlareLevel =>
        FlareManager.Instance != null
            ? (float)(FlareManager.Instance.Intensity - 1) / (FlareManager.Instance.maxIntensitySteps - 1)
            : 0f;

    private float CurrentFlareMultiplier =>
        FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

    private bool  eKeyWasPressed         = false;
    private bool  bubbleAppliedThisPress = false;
    private float cooldownTimer          = 0f;
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

        // E key: single click = one push impulse. No burn requirement.
        KeyCode pushKey = GetAbility1Key();
        if (pushKey != KeyCode.None && Input.GetKeyDown(pushKey) && cooldownTimer <= 0f)
        {
            if (hasCurrentTarget && allomancer != null && allomancer.GetMetalReserve(AllomancySkill.MetalType.Steel) > 0)
            {
                PushMetals();
                allomancer.DrainMetal(AllomancySkill.MetalType.Steel, metalCostPerSecond);
                cooldownTimer = pushCooldown;
                if (IsFlaring) StartFlaringVignette();
            }
        }

        // Steel Bubble: radial push (single click, not continuous)
        if (enableSteelBubble)
        {
            bool bubbleDown = steelBubbleKey != KeyCode.None && Input.GetKeyDown(steelBubbleKey);

            if (bubbleDown && steelBubbleCooldownTimer <= 0f
                && allomancer != null && allomancer.GetMetalReserve(AllomancySkill.MetalType.Steel) > 0)
            {
                PushMetalsInBubble();
                allomancer.DrainMetal(AllomancySkill.MetalType.Steel, metalCostPerSecond * steelBubbleMetalCostMultiplier);
                steelBubbleCooldownTimer = steelBubbleCooldown;
            }
        }

        UpdatePrediction();
        UpdateCrosshairColor();
    }

    // No burn state management — push works on click

    // ── Target Detection ──────────────────────────────────────────────────────

    private Renderer lastHighlightedRenderer;
    private Color originalColor;

    private float targetScanTimer = 0f;
    private const float TARGET_SCAN_INTERVAL = 0.1f; // Scan 10x/sec instead of every frame

    void UpdateTargetedMetal()
    {
        // Throttle expensive physics scan
        targetScanTimer -= Time.deltaTime;
        if (targetScanTimer > 0f) return;
        targetScanTimer = TARGET_SCAN_INTERVAL;

        hasCurrentTarget = false; currentTarget = null;
        currentTargetRigidbody = null; metalInRange = false;

        Collider[] hits = Physics.OverlapSphere(playerRigidbody.position, maxRange, metalLayer);
        if (hits.Length == 0) return;

        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null || rb == playerRigidbody) continue;

            float dist = Vector3.Distance(playerRigidbody.position, rb.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                currentTargetRigidbody = rb;
                currentTarget = hit.GetComponent<AllomanticTarget>();

                if (currentTarget == null || currentTarget.canBePushed)
                {
                    hasCurrentTarget = true;
                    metalInRange = true;
                }
            }
        }
    }

    // ── Push ──────────────────────────────────────────────────────────────────

    void PushMetals()
    {
        if (playerRigidbody == null || !hasCurrentTarget || currentTargetRigidbody == null) return;

        Rigidbody        targetRb  = currentTargetRigidbody;
        AllomanticTarget target    = currentTarget;

        if (targetRb == playerRigidbody)           return;
        if (target != null && !target.canBePushed) return;

        float distance = Vector3.Distance(playerRigidbody.position, targetRb.position);
        Vector3 pushDirection = (targetRb.position - playerRigidbody.position).normalized;
        bool isAnchored = (target != null && target.isAnchored) || targetRb.isKinematic;

        // Flare multiplier
        float flare = CurrentFlareMultiplier;

        // Distance scaling: stronger push at close range
        float distanceMult = inverseDistanceScaling
            ? Mathf.Clamp(maxRange / Mathf.Max(distance, minDistance), 0.5f, 3f)
            : 1f;

        if (isAnchored)
        {
            // ANCHORED: Player recoils backward (how Mistborn launch themselves)
            float recoilSpeed = pushSpeed * flare * distanceMult;
            Vector3 recoil = -pushDirection * Mathf.Min(recoilSpeed, maxRecoilSpeed);
            playerRigidbody.AddForce(recoil, ForceMode.VelocityChange);
        }
        else
        {
            // LOOSE OBJECT: Newton's 3rd Law — lighter party moves more
            float playerMass = playerRigidbody.mass;
            float targetMass = targetRb.mass;
            float totalMass = playerMass + targetMass;

            float pushMag = loosePushForce * flare * distanceMult;

            // Object pushed away
            float objectSpeed = pushMag * (playerMass / totalMass);
            objectSpeed = Mathf.Min(objectSpeed, loosePushForce * 3f);
            targetRb.AddForce(pushDirection * objectSpeed, ForceMode.VelocityChange);

            // Player recoils backward
            float playerSpeed = pushMag * (targetMass / totalMass);
            playerSpeed = Mathf.Min(playerSpeed, maxRecoilSpeed);
            playerRigidbody.AddForce(-pushDirection * playerSpeed, ForceMode.VelocityChange);
        }

        // Visual feedback
        CameraShakeManager.Instance?.Shake(shakeDuration, shakeMagnitude);
        SoundManager.Instance?.PlayPushSound();

    }

    void PushMetalsInBubble()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, steelBubbleRadius, metalLayer);
        foreach (var hitCollider in hitColliders)
        {
            Rigidbody rb = hitCollider.attachedRigidbody;
            if (rb != null && rb != playerRigidbody)
            {
                Vector3 dir = rb.position - transform.position;
                rb.AddForce(dir.normalized * steelBubbleForce * CurrentFlareMultiplier, ForceMode.Impulse);
            }
        }
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
        predictionLine.endWidth = 0.01f;
        predictionLine.material = new Material(Shader.Find("Sprites/Default"));
        predictionLine.startColor = Color.blue;
        predictionLine.endColor = new Color(0, 0, 1, 0.5f);
        predictionLine.positionCount = 0;
    }

    void UpdatePrediction()
    {
        if (predictionLine == null) return;

        // Show targeting line when there's a valid target (no burn required)
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
