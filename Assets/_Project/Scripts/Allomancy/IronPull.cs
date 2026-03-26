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
 * - Left Ctrl  → Toggle burning ON / OFF (via FlareManager)
 * - Q key      → While burning, pull targeted metal object (single impulse per press)
 * - Scroll wheel → Adjust flare intensity (via FlareManager)
 *
 * BURN REQUIREMENT:
 * FlareManager.Instance.IsBurning must be true (Left Ctrl toggled on) before Q
 * will do anything. Mirrors the SteelPush gate on IsFlaring/IsSteelFlaring.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IronPull : MonoBehaviour
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private float CurrentFlareMultiplier =>
        FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

    /// <summary>True only when the player has Iron burning toggled on.</summary>
    private bool IsBurning =>
        FlareManager.Instance != null && FlareManager.Instance.IsBurning;

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Settings")]
    public float minDistance = 1f;
    public float maxRange = 30f;
    public float metalCostPerSecond = 2f;

    [Header("Pull Physics — PHYSICS-MATH-BOOK.md Section 2")]
    [Tooltip("Pull speed when yanking toward anchored metal")]
    public float pullSpeed = 20f;
    [Tooltip("Max speed player can reach from pulling")]
    public float maxPullSpeed = 18f;
    [Tooltip("Speed applied to loose objects pulled toward player")]
    public float loosePullForce = 30f;
    [Tooltip("Stronger pull at close range")]
    public bool inverseDistanceScaling = true;

    [Header("References")]
    public Camera playerCamera;
    public LayerMask metalLayer;
    public Allomancer allomancer;
    public Rigidbody playerRigidbody;
    public Transform chestTransform;

    [Header("Visual Effects")]
    public GameObject pullEffectPrefab;
    public float shakeMagnitude = 0.1f;
    public float shakeDuration = 0.1f;
    public float shakeForceThreshold = 100f;
    public bool enablePullScreenTint = true;
    public Color weakPullTint   = new Color(0f, 0.5f, 1f, 0.1f);
    public Color mediumPullTint = new Color(0f, 0.8f, 1f, 0.2f);
    public Color strongPullTint = new Color(0f, 1f,   1f, 0.3f);
    public float pullTintDuration = 0.2f;

    [Header("Pull Prediction")]
    public bool enablePullPrediction = true;
    public Color predictionColor = new Color(0f, 0.5f, 1f, 0.5f);
    public int predictionPoints = 20;

    [Header("Debug")]
    public bool debugPullOperations = false;
    public bool debugFlareState = false;

    // ── Private State ─────────────────────────────────────────────────────────

    private float cooldownTimer = 0f;

    private RaycastHit currentTargetHit;
    private AllomanticTarget currentTarget;
    private Rigidbody currentTargetRigidbody;
    private bool hasCurrentTarget = false;
    private bool isAnchored = false;

    private Coroutine pullTintCoroutine;
    private Color currentPullTint = Color.clear;

    private LineRenderer predictionLine;
    private bool isPredictionActive = false;

    // ── Keybind Helper ────────────────────────────────────────────────────────

    private KeyCode GetAbility1Key()
    {
        // Q always pulls — no metal selector dependency
        return KeyCode.Q;
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

        // ── Q key: single-click pull impulse ──────────────────────────────────
        // Requires:
        //   1. FlareManager.IsBurning — player must have Left Ctrl toggled on
        //   2. Metal reserve > 0
        //   3. A valid metal target in range
        //   4. Cooldown elapsed
        KeyCode pullKey = GetAbility1Key();

        if (pullKey != KeyCode.None && Input.GetKeyDown(pullKey) && cooldownTimer <= 0f)
        {
            if (!IsBurning)
            {
                // Silently block — burning is off. Optionally log for debug.
                if (debugFlareState)
                    Debug.Log("[PULL] Blocked: not burning Iron. Toggle burning with Left Ctrl.");
            }
            else if (allomancer == null || allomancer.GetMetalReserve(AllomancySkill.MetalType.Iron) <= 0)
            {
                if (debugPullOperations)
                    Debug.Log("[PULL] Blocked: Iron reserve empty.");
            }
            else if (hasCurrentTarget)
            {
                PullMetals();
                allomancer.DrainMetal(AllomancySkill.MetalType.Iron, metalCostPerSecond);
                cooldownTimer = 0.2f;
            }
            else
            {
                if (debugPullOperations)
                    Debug.Log("[PULL] No target in range.");
            }
        }

        UpdatePrediction();
    }

    // ── Target Detection ──────────────────────────────────────────────────────

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
        Vector3 dirToTarget  = currentTargetRigidbody.position - playerRigidbody.position;
        float   distance     = dirToTarget.magnitude;
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
            float   speed    = pullSpeed * flare * distanceMult;
            Vector3 velocity = pullDirection * Mathf.Min(speed, maxPullSpeed);
            playerRigidbody.AddForce(velocity, ForceMode.VelocityChange);

            if (debugPullOperations)
                Debug.Log($"[PULL] Yanked player toward anchored {currentTargetRigidbody.name} | speed={speed:F1} | flare={flare:F2}");
        }
        else
        {
            // LOOSE OBJECT: Newton's 3rd Law — lighter party moves more
            float playerMass = playerRigidbody.mass;
            float objectMass = currentTargetRigidbody.mass;
            float totalMass  = playerMass + objectMass;
            float pullMag    = loosePullForce * flare * distanceMult;

            float playerSpeed = Mathf.Min(pullMag * (objectMass / totalMass), maxPullSpeed);
            playerRigidbody.AddForce(pullDirection * playerSpeed, ForceMode.VelocityChange);

            float objectSpeed = Mathf.Min(pullMag * (playerMass / totalMass), loosePullForce * 2f);
            currentTargetRigidbody.AddForce(-pullDirection * objectSpeed, ForceMode.VelocityChange);

            if (debugPullOperations)
                Debug.Log($"[PULL] Pulled {currentTargetRigidbody.name} | objectSpeed={objectSpeed:F1} | playerSpeed={playerSpeed:F1} | flare={flare:F2}");
        }

        // --- Visual feedback ---
        CameraShakeManager.Instance?.Shake(shakeDuration, shakeMagnitude);
        SoundManager.Instance?.PlayPullSound();

        Vector3 chestPos = chestTransform != null ? chestTransform.position : transform.position;
        PushPullTrail.Instance?.ShowPullTrail(currentTargetRigidbody.position, chestPos);
    }

    // ── Prediction ────────────────────────────────────────────────────────────

    void CreatePredictionLine()
    {
        GameObject lineObj = new GameObject("PullPredictionLine");
        predictionLine = lineObj.AddComponent<LineRenderer>();

        Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
        predictionLine.material   = new Material(shader);
        predictionLine.startColor = predictionColor;
        predictionLine.endColor   = predictionColor;
        predictionLine.startWidth = 0.03f;
        predictionLine.endWidth   = 0.01f;
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
            float t  = i / (float)(predictionPoints - 1);
            points[i] = Vector3.Lerp(startPos, endPos, t);
        }

        float dist = Vector3.Distance(startPos, endPos);
        Color lineColor = dist < 5f  ? new Color(0f, 1f,   1f, 0.8f)
                        : dist < 15f ? new Color(0f, 0.7f, 1f, 0.6f)
                                     : new Color(0f, 0.5f, 1f, 0.4f);

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
                        : force > shakeForceThreshold      ? mediumPullTint
                                                           : weakPullTint;
        float elapsed = 0f;
        while (elapsed < pullTintDuration)
        {
            float alpha    = Mathf.Lerp(tintColor.a, 0f, elapsed / pullTintDuration);
            currentPullTint = new Color(tintColor.r, tintColor.g, tintColor.b, alpha);
            elapsed        += Time.deltaTime;
            yield return null;
        }
        currentPullTint  = Color.clear;
        pullTintCoroutine = null;
    }
}
