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
 *
 * TARGETING:
 * When burning is active, targeting defers to MetalLineRenderer.GetClosestMetalRigidbody()
 * so the highlighted object and the pull target are always the same object.
 * When not burning, the standard camera-alignment scan is used as fallback.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[PlayerComponent("Allomancy Metals", order: 20)]
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
    public float metalCostPerSecond = 0.5f;

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
    public Camera        playerCamera;
    public LayerMask     metalLayer;
    public Allomancer    allomancer;
    public Rigidbody     playerRigidbody;
    public Transform     chestTransform;
    public MetalSelector metalSelector;

    // ── NEW: reference to the shared line renderer so we can read its target ──
    [Header("Allomantic Sight")]
    [Tooltip("Assign the MetalLineRenderer on this player. When burning, Iron will " +
             "pull whatever MetalLineRenderer has highlighted instead of doing its own scan.")]
    public MetalLineRenderer metalLineRenderer;

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
        if (metalSelector != null)
        {
            if (metalSelector.GetPrimaryMetal() == AllomancySkill.MetalType.Iron)
                return Keybinds.Ability1;
            if (metalSelector.GetSecondaryMetal() == AllomancySkill.MetalType.Iron)
                return Keybinds.Ability2;
            return KeyCode.None;
        }
        return Keybinds.Ability2;
    }

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    void Start()
    {
        if (playerRigidbody == null) playerRigidbody = GetComponentInParent<Rigidbody>();
        if (playerCamera    == null) playerCamera    = Camera.main;
        if (allomancer      == null) allomancer      = GetComponentInParent<Allomancer>();

        metalLayer = LayerMask.GetMask("Metal");

        if (metalSelector == null) metalSelector = GetComponentInParent<MetalSelector>();

        // ── NEW: auto-find MetalLineRenderer on this player if not assigned ──
        if (metalLineRenderer == null)
            metalLineRenderer = GetComponentInParent<MetalLineRenderer>();

        if (chestTransform == null)
            chestTransform = playerRigidbody != null ? playerRigidbody.transform : transform;

        CreatePredictionLine();
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        UpdateTargetedMetal();

        KeyCode pullKey = GetAbility1Key();

        if (pullKey != KeyCode.None && Input.GetKeyDown(pullKey) && cooldownTimer <= 0f)
        {
            if (!IsBurning)
            {
                // Silently block — burning is off.
            }
            else if (allomancer == null || allomancer.GetMetalReserve(AllomancySkill.MetalType.Iron) <= 0)
            {
                // Iron reserve empty.
            }
            else if (hasCurrentTarget)
            {
                PullMetals();
                allomancer.DrainMetal(AllomancySkill.MetalType.Iron, metalCostPerSecond);
                cooldownTimer = 0.2f;
            }
        }

        UpdatePrediction();
    }

    // ── Target Detection ──────────────────────────────────────────────────────

    private float targetScanTimer = 0f;
    private const float TARGET_SCAN_INTERVAL = 0.1f;

    void UpdateTargetedMetal()
    {
        // ── NEW: When burning, always use MetalLineRenderer's highlighted target.
        // This guarantees the mesh highlight and the pull target are the same object.
        if (IsBurning && metalLineRenderer != null)
        {
            Rigidbody mlrRb = metalLineRenderer.GetClosestMetalRigidbody();
            if (mlrRb != null && mlrRb != playerRigidbody)
            {
                currentTargetRigidbody = mlrRb;
                currentTarget          = mlrRb.GetComponentInParent<AllomanticTarget>();
                hasCurrentTarget       = true;
                isAnchored             = currentTarget != null ? currentTarget.isAnchored : mlrRb.isKinematic;
                return;
            }
            // MetalLineRenderer found nothing — fall through to own scan below.
        }

        // ── Fallback: own camera-alignment scan (used when not burning, or if
        // MetalLineRenderer hasn't found anything yet).
        targetScanTimer -= Time.deltaTime;
        if (targetScanTimer > 0f) return;
        targetScanTimer = TARGET_SCAN_INTERVAL;

        hasCurrentTarget       = false;
        currentTarget          = null;
        currentTargetRigidbody = null;
        isAnchored             = false;

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
            if (at != null && !at.canBePulled) continue;

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
                isAnchored             = at != null ? at.isAnchored : rb.isKinematic;
            }
        }
    }

    // ── Pull Logic ────────────────────────────────────────────────────────────

    void PullMetals()
    {
        if (playerRigidbody == null || currentTargetRigidbody == null) return;
        if (!hasCurrentTarget) return;
        if (currentTarget != null && !currentTarget.canBePulled) return;

        Vector3 dirToTarget  = currentTargetRigidbody.position - playerRigidbody.position;
        float   distance     = dirToTarget.magnitude;
        Vector3 pullDirection = dirToTarget.normalized;

        float flare = CurrentFlareMultiplier;

        float distanceMult = inverseDistanceScaling
            ? Mathf.Clamp(maxRange / Mathf.Max(distance, minDistance), 0.5f, 3f)
            : 1f;

        if (isAnchored)
        {
            float   speed    = pullSpeed * flare * distanceMult;
            Vector3 velocity = pullDirection * Mathf.Min(speed, maxPullSpeed);
            playerRigidbody.AddForce(velocity, ForceMode.VelocityChange);
        }
        else
        {
            float playerMass = playerRigidbody.mass;
            float objectMass = currentTargetRigidbody.mass;
            float totalMass  = playerMass + objectMass;
            float pullMag    = loosePullForce * flare * distanceMult;

            float playerSpeed = Mathf.Min(pullMag * (objectMass / totalMass), maxPullSpeed);
            playerRigidbody.AddForce(pullDirection * playerSpeed, ForceMode.VelocityChange);

            float objectSpeed = Mathf.Min(pullMag * (playerMass / totalMass), loosePullForce * 2f);
            currentTargetRigidbody.AddForce(-pullDirection * objectSpeed, ForceMode.VelocityChange);
        }

        float pullForce = isAnchored ? pullSpeed * CurrentFlareMultiplier : loosePullForce * CurrentFlareMultiplier;
        TriggerPullTint(pullForce);
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
