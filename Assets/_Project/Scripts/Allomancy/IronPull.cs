/* IronPull.cs
 *
 * PURPOSE:
 * Implements the Iron Allomancy ability (Lurcher) – pull metal objects toward the player.
 *
 * FLARE INTEGRATION (scroll wheel):
 * ==================================
 * Pull force now scales continuously with FlareManager.Instance.FlareMultiplier:
 *   - Intensity 0  (no flare)  → 1.0× force, 1.0× metal cost
 *   - Intensity 5  (mid flare) → ~1.75× force, proportional drain
 *   - Intensity 10 (max flare) → maxFlareMultiplier× force, max drain
 *
 * The IsIronFlaring check (from FlareManager) still gates the pull action itself —
 * you must be flaring to execute a pull — but force is now smooth, not binary.
 *
 * CONTROLS (unchanged):
 * - Q key held    → While flaring, pull targeted metal object
 * - Q release     → Stop burning Iron
 * - Scroll wheel  → Adjust flare intensity (via FlareManager)
 * - Left Ctrl     → Toggle max/off flare (via FlareManager)
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IronPull : MonoBehaviour
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Smooth 0–1 flare intensity from scroll wheel.</summary>
    private float FlareLevel =>
        FlareManager.Instance != null
            ? (float)FlareManager.Instance.FlareIntensity / FlareManager.Instance.maxIntensitySteps
            : 0f;

    /// <summary>Force multiplier derived from scroll-wheel flare intensity.</summary>
    private float CurrentFlareMultiplier =>
        FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

    /// <summary>True when the player is actively flaring (gates pull execution).</summary>
    private bool IsFlaring =>
        FlareManager.Instance != null && FlareManager.Instance.IsIronFlaring;

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Settings")]
    public float referenceMass    = 80f;
    public float referenceDistance= 3f;
    public float minDistance      = 1f;
    public float maxRange         = 30f;
    public float metalCostPerSecond = 2f;

    [Header("Allomancy Physics")]
    [Tooltip("Base allomantic strength (scaled up by flare multiplier at runtime).")]
    public float allomanticStrength = 50f;
    public float maxCoinVelocity    = 20f;
    [Range(1f, 2f)] public float distanceExponent = 1f;
    [Range(0f, 1f)] public float velocityDamping  = 0.5f;

    [Header("Flare Scaling")]
    [Tooltip("Maximum force multiplier at full flare. Sync with FlareManager.maxFlareMultiplier.")]
    [Range(1.5f, 4f)]
    public float maxFlareMultiplier = 2.5f;

    [Tooltip("Metal cost multiplier at full flare intensity.")]
    [Range(1f, 5f)]
    public float flaringMetalCostMultiplier = 3f;

    [Header("References")]
    public Camera        playerCamera;
    public LayerMask     metalLayer;
    public Allomancer    allomancer;
    public Rigidbody     playerRigidbody;
    public Transform     chestTransform;

    [Header("Visual Effects")]
    public GameObject pullEffectPrefab;
    public float shakeMagnitude      = 0.1f;
    public float shakeDuration       = 0.1f;
    public float shakeForceThreshold = 100f;
    public bool  enablePullScreenTint = true;
    public Color weakPullTint   = new Color(0f, 0.5f, 1f, 0.1f);
    public Color mediumPullTint = new Color(0f, 0.8f, 1f, 0.2f);
    public Color strongPullTint = new Color(0f, 1f,   1f, 0.3f);
    public float pullTintDuration = 0.2f;

    [Header("Pull Prediction")]
    public bool  enablePullPrediction = true;
    public Color predictionColor = new Color(0f, 0.5f, 1f, 0.5f);
    public int   predictionPoints = 20;

    [Header("Debug")]
    public bool debugPullOperations = false;

    // ── Private State ─────────────────────────────────────────────────────────
    private bool isBurning           = false;
    private bool pullAppliedThisPress= false;
    private bool qKeyWasPressed      = false;
    private float cooldownTimer      = 0f;

    private RaycastHit       currentTargetHit;
    private AllomanticTarget currentTarget;
    private Rigidbody        currentTargetRigidbody;
    private bool             hasCurrentTarget = false;

    private Coroutine pullTintCoroutine;
    private Color     currentPullTint = Color.clear;

    private LineRenderer predictionLine;
    private bool         isPredictionActive = false;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    void Start()
    {
        if (playerRigidbody == null) playerRigidbody = GetComponentInParent<Rigidbody>();
        if (playerCamera    == null) playerCamera    = Camera.main;
        if (allomancer      == null) allomancer      = GetComponentInParent<Allomancer>();

        metalLayer = LayerMask.GetMask("Metal");

        if (chestTransform == null)
            chestTransform = playerRigidbody != null ? playerRigidbody.transform : transform;

        CreatePredictionLine();
    }

    void Update()
    {
        // Out of metal → stop everything
        if (allomancer != null && !allomancer.canBurnMetal)
        {
            if (isBurning) StopBurning();
            return;
        }

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        UpdateTargetedMetal();

        bool qKeyDown = Input.GetKeyDown(KeyCode.Q);
        bool qKeyUp   = Input.GetKeyUp(KeyCode.Q);

        // Q KEY: execute pull only while flaring
        if (qKeyDown && !qKeyWasPressed && cooldownTimer <= 0f)
        {
            if (IsFlaring)
            {
                qKeyWasPressed = true;
                if (!isBurning) StartBurning();

                PullMetals();
                DrainMetal(flaringMetalCostMultiplier);
            }
        }

        if (qKeyUp)
        {
            qKeyWasPressed = false;
            StopBurning();
        }

        UpdatePrediction();
    }

    // ── Burning ───────────────────────────────────────────────────────────────

    void StartBurning()
    {
        if (isBurning) return;
        isBurning = true;
        allomancer?.StartBurning(AllomancySkill.MetalType.Iron);
    }

    void StopBurning()
    {
        if (!isBurning) return;
        isBurning     = false;
        cooldownTimer = 0.2f;
        if (debugPullOperations) Debug.Log("[IRON PULL] StopBurning()");
        allomancer?.StopBurning();
    }

    // ── Target Detection ──────────────────────────────────────────────────────

    void UpdateTargetedMetal()
    {
        hasCurrentTarget       = false;
        currentTarget          = null;
        currentTargetRigidbody = null;

        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null) return;

        float closestDist = maxRange;

        // Check AllomanticTarget objects
        foreach (var metal in FindObjectsOfType<AllomanticTarget>())
        {
            if (metal == null) continue;
            Rigidbody rb = metal.GetComponent<Rigidbody>();
            if (rb == null || rb == playerRigidbody || !metal.canBePulled) continue;

            float dist = Vector3.Distance(rb.position, playerCamera.transform.position);
            if (dist < closestDist && dist > 0.1f)
            {
                closestDist            = dist;
                currentTargetRigidbody = rb;
                currentTarget          = metal;
                hasCurrentTarget       = true;
            }
        }

        // Also check Metal-layer colliders
        Collider[] colliders = Physics.OverlapSphere(playerCamera.transform.position, maxRange, metalLayer);
        foreach (Collider col in colliders)
        {
            if (col == null) continue;
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb == null || rb == playerRigidbody) continue;

            float dist = Vector3.Distance(rb.position, playerCamera.transform.position);
            if (dist < closestDist && dist > 0.1f && dist <= maxRange)
            {
                closestDist            = dist;
                currentTargetRigidbody = rb;
                currentTarget          = col.GetComponent<AllomanticTarget>();
                hasCurrentTarget       = true;
            }
        }
    }

    // ── Pull Logic ────────────────────────────────────────────────────────────

    void PullMetals()
    {
        if (playerRigidbody == null)         return;
        if (!hasCurrentTarget || currentTargetRigidbody == null) return;

        Rigidbody        targetRigidbody = currentTargetRigidbody;
        AllomanticTarget target           = currentTarget;

        if (targetRigidbody == playerRigidbody)    return;
        if (target != null && !target.canBePulled) return;

        Vector3 pullOrigin    = playerRigidbody.position;
        float   distance      = Vector3.Distance(pullOrigin, targetRigidbody.position);
        Vector3 dirToTarget   = targetRigidbody.position - pullOrigin;
        bool    isAnchored    = (target != null && target.isAnchored) || targetRigidbody.isKinematic;

        // ── Scale strength by scroll-wheel flare level ──────────────────────
        float strength = allomanticStrength
                       * (playerRigidbody.mass / referenceMass)
                       * CurrentFlareMultiplier;

        if (debugPullOperations)
            Debug.Log($"[IRON PULL] FlareLevel={FlareLevel:F2} Multiplier={CurrentFlareMultiplier:F2} Strength={strength:F0}");

        float distanceFactor = Mathf.Clamp01(1f - (distance / maxRange));
        float force          = strength * distanceFactor;

        if (force > 0.1f)
        {
            if (isAnchored)
                playerRigidbody.AddForce(dirToTarget.normalized * force);
            else
                targetRigidbody.AddForce(-dirToTarget.normalized * force, ForceMode.Impulse);
        }

        if (force > shakeForceThreshold)
        {
            ShakeCamera(shakeMagnitude * Mathf.Clamp01(FlareLevel + 0.3f));
            TriggerPullTint(force);
        }
    }

    // ── Metal Drain ───────────────────────────────────────────────────────────

    void DrainMetal(float multiplier = 1f)
    {
        if (allomancer == null) return;

        // Cost scales with flare level: 1× at zero, up to flaringMetalCostMultiplier at max
        float flareCostScale = Mathf.Lerp(1f, flaringMetalCostMultiplier, FlareLevel);
        float drainAmount    = metalCostPerSecond * Time.deltaTime * multiplier * flareCostScale;
        float actionDrain    = metalCostPerSecond * 0.5f * multiplier;

        allomancer.DrainMetal(AllomancySkill.MetalType.Iron, drainAmount + actionDrain);
    }

    // ── Prediction ────────────────────────────────────────────────────────────

    void CreatePredictionLine()
    {
        GameObject lineObj = new GameObject("PullPredictionLine");
        predictionLine = lineObj.AddComponent<LineRenderer>();

        Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
        predictionLine.material      = new Material(shader);
        predictionLine.startColor    = predictionColor;
        predictionLine.endColor      = predictionColor;
        predictionLine.startWidth    = 0.03f;
        predictionLine.endWidth      = 0.01f;
        predictionLine.positionCount = predictionPoints;
        predictionLine.useWorldSpace = true;
        predictionLine.gameObject.SetActive(false);
    }

    void UpdatePrediction()
    {
        bool shouldShow = enablePullPrediction && hasCurrentTarget
                       && currentTarget != null && currentTarget.canBePulled;

        if (shouldShow) DrawPredictionLine();
        else if (isPredictionActive)
        {
            predictionLine.gameObject.SetActive(false);
            isPredictionActive = false;
        }
    }

    void DrawPredictionLine()
    {
        if (predictionLine == null || currentTargetRigidbody == null) return;

        Vector3 startPos = currentTargetRigidbody.position;
        Vector3 endPos   = playerRigidbody.position;

        Vector3[] points = new Vector3[predictionPoints];
        for (int i = 0; i < predictionPoints; i++)
        {
            float t  = i / (float)(predictionPoints - 1);
            points[i]= Vector3.Lerp(startPos, endPos, t);
        }

        float   distance  = Vector3.Distance(startPos, endPos);
        Color   lineColor = distance < 5f  ? new Color(0f, 1f,   1f, 0.8f)
                          : distance < 15f ? new Color(0f, 0.7f, 1f, 0.6f)
                          :                  new Color(0f, 0.5f, 1f, 0.4f);

        predictionLine.startColor    = lineColor;
        predictionLine.endColor      = lineColor;
        predictionLine.positionCount = predictionPoints;
        predictionLine.SetPositions(points);
        predictionLine.gameObject.SetActive(true);
        isPredictionActive = true;
    }

    // ── Visual Helpers ────────────────────────────────────────────────────────

    void ShakeCamera(float magnitude)
    {
        if (playerCamera == null || magnitude <= 0f) return;
        StartCoroutine(ShakeCoroutine(magnitude));
    }

    IEnumerator ShakeCoroutine(float magnitude)
    {
        Vector3 originalPos = playerCamera.transform.localPosition;
        float   elapsed     = 0f;
        while (elapsed < shakeDuration)
        {
            playerCamera.transform.localPosition = originalPos
                + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * magnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerCamera.transform.localPosition = originalPos;
    }

    void TriggerPullTint(float force)
    {
        if (!enablePullScreenTint) return;
        if (pullTintCoroutine != null) StopCoroutine(pullTintCoroutine);
        pullTintCoroutine = StartCoroutine(PullTintCoroutine(force));
    }

    IEnumerator PullTintCoroutine(float force)
    {
        Color tintColor = force > shakeForceThreshold * 2f ? strongPullTint
                        : force > shakeForceThreshold       ? mediumPullTint
                        : weakPullTint;

        float elapsed = 0f;
        while (elapsed < pullTintDuration)
        {
            float alpha     = Mathf.Lerp(tintColor.a, 0f, elapsed / pullTintDuration);
            currentPullTint = new Color(tintColor.r, tintColor.g, tintColor.b, alpha);
            elapsed        += Time.deltaTime;
            yield return null;
        }
        currentPullTint   = Color.clear;
        pullTintCoroutine = null;
    }

    // ── GUI ───────────────────────────────────────────────────────────────────

    void OnGUI()
    {
        if (currentPullTint.a > 0.01f)
        {
            GUI.color = currentPullTint;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        if (IsFlaring && debugPullOperations)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.cyan;
            style.fontSize = 14;

            GUI.Label(new Rect(10, 250, 300, 20), $"Iron Pull – FlareLevel: {FlareLevel:F2}", style);
            GUI.Label(new Rect(10, 270, 300, 20), $"Multiplier: {CurrentFlareMultiplier:F2}×", style);

            if (hasCurrentTarget && currentTargetRigidbody != null)
            {
                float dist = Vector3.Distance(playerRigidbody.position, currentTargetRigidbody.position);
                GUI.Label(new Rect(10, 290, 300, 20), $"Target: {dist:F1}m", style);
            }
        }
    }
}
