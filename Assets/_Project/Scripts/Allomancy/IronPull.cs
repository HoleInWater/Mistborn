/* IronPull.cs
 *
 * PURPOSE:
 * Implements the Iron Allomancy ability (Lurcher) – pull metal objects toward the player.
 *
 * FLARE INTEGRATION:
 * ==================
 * Uses the single shared FlareManager intensity (1–10, scroll wheel).
 * Left Ctrl starts burning; Q executes a pull while burning is active.
 * Force scales smoothly with FlareManager.FlareMultiplier.
 *
 * CONTROLS:
 * - Left Ctrl   → Toggle burning ON / OFF (via FlareManager)
 * - Scroll wheel→ Adjust shared intensity 1–10 (via FlareManager)
 * - Q key       → Execute pull (requires burning to be active)
 * - Q release   → Stop burning Iron locally
 */

using UnityEngine;
using System.Collections;

public class IronPull : MonoBehaviour
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Normalised 0–1 flare level from shared intensity.</summary>
    private float FlareLevel =>
        FlareManager.Instance != null && FlareManager.Instance.IsBurning
            ? (float)(FlareManager.Instance.Intensity - 1) / (FlareManager.Instance.maxIntensitySteps - 1)
            : 0f;

    /// <summary>Force multiplier from shared intensity.</summary>
    private float CurrentFlareMultiplier =>
        FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

    /// <summary>True when burning is active.</summary>
    private bool IsFlaring =>
        FlareManager.Instance != null && FlareManager.Instance.IsBurning;

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Settings")]
    public float referenceMass     = 80f;
    public float referenceDistance = 3f;
    public float minDistance       = 1f;
    public float maxRange          = 30f;
    public float metalCostPerSecond= 2f;

    [Header("Allomancy Physics")]
    [Tooltip("Base allomantic strength (scaled up by flare multiplier at runtime).")]
    public float allomanticStrength = 50f;
    public float maxCoinVelocity    = 20f;
    [Range(1f, 2f)] public float distanceExponent = 1f;
    [Range(0f, 1f)] public float velocityDamping  = 0.5f;

    [Header("Flare Scaling")]
    [Tooltip("Metal cost multiplier at full intensity. Matches FlareManager.maxFlareMultiplier.")]
    [Range(1f, 5f)]
    public float flaringMetalCostMultiplier = 3f;

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
    private bool  isBurning      = false;
    private bool  qKeyWasPressed = false;
    private float cooldownTimer  = 0f;

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
        if (allomancer != null && !allomancer.canBurnMetal)
        {
            if (isBurning) StopBurning();
            return;
        }

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        UpdateTargetedMetal();

        bool qKeyDown = Input.GetKeyDown(KeyCode.Q);
        bool qKeyUp   = Input.GetKeyUp(KeyCode.Q);

        if (qKeyDown && !qKeyWasPressed && cooldownTimer <= 0f && IsFlaring)
        {
            qKeyWasPressed = true;
            if (!isBurning) StartBurning();
            PullMetals();
            DrainMetal(flaringMetalCostMultiplier);
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

        foreach (Collider col in Physics.OverlapSphere(playerCamera.transform.position, maxRange, metalLayer))
        {
            if (col == null) continue;
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb == null || rb == playerRigidbody) continue;

            float dist = Vector3.Distance(rb.position, playerCamera.transform.position);
            if (dist < closestDist && dist > 0.1f)
            {
                closestDist            = dist;
                currentTargetRigidbody = rb;
                currentTarget          = col.GetComponent<AllomanticTarget>();
                hasCurrentTarget       = true;
            }
        }
    }

    // ── Pull ──────────────────────────────────────────────────────────────────

    void PullMetals()
    {
        if (playerRigidbody == null || !hasCurrentTarget || currentTargetRigidbody == null) return;

        Rigidbody        targetRb = currentTargetRigidbody;
        AllomanticTarget target   = currentTarget;

        if (targetRb == playerRigidbody)               return;
        if (target != null && !target.canBePulled)     return;

        Vector3 pullOrigin  = playerRigidbody.position;
        float   distance    = Vector3.Distance(pullOrigin, targetRb.position);
        Vector3 dirToTarget = targetRb.position - pullOrigin;
        bool    isAnchored  = (target != null && target.isAnchored) || targetRb.isKinematic;

        // Scale strength by shared intensity multiplier
        float strength = allomanticStrength
                       * (playerRigidbody.mass / referenceMass)
                       * CurrentFlareMultiplier;

        float distanceFactor = Mathf.Clamp01(1f - (distance / maxRange));
        float force          = strength * distanceFactor;

        if (debugPullOperations)
            Debug.Log($"[IRON PULL] Intensity={FlareManager.Instance?.Intensity} Multiplier={CurrentFlareMultiplier:F2} Force={force:F0}");

        if (force > 0.1f)
        {
            if (isAnchored)
                playerRigidbody.AddForce(dirToTarget.normalized * force);
            else
                targetRb.AddForce(-dirToTarget.normalized * force, ForceMode.Impulse);
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
        float flareCostScale = Mathf.Lerp(1f, flaringMetalCostMultiplier, FlareLevel);
        float drain          = metalCostPerSecond * Time.deltaTime * multiplier * flareCostScale
                             + metalCostPerSecond * 0.5f * multiplier;
        allomancer.DrainMetal(AllomancySkill.MetalType.Iron, drain);
    }

    // ── Prediction ────────────────────────────────────────────────────────────

    void CreatePredictionLine()
    {
        GameObject lineObj = new GameObject("PullPredictionLine");
        predictionLine = lineObj.AddComponent<LineRenderer>();
        Shader shader  = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
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

        Vector3[] points  = new Vector3[predictionPoints];
        Vector3   start   = currentTargetRigidbody.position;
        Vector3   end     = playerRigidbody.position;

        for (int i = 0; i < predictionPoints; i++)
            points[i] = Vector3.Lerp(start, end, i / (float)(predictionPoints - 1));

        float   dist      = Vector3.Distance(start, end);
        Color   lineColor = dist < 5f  ? new Color(0f, 1f,   1f, 0.8f)
                          : dist < 15f ? new Color(0f, 0.7f, 1f, 0.6f)
                          :              new Color(0f, 0.5f, 1f, 0.4f);

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
        Vector3 orig    = playerCamera.transform.localPosition;
        float   elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            playerCamera.transform.localPosition = orig
                + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * magnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerCamera.transform.localPosition = orig;
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

    void OnGUI()
    {
        if (currentPullTint.a > 0.01f)
        {
            GUI.color = currentPullTint;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
