/* SteelPush.cs
 *
 * PURPOSE:
 * Implements the Steel Allomancy ability (Coinshot) – push metal objects away.
 *
 * FLARE INTEGRATION (scroll wheel):
 * ==================================
 * Force is now scaled continuously by FlareManager.Instance.FlareMultiplier:
 *   - Intensity 0  (no flare)  → 1.0× force, 1.0× metal cost
 *   - Intensity 5  (mid flare) → ~1.75× force, proportional drain
 *   - Intensity 10 (max flare) → maxFlareMultiplier× force, max drain
 *
 * The binary IsFlaring / IsSteelFlaring check is kept for effects (vignette, shake)
 * but force and cost now use the smooth FlareMultiplier value.
 *
 * CONTROLS (unchanged):
 * - E key held    → Push targeted metal object
 * - F key         → Steel bubble (radial push)
 * - Scroll wheel  → Adjust flare intensity (via FlareManager)
 * - Left Ctrl     → Toggle max/off flare (via FlareManager)
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SteelPush : MonoBehaviour
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Smooth 0–1 flare intensity from scroll wheel (0 = no flare, 1 = max).</summary>
    private float FlareLevel =>
        FlareManager.Instance != null
            ? (float)FlareManager.Instance.FlareIntensity / FlareManager.Instance.maxIntensitySteps
            : 0f;

    /// <summary>Force multiplier derived from current flare intensity.</summary>
    private float CurrentFlareMultiplier =>
        FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

    /// <summary>True when any flare is active (for effects / vignette).</summary>
    private bool IsFlaring =>
        FlareManager.Instance != null && FlareManager.Instance.IsSteelFlaring;

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Push Settings")]
    [Tooltip("Base force applied when pushing.")]
    public float pushForce = 800f;
    public float referenceMass = 80f;
    public float referenceDistance = 3f;
    public float minDistance = 1f;
    public float maxRange = 30f;
    public float metalCostPerSecond = 2f;
    public float pushCooldown = 0.2f;

    [Header("Allomancy Physics")]
    public float allomanticStrength = 1000f;
    public float maxCoinVelocity = 400f;
    [Range(1f, 2f)] public float distanceExponent = 1f;
    [Range(0f, 1f)] public float velocityDamping = 0.5f;

    [Header("Flare Scaling")]
    [Tooltip("Maximum force multiplier at full flare (intensity 10). " +
             "Matches FlareManager.maxFlareMultiplier if left in sync.")]
    [Range(1.5f, 3f)]
    public float maxFlareMultiplier = 2f;

    [Tooltip("Metal cost multiplier at full flare intensity.")]
    [Range(1f, 5f)]
    public float flaringMetalCostMultiplier = 3f;

    [Tooltip("Skill mastery bonus (flat multiplier on top of everything).")]
    [Range(1f, 2f)]
    public float masteryBonus = 1f;

    [Header("References")]
    public Camera playerCamera;
    public LayerMask metalLayer;
    public Allomancer allomancer;
    public Rigidbody playerRigidbody;
    public Transform chestTransform;

    [Header("Visual Effects")]
    public GameObject pushEffectPrefab;
    public float shakeMagnitude = 0.1f;
    public float shakeDuration  = 0.1f;
    public float shakeForceThreshold = 100f;

    [Header("Focused Push")]
    public KeyCode focusKey = KeyCode.LeftControl;
    public Color focusedPushColor = Color.red;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pushSound;
    public float soundVolume = 0.5f;

    [Header("Flaring Visual Effect")]
    public UnityEngine.UI.Image vignetteImage;
    public Color flaringColor = new Color(1f, 0.2f, 0f, 0.3f);
    public float vignettePulseDuration = 0.5f;
    public float vignetteMaxAlpha = 0.3f;

    [Header("UI Feedback")]
    public UnityEngine.UI.Image crosshairImage;
    public Color metalInRangeColor = Color.green;
    public Color noMetalColor = Color.white;

    [Header("Push Prediction")]
    public bool enablePushPrediction = true;
    public Color predictionColor = new Color(1f, 1f, 0f, 0.5f);
    public int predictionPoints = 20;
    public float predictionTimeStep = 0.1f;
    public bool showPredictionOnHold = true;

    [Header("Push Force Visual Feedback")]
    public bool enablePushScreenTint = true;
    public Color weakPushTint   = new Color(0f, 1f, 0f, 0.1f);
    public Color mediumPushTint = new Color(1f, 1f, 0f, 0.2f);
    public Color strongPushTint = new Color(1f, 0f, 0f, 0.3f);
    public float pushTintDuration = 0.2f;

    [Header("Steel Bubble (Defensive)")]
    public bool enableSteelBubble = true;
    public KeyCode steelBubbleKey = KeyCode.F;
    public float steelBubbleRadius = 2.5f;
    public float steelBubbleForce  = 50f;
    public float steelBubbleCooldown = 0.5f;
    public float steelBubbleMetalCostMultiplier = 1.5f;

    [Header("Flight Mechanics")]
    public float flightLaunchMultiplier = 1.5f;
    public float flightAngleThreshold  = 45f;

    [Header("Impulse Mode")]
    public float impulseMassThreshold = 5f;
    public float impulseCalibration   = 0.000917f;
    public bool debugCalibration      = false;
    public bool debugPushOperations   = true;

    // ── Private State ─────────────────────────────────────────────────────────
    private bool isBurning = false;
    private bool pushAppliedThisPress  = false;
    private bool bubbleAppliedThisPress= false;
    private bool eKeyWasPressed        = false;
    private Coroutine vignetteCoroutine;
    private bool metalInRange  = false;
    private float cooldownTimer          = 0f;
    private float steelBubbleCooldownTimer = 0f;
    private bool isSteelBubbleActive   = false;

    private RaycastHit currentTargetHit;
    private AllomanticTarget currentTarget;
    private Rigidbody currentTargetRigidbody;
    private bool hasCurrentTarget = false;

    private LineRenderer predictionLine;
    private bool isPredictionActive = false;

    private Coroutine pushTintCoroutine;
    private Color currentPushTint = Color.clear;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    void Start()
    {
        if (playerRigidbody == null) playerRigidbody = GetComponentInParent<Rigidbody>();
        if (playerCamera    == null) playerCamera    = Camera.main;
        if (allomancer      == null) allomancer      = GetComponentInParent<Allomancer>();

        if (chestTransform == null)
        {
            Transform player = GetComponentInParent<Transform>();
            if (player != null)
            {
                Transform chest = player.Find("Chest")
                               ?? player.Find("ChestBone")
                               ?? player.Find("Spine2")
                               ?? player.Find("Torso");
                chestTransform = chest != null ? chest : player;
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

        bool eKeyDown = Input.GetKeyDown(KeyCode.E);
        bool eKeyUp   = Input.GetKeyUp(KeyCode.E);

        // E KEY DOWN: push requires flaring to be active
        if (eKeyDown && !eKeyWasPressed && cooldownTimer <= 0f)
        {
            if (IsFlaring)
            {
                eKeyWasPressed = true;
                if (!isBurning) StartBurning();
                PushMetals();
                DrainMetal(flaringMetalCostMultiplier);
                StartFlaringVignette();
            }
        }

        if (eKeyUp)
        {
            eKeyWasPressed = false;
            StopBurning();
        }

        // Continuous drain while burning
        if (isBurning)
            DrainMetal(1f);

        // Steel Bubble – F key, requires flaring
        if (enableSteelBubble)
        {
            if (Input.GetKeyDown(steelBubbleKey))
            {
                if (IsFlaring && steelBubbleCooldownTimer <= 0f && !bubbleAppliedThisPress)
                {
                    if (!isBurning) StartBurning();
                    isSteelBubbleActive = true;
                    PushMetalsInBubble();
                    DrainMetal(steelBubbleMetalCostMultiplier);
                    steelBubbleCooldownTimer = steelBubbleCooldown;
                    bubbleAppliedThisPress = true;
                }
            }
            if (Input.GetKeyUp(steelBubbleKey))
            {
                bubbleAppliedThisPress = false;
                isSteelBubbleActive    = false;
            }
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
        isBurning = false;
        cooldownTimer = pushCooldown;
        allomancer?.StopBurning();
    }

    // ── Metal Detection ───────────────────────────────────────────────────────

    void UpdateTargetedMetal()
    {
        hasCurrentTarget        = false;
        currentTarget           = null;
        currentTargetRigidbody  = null;
        metalInRange            = false;

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

    // ── Push Logic ────────────────────────────────────────────────────────────

    void PushMetals()
    {
        if (playerRigidbody == null) { Debug.LogError("[PUSH] playerRigidbody is null!"); return; }
        if (!hasCurrentTarget || currentTargetRigidbody == null)
        {
            if (debugPushOperations) Debug.Log("[PUSH] No target – aim at metal");
            return;
        }

        Rigidbody        targetRigidbody = currentTargetRigidbody;
        AllomanticTarget target           = currentTarget;

        if (targetRigidbody == playerRigidbody)          return;
        if (target != null && !target.canBePushed)        return;

        float targetMass = target != null ? target.GetEffectiveMass() : targetRigidbody.mass;
        float distance   = Vector3.Distance(playerRigidbody.position, targetRigidbody.position);
        Vector3 dir      = targetRigidbody.position - playerRigidbody.position;
        bool isAnchored  = (target != null && target.isAnchored) || targetRigidbody.isKinematic;

        float playerMass    = playerRigidbody.mass;
        float weightFactor  = playerMass / referenceMass;

        // ── Scale strength by scroll-wheel flare level ──────────────────────
        float strength = allomanticStrength * weightFactor * masteryBonus * CurrentFlareMultiplier;

        if (debugPushOperations && IsFlaring)
            Debug.Log($"[PUSH] FlareLevel={FlareLevel:F2} Multiplier={CurrentFlareMultiplier:F2} Strength={strength:F0}");

        float effectiveDistance = Mathf.Max(distance, minDistance);
        float distanceFactor    = Mathf.Pow(referenceDistance / effectiveDistance, distanceExponent);

        Vector3 targetVelocity     = targetRigidbody.linearVelocity;
        float   velAwayFromPlayer  = Vector3.Dot(targetVelocity, dir.normalized);
        float   velDampFactor      = 1f;
        if (velAwayFromPlayer > 0)
        {
            float ratio = Mathf.Clamp01(velAwayFromPlayer / maxCoinVelocity);
            velDampFactor = 1f - (ratio * velocityDamping);
        }

        float force = strength * distanceFactor * velDampFactor;

        if (isAnchored)
        {
            playerRigidbody.AddForce(-dir.normalized * force);
            if (debugPushOperations) Debug.Log($"[PUSH] Pushed player: {force:F0}N");
        }
        else if (force > 1f)
        {
            if (targetVelocity.magnitude < maxCoinVelocity)
            {
                targetRigidbody.AddForce(dir.normalized * force, ForceMode.Impulse);
                if (debugPushOperations) Debug.Log($"[PUSH] Pushed {targetRigidbody.name}: {force:F0}N");
            }
        }

        if (force > shakeForceThreshold)
        {
            ShakeCamera(shakeMagnitude * Mathf.Clamp01(FlareLevel + 0.3f));
            TriggerPushTint(force);
        }
    }

    void PushMetalsInBubble()
    {
        if (playerRigidbody == null) return;

        Collider[] colliders = Physics.OverlapSphere(playerRigidbody.position, steelBubbleRadius, metalLayer);
        if (debugPushOperations) Debug.Log($"[BUBBLE] {colliders.Length} metals in range");

        foreach (Collider collider in colliders)
        {
            Rigidbody        rb     = collider.attachedRigidbody;
            if (rb == null || rb == playerRigidbody) continue;

            AllomanticTarget target = collider.GetComponent<AllomanticTarget>();
            if (target != null && !target.canBePushed) continue;

            // Scale bubble force by flare level
            float force     = steelBubbleForce * CurrentFlareMultiplier;
            Vector3 dir     = (rb.position - playerRigidbody.position).normalized;
            bool isAnchored = (target != null && target.isAnchored) || rb.isKinematic;

            if (isAnchored)
                playerRigidbody.AddForce(-dir * force * Time.deltaTime);
            else
                rb.AddForce(dir * force, ForceMode.Impulse);

            TriggerPushTint(force);
        }
    }

    // ── Metal Drain ───────────────────────────────────────────────────────────

    void DrainMetal(float multiplier = 1f)
    {
        if (allomancer == null) return;

        // Cost scales with flare level: 1× at zero, up to flaringMetalCostMultiplier at max
        float flareCostScale = Mathf.Lerp(1f, flaringMetalCostMultiplier, FlareLevel);
        float drainAmount    = metalCostPerSecond * Time.deltaTime * multiplier * flareCostScale;

        allomancer.DrainMetal(AllomancySkill.MetalType.Steel, drainAmount);
    }

    // ── Prediction ────────────────────────────────────────────────────────────

    void CreatePredictionLine()
    {
        GameObject lineObj = new GameObject("PushPredictionLine");
        predictionLine = lineObj.AddComponent<LineRenderer>();

        Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
        predictionLine.material        = new Material(shader);
        predictionLine.startColor      = predictionColor;
        predictionLine.endColor        = predictionColor;
        predictionLine.startWidth      = 0.03f;
        predictionLine.endWidth        = 0.01f;
        predictionLine.positionCount   = predictionPoints;
        predictionLine.useWorldSpace   = true;
        predictionLine.gameObject.SetActive(false);
    }

    void UpdatePrediction()
    {
        bool isPushing         = Input.GetKey(KeyCode.E);
        bool shouldShowPredict = enablePushPrediction && showPredictionOnHold
                              && isPushing && isBurning
                              && hasCurrentTarget && currentTarget != null
                              && currentTarget.canBePushed;

        if (shouldShowPredict) DrawPredictionLine();
        else if (isPredictionActive)
        {
            predictionLine.gameObject.SetActive(false);
            isPredictionActive = false;
        }
    }

    void DrawPredictionLine()
    {
        if (predictionLine == null || currentTargetRigidbody == null) return;

        float   targetMass   = currentTarget != null ? currentTarget.GetEffectiveMass() : currentTargetRigidbody.mass;
        float   playerMass   = playerRigidbody != null ? playerRigidbody.mass : 80f;
        float   weightFactor = playerMass / referenceMass;
        float   force        = pushForce * weightFactor * CurrentFlareMultiplier;
        float   distance     = currentTargetHit.distance;

        if (distance > 0.01f && distance <= maxRange)
        {
            float eff  = Mathf.Max(distance, minDistance);
            float df   = Mathf.Min(referenceDistance / eff, 2f);
            force *= df;
        }
        else if (distance > maxRange) force = 0f;

        Vector3 initialVelocity;
        if (targetMass <= impulseMassThreshold)
        {
            float impulseForce = force * impulseCalibration;
            Vector3 pushDir    = (currentTargetRigidbody.position - playerRigidbody.position).normalized;
            initialVelocity    = pushDir * (impulseForce / targetMass);
        }
        else
        {
            Vector3 pushDir = (currentTargetRigidbody.position - playerRigidbody.position).normalized;
            initialVelocity = pushDir * (force / targetMass) * 0.1f;
        }

        Vector3[] points   = new Vector3[predictionPoints];
        Vector3   startPos = currentTargetRigidbody.position;
        Vector3   velocity = initialVelocity;

        for (int i = 0; i < predictionPoints; i++)
        {
            points[i]  = startPos;
            startPos  += velocity * predictionTimeStep;
            velocity  += Physics.gravity * predictionTimeStep;
        }

        float speed = initialVelocity.magnitude;
        predictionLine.startColor    = Color.cyan;
        predictionLine.endColor      = speed < 10f ? Color.green : speed < 30f ? Color.yellow : Color.red;
        predictionLine.positionCount = predictionPoints;
        predictionLine.SetPositions(points);
        predictionLine.gameObject.SetActive(true);
        isPredictionActive = true;
    }

    // ── Visual Helpers ────────────────────────────────────────────────────────

    void UpdateCrosshairColor()
    {
        if (crosshairImage == null) return;
        crosshairImage.color = metalInRange ? metalInRangeColor : noMetalColor;
    }

    void TriggerPushTint(float force)
    {
        if (!enablePushScreenTint) return;
        if (pushTintCoroutine != null) StopCoroutine(pushTintCoroutine);
        pushTintCoroutine = StartCoroutine(PushTintCoroutine(force));
    }

    IEnumerator PushTintCoroutine(float force)
    {
        Color tintColor = force < pushForce * 0.3f ? weakPushTint
                        : force < pushForce * 0.7f ? mediumPushTint
                        : strongPushTint;

        float elapsed = 0f;
        while (elapsed < pushTintDuration)
        {
            float alpha  = Mathf.Lerp(tintColor.a, 0f, elapsed / pushTintDuration);
            currentPushTint = new Color(tintColor.r, tintColor.g, tintColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        currentPushTint   = Color.clear;
        pushTintCoroutine = null;
    }

    void StartFlaringVignette()
    {
        if (vignetteImage == null) return;
        if (vignetteCoroutine != null) StopCoroutine(vignetteCoroutine);
        vignetteCoroutine = StartCoroutine(VignettePulseCoroutine());
    }

    IEnumerator VignettePulseCoroutine()
    {
        vignetteImage.gameObject.SetActive(true);

        // Intensity affects how bright the vignette flashes
        float maxAlpha = vignetteMaxAlpha * CurrentFlareMultiplier;
        float elapsed  = 0f;

        while (elapsed < vignettePulseDuration)
        {
            float t     = elapsed / vignettePulseDuration;
            float alpha = Mathf.Sin(t * Mathf.PI) * maxAlpha;
            Color c     = flaringColor;
            c.a         = alpha;
            vignetteImage.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }

        vignetteImage.gameObject.SetActive(false);
        vignetteCoroutine = null;
    }

    void ShakeCamera(float magnitude)
    {
        if (playerCamera == null || magnitude <= 0f) return;
        StartCoroutine(ShakeCoroutine(magnitude));
    }

    IEnumerator ShakeCoroutine(float magnitude)
    {
        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            playerCamera.transform.localPosition = originalPos
                + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * magnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerCamera.transform.localPosition = originalPos;
    }

    // ── Gizmos / GUI ──────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (playerRigidbody == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerRigidbody.position, maxRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(playerRigidbody.position, referenceDistance);
        if (enableSteelBubble)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(playerRigidbody.position, steelBubbleRadius);
        }
    }

    void OnGUI()
    {
        if (currentPushTint.a > 0.01f)
        {
            GUI.color = currentPushTint;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        if (!debugCalibration || !isBurning) return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        style.fontSize = 14;

        float y = 100f;
        GUI.Label(new Rect(10, y, 400, 20), "Steel Push Debug", style);           y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"FlareLevel     : {FlareLevel:F2}", style); y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"FlareMultiplier: {CurrentFlareMultiplier:F2}×", style); y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"Push Force     : {pushForce} N", style); y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"Metal in Range : {metalInRange}", style); y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"Flaring        : {IsFlaring}", style); y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"Cooldown       : {cooldownTimer:F2}s", style);
    }

    void OnDestroy()
    {
        if (predictionLine != null) Destroy(predictionLine.gameObject);
    }

    // ── Static Helper ─────────────────────────────────────────────────────────

    public static float CalculatePushForce(float distance, float basePushForce, float playerMass,
        float referenceMass = 80f, float referenceDistance = 3f, float maxRange = 30f, bool flaring = false)
    {
        float weightFactor = playerMass / referenceMass;
        float force        = basePushForce * weightFactor;

        if (distance > 0.01f && distance <= maxRange)
        {
            float df = Mathf.Min(referenceDistance / Mathf.Max(distance, 1f), 2f);
            force *= df;
        }
        else if (distance > maxRange) force = 0f;

        if (flaring) force *= 2f;
        return force;
    }
}
