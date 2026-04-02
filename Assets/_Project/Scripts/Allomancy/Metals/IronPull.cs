/* IronPull.cs
 *
 * PURPOSE:
 * Implements the Iron Allomancy ability (Lurcher) – pull metal objects toward the player.
 *
 * TARGETING LINE:
 * The targeting line is owned by MetalLineRenderer, not this script.
 * IronPull just handles pull physics and targeting logic.
 */

using UnityEngine;
using System.Collections;

[PlayerComponent("Allomancy Metals", order: 20)]
public class IronPull : MonoBehaviour
{
    private float CurrentFlareMultiplier =>
        FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

    private bool IsBurning =>
        FlareManager.Instance != null && FlareManager.Instance.IsBurning;

    [Header("Settings")]
    public float minDistance        = 1f;
    public float maxRange           = 60f;   // ~200 ft — lore canon: "a few hundred feet"
    public float metalCostPerSecond = AllomancyConstants.IronDrainRate; // MAG: 20 min burn (~0.0833/s)

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

    [Header("Debug")]
    public bool debugPullOperations = false;

    private float cooldownTimer = 0f;

    private AllomanticTarget currentTarget;
    private Rigidbody        currentTargetRigidbody;
    private bool             hasCurrentTarget = false;
    private bool             isAnchored       = false;

    private Coroutine pullTintCoroutine;
    private Color     currentPullTint = Color.clear;

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
    }

    [Header("Pull Cooldown")]
    public float pullCooldown = 0.2f;

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        UpdateTargetedMetal();

        // Pull force and drain run every frame for smooth arc trajectories.
        // The pull direction updates each frame as positions change, so diagonal
        // pulls (swinging toward a wall anchor, arcing across a gap) produce
        // smooth parabolic paths instead of pulsed saw-tooth.
        KeyCode pullKey = GetAbility1Key();
        bool holdingPull = pullKey != KeyCode.None && Input.GetKey(pullKey);
        if (holdingPull && IsBurning && hasCurrentTarget
            && allomancer != null && allomancer.GetMetalReserve(AllomancySkill.MetalType.Iron) > 0)
        {
            allomancer.DrainMetal(AllomancySkill.MetalType.Iron, metalCostPerSecond * Time.deltaTime);

            // Continuous force — smooth arcs
            ApplyPullForce();

            if (cooldownTimer <= 0f)
            {
                PullEffects();
                cooldownTimer = pullCooldown;
            }
        }
    }

    private float targetScanTimer    = 0f;
    private const float SCAN_INTERVAL = 0.1f;

    void UpdateTargetedMetal()
    {
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

    /// <summary>
    /// Calculates pull direction, distance falloff, and anchor state for the current target.
    /// Returns false if the pull should not proceed.
    /// </summary>
    bool GetPullParams(out Vector3 pullDir, out float distMult)
    {
        pullDir  = Vector3.zero;
        distMult = 0f;

        if (playerRigidbody == null || currentTargetRigidbody == null || !hasCurrentTarget) return false;
        if (currentTarget != null && !currentTarget.canBePulled) return false;

        // Pull originates from the allomancer's "center of self" (chest), not center of mass.
        Vector3 origin    = chestTransform != null ? chestTransform.position : playerRigidbody.position;
        Vector3 toTarget  = currentTargetRigidbody.position - origin;
        float   distance  = toTarget.magnitude;
        pullDir = distance > 0.001f ? toTarget / distance : Vector3.zero;

        // Linear distance falloff — direction recalculated each frame for arc accuracy
        float r = Mathf.Max(distance, minDistance);
        float anchorBonus = Mathf.Clamp(Mathf.Log10(Mathf.Max(1f, currentTargetRigidbody.mass)), 0f, 1f);
        float effectiveRange = maxRange * (1f + anchorBonus * 0.5f);
        distMult = inverseDistanceScaling ? Mathf.Clamp01(1f - r / effectiveRange) : 1f;
        return true;
    }

    /// <summary>
    /// Applied every frame while the pull key is held.
    /// Per-frame scaling (Time.deltaTime / pullCooldown) preserves the same net impulse
    /// as the old pulsed system, but produces smooth parabolic arcs for any angle —
    /// pulling toward a wall anchor at 45° arcs the player in a smooth curve instead
    /// of a staircase.
    /// </summary>
    void ApplyPullForce()
    {
        if (!GetPullParams(out Vector3 pullDir, out float distMult)) return;

        float flare      = CurrentFlareMultiplier;
        float frameScale = Time.deltaTime / pullCooldown;

        if (isAnchored)
        {
            float speed = Mathf.Min(pullSpeed * flare * distMult, maxPullSpeed);
            playerRigidbody.AddForce(pullDir * speed * frameScale, ForceMode.VelocityChange);
        }
        else
        {
            float playerMass = playerRigidbody.mass;
            float objectMass = currentTargetRigidbody.mass;
            float totalMass  = playerMass + objectMass;
            float pullMag    = loosePullForce * flare * distMult;

            float playerV = Mathf.Min(pullMag * (objectMass / totalMass), maxPullSpeed);
            float objectV = Mathf.Min(pullMag * (playerMass / totalMass), loosePullForce * 2f);

            playerRigidbody.AddForce(pullDir * playerV * frameScale, ForceMode.VelocityChange);
            currentTargetRigidbody.AddForce(-pullDir * objectV * frameScale, ForceMode.VelocityChange);
        }
    }

    /// <summary>
    /// Throttled effects (sound, camera shake, trail, disarm check).
    /// Called on cooldown timer — does NOT apply physics force.
    /// </summary>
    void PullEffects()
    {
        if (!GetPullParams(out Vector3 pullDir, out float distMult)) return;

        float pullForce = isAnchored ? pullSpeed * CurrentFlareMultiplier : loosePullForce * CurrentFlareMultiplier;
        TriggerPullTint(pullForce);
        CameraShakeManager.Instance?.Shake(shakeDuration, shakeMagnitude);
        SoundManager.Instance?.PlayPullSound();

        Vector3 chestPos = chestTransform != null ? chestTransform.position : transform.position;
        PushPullTrail.Instance?.ShowPullTrail(currentTargetRigidbody.position, chestPos);

        HeldWeaponMarker marker = currentTargetRigidbody.GetComponentInChildren<HeldWeaponMarker>()
                               ?? currentTargetRigidbody.GetComponent<HeldWeaponMarker>();
        marker?.TryDisarm(pullForce);
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
