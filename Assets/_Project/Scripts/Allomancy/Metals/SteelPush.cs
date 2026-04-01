/* SteelPush.cs
 *
 * PURPOSE:
 * Implements the Steel Allomancy ability (Coinshot) – push metal objects away.
 *
 * TARGETING LINE:
 * The targeting line is owned by MetalLineRenderer, not this script.
 * SteelPush just handles push physics and targeting logic.
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[PlayerComponent("Allomancy Metals", order: 10)]
public class SteelPush : MonoBehaviour
{
    private float CurrentFlareMultiplier =>
        FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

    private bool IsBurning =>
        FlareManager.Instance != null && FlareManager.Instance.IsBurning;

    [Header("Push Settings")]
    public float minDistance        = 1f;
    public float maxRange           = 30f;
    public float metalCostPerSecond = 0.5f;
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
    public Allomancer    allomancer;
    public Rigidbody     playerRigidbody;
    public Transform     chestTransform;
    public MetalSelector metalSelector;

    [Header("Allomantic Sight")]
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

    private AllomanticTarget currentTarget;
    private Rigidbody        currentTargetRigidbody;
    private bool             hasCurrentTarget = false;
    private bool             metalInRange     = false;

    private Coroutine vignetteCoroutine;

    private KeyCode GetAbility1Key()
    {
        if (metalSelector != null)
        {
            if (metalSelector.GetPrimaryMetal()   == AllomancySkill.MetalType.Steel) return Keybinds.Ability1;
            if (metalSelector.GetSecondaryMetal() == AllomancySkill.MetalType.Steel) return Keybinds.Ability2;
            return KeyCode.None;
        }
        return Keybinds.SteelPush;
    }

    private KeyCode GetAbility2Key()
    {
        if (metalSelector != null)
        {
            if (metalSelector.GetPrimaryMetal()   == AllomancySkill.MetalType.Steel) return Keybinds.Ability3;
            if (metalSelector.GetSecondaryMetal() == AllomancySkill.MetalType.Steel) return Keybinds.Ability4;
            return KeyCode.None;
        }
        return Keybinds.Ability3;
    }

    void Start()
    {
        if (playerRigidbody   == null) playerRigidbody   = GetComponentInParent<Rigidbody>();
        if (playerCamera      == null) playerCamera      = Camera.main;
        if (allomancer        == null) allomancer        = GetComponentInParent<Allomancer>();
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

        KeyCode pushKey = GetAbility1Key();
        if (pushKey != KeyCode.None && Input.GetKeyDown(pushKey) && cooldownTimer <= 0f)
        {
            if (IsBurning && hasCurrentTarget
                && allomancer != null && allomancer.GetMetalReserve(AllomancySkill.MetalType.Steel) > 0)
            {
                PushMetals();
                allomancer.DrainMetal(AllomancySkill.MetalType.Steel, metalCostPerSecond);
                cooldownTimer = pushCooldown;
                StartFlaringVignette();
            }
        }

        KeyCode bubbleKey = steelBubbleKey;
        if (enableSteelBubble && bubbleKey != KeyCode.None
            && Input.GetKeyDown(bubbleKey) && IsBurning && steelBubbleCooldownTimer <= 0f)
        {
            if (allomancer != null && allomancer.GetMetalReserve(AllomancySkill.MetalType.Steel) > 0)
            {
                PushMetalsInBubble();
                allomancer.DrainMetal(AllomancySkill.MetalType.Steel,
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
                currentTarget          = mlrRb.GetComponentInParent<AllomanticTarget>();
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

            AllomanticTarget at = hit.GetComponentInParent<AllomanticTarget>();
            if (at != null && !at.canBePushed) continue;

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
                metalInRange           = true;
            }
        }
    }

    void PushMetals()
    {
        if (playerRigidbody == null || !hasCurrentTarget || currentTargetRigidbody == null) return;

        Rigidbody        targetRb = currentTargetRigidbody;
        AllomanticTarget target   = currentTarget;

        if (targetRb == playerRigidbody)           return;
        if (target != null && !target.canBePushed) return;

        float   distance      = Vector3.Distance(playerRigidbody.position, targetRb.position);
        Vector3 pushDirection = (targetRb.position - playerRigidbody.position).normalized;
        bool    isAnchored    = (target != null && target.isAnchored) || targetRb.isKinematic;

        float flare        = CurrentFlareMultiplier;
        float distanceMult = inverseDistanceScaling
            ? Mathf.Clamp(maxRange / Mathf.Max(distance, minDistance), 0.5f, 3f) : 1f;

        if (isAnchored)
        {
            Vector3 recoil = -pushDirection * Mathf.Min(pushSpeed * flare * distanceMult, maxRecoilSpeed * flare);
            playerRigidbody.AddForce(recoil, ForceMode.VelocityChange);
        }
        else
        {
            float playerMass = playerRigidbody.mass;
            float targetMass = targetRb.mass;
            float totalMass  = playerMass + targetMass;
            float pushMag    = loosePushForce * flare * distanceMult;

            targetRb.AddForce(pushDirection * Mathf.Min(pushMag * (playerMass / totalMass), loosePushForce * 3f), ForceMode.VelocityChange);
            playerRigidbody.AddForce(-pushDirection * Mathf.Min(pushMag * (targetMass / totalMass), maxRecoilSpeed), ForceMode.VelocityChange);
        }

        CameraShakeManager.Instance?.Shake(shakeDuration, shakeMagnitude);
        SoundManager.Instance?.PlayPushSound();

        Vector3 chestPos = chestTransform != null ? chestTransform.position : transform.position;
        PushPullTrail.Instance?.ShowPushTrail(chestPos, targetRb.position);

        // Disarm check — if we just pushed an enemy's held weapon, rip it from their hand
        float appliedForce = isAnchored
            ? Mathf.Min(pushSpeed * CurrentFlareMultiplier, maxRecoilSpeed * CurrentFlareMultiplier)
            : loosePushForce * CurrentFlareMultiplier;
        HeldWeaponMarker marker = targetRb.GetComponentInChildren<HeldWeaponMarker>()
                               ?? targetRb.GetComponent<HeldWeaponMarker>();
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
