/* IronPull.cs
 *
 * PURPOSE:
 * Implements the Iron Allomancy ability (Lurcher) – pull metal objects toward the player.
 * Physics are lore-accurate: mass ratios determine who moves more.
 * Whoever has less mass moves more (Newton's 3rd Law + Mistborn canon).
 *
 * FLARE INTEGRATION (scroll wheel):
 * Pull force scales continuously with FlareManager.Instance.FlareMultiplier.
 *
 * CONTROLS:
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

    private float FlareLevel =>
        FlareManager.Instance != null
            ? (float)FlareManager.Instance.Intensity / FlareManager.Instance.maxIntensitySteps
            : 0f;

    private float CurrentFlareMultiplier =>
        FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

    private bool IsFlaring =>
        FlareManager.Instance != null && FlareManager.Instance.IsIronFlaring;

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Settings")]
    public float referenceMass       = 80f;
    public float referenceDistance   = 3f;
    public float minDistance         = 1f;
    public float maxRange            = 30f;
    public float metalCostPerSecond  = 2f;

    [Header("Allomancy Physics")]
    public float allomanticStrength  = 50f;
    public float maxCoinVelocity     = 20f;
    [Range(1f, 2f)] public float distanceExponent = 1f;
    [Range(0f, 1f)] public float velocityDamping  = 0.5f;

    [Header("Flare Scaling")]
    [Range(1.5f, 4f)]  public float maxFlareMultiplier        = 2.5f;
    [Range(1f,   5f)]  public float flaringMetalCostMultiplier = 3f;

    [Header("References")]
    public Camera        playerCamera;
    public LayerMask     metalLayer;
    public Allomancer    allomancer;
    public Rigidbody     playerRigidbody;
    public Transform     chestTransform;

    [Header("Visual Effects")]
    public GameObject pullEffectPrefab;
    public float shakeMagnitude       = 0.1f;
    public float shakeDuration        = 0.1f;
    public float shakeForceThreshold  = 100f;
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
    public bool debugFlareState = false;

    // ── Private State ─────────────────────────────────────────────────────────
    private bool _isBurning = false;
    private bool isBurning 
    {
        get 
        {
            bool globalBurn = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Iron;
            return _isBurning || globalBurn;
        }
        set { _isBurning = value; }
    }
    private bool  pullAppliedThisPress = false;
    private bool  qKeyWasPressed       = false;
    private float cooldownTimer        = 0f;

    private RaycastHit       currentTargetHit;
    private AllomanticTarget currentTarget;
    private Rigidbody        currentTargetRigidbody;
    private bool             hasCurrentTarget = false;
    private bool             isAnchored       = false;

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
        allomancer?.StopBurning();
    }

    // ── Target Detection ──────────────────────────────────────────────────────

    void UpdateTargetedMetal()
    {
        hasCurrentTarget       = false;
        currentTarget          = null;
        currentTargetRigidbody = null;
        isAnchored             = false;

        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null) return;

        float closestWeight = float.MaxValue;

        // Optimized Registry Scan
        foreach (var metal in MistbornRegistry.ActiveMetalTargets)
        {
            if (metal == null || !metal.canBePulled) continue;
            Rigidbody rb = metal.GetComponent<Rigidbody>();
            if (rb == null || rb == playerRigidbody) continue;

            float dist = Vector3.Distance(rb.position, playerCamera.transform.position);
            if (dist > maxRange || dist < 0.1f) continue;

            // Simple weight: distance from center of screen + distance to player
            Vector3 viewportPos = playerCamera.WorldToViewportPoint(rb.position);
            bool isOnScreen = viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;

            if (isOnScreen)
            {
                float centerDiff = Vector2.Distance(new Vector2(viewportPos.x, viewportPos.y), new Vector2(0.5f, 0.5f));
                float weight = centerDiff * 10f + dist;

                if (weight < closestWeight)
                {
                    closestWeight = weight;
                    currentTargetRigidbody = rb;
                    currentTarget = metal;
                    hasCurrentTarget = true;
                    isAnchored = metal.isAnchored || rb.isKinematic;
                }
            }
        }
    }

    // ── Pull Logic ────────────────────────────────────────────────────────────

    void PullMetals()
    {
        // --- Null checks ---
        if (playerRigidbody == null || currentTargetRigidbody == null)
            return;
        if (!hasCurrentTarget)
            return;
        if (currentTarget != null && !currentTarget.canBePulled)
            return;

        // --- Direction and distance ---
        Vector3 dirToTarget = currentTargetRigidbody.position - playerRigidbody.position;
        float   distance    = dirToTarget.magnitude;

        // --- Strength calculation ---
        float strength = allomanticStrength
                       * (playerRigidbody.mass / referenceMass)
                       * CurrentFlareMultiplier;

        float distanceFactor = Mathf.Clamp01(1f - (distance / maxRange));
        float force          = strength * distanceFactor;

        // --- Mass setup ---
        float playerMass = playerRigidbody.mass;
        float objectMass = currentTargetRigidbody.mass;

        if (isAnchored)
            objectMass = Mathf.Infinity;

        float totalMass   = playerMass + objectMass;
        float playerRatio = objectMass / totalMass;
        float objectRatio = playerMass / totalMass;

        Vector3 forceDir = dirToTarget.normalized * force;

        // --- Apply forces ---
        playerRigidbody.AddForce(forceDir * playerRatio, ForceMode.Impulse);

        if (!isAnchored)
            currentTargetRigidbody.AddForce(-forceDir * objectRatio, ForceMode.Impulse);

        // --- Visual feedback ---
        if (force > shakeForceThreshold)
        {
            CameraShakeManager.Instance?.Shake(shakeDuration, shakeMagnitude * Mathf.Clamp01(FlareLevel + 0.3f));
            TriggerPullTint(force);
        }

        if (debugPullOperations)
            Debug.Log($"[IRON PULL] force={force:F0} playerRatio={playerRatio:F2} objectRatio={objectRatio:F2} anchored={isAnchored}");
    }

    // ── Metal Drain ───────────────────────────────────────────────────────────

    void DrainMetal(float multiplier = 1f)
    {
        if (allomancer == null) return;

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
            float t   = i / (float)(predictionPoints - 1);
            points[i] = Vector3.Lerp(startPos, endPos, t);
        }

        float dist      = Vector3.Distance(startPos, endPos);
        Color lineColor = dist < 5f  ? new Color(0f, 1f,   1f, 0.8f)
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
            style.fontSize         = 14;

            GUI.Label(new Rect(10, 250, 300, 20), $"Iron Pull – FlareLevel: {FlareLevel:F2}", style);
            GUI.Label(new Rect(10, 270, 300, 20), $"Multiplier: {CurrentFlareMultiplier:F2}×",  style);

            if (hasCurrentTarget && currentTargetRigidbody != null)
            {
                float dist = Vector3.Distance(playerRigidbody.position, currentTargetRigidbody.position);
                GUI.Label(new Rect(10, 290, 300, 20), $"Target: {dist:F1}m", style);
            }
        }
    }
}
