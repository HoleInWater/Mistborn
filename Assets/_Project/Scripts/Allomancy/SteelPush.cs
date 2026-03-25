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
    // [AGENT REVIEW] Dynamically bound based on primary/secondary state
    public KeyCode steelBubbleKey => GetAbility2Key();

    private KeyCode GetAbility1Key()
    {
        if (allomancer == null) return KeyCode.E; 
        var selector = allomancer.GetComponent<MetalSelector>();
        if (selector == null) return KeyCode.E;
        if (selector.GetPrimaryMetal() == AllomancySkill.MetalType.Steel) return KeyCode.E;
        if (selector.GetSecondaryMetal() == AllomancySkill.MetalType.Steel) return KeyCode.Q;
        return KeyCode.None;
    }

    private KeyCode GetAbility2Key()
    {
        if (allomancer == null) return KeyCode.F; 
        var selector = allomancer.GetComponent<MetalSelector>();
        if (selector == null) return KeyCode.F;
        if (selector.GetPrimaryMetal() == AllomancySkill.MetalType.Steel) return KeyCode.F;
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

        KeyCode pushKey = GetAbility1Key();
        bool eDown = pushKey != KeyCode.None && Input.GetKeyDown(pushKey);
        bool eHeld = pushKey != KeyCode.None && Input.GetKey(pushKey);
        
        // Safety Catch: If unequipped mid-use, definitively trigger the Up state to halt physics
        bool eUp   = (pushKey != KeyCode.None && Input.GetKeyUp(pushKey)) || (eKeyWasPressed && pushKey == KeyCode.None);

        if (eDown && !eKeyWasPressed && cooldownTimer <= 0f)
        {
            Debug.LogError($"[STEEL PUSH] Clicked {pushKey}! Target Valid? {hasCurrentTarget}. Calling PushMetals.");
            eKeyWasPressed = true;
            AutoSwitchToThisMetal();
            if (!isBurning) StartBurning();
            PushMetals(); 
            if (IsFlaring) StartFlaringVignette();
        }

        if (eUp)
        {
            eKeyWasPressed = false;
            StopBurning();
        }

        if (isBurning) DrainMetal(1f);

        if (enableSteelBubble)
        {
            bool bubbleDown = steelBubbleKey != KeyCode.None && Input.GetKeyDown(steelBubbleKey);

            if (bubbleDown && steelBubbleCooldownTimer <= 0f)
            {
                AutoSwitchToThisMetal();
                if (!isBurning) StartBurning();
                PushMetalsInBubble();
                steelBubbleCooldownTimer = steelBubbleCooldown;
            }
        }

        UpdatePrediction();
        UpdateCrosshairColor();
    }

    private void AutoSwitchToThisMetal()
    {
        if (allomancer == null) return;
        var selector = allomancer.GetComponent<MetalSelector>();
        if (selector == null) return;
        
        if (selector.GetPrimaryMetal() == AllomancySkill.MetalType.Steel) selector.SetPrimaryActive(true);
        else if (selector.GetSecondaryMetal() == AllomancySkill.MetalType.Steel) selector.SetPrimaryActive(false);
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

    private Renderer lastHighlightedRenderer;
    private Color originalColor;

    void UpdateTargetedMetal()
    {
        hasCurrentTarget = false; currentTarget = null;
        currentTargetRigidbody = null; metalInRange = false;

        // Native physics layer scanner fully bypassing MistbornRegistry faults
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
                
                // Extra safety: only lock if pushable or lacks component entirely
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

        float   distance   = Vector3.Distance(playerRigidbody.position, targetRb.position);
        Vector3 dir        = targetRb.position - playerRigidbody.position;
        bool    isAnchored = (target != null && target.isAnchored) || targetRb.isKinematic;

        // ── Lore-Accurate Force: F(a) = A × m1 × m2 / r² ────────
        // From PHYSICS-MATH-BOOK.md Section 2: Steel & Iron Push/Pull
        // A = Allomantic strength constant (scaled by flaring)
        // m1 = Allomancer mass, m2 = target metal mass, r = distance
        float playerMass = playerRigidbody.mass;
        float targetMass = targetRb.mass;
        float eff = Mathf.Max(distance, minDistance);

        float A = allomanticStrength * masteryBonus * CurrentFlareMultiplier;
        float forceMag = A * playerMass * targetMass / (eff * eff);

        // Newton's 3rd Law mass ratios — whoever is lighter moves more
        float totalMass = isAnchored ? playerMass : playerMass + targetMass;
        float playerRatio = isAnchored ? 1f : targetMass / totalMass;
        float objectRatio = isAnchored ? 0f : playerMass / totalMass;

        Vector3 forceDir = dir.normalized * forceMag;

        // Clamp to prevent physics tunneling
        float maxForce = targetMass * maxCoinVelocity;
        if (forceDir.magnitude > maxForce)
            forceDir = forceDir.normalized * maxForce;

        // Apply push: object away, player recoils back
        if (!isAnchored)
            targetRb.AddForce(forceDir * objectRatio, ForceMode.Impulse);

        playerRigidbody.AddForce(-forceDir * playerRatio, ForceMode.Impulse);

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

        // FIXED NullReferenceException: Query the base GameObject directly since currentTarget (the script component) can be null!
        Renderer targetRenderer = (hasCurrentTarget && currentTargetRigidbody != null) 
            ? currentTargetRigidbody.gameObject.GetComponentInChildren<Renderer>() 
            : null;
        
        // Reset previous highlight if target changed or we stopped burning
        if (lastHighlightedRenderer != null && (lastHighlightedRenderer != targetRenderer || !isBurning))
        {
            if (lastHighlightedRenderer.material != null)
                lastHighlightedRenderer.material.color = originalColor;
            lastHighlightedRenderer = null;
        }

        if (hasCurrentTarget && isBurning && currentTargetRigidbody != null)
        {
            predictionLine.positionCount = 2;
            predictionLine.SetPosition(0, chestTransform.position);
            predictionLine.SetPosition(1, currentTargetRigidbody.position);
            
            // Turn the mesh strictly blue!
            if (targetRenderer != null && lastHighlightedRenderer != targetRenderer && targetRenderer.material != null)
            {
                lastHighlightedRenderer = targetRenderer;
                originalColor = targetRenderer.material.color;
                targetRenderer.material.color = Color.blue;
            }
        }
        else
        {
            predictionLine.positionCount = 0;
        }
    }
}
