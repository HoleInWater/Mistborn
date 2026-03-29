/* IronPull.cs
 *
 * PURPOSE:
 * Implements the Iron Allomancy ability (Lurcher) – pull metal objects toward the player.
 * Physics are lore-accurate: mass ratios determine who moves more.
 *
 * CONTROLS:
 * - Left Ctrl    → Toggle burning ON / OFF (via FlareManager)
 * - Q key        → While burning, pull targeted metal object
 * - Scroll wheel → Adjust flare intensity (via FlareManager)
 *
 * TARGETING:
 * When burning is active, targeting defers to MetalLineRenderer.GetClosestMetalRigidbody()
 * so the highlighted object and the pull target are always the same object.
 * When not burning, the standard camera-alignment scan is used as fallback.
 */

using UnityEngine;
using System.Collections;

[PlayerComponent("Allomancy Metals", order: 20)]
public class IronPull : MonoBehaviour
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private float CurrentFlareMultiplier =>
        FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

    private bool IsBurning =>
        FlareManager.Instance != null && FlareManager.Instance.IsBurning;

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Settings")]
    public float minDistance        = 1f;
    public float maxRange           = 30f;
    public float metalCostPerSecond = 0.5f;

    [Header("Pull Physics")]
    public float pullSpeed              = 20f;
    public float maxPullSpeed           = 18f;
    public float loosePullForce         = 30f;
    public bool  inverseDistanceScaling = true;

    [Header("References")]
    public Camera        playerCamera;
    public LayerMask     metalLayer;
    public Allomancer    allomancer;
    public Rigidbody     playerRigidbody;
    public Transform     chestTransform;
    public MetalSelector metalSelector;

    [Header("Allomantic Sight")]
    [Tooltip("Auto-found if left empty. When burning, Iron targets whatever MetalLineRenderer has highlighted.")]
    public MetalLineRenderer metalLineRenderer;

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
    public Color predictionColor      = new Color(0f, 0.5f, 1f, 0.6f);
    public int   predictionPoints     = 20;

    [Header("Debug")]
    public bool debugPullOperations = false;

    // ── Private State ─────────────────────────────────────────────────────────

    private float cooldownTimer = 0f;

    private AllomanticTarget currentTarget;
    private Rigidbody        currentTargetRigidbody;
    private bool             hasCurrentTarget = false;
    private bool             isAnchored       = false;

    private Coroutine    pullTintCoroutine;
    private Color        currentPullTint = Color.clear;
    private LineRenderer predictionLine;
    private bool         isPredictionActive = false;

    // ── Keybind ───────────────────────────────────────────────────────────────

    private KeyCode GetAbility1Key()
    {
        if (metalSelector != null)
        {
            if (metalSelector.GetPrimaryMetal()   == AllomancySkill.MetalType.Iron) return Keybinds.Ability1;
            if (metalSelector.GetSecondaryMetal() == AllomancySkill.MetalType.Iron) return Keybinds.Ability2;
            return KeyCode.None;
        }
        return Keybinds.Ability2;
    }

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    void Start()
    {
        if (playerRigidbody   == null) playerRigidbody   = GetComponentInParent<Rigidbody>();
        if (playerCamera      == null) playerCamera      = Camera.main;
        if (allomancer        == null) allomancer        = GetComponentInParent<Allomancer>();
        if (metalSelector     == null) metalSelector     = GetComponentInParent<MetalSelector>();
        if (metalLineRenderer == null) metalLineRenderer = GetComponentInParent<MetalLineRenderer>();

        metalLayer = LayerMask.GetMask("Metal");

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
            if (IsBurning && hasCurrentTarget
                && allomancer != null && allomancer.GetMetalReserve(AllomancySkill.MetalType.Iron) > 0)
            {
                PullMetals();
                allomancer.DrainMetal(AllomancySkill.MetalType.Iron, metalCostPerSecond);
                cooldownTimer = 0.2f;
            }
        }

        UpdatePrediction();
    }

    // ── Target Detection ──────────────────────────────────────────────────────

    private float targetScanTimer    = 0f;
    private const float SCAN_INTERVAL = 0.1f;

    void UpdateTargetedMetal()
    {
        // While burning, always use the MetalLineRenderer's closest target so
        // the highlight and the pull target are guaranteed to be the same object.
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
        }

        // Fallback: camera-alignment scan (not burning, or MLR found nothing yet).
        targetScanTimer -= Time.deltaTime;
        if (targetScanTimer > 0f) return;
        targetScanTimer = SCAN_INTERVAL;

        hasCurrentTarget       = false;
        currentTarget          = null;
        currentTargetRigidbody = null;
        isAnchored             = false;

        if (playerRigidbody == null) return;
        LayerMask targetLayer = metalLayer != 0 ? metalLayer : LayerMask.GetMask("Metal");
        Collider[] hits = Physics.OverlapSphere(playerRigidbody.position, maxRange, targetLayer);
        if (hits.Length == 0) return;

        Vector3 camForward = playerCamera != null ? playerCamera.transform.forward : transform.forward;
        float   bestScore  = float.MinValue;

        foreach (var hit in hits)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null || rb == playerRigidbody) continue;

            AllomanticTarget at = hit.GetComponentInParent<AllomanticTarget>();
            if (at != null && !at.canBePulled) continue;

            Vector3 toTarget  = rb.position - playerRigidbody.position;
            float   dist      = toTarget.magnitude;
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

    // ── Pull Physics ──────────────────────────────────────────────────────────

    void PullMetals()
    {
        if (playerRigidbody == null || currentTargetRigidbody == null || !hasCurrentTarget) return;
        if (currentTarget != null && !currentTarget.canBePulled) return;

        Vector3 dirToTarget   = currentTargetRigidbody.position - playerRigidbody.position;
        float   distance      = dirToTarget.magnitude;
        Vector3 pullDirection = dirToTarget.normalized;
        float   flare         = CurrentFlareMultiplier;
        float   distanceMult  = inverseDistanceScaling
            ? Mathf.Clamp(maxRange / Mathf.Max(distance, minDistance), 0.5f, 3f) : 1f;

        if (isAnchored)
        {
            float speed = Mathf.Min(pullSpeed * flare * distanceMult, maxPullSpeed);
            playerRigidbody.AddForce(pullDirection * speed, ForceMode.VelocityChange);
        }
        else
        {
            float playerMass = playerRigidbody.mass;
            float objectMass = currentTargetRigidbody.mass;
            float totalMass  = playerMass + objectMass;
            float pullMag    = loosePullForce * flare * distanceMult;

            playerRigidbody.AddForce(pullDirection
                * Mathf.Min(pullMag * (objectMass / totalMass), maxPullSpeed), ForceMode.VelocityChange);
            currentTargetRigidbody.AddForce(-pullDirection
                * Mathf.Min(pullMag * (playerMass / totalMass), loosePullForce * 2f), ForceMode.VelocityChange);
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
        // Own child GameObject — prevents it from drawing at origin on the first frame.
        GameObject lineObj = new GameObject("IronPredictionLine");
        lineObj.transform.SetParent(transform);
        predictionLine            = lineObj.AddComponent<LineRenderer>();
        predictionLine.material   = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color"));
        predictionLine.startColor = predictionColor;
        predictionLine.endColor   = new Color(predictionColor.r, predictionColor.g, predictionColor.b, 0.2f);
        predictionLine.startWidth = 0.03f;
        predictionLine.endWidth   = 0.01f;
        predictionLine.positionCount = 0;    // nothing to draw yet
        predictionLine.useWorldSpace = true;
        lineObj.SetActive(false);             // fully off until burning + target
    }

    void UpdatePrediction()
    {
        if (predictionLine == null) return;

        if (enablePullPrediction && IsBurning && hasCurrentTarget && currentTargetRigidbody != null)
        {
            predictionLine.gameObject.SetActive(true);
            predictionLine.positionCount = predictionPoints;

            Vector3 start = currentTargetRigidbody.position;
            Vector3 end   = chestTransform != null ? chestTransform.position : transform.position;

            for (int i = 0; i < predictionPoints; i++)
                predictionLine.SetPosition(i, Vector3.Lerp(start, end, i / (float)(predictionPoints - 1)));

            float dist = Vector3.Distance(start, end);
            Color c = dist < 5f  ? new Color(0f, 1f,   1f, 0.8f)
                    : dist < 15f ? new Color(0f, 0.7f, 1f, 0.6f)
                                 : new Color(0f, 0.5f, 1f, 0.4f);
            predictionLine.startColor = c;
            predictionLine.endColor   = c;
            isPredictionActive = true;
        }
        else
        {
            if (isPredictionActive)
            {
                predictionLine.gameObject.SetActive(false);
                predictionLine.positionCount = 0;
                isPredictionActive = false;
            }
        }
    }

    // ── Tint ──────────────────────────────────────────────────────────────────

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
            float alpha     = Mathf.Lerp(tintColor.a, 0f, elapsed / pullTintDuration);
            currentPullTint = new Color(tintColor.r, tintColor.g, tintColor.b, alpha);
            elapsed        += Time.deltaTime;
            yield return null;
        }
        currentPullTint   = Color.clear;
        pullTintCoroutine = null;
    }
}
