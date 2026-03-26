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
    public float minDistance         = 1f;
    public float maxRange            = 30f;
    public float metalCostPerSecond  = 2f;

    [Header("Pull Physics — PHYSICS-MATH-BOOK.md Section 2")]
    [Tooltip("Base pull speed applied to player when pulling toward anchored metal")]
    public float pullSpeed = 12f;
    [Tooltip("Max speed player can reach from pulling")]
    public float maxPullSpeed = 25f;
    [Tooltip("Pull force on loose objects (coins, small metals) toward player")]
    public float loosePullForce = 15f;
    [Tooltip("Stronger pull at close range (inverse distance scaling)")]
    public bool inverseDistanceScaling = true;
    [Range(0f, 1f)] public float velocityDamping  = 0.5f;

    [Header("Flare Scaling")]
    [Range(1.5f, 4f)]  public float maxFlareMultiplier         = 2.5f;
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
    public bool debugFlareState     = false;

    // ── Private State ─────────────────────────────────────────────────────────
    // No burn state — pull works on click

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

    // ── Keybind Helper ────────────────────────────────────────────────────────

    // [AGENT REVIEW] Dynamic primary/secondary keybind
    private KeyCode GetAbility1Key()
    {
        if (allomancer == null) return KeyCode.Q;
        var selector = allomancer.GetComponent<MetalSelector>();
        if (selector == null) return KeyCode.Q;
        if (selector.GetPrimaryMetal()   == AllomancySkill.MetalType.Iron) return KeyCode.E;
        if (selector.GetSecondaryMetal() == AllomancySkill.MetalType.Iron) return KeyCode.Q;
        return KeyCode.None;
    }

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
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        UpdateTargetedMetal();

        // Q key: single click = one pull impulse. No burn requirement.
        KeyCode pullKey = GetAbility1Key();
        if (pullKey != KeyCode.None && Input.GetKeyDown(pullKey) && cooldownTimer <= 0f)
        {
            if (hasCurrentTarget && allomancer != null && allomancer.GetMetalReserve(AllomancySkill.MetalType.Iron) > 0)
            {
                PullMetals();
                allomancer.DrainMetal(AllomancySkill.MetalType.Iron, metalCostPerSecond);
                cooldownTimer = 0.2f;
            }
        }

        UpdatePrediction();
    }

    // No burn state management — pull works on click

    // ── Target Detection ──────────────────────────────────────────────────────

    private float targetScanTimer = 0f;
    private const float TARGET_SCAN_INTERVAL = 0.1f;

    void UpdateTargetedMetal()
    {
        // Throttle expensive physics scan
        targetScanTimer -= Time.deltaTime;
        if (targetScanTimer > 0f) return;
        targetScanTimer = TARGET_SCAN_INTERVAL;

        hasCurrentTarget       = false;
        currentTarget          = null;
        currentTargetRigidbody = null;
        isAnchored             = false;

        LayerMask targetLayer = metalLayer != 0 ? metalLayer : LayerMask.GetMask("Metal");
        Collider[] hits = Physics.OverlapSphere(playerRigidbody.position, maxRange, targetLayer);
        if (hits.Length == 0) return;

        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null || rb == playerRigidbody) continue;

            float dist = Vector3.Distance(playerRigidbody.position, rb.position);
            if (dist < closestDist)
            {
                closestDist            = dist;
                currentTargetRigidbody = rb;
                currentTarget          = hit.GetComponent<AllomanticTarget>();

                if (currentTarget == null || currentTarget.canBePulled)
                {
                    hasCurrentTarget = true;
                    isAnchored       = currentTarget != null ? currentTarget.isAnchored : rb.isKinematic;
                }
            }
        }
    }

    // ── Pull Logic ────────────────────────────────────────────────────────────

    void PullMetals()
    {
        if (playerRigidbody == null || currentTargetRigidbody == null) return;
        if (!hasCurrentTarget) return;
        if (currentTarget != null && !currentTarget.canBePulled) return;

        // --- Direction ---
        Vector3 dirToTarget = currentTargetRigidbody.position - playerRigidbody.position;
        float distance = dirToTarget.magnitude;
        Vector3 pullDirection = dirToTarget.normalized;

        // --- Flare multiplier ---
        float flare = CurrentFlareMultiplier;

        // --- Distance scaling: stronger pull at close range ---
        float distanceMult = inverseDistanceScaling
            ? Mathf.Clamp(maxRange / Mathf.Max(distance, minDistance), 0.5f, 3f)
            : 1f;

        if (isAnchored)
        {
            // ANCHORED: Player gets yanked toward the heavy metal
            // This is how Mistborn fly — pull toward building anchors
            float speed = pullSpeed * flare * distanceMult;
            Vector3 velocity = pullDirection * Mathf.Min(speed, maxPullSpeed);

            // Add velocity toward target (VelocityChange = direct velocity, ignores mass)
            playerRigidbody.AddForce(velocity, ForceMode.VelocityChange);
        }
        else
        {
            // LOOSE OBJECT: Newton's 3rd Law — lighter party moves more
            float playerMass = playerRigidbody.mass;
            float objectMass = currentTargetRigidbody.mass;
            float totalMass = playerMass + objectMass;

            float pullMag = loosePullForce * flare * distanceMult;

            // Player moves toward object
            float playerSpeed = pullMag * (objectMass / totalMass);
            playerSpeed = Mathf.Min(playerSpeed, maxPullSpeed);
            playerRigidbody.AddForce(pullDirection * playerSpeed, ForceMode.VelocityChange);

            // Object moves toward player
            float objectSpeed = pullMag * (playerMass / totalMass);
            objectSpeed = Mathf.Min(objectSpeed, loosePullForce * 2f);
            currentTargetRigidbody.AddForce(-pullDirection * objectSpeed, ForceMode.VelocityChange);
        }

        // --- Visual feedback ---
        CameraShakeManager.Instance?.Shake(shakeDuration, shakeMagnitude * Mathf.Clamp01(FlareLevel + 0.3f));
        TriggerPullTint(pullSpeed);

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

        if (shouldShow)
            DrawPredictionLine();
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
