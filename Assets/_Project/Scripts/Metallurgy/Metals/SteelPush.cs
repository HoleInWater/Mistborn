/* SteelPush.cs
 *
 * PURPOSE:
 * Implements the Steel Metallurgy ability (Launcher) – push metal objects away.
 *
 * TARGETING LINE:
 * The targeting line is owned by MetalLineRenderer, not this script.
 * SteelPush just handles push physics and targeting logic.
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[PlayerComponent("Metallurgy Metals", order: 10)]
public class SteelPush : MonoBehaviour
{
    private float CurrentFlareMultiplier =>
        FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

    private bool IsBurning =>
        FlareManager.Instance != null && FlareManager.Instance.IsBurning;

    [Header("Push Settings")]
    public float minDistance        = 1f;
    public float maxRange           = 60f;   // ~200 ft — lore canon: "a few hundred feet"
    public float metalCostPerSecond = MetallurgyConstants.SteelDrainRate; // MAG: 20 min burn (~0.0833/s)
    public float pushCooldown       = 0.2f;

    [Header("Push Physics")]
    public float pushSpeed              = 25f;
    public float maxRecoilSpeed         = 20f;
    public float loosePushForce         = 35f;
    public bool  inverseDistanceScaling = true;

    [Header("Flare Scaling")]
    [Range(1f, 5f)] public float flaringMetalCostMultiplier = 3f;
    [Range(1f, 2f)] public float masteryBonus               = 1f;

    [Header("References")]
    public Camera        playerCamera;
    public LayerMask     metalLayer;
    public Metallurgist    metallurgist;
    public Rigidbody     playerRigidbody;
    public Transform     chestTransform;
    public MetalSelector metalSelector;

    [Header("Metallurgic Sight")]
    public MetalLineRenderer metalLineRenderer;

    [Header("Visual Effects")]
    public float shakeMagnitude       = 0.1f;
    public float shakeDuration        = 0.1f;
    public float shakeForceThreshold  = 100f;
    public bool  enablePushScreenTint = true;
    public Color weakPushTint         = new Color(0f, 1f, 0f, 0.1f);
    public Color mediumPushTint       = new Color(1f, 1f, 0f, 0.2f);
    public Color strongPushTint       = new Color(1f, 0f, 0f, 0.3f);
    public float pushTintDuration     = 0.2f;

    [Header("Flaring Vignette")]
    public UnityEngine.UI.Image vignetteImage;
    public Color flaringColor          = new Color(1f, 0.2f, 0f, 0.3f);
    public float vignettePulseDuration = 0.5f;
    public float vignetteMaxAlpha      = 0.3f;

    [Header("UI")]
    public UnityEngine.UI.Image crosshairImage;
    public Color metalInRangeColor = Color.green;
    public Color noMetalColor      = Color.white;

    [Header("Steel Bubble")]
    public bool  enableSteelBubble              = true;
    public float steelBubbleRadius              = 2.5f;
    public float steelBubbleForce               = 50f;
    public float steelBubbleCooldown            = 0.5f;
    public float steelBubbleMetalCostMultiplier = 1.5f;
    public KeyCode steelBubbleKey => GetAbility2Key();

    [Header("Impulse Mode")]
    public float impulseMassThreshold = 5f;
    public float impulseCalibration   = 0.000917f;

    [Header("Debug")]
    public bool debugPushOperations = false;

    private float cooldownTimer            = 0f;
    private float steelBubbleCooldownTimer = 0f;

    private MetallurgicTarget currentTarget;
    private Rigidbody        currentTargetRigidbody;
    private bool             hasCurrentTarget = false;
    private bool             metalInRange     = false;

    private Coroutine vignetteCoroutine;

    private KeyCode GetAbility1Key()
    {
        if (metalSelector != null)
        {
            if (metalSelector.GetPrimaryMetal()   == MetallurgySkill.MetalType.Steel) return Keybinds.Ability1;
            if (metalSelector.GetSecondaryMetal() == MetallurgySkill.MetalType.Steel) return Keybinds.Ability2;
            return KeyCode.None;
        }
        return Keybinds.SteelPush;
    }

    private KeyCode GetAbility2Key()
    {
        if (metalSelector != null)
        {
            if (metalSelector.GetPrimaryMetal()   == MetallurgySkill.MetalType.Steel) return Keybinds.Ability3;
            if (metalSelector.GetSecondaryMetal() == MetallurgySkill.MetalType.Steel) return Keybinds.Ability4;
            return KeyCode.None;
        }
        return Keybinds.Ability3;
    }

    void Start()
    {
        if (playerRigidbody   == null) playerRigidbody   = GetComponentInParent<Rigidbody>();
        if (playerCamera      == null) playerCamera      = Camera.main;
        if (metallurgist        == null) metallurgist        = GetComponentInParent<Metallurgist>();
        if (metalSelector     == null) metalSelector     = GetComponentInParent<MetalSelector>();
        if (metalLineRenderer == null) metalLineRenderer = GetComponentInParent<MetalLineRenderer>();

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
    }

    void Update()
    {
        if (cooldownTimer            > 0f) cooldownTimer            -= Time.deltaTime;
        if (steelBubbleCooldownTimer > 0f) steelBubbleCooldownTimer -= Time.deltaTime;

        UpdateTargetedMetal();

        // Push force and drain run every frame for smooth arc trajectories.
        // The push direction updates each frame as positions change, so diagonal
        // and arced pushes naturally produce parabolic paths under gravity.
        // Visual/audio effects remain on cooldownTimer to avoid spam.
        KeyCode pushKey = GetAbility1Key();
        bool holdingPush = pushKey != KeyCode.None && Input.GetKey(pushKey);
        if (holdingPush && IsBurning && hasCurrentTarget
            && metallurgist != null && metallurgist.GetMetalReserve(MetallurgySkill.MetalType.Steel) > 0)
        {
            metallurgist.DrainMetal(MetallurgySkill.MetalType.Steel, metalCostPerSecond * Time.deltaTime);

            // Continuous force application — smooth arcs instead of pulsed saw-tooth
            ApplyPushForce();

            if (cooldownTimer <= 0f)
            {
                PushEffects();
                cooldownTimer = pushCooldown;
                StartFlaringVignette();
            }
        }

        KeyCode bubbleKey = steelBubbleKey;
        if (enableSteelBubble && bubbleKey != KeyCode.None
            && Input.GetKeyDown(bubbleKey) && IsBurning && steelBubbleCooldownTimer <= 0f)
        {
            if (metallurgist != null && metallurgist.GetMetalReserve(MetallurgySkill.MetalType.Steel) > 0)
            {
                PushMetalsInBubble();
                metallurgist.DrainMetal(MetallurgySkill.MetalType.Steel,
                    metalCostPerSecond * steelBubbleMetalCostMultiplier * CurrentFlareMultiplier);
                steelBubbleCooldownTimer = steelBubbleCooldown;
            }
        }

        UpdateCrosshairColor();
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
                currentTarget          = mlrRb.GetComponentInParent<MetallurgicTarget>();
                hasCurrentTarget       = true;
                metalInRange           = true;
                return;
            }
        }

        targetScanTimer -= Time.deltaTime;
        if (targetScanTimer > 0f) return;
        targetScanTimer = SCAN_INTERVAL;

        hasCurrentTarget       = false;
        currentTarget          = null;
        currentTargetRigidbody = null;
        metalInRange           = false;

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

            MetallurgicTarget at = hit.GetComponentInParent<MetallurgicTarget>();
            if (at != null && !at.canBePushed) continue;

            Vector3 toTarget  = rb.worldCenterOfMass - playerRigidbody.position;
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
                metalInRange           = true;
            }
        }
    }

    /// <summary>
    /// Calculates push direction, distance falloff, and anchor state for the current target.
    /// Used by both ApplyPushForce (per-frame) and PushEffects (cooldown-throttled).
    /// Returns false if the push should not proceed.
    /// </summary>
    bool GetPushParams(out Vector3 pushDir, out float distMult, out bool anchored)
    {
        pushDir   = Vector3.zero;
        distMult  = 0f;
        anchored  = false;

        if (playerRigidbody == null || !hasCurrentTarget || currentTargetRigidbody == null) return false;
        if (currentTargetRigidbody == playerRigidbody)           return false;
        if (currentTarget != null && !currentTarget.canBePushed) return false;

        // Push originates from the metallurgist's "center of self" (chest), not center of mass.
        // Lore: blue lines extend from the chest — where you'd point if you said "who, me?"
        // Lore: blue lines target the metal's center of mass — even if that point
        // is in empty space (hollow ring, bent bar). worldCenterOfMass handles this.
        Vector3 origin   = chestTransform != null ? chestTransform.position : playerRigidbody.position;
        Vector3 targetCoM = currentTargetRigidbody.worldCenterOfMass;
        Vector3 toTarget = targetCoM - origin;
        float   distance = toTarget.magnitude;

        pushDir  = distance > 0.001f ? toTarget / distance : Vector3.up;
        anchored = (currentTarget != null && currentTarget.isAnchored) || currentTargetRigidbody.isKinematic;

        // Linear distance falloff: F = F_max × (1 − r/R)  (PHYSICS-MATH-BOOK.md Section 1b)
        // Direction updates every frame, so diagonal/arced pushes create smooth parabolic
        // paths as gravity combines with the continuously-updated push vector.
        float r = Mathf.Max(distance, minDistance);
        float anchorBonus = Mathf.Clamp(Mathf.Log10(Mathf.Max(1f, currentTargetRigidbody.mass)), 0f, 1f);
        float effectiveRange = maxRange * (1f + anchorBonus * 0.5f);
        distMult = inverseDistanceScaling ? Mathf.Clamp01(1f - r / effectiveRange) : 1f;
        return true;
    }

    /// <summary>
    /// Applied every frame while the push key is held. Uses per-frame scaling
    /// (Time.deltaTime / pushCooldown) so the net impulse over time matches the
    /// old pulsed system, but produces smooth parabolic arcs instead of saw-tooth.
    /// The push direction naturally handles any angle — vertical, diagonal, horizontal —
    /// and gravity combines with it each frame to create the correct arc trajectory.
    /// </summary>
    void ApplyPushForce()
    {
        if (!GetPushParams(out Vector3 pushDir, out float distMult, out bool anchored)) return;

        float flare      = CurrentFlareMultiplier;
        // Scale per-frame velocity delta: same total impulse/second as old per-cooldown system
        float frameScale = Time.deltaTime / pushCooldown;

        if (anchored)
        {
            float recoilMag = Mathf.Min(pushSpeed * flare * distMult, maxRecoilSpeed * flare);
            playerRigidbody.AddForce(-pushDir * recoilMag * frameScale, ForceMode.VelocityChange);
        }
        else
        {
            float playerMass = playerRigidbody.mass;
            float targetMass = currentTargetRigidbody.mass;
            float totalMass  = playerMass + targetMass;
            float pushMag    = loosePushForce * flare * distMult;

            float targetV = Mathf.Min(pushMag * (playerMass / totalMass), loosePushForce * 3f);
            float playerV = Mathf.Min(pushMag * (targetMass / totalMass), maxRecoilSpeed);

            currentTargetRigidbody.AddForce(pushDir  * targetV * frameScale, ForceMode.VelocityChange);
            playerRigidbody.AddForce(-pushDir * playerV * frameScale, ForceMode.VelocityChange);
        }
    }

    /// <summary>
    /// Throttled effects (sound, camera shake, trail, disarm check).
    /// Called on cooldown timer — does NOT apply physics force.
    /// </summary>
    void PushEffects()
    {
        if (!GetPushParams(out Vector3 pushDir, out float distMult, out bool anchored)) return;

        CameraShakeManager.Instance?.Shake(shakeDuration, shakeMagnitude);
        SoundManager.Instance?.PlayPushSound();

        Vector3 chestPos = chestTransform != null ? chestTransform.position : transform.position;
        PushPullTrail.Instance?.ShowPushTrail(chestPos, currentTargetRigidbody.worldCenterOfMass);

        // Disarm check — if we just pushed an enemy's held weapon, rip it from their hand
        float appliedForce = anchored
            ? Mathf.Min(pushSpeed * CurrentFlareMultiplier, maxRecoilSpeed * CurrentFlareMultiplier)
            : loosePushForce * CurrentFlareMultiplier;
        HeldWeaponMarker marker = currentTargetRigidbody.GetComponentInChildren<HeldWeaponMarker>()
                               ?? currentTargetRigidbody.GetComponent<HeldWeaponMarker>();
        marker?.TryDisarm(appliedForce);
    }

    void PushMetalsInBubble()
    {
        float     flareMult   = CurrentFlareMultiplier;
        LayerMask targetLayer = metalLayer != 0 ? metalLayer : LayerMask.GetMask("Metal");
        Collider[] hits       = Physics.OverlapSphere(transform.position, steelBubbleRadius * flareMult, targetLayer);

        foreach (var hit in hits)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb != null && rb != playerRigidbody)
                rb.AddForce((rb.position - transform.position).normalized * steelBubbleForce * flareMult, ForceMode.Impulse);
        }

        CameraShakeManager.Instance?.Shake(shakeDuration * flareMult, shakeMagnitude * flareMult);
    }

    void StartFlaringVignette()
    {
        if (vignetteCoroutine != null) StopCoroutine(vignetteCoroutine);
        vignetteCoroutine = StartCoroutine(PulseVignette());
    }

    IEnumerator PulseVignette()
    {
        if (vignetteImage == null) yield break;
        float elapsed = 0f;
        while (elapsed < vignettePulseDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(vignetteMaxAlpha, 0f, elapsed / vignettePulseDuration);
            vignetteImage.color = new Color(flaringColor.r, flaringColor.g, flaringColor.b, alpha);
            yield return null;
        }
        vignetteImage.color = Color.clear;
    }

    void UpdateCrosshairColor()
    {
        if (crosshairImage == null) return;
        crosshairImage.color = metalInRange ? metalInRangeColor : noMetalColor;
    }
}
