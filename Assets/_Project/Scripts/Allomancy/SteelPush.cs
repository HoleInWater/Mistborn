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
    public float pushForce         = 800f;
    public float referenceMass     = 80f;
    public float referenceDistance = 3f;
    public float minDistance       = 1f;
    public float maxRange          = 30f;
    public float metalCostPerSecond= 2f;
    public float pushCooldown      = 0.2f;

    [Header("Allomancy Physics")]
    public float allomanticStrength = 1000f;
    public float maxCoinVelocity    = 400f;
    [Range(1f, 2f)] public float distanceExponent = 1f;
    [Range(0f, 1f)] public float velocityDamping  = 0.5f;

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
    public KeyCode steelBubbleKey                = KeyCode.F;
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
    private bool _isBurning = false;
    private bool isBurning 
    {
        get 
        {
            bool globalBurn = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Steel;
            return _isBurning || globalBurn;
        }
        set { _isBurning = value; }
    }

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
        if (allomancer != null && !allomancer.canBurnMetal)
        {
            if (isBurning) StopBurning();
            return;
        }

        if (cooldownTimer > 0f)            cooldownTimer            -= Time.deltaTime;
        if (steelBubbleCooldownTimer > 0f) steelBubbleCooldownTimer -= Time.deltaTime;

        UpdateTargetedMetal();

        bool mouseDown = Input.GetMouseButtonDown(1);
        bool mouseUp   = Input.GetMouseButtonUp(1);
        bool mouseHeld = Input.GetMouseButton(1);

        if (mouseDown && !eKeyWasPressed && cooldownTimer <= 0f)
        {
            eKeyWasPressed = true;
            if (!isBurning) StartBurning();
            PushMetals();
            DrainMetal(flaringMetalCostMultiplier);
        }

        if (mouseUp)
        {
            eKeyWasPressed = false;
            StopBurning();
        }

        if (mouseHeld && isBurning)
        {
            PushMetals();
            DrainMetal(1f);
        }

        if (enableSteelBubble)
        {
            if (Input.GetKeyDown(steelBubbleKey) && IsFlaring
                && steelBubbleCooldownTimer <= 0f && !bubbleAppliedThisPress)
            {
                if (!isBurning) StartBurning();
                PushMetalsInBubble();
                DrainMetal(steelBubbleMetalCostMultiplier);
                steelBubbleCooldownTimer = steelBubbleCooldown;
                bubbleAppliedThisPress   = true;
            }
            if (Input.GetKeyUp(steelBubbleKey))
                bubbleAppliedThisPress = false;
        }

        UpdatePrediction();
        UpdateCrosshairColor();
    }

    // ── Burning ───────────────────────────────────────────────────────────────

    void StartBurning()
    {
        if (isBurning) return;
        isBurning = true;
        allomancer?.StartBurning(AllomancySkill.MetalType.Steel);
    }

    void StopBurning()
    {
        if (!isBurning) return;
        isBurning     = false;
        cooldownTimer = pushCooldown;
        allomancer?.StopBurning();
    }

    // ── Target Detection ──────────────────────────────────────────────────────

    void UpdateTargetedMetal()
    {
        hasCurrentTarget = false; currentTarget = null;
        currentTargetRigidbody = null; metalInRange = false;

        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out currentTargetHit, maxRange, metalLayer))
        {
            Rigidbody rb = currentTargetHit.rigidbody;
            if (rb != null && rb != playerRigidbody)
            {
                currentTargetRigidbody = rb;
                currentTarget          = currentTargetHit.collider.GetComponent<AllomanticTarget>();
                hasCurrentTarget       = true;
                metalInRange           = true;
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

        float   distance   = Vector3.Distance(playerRigidbody.position, targetRb.position);
        Vector3 dir        = targetRb.position - playerRigidbody.position;
        bool    isAnchored = (target != null && target.isAnchored) || targetRb.isKinematic;

        // Scale strength by shared intensity multiplier
        float strength = allomanticStrength
                       * (playerRigidbody.mass / referenceMass)
                       * masteryBonus
                       * CurrentFlareMultiplier;

        float eff           = Mathf.Max(distance, minDistance);
        float forceMag      = strength / Mathf.Pow(eff, distanceExponent);
        Vector3 forceVector = dir.normalized * forceMag;

        // Clamp impulse to prevent physics tunneling on very small objects (e.g. coins) 
        // during Duralumin-boosted pushes.
        float maxAllowedImpulse = targetRb.mass * maxCoinVelocity;
        if (forceVector.magnitude > maxAllowedImpulse)
        {
            forceVector = forceVector.normalized * maxAllowedImpulse;
        }

        // Apply force
        if (!isAnchored)
        {
            targetRb.AddForce(forceVector, ForceMode.Impulse);
        }

        // Newton's Third Law (Recoil)
        playerRigidbody.AddForce(-forceVector, ForceMode.Impulse);

        if (debugPushOperations)
        {
            Debug.Log($"[STEEL] Push on {targetRb.name}. Distance: {distance:F2}m. Force: {forceMag:F2}");
        }

        if (forceMag > shakeForceThreshold)
        {
            CameraShakeManager.Instance?.Shake(shakeDuration, shakeMagnitude);
        }
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
        if (!enablePushPrediction) return;
        predictionLine = gameObject.AddComponent<LineRenderer>();
        predictionLine.startWidth = 0.05f;
        predictionLine.endWidth = 0.01f;
        predictionLine.material = new Material(Shader.Find("Sprites/Default"));
        predictionLine.startColor = predictionColor;
        predictionLine.endColor = new Color(predictionColor.r, predictionColor.g, predictionColor.b, 0);
        predictionLine.positionCount = 0;
    }

    void UpdatePrediction()
    {
        if (!enablePushPrediction || predictionLine == null) return;

        if (hasCurrentTarget && (showPredictionOnHold ? Input.GetKey(KeyCode.E) : true))
        {
            predictionLine.positionCount = 2;
            predictionLine.SetPosition(0, chestTransform.position);
            predictionLine.SetPosition(1, currentTargetHit.point);
        }
        else
        {
            predictionLine.positionCount = 0;
        }
    }
}
