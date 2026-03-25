using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;

/// <summary>
/// Implements the Tin Allomancy ability (enhanced senses).
/// Lore: Tin enhances ALL five senses — sight, hearing, touch, smell, taste.
/// Allows seeing through mist/darkness, hearing distant sounds, feeling vibrations,
/// detecting scents, and identifying metals by taste. Risks sensory overload.
/// </summary>
public class Tin : MonoBehaviour
{
    // ── Sight ────────────────────────────────────────────────────────────────
    [Header("Sight Enhancement")]
    public float fovIncreaseBase = 5f;
    public float fovIncreaseMax = 20f;
    [Range(0, 1)] public float nightVisionIntensity = 0.5f;
    [Range(0, 1)] public float mistPiercingStrength = 0.7f;

    // ── Hearing ──────────────────────────────────────────────────────────────
    [Header("Hearing Enhancement")]
    public float audioVolumeMultiplierBase = 1.2f;
    public float audioVolumeMultiplierMax = 3.0f;
    public float hearingDetectionRange = 40f;
    public float footstepDetectionRange = 25f;

    // ── Touch ────────────────────────────────────────────────────────────────
    [Header("Touch Enhancement")]
    [Tooltip("Detect nearby metal objects through vibration sense")]
    public float touchDetectionRange = 15f;
    [Tooltip("Intensity of controller rumble / screen pulse for nearby metals")]
    [Range(0f, 1f)] public float vibrationIntensity = 0.3f;
    [Tooltip("Feel ground vibrations from movement (footsteps, impacts)")]
    public float groundVibrationRange = 20f;
    public float touchSensitivity = 1f;

    // ── Smell ────────────────────────────────────────────────────────────────
    [Header("Smell Enhancement")]
    [Tooltip("Range to detect enemies/NPCs by scent")]
    public float smellDetectionRange = 30f;
    [Tooltip("Range to detect specific items (metals, food, chemicals)")]
    public float itemSmellRange = 15f;
    public float scentTrailDuration = 5f;

    // ── Taste ────────────────────────────────────────────────────────────────
    [Header("Taste Enhancement")]
    [Tooltip("Identify metal type and purity when consuming")]
    public bool canIdentifyMetalPurity = true;
    [Tooltip("Detect poisons in consumed items")]
    public bool canDetectPoisons = true;
    [Tooltip("Range for airborne taste detection (smoke, ash, chemicals)")]
    public float airborneDetectionRange = 10f;

    // ── General ──────────────────────────────────────────────────────────────
    [Header("Settings")]
    public float baseMetalCostPerSecond = 1f;
    public float reflexSpeedBoost = 1.15f;

    [Header("Sensory Overload")]
    public float overloadRecoveryRate = 2f;
    public float visualOverloadThreshold = 0.8f;
    public float audioOverloadThreshold = 0.8f;
    public float touchOverloadThreshold = 0.7f;

    [Header("References")]
    public Camera playerCamera;
    public Allomancer allomancer;
    public Volume globalVolume;

    // ── Internal State ───────────────────────────────────────────────────────
    private bool isBurning = false;
    private float originalFOV;
    private float originalAudioVolume;
    private float currentOverloadVisual = 0f;
    private float currentOverloadAudio = 0f;
    private float currentOverloadTouch = 0f;

    // Detected entities (updated each frame while burning)
    private List<TinDetectedEntity> detectedBySmell = new List<TinDetectedEntity>();
    private List<TinDetectedEntity> detectedByHearing = new List<TinDetectedEntity>();
    private List<TinDetectedEntity> detectedByTouch = new List<TinDetectedEntity>();

    // References & Cache
    private BasicPlayerMove playerMove;
    private Vector3 originalCameraLocalPos;
    private float shakeIntensity = 0f;
    private AudioLowPassFilter lowPass;
    private AudioHighPassFilter highPass;
    private float vibrationPulseTimer = 0f;

    // HDRP Overrides
    private Exposure exposure;
    private Fog fog;
    private ColorAdjustments colorAdjustments;
    private Vignette vignette;

    // ── Data Structures ──────────────────────────────────────────────────────

    public struct TinDetectedEntity
    {
        public Transform transform;
        public float distance;
        public float intensity;
        public DetectionSense sense;
        public string description;
    }

    public enum DetectionSense { Sight, Hearing, Touch, Smell, Taste }

    // ── Public API ───────────────────────────────────────────────────────────

    public bool IsBurningTin => isBurning;
    public IReadOnlyList<TinDetectedEntity> SmellDetections => detectedBySmell;
    public IReadOnlyList<TinDetectedEntity> HearingDetections => detectedByHearing;
    public IReadOnlyList<TinDetectedEntity> TouchDetections => detectedByTouch;

    /// <summary>
    /// Identify a metal's type and purity via enhanced taste.
    /// Lore: Tin-enhanced taste can detect impurities and identify alloys.
    /// </summary>
    public MetalTasteResult TasteIdentifyMetal(AllomancySkill.MetalType metal, float purity = 1f)
    {
        if (!isBurning || !canIdentifyMetalPurity)
            return new MetalTasteResult { identified = false, description = "Senses not enhanced." };

        float flareMult = GetFlareMultiplier();
        float accuracy = Mathf.Clamp01(0.5f + flareMult * 0.25f);

        string desc = GetMetalTasteDescription(metal);
        bool isPure = purity > 0.95f;
        bool poisoned = purity < 0.3f;

        return new MetalTasteResult
        {
            identified = true,
            metalType = metal,
            purity = purity,
            accuracy = accuracy,
            isPure = isPure,
            isPoisoned = poisoned && canDetectPoisons,
            description = desc
        };
    }

    public struct MetalTasteResult
    {
        public bool identified;
        public AllomancySkill.MetalType metalType;
        public float purity;
        public float accuracy;
        public bool isPure;
        public bool isPoisoned;
        public string description;
    }

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (allomancer == null) allomancer = GetComponentInParent<Allomancer>();
        if (playerMove == null) playerMove = GetComponentInParent<BasicPlayerMove>();

        if (playerCamera != null)
        {
            originalFOV = playerCamera.fieldOfView;
            originalCameraLocalPos = playerCamera.transform.localPosition;
        }

        originalAudioVolume = AudioListener.volume;

        // Initialize HDRP Volume components
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out exposure);
            globalVolume.profile.TryGet(out fog);
            globalVolume.profile.TryGet(out colorAdjustments);
            globalVolume.profile.TryGet(out vignette);
        }

        // Audio filters on camera
        if (playerCamera != null)
        {
            lowPass = playerCamera.GetComponent<AudioLowPassFilter>();
            if (lowPass == null) lowPass = playerCamera.gameObject.AddComponent<AudioLowPassFilter>();
            lowPass.enabled = false;

            highPass = playerCamera.GetComponent<AudioHighPassFilter>();
            if (highPass == null) highPass = playerCamera.gameObject.AddComponent<AudioHighPassFilter>();
            highPass.enabled = false;
        }
    }

    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning()
                 && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Tin;

        if (isBurning)
        {
            float flareMult = GetFlareMultiplier();

            // All five senses
            ApplySightEnhancement(flareMult);
            ApplyHearingEnhancement(flareMult);
            ApplyTouchEnhancement(flareMult);
            ApplySmellEnhancement(flareMult);
            ApplyAirborneDetection(flareMult);

            // Reflex boost if not overloaded
            if (playerMove != null && GetTotalOverload() < 0.2f)
                playerMove.externalSpeedMultiplier = reflexSpeedBoost;

            HandleSensoryOverload(flareMult);
            DrainMetal(flareMult);
        }
        else if (wasBurning)
        {
            ResetAllEffects();
        }

        // Recover from overload over time
        float recoveryScale = isBurning ? 0.5f : 1.5f;
        if (currentOverloadVisual > 0) currentOverloadVisual -= overloadRecoveryRate * recoveryScale * Time.deltaTime;
        if (currentOverloadAudio > 0) currentOverloadAudio -= overloadRecoveryRate * recoveryScale * Time.deltaTime;
        if (currentOverloadTouch > 0) currentOverloadTouch -= overloadRecoveryRate * recoveryScale * Time.deltaTime;

        ApplyOverloadVisuals();
        ApplyOverloadAudio();
        ApplyPhysicalOverload();
    }

    // ── Sense 1: Sight ───────────────────────────────────────────────────────

    void ApplySightEnhancement(float flareMult)
    {
        // FOV increase
        if (playerCamera != null)
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView,
                originalFOV + (fovIncreaseBase * flareMult), Time.deltaTime * 5f);

        // Night vision via HDRP exposure
        if (exposure != null)
            exposure.compensation.value = nightVisionIntensity * flareMult;

        // Mist piercing via fog reduction
        if (fog != null)
            fog.meanFreePath.value = 100f * (1f + mistPiercingStrength * flareMult);

        // Enhanced contrast and saturation
        if (colorAdjustments != null)
        {
            colorAdjustments.contrast.value = 10f * flareMult;
            colorAdjustments.saturation.value = 5f * flareMult;
        }
    }

    // ── Sense 2: Hearing ─────────────────────────────────────────────────────

    void ApplyHearingEnhancement(float flareMult)
    {
        // Volume amplification
        AudioListener.volume = originalAudioVolume *
            Mathf.Lerp(audioVolumeMultiplierBase, audioVolumeMultiplierMax, (flareMult - 1f) / 1.5f);

        // Detect entities by sound (footsteps, breathing, clanking armor)
        detectedByHearing.Clear();
        float effectiveRange = hearingDetectionRange * flareMult;

        Collider[] nearby = Physics.OverlapSphere(transform.position, effectiveRange);
        foreach (var col in nearby)
        {
            if (col.transform == transform) continue;

            // Detect enemies
            EnemyAI enemy = col.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                float intensity = 1f - (dist / effectiveRange);
                detectedByHearing.Add(new TinDetectedEntity
                {
                    transform = col.transform,
                    distance = dist,
                    intensity = intensity,
                    sense = DetectionSense.Hearing,
                    description = GetHearingDescription(enemy, dist)
                });
            }
        }
    }

    // ── Sense 3: Touch ───────────────────────────────────────────────────────

    void ApplyTouchEnhancement(float flareMult)
    {
        detectedByTouch.Clear();
        float effectiveRange = touchDetectionRange * flareMult;

        // Detect nearby metal objects via vibration sense
        LayerMask metalMask = LayerMask.GetMask("Metal");
        Collider[] metals = Physics.OverlapSphere(transform.position, effectiveRange, metalMask);

        float closestMetalDist = float.MaxValue;

        foreach (var col in metals)
        {
            if (col.transform == transform) continue;
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < closestMetalDist) closestMetalDist = dist;

            AllomanticTarget target = col.GetComponent<AllomanticTarget>();
            detectedByTouch.Add(new TinDetectedEntity
            {
                transform = col.transform,
                distance = dist,
                intensity = 1f - (dist / effectiveRange),
                sense = DetectionSense.Touch,
                description = target != null
                    ? $"Metal vibration: {target.metalType} ({dist:F1}m)"
                    : $"Metal vibration ({dist:F1}m)"
            });
        }

        // Detect ground vibrations from nearby moving entities
        float vibRange = groundVibrationRange * flareMult;
        Collider[] entities = Physics.OverlapSphere(transform.position, vibRange);
        foreach (var col in entities)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb == null || col.transform == transform) continue;
            if (rb.linearVelocity.magnitude < 0.5f) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            float vibIntensity = (rb.linearVelocity.magnitude / 10f) * (1f - dist / vibRange) * touchSensitivity;

            if (vibIntensity > 0.1f)
            {
                detectedByTouch.Add(new TinDetectedEntity
                {
                    transform = col.transform,
                    distance = dist,
                    intensity = vibIntensity,
                    sense = DetectionSense.Touch,
                    description = $"Ground vibration ({dist:F1}m, strength {vibIntensity:F2})"
                });
            }
        }

        // Pulse visual feedback based on closest metal proximity
        if (closestMetalDist < effectiveRange)
        {
            vibrationPulseTimer += Time.deltaTime * (1f + (1f - closestMetalDist / effectiveRange) * 3f);
            float pulse = Mathf.Sin(vibrationPulseTimer * Mathf.PI * 2f) * 0.5f + 0.5f;
            float pulseIntensity = vibrationIntensity * pulse * (1f - closestMetalDist / effectiveRange);

            // Subtle camera micro-shake for touch feedback
            if (playerCamera != null && pulseIntensity > 0.05f)
            {
                CameraShakeManager.Instance?.Shake(0.05f, pulseIntensity * 0.02f);
            }
        }

        // Touch overload from intense nearby vibrations
        if (metals.Length > 5)
        {
            currentOverloadTouch = Mathf.Clamp01(currentOverloadTouch +
                metals.Length * 0.02f * flareMult * Time.deltaTime);
        }
    }

    // ── Sense 4: Smell ───────────────────────────────────────────────────────

    void ApplySmellEnhancement(float flareMult)
    {
        detectedBySmell.Clear();
        float effectiveRange = smellDetectionRange * flareMult;

        Collider[] nearby = Physics.OverlapSphere(transform.position, effectiveRange);
        foreach (var col in nearby)
        {
            if (col.transform == transform) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            float intensity = 1f - (dist / effectiveRange);

            // Detect enemies by scent
            EnemyAI enemy = col.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                detectedBySmell.Add(new TinDetectedEntity
                {
                    transform = col.transform,
                    distance = dist,
                    intensity = intensity,
                    sense = DetectionSense.Smell,
                    description = GetSmellDescription(enemy.enemyType, dist)
                });
                continue;
            }

            // Detect items by scent
            AllomanticTarget metalItem = col.GetComponent<AllomanticTarget>();
            if (metalItem != null && dist < itemSmellRange * flareMult)
            {
                detectedBySmell.Add(new TinDetectedEntity
                {
                    transform = col.transform,
                    distance = dist,
                    intensity = intensity,
                    sense = DetectionSense.Smell,
                    description = $"Metallic scent ({dist:F1}m)"
                });
            }
        }
    }

    // ── Sense 5: Taste (Airborne) ────────────────────────────────────────────

    void ApplyAirborneDetection(float flareMult)
    {
        // Taste particles in the air: ash, smoke, chemicals, mist composition
        // This is passive — detects environmental conditions
        float effectiveRange = airborneDetectionRange * flareMult;

        // Detect environmental hazards in range
        Collider[] hazards = Physics.OverlapSphere(transform.position, effectiveRange);
        foreach (var col in hazards)
        {
            if (col.CompareTag("Hazard") || col.CompareTag("Poison"))
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                detectedBySmell.Add(new TinDetectedEntity
                {
                    transform = col.transform,
                    distance = dist,
                    intensity = 1f - (dist / effectiveRange),
                    sense = DetectionSense.Taste,
                    description = col.CompareTag("Poison")
                        ? $"Bitter poison taste in the air ({dist:F1}m)"
                        : $"Acrid hazard detected ({dist:F1}m)"
                });
            }
        }
    }

    // ── Reset ────────────────────────────────────────────────────────────────

    void ResetAllEffects()
    {
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = originalFOV;
            playerCamera.transform.localPosition = originalCameraLocalPos;
        }

        AudioListener.volume = originalAudioVolume;
        if (playerMove != null) playerMove.externalSpeedMultiplier = 1f;

        if (lowPass != null) lowPass.enabled = false;
        if (highPass != null) highPass.enabled = false;

        if (exposure != null) exposure.compensation.value = 0;
        if (fog != null) fog.meanFreePath.value = 100f;
        if (colorAdjustments != null)
        {
            colorAdjustments.contrast.value = 0;
            colorAdjustments.saturation.value = 0;
        }

        detectedBySmell.Clear();
        detectedByHearing.Clear();
        detectedByTouch.Clear();
    }

    // ── Sensory Overload ─────────────────────────────────────────────────────

    void HandleSensoryOverload(float flareMult)
    {
        foreach (var source in SensorySource.ActiveSources)
        {
            if (source == null) continue;
            float dist = Vector3.Distance(transform.position, source.transform.position);
            if (dist < source.radius)
            {
                float falloff = source.falloff > 0
                    ? Mathf.Pow(1f - (dist / source.radius), source.falloff)
                    : (1f - (dist / source.radius));
                float intensity = falloff * source.intensity * flareMult;

                if (source.type == SensorySource.SourceType.BrightLight)
                    currentOverloadVisual = Mathf.Clamp01(currentOverloadVisual + intensity * Time.deltaTime * 5f);
                else
                    currentOverloadAudio = Mathf.Clamp01(currentOverloadAudio + intensity * Time.deltaTime * 5f);
            }
        }
    }

    void ApplyOverloadVisuals()
    {
        if (vignette != null)
        {
            vignette.intensity.value = currentOverloadVisual * 0.6f;
            vignette.color.value = Color.white;
        }

        if (exposure != null && currentOverloadVisual > 0.1f)
        {
            exposure.compensation.value = currentOverloadVisual * 8f;
        }

        float maxOverload = Mathf.Max(currentOverloadVisual, currentOverloadAudio, currentOverloadTouch);
        if (maxOverload > 0.5f)
        {
            shakeIntensity = maxOverload * 0.1f;
            CameraShakeManager.Instance?.Shake(0.1f, shakeIntensity);
        }
        else if (playerCamera != null)
        {
            if (CameraShakeManager.Instance == null)
                playerCamera.transform.localPosition = Vector3.Lerp(
                    playerCamera.transform.localPosition, originalCameraLocalPos, Time.deltaTime * 5f);
        }
    }

    void ApplyOverloadAudio()
    {
        if (lowPass == null || highPass == null) return;

        if (currentOverloadAudio > 0.1f)
        {
            lowPass.enabled = true;
            highPass.enabled = true;
            lowPass.cutoffFrequency = Mathf.Lerp(22000, 800, currentOverloadAudio);
            highPass.cutoffFrequency = Mathf.Lerp(10, 4000, currentOverloadAudio);
        }
        else
        {
            lowPass.enabled = false;
            highPass.enabled = false;
        }
    }

    void ApplyPhysicalOverload()
    {
        if (playerMove == null) return;

        float totalOverload = GetTotalOverload();
        if (totalOverload > 0.3f)
        {
            playerMove.externalSpeedMultiplier = Mathf.Lerp(reflexSpeedBoost, 0.4f, (totalOverload - 0.3f) / 0.7f);

            if (totalOverload > 0.7f && playerCamera != null)
            {
                float tilt = Mathf.Sin(Time.time * 2f) * (totalOverload * 5f);
                playerCamera.transform.localRotation = Quaternion.Euler(0, 0, tilt);
            }

            if (totalOverload > 0.9f)
            {
                if (Mathf.Sin(Time.time * 8f) > 0.5f)
                    playerMove.externalSpeedMultiplier *= 0.1f;
            }
        }
        else if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Lerp(
                playerCamera.transform.localRotation, Quaternion.identity, Time.deltaTime * 3f);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    float GetFlareMultiplier()
    {
        return (FlareManager.Instance != null) ? FlareManager.Instance.FlareMultiplier : 1.0f;
    }

    float GetTotalOverload()
    {
        return Mathf.Clamp01(currentOverloadVisual + currentOverloadAudio + currentOverloadTouch);
    }

    void DrainMetal(float flareMult)
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(AllomancySkill.MetalType.Tin, baseMetalCostPerSecond * flareMult * Time.deltaTime);
    }

    string GetHearingDescription(EnemyAI enemy, float dist)
    {
        switch (enemy.enemyType)
        {
            case EnemyAI.EnemyType.Guard:
                return dist < 10f ? "Clanking armor and steady breathing" : "Distant metallic footsteps";
            case EnemyAI.EnemyType.Koloss:
                return dist < 15f ? "Heavy thundering footsteps and grunting" : "Distant ground tremors";
            case EnemyAI.EnemyType.SteelInquisitor:
                return dist < 10f ? "Eerie scraping of metal spikes" : "Faint unsettling metallic hum";
            case EnemyAI.EnemyType.Coinshot:
                return dist < 10f ? "Jingling coins and quick breathing" : "Faint coin rattling";
            case EnemyAI.EnemyType.Mistwraith:
                return dist < 10f ? "Wet squelching and bone scraping" : "Faint slithering sounds";
            default:
                return dist < 10f ? "Footsteps and breathing" : "Distant movement";
        }
    }

    string GetSmellDescription(EnemyAI.EnemyType type, float dist)
    {
        switch (type)
        {
            case EnemyAI.EnemyType.Guard:
                return $"Sweat, leather, and oil ({dist:F1}m)";
            case EnemyAI.EnemyType.Koloss:
                return $"Overpowering stench of blood and unwashed skin ({dist:F1}m)";
            case EnemyAI.EnemyType.SteelInquisitor:
                return $"Cold metal and something deeply wrong ({dist:F1}m)";
            case EnemyAI.EnemyType.Coinshot:
                return $"Metal polish and nervousness ({dist:F1}m)";
            case EnemyAI.EnemyType.Mistwraith:
                return $"Decay and damp earth ({dist:F1}m)";
            case EnemyAI.EnemyType.NobleGuard:
                return $"Perfume and polished steel ({dist:F1}m)";
            default:
                return $"Living presence ({dist:F1}m)";
        }
    }

    string GetMetalTasteDescription(AllomancySkill.MetalType metal)
    {
        switch (metal)
        {
            case AllomancySkill.MetalType.Steel: return "Sharp, biting tang with a hint of carbon";
            case AllomancySkill.MetalType.Iron: return "Heavy, earthy mineral taste with metallic weight";
            case AllomancySkill.MetalType.Pewter: return "Warm, thick coating that numbs the tongue";
            case AllomancySkill.MetalType.Tin: return "Bright, electric tingle that sharpens the palate";
            case AllomancySkill.MetalType.Zinc: return "Sour, acidic bite that makes the mouth water";
            case AllomancySkill.MetalType.Brass: return "Smooth, warm sweetness with a copper undertone";
            case AllomancySkill.MetalType.Copper: return "Pennies and earth, slightly astringent";
            case AllomancySkill.MetalType.Bronze: return "Rich, complex tang like aged wine and metal";
            case AllomancySkill.MetalType.Atium: return "Cold, crystalline clarity — unlike anything mortal";
            case AllomancySkill.MetalType.Gold: return "Soft, noble warmth that coats the mouth";
            case AllomancySkill.MetalType.Electrum: return "Quicksilver flash, simultaneously sweet and metallic";
            case AllomancySkill.MetalType.Aluminum: return "Nothing — a startling absence of taste";
            case AllomancySkill.MetalType.Duralumin: return "Intense burning, like swallowing lightning";
            case AllomancySkill.MetalType.Bendalloy: return "Stretchy, elastic sensation that lingers";
            case AllomancySkill.MetalType.Cadmium: return "Thick, viscous slowness coating the tongue";
            case AllomancySkill.MetalType.Chromium: return "Hollow, consuming void that swallows flavor";
            case AllomancySkill.MetalType.Nicrosil: return "Amplifying resonance, every taste magnified";
            default: return "Unfamiliar metallic taste";
        }
    }
}
