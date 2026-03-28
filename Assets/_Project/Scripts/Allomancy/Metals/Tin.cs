///
/// [TIN ALLOMANCY — LORE-ACCURATE IMPLEMENTATION]
///
/// Canon (Brandon Sanderson, Mistborn Era 1):
///   - Enhances all five senses simultaneously: sight, hearing, smell, touch, taste
///   - Sight:   longer range, detail at distance, see through mist, detect motion
///   - Hearing: hear whispers at range, footsteps through walls, heartbeats
///   - Smell:   track people by scent (implemented as directional 3D audio ping)
///   - Touch:   feel vibrations from footsteps through ground (subtle camera pulse)
///   - Taste:   not game-relevant; omitted
///   - Flaring: super-amplifies senses, higher metal drain, opens heartbeat cues
///   - PAIN: bright lights and loud sounds actively hurt while burning Tin
///   - Runout: world goes "dull and flat" — desaturation + brief low-pass on depletion
///   - Effects END IMMEDIATELY when burning stops (no lingering enhancement, but
///     overload pain CAN linger — the trauma remains even after the metal is gone)
///
/// BURN GATE:  Allomancer.IsBurning() AND current metal == Tin  (B key toggle)
/// FLARE GATE: FlareManager.FlareMultiplier >= flaringThreshold (scroll wheel)
///

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

/// <summary>
/// Tin Allomancy — enhances all five senses with lore-accurate benefits and costs.
/// </summary>
public class Tin : MonoBehaviour
{
    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>Operational state of this Tineye's burn.</summary>
    public enum TinState { Off, Burning, Flaring }

    /// <summary>
    /// Current burn state. Readable by HUD, animations, and other systems.
    /// </summary>
    public TinState CurrentState { get; private set; } = TinState.Off;

    // ── Flare Threshold ───────────────────────────────────────────────────────

    [Header("Flare Settings")]
    [SerializeField]
    [Tooltip("FlareMultiplier value at which Burning transitions to Flaring. " +
             "Roughly maps to intensity 7/10 with the default FlareManager config.")]
    [Range(1.5f, 3f)]
    private float flaringThreshold = 2.0f;

    // ── Vision ────────────────────────────────────────────────────────────────

    [Header("Vision — Sight Enhancement")]
    [SerializeField]
    [Tooltip("Degrees of FOV reduction while burning. Lore: focused perception, " +
             "detail at distance (binocular-like effect). Subtle — keep below 8.")]
    [Range(0f, 8f)]
    private float fovFocusDegrees = 3f;

    [SerializeField]
    [Tooltip("Additional FOV reduction when Flaring. Intense focus on distant detail.")]
    [Range(0f, 8f)]
    private float fovFlaringExtra = 4f;

    [SerializeField]
    [Tooltip("Extra units added to camera far clip plane while burning Tin. " +
             "Lore: Tineyes can see much farther than normal humans.")]
    [Range(0f, 2000f)]
    public float farClipBonus = 600f;

    [SerializeField]
    [Tooltip("HDRP exposure compensation while burning (night vision / low-light). " +
             "0 = no night vision, 1 = strong. Lore: can see in low light but not pitch dark.")]
    [Range(0f, 1f)]
    private float nightVisionIntensity = 0.45f;

    [SerializeField]
    [Tooltip("How much Tin reduces in-world mist/fog. " +
             "Lore: Tineyes see through the mists better than anyone.")]
    [Range(0f, 1f)]
    private float mistPiercingStrength = 0.65f;

    // ── Audio ─────────────────────────────────────────────────────────────────

    [Header("Audio — Hearing Enhancement")]
    [SerializeField]
    [Tooltip("AudioListener.volume multiplier at minimum burn (intensity 1). " +
             "Lore: sounds become clearer and more detailed.")]
    [Range(1f, 2.5f)]
    private float audioVolumeBase = 1.3f;

    [SerializeField]
    [Tooltip("AudioListener.volume multiplier at max flare. " +
             "Lore: can hear whispers across a room, heartbeats of nearby guards.")]
    [Range(1.5f, 4f)]
    private float audioVolumeFlare = 2.4f;

    // ── Tin Vignette ──────────────────────────────────────────────────────────

    [Header("Vignette — Tin Active Indicator")]
    [SerializeField]
    [Tooltip("Intensity of the blue-silver vignette while burning normally. " +
             "Lore: characters describe heightened senses as the world becoming 'more real'.")]
    [Range(0f, 0.35f)]
    private float tinVignetteIntensity = 0.11f;

    [SerializeField]
    [Tooltip("Vignette intensity while Flaring — stronger silver ring around the screen.")]
    [Range(0f, 0.5f)]
    private float tinVignetteFlaringIntensity = 0.20f;

    [SerializeField]
    [Tooltip("The blue-silver color of the Tin vignette. Default is a cold silver-blue.")]
    private Color tinVignetteColor = new Color(0.65f, 0.82f, 1.0f);

    // ── Reflexes ──────────────────────────────────────────────────────────────

    [Header("Reflexes")]
    [SerializeField]
    [Tooltip("Movement speed multiplier from heightened reflexes when NOT overloaded. " +
             "Lore: Spook's tin-savant reflexes let him dodge faster than normal.")]
    [Range(1f, 1.25f)]
    private float reflexSpeedBoost = 1.10f;

    // ── Scent Detection (Abstract Smell) ──────────────────────────────────────

    [Header("Scent Detection — Abstract Smell")]
    [SerializeField]
    [Tooltip("Radius within which Tin burning detects nearby humans by scent. " +
             "Lore: Vin tracks guards through walls by their sweat and fear-smell.")]
    [Range(5f, 40f)]
    private float scentRadius = 22f;

    [SerializeField]
    [Tooltip("How often a scent scan fires in seconds. " +
             "Shorter = more responsive but more audio pings.")]
    [Range(0.5f, 3f)]
    private float scentPingInterval = 1.5f;

    [SerializeField]
    [Tooltip("3D audio clip played at each enemy position on scent detection. " +
             "Assign a short, subtle clip. The player hears WHERE the enemy is — through walls. " +
             "If null, scent detection is silent (visual only in editor gizmos).")]
    private AudioClip scentPingClip;

    [SerializeField]
    [Tooltip("Volume of the scent ping. Very low — this is a subtle awareness cue.")]
    [Range(0f, 0.4f)]
    private float scentPingVolume = 0.10f;

    // ── Vibration Detection (Abstract Touch) ─────────────────────────────────

    [Header("Vibration Detection — Abstract Touch")]
    [SerializeField]
    [Tooltip("Radius within which footstep vibrations travel through the ground. " +
             "Lore: Tineyes feel enemy footsteps as tremors through stone floors.")]
    [Range(3f, 25f)]
    private float vibrationRadius = 14f;

    [SerializeField]
    [Tooltip("Minimum enemy NavMesh speed (m/s) to produce a sensed vibration. " +
             "Guards standing still produce no vibration — only moving enemies do.")]
    [Range(0.1f, 2f)]
    private float vibrationSpeedThreshold = 0.35f;

    [SerializeField]
    [Tooltip("Camera shake magnitude per unit of enemy speed. Keep very low (0.005–0.02).")]
    [Range(0f, 0.04f)]
    private float vibrationShakeMagnitude = 0.008f;

    // ── Sensory Overload ──────────────────────────────────────────────────────

    [Header("Sensory Overload — The Double-Edged Sword")]
    [SerializeField]
    [Tooltip("Rate at which overload decays per second when not being stimulated. " +
             "Recovery is slower while still burning Tin (0.5x) — the input keeps coming.")]
    private float overloadRecoveryRate = 2.5f;

    [SerializeField]
    [Tooltip("Visual overload level (0-1) at which movement penalties begin.")]
    [Range(0f, 1f)]
    private float overloadImpairThreshold = 0.3f;

    // ── Metal Cost ────────────────────────────────────────────────────────────

    [Header("Metal Cost")]
    [SerializeField]
    [Tooltip("Tin reserve drained per second at base burn (intensity 1). " +
             "Tin is one of the cheaper metals in the books — keep this low.")]
    private float baseMetalCostPerSecond = 0.9f;

    // ── References ────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField]
    [Tooltip("HDRP Global Volume for post-processing. Must be assigned in the Inspector.")]
    public Volume globalVolume;

    [SerializeField]
    [Tooltip("Player camera. Auto-found via Camera.main if null.")]
    public Camera playerCamera;

    [SerializeField]
    [Tooltip("Allomancer component. Auto-found in parent if null.")]
    public Allomancer allomancer;

    // ── Private State ─────────────────────────────────────────────────────────

    private BasicPlayerMove playerMove;

    // Camera and audio baselines (captured in Start)
    private float originalFOV;
    private float originalFarClip;
    private Vector3 originalCameraLocalPos;
    private float originalAudioVolume;

    // Overload accumulators (0-1)
    private float currentOverloadVisual = 0f;
    private float currentOverloadAudio  = 0f;

    // Scent ping timer
    private float scentTimer = 0f;

    // Set to true the frame Tin reserve hits zero — triggers WorldGoesDull on stop
    private bool metalRanOut = false;

    // Audio filters (on the camera)
    private AudioLowPassFilter  lowPass;
    private AudioHighPassFilter highPass;

    // HDRP Volume overrides (obtained from globalVolume.profile in Start)
    private Exposure         exposure;
    private Fog              fog;
    private ColorAdjustments colorAdjustments;
    private Vignette         vignette;

    // World-goes-dull coroutine handle
    private Coroutine worldGoesDullCoroutine;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    void Start()
    {
        // Auto-find references
        if (playerCamera == null) playerCamera = Camera.main;
        if (allomancer   == null) allomancer   = GetComponentInParent<Allomancer>();
        playerMove = GetComponentInParent<BasicPlayerMove>();

        // Capture baselines before any Tin effects are applied
        if (playerCamera != null)
        {
            originalFOV            = playerCamera.fieldOfView;
            originalFarClip        = playerCamera.farClipPlane;
            originalCameraLocalPos = playerCamera.transform.localPosition;

            // Ensure audio filters exist on the camera GameObject
            lowPass = playerCamera.GetComponent<AudioLowPassFilter>();
            if (lowPass == null) lowPass = playerCamera.gameObject.AddComponent<AudioLowPassFilter>();
            lowPass.enabled = false;

            highPass = playerCamera.GetComponent<AudioHighPassFilter>();
            if (highPass == null) highPass = playerCamera.gameObject.AddComponent<AudioHighPassFilter>();
            highPass.enabled = false;
        }

        originalAudioVolume = AudioListener.volume;

        // Obtain HDRP Volume overrides from the assigned global volume
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out exposure);
            globalVolume.profile.TryGet(out fog);
            globalVolume.profile.TryGet(out colorAdjustments);
            globalVolume.profile.TryGet(out vignette);
        }
    }

    void Update()
    {
        // ── Determine target TinState ──────────────────────────────────────

        float tinReserve = allomancer != null
            ? allomancer.GetMetalReserve(AllomancySkill.MetalType.Tin)
            : 0f;

        bool burningTin = allomancer != null
                       && allomancer.IsBurning()
                       && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Tin
                       && tinReserve > 0f;

        float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

        TinState targetState;
        if (!burningTin)
            targetState = TinState.Off;
        else if (flareMult >= flaringThreshold)
            targetState = TinState.Flaring;
        else
            targetState = TinState.Burning;

        // ── Detect reserve depletion this frame ───────────────────────────

        if (CurrentState != TinState.Off && tinReserve <= 0f)
            metalRanOut = true;

        // ── State transition hooks ─────────────────────────────────────────

        if (targetState != CurrentState)
        {
            if (targetState == TinState.Off)
            {
                OnStopBurning(metalRanOut);
                metalRanOut = false;
            }
            else if (CurrentState == TinState.Off)
            {
                OnStartBurning();
            }
            CurrentState = targetState;
        }

        // ── Per-state tick ─────────────────────────────────────────────────

        if (CurrentState != TinState.Off)
        {
            ApplyVisionEffects(flareMult);
            ApplyAudioVolumeEffect(flareMult);
            ApplyTinVignette();
            HandleSensoryOverload(flareMult);
            UpdateScentDetection();
            UpdateVibrationDetection();
            UpdateReflexSpeed();
            DrainMetal(flareMult);
        }

        // ── Overload recovery ──────────────────────────────────────────────
        // Recovers even while Off — lore: flash-blindness lingers after you stop burning
        float recoveryScale = CurrentState != TinState.Off ? 0.5f : 1.5f;
        currentOverloadVisual = Mathf.Max(0f, currentOverloadVisual - overloadRecoveryRate * recoveryScale * Time.deltaTime);
        currentOverloadAudio  = Mathf.Max(0f, currentOverloadAudio  - overloadRecoveryRate * recoveryScale * Time.deltaTime);

        // Overload effects applied every frame (pain lingers after stopping)
        ApplyOverloadVisuals();
        ApplyOverloadAudio();
        ApplyPhysicalOverload();

        // Reset speed multiplier when fully recovered and not burning
        if (CurrentState == TinState.Off
            && currentOverloadVisual < 0.05f
            && currentOverloadAudio  < 0.05f
            && playerMove != null
            && playerMove.externalSpeedMultiplier != 1f)
        {
            playerMove.externalSpeedMultiplier = 1f;
        }
    }

    // ── State Transition Hooks ────────────────────────────────────────────────

    /// <summary>Called once when transitioning from Off to Burning or Flaring.</summary>
    private void OnStartBurning()
    {
        // Extend far clip plane — Tineyes can see much farther
        if (playerCamera != null)
            playerCamera.farClipPlane = originalFarClip + farClipBonus;

        // Audio cue: a subtle sound signals the sharpening of senses
        SoundManager.Instance?.PlayFlareSound();
    }

    /// <summary>
    /// Called once when transitioning to Off.
    /// <param name="ranOut">True when the reserve hit zero (triggers WorldGoesDull).</param>
    /// </summary>
    private void OnStopBurning(bool ranOut)
    {
        // Immediately snap back to baselines — lore: effects stop when burning stops
        if (playerCamera != null)
        {
            playerCamera.fieldOfView  = originalFOV;
            playerCamera.farClipPlane = originalFarClip;
        }

        AudioListener.volume = originalAudioVolume;

        if (playerMove != null) playerMove.externalSpeedMultiplier = 1f;

        // Restore HDRP baselines
        if (exposure         != null) exposure.compensation.value         = 0f;
        if (fog              != null) fog.meanFreePath.value               = 100f;
        if (colorAdjustments != null)
        {
            colorAdjustments.contrast.value   = 0f;
            colorAdjustments.saturation.value = 0f;
        }
        // Don't zero vignette here — overload may still be active and will handle it
        if (vignette != null && currentOverloadVisual < 0.05f)
            vignette.intensity.value = 0f;

        if (lowPass  != null) lowPass.enabled  = false;
        if (highPass != null) highPass.enabled = false;

        // Lore-critical: when metal RUNS OUT, the contrast between Tin-vision and normal
        // vision is jarring. Trigger the "world goes dull" moment.
        if (ranOut)
        {
            if (worldGoesDullCoroutine != null) StopCoroutine(worldGoesDullCoroutine);
            worldGoesDullCoroutine = StartCoroutine(WorldGoesDullCoroutine());
        }
    }

    // ── Vision ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Applies FOV focus (slight decrease = detail at distance), night vision exposure,
    /// mist piercing via fog reduction, and contrast/sharpness boost.
    /// </summary>
    private void ApplyVisionEffects(float flareMult)
    {
        if (playerCamera == null) return;

        // Slight FOV decrease — focused perception, seeing detail at distance
        // Lore: not a wider view, but a MORE DETAILED one. Narrowing simulates this.
        float focusAmount = fovFocusDegrees + (CurrentState == TinState.Flaring ? fovFlaringExtra : 0f);
        float targetFOV   = originalFOV - focusAmount;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * 4f);

        // Night vision: HDRP exposure boost for low-light visibility
        // Only apply if overload isn't already controlling exposure
        if (exposure != null && currentOverloadVisual <= 0f)
            exposure.compensation.value = nightVisionIntensity * flareMult;

        // Mist piercing: reduce fog density
        if (fog != null)
            fog.meanFreePath.value = 100f * (1f + mistPiercingStrength * flareMult);

        // Subtle contrast and saturation: the world looks sharper, more vivid
        if (colorAdjustments != null)
        {
            float t = Mathf.Clamp01((flareMult - 1f) / 2.2f);
            colorAdjustments.contrast.value   = Mathf.Lerp(0f, 14f, t);
            colorAdjustments.saturation.value = Mathf.Lerp(0f,  9f, t);
        }
    }

    // ── Audio Volume ──────────────────────────────────────────────────────────

    /// <summary>
    /// Boosts AudioListener global volume to simulate enhanced hearing.
    /// Skipped if audio overload is currently active — pain controls audio then.
    /// </summary>
    private void ApplyAudioVolumeEffect(float flareMult)
    {
        // Overload handles audio during pain — don't fight it
        if (currentOverloadAudio > 0.25f) return;

        float t = Mathf.Clamp01((flareMult - 1f) / 2.2f);
        AudioListener.volume = originalAudioVolume * Mathf.Lerp(audioVolumeBase, audioVolumeFlare, t);
    }

    // ── Tin Vignette ──────────────────────────────────────────────────────────

    /// <summary>
    /// Draws the blue-silver vignette while burning. Yields to overload vignette when active.
    /// Lore: "The world seemed more real, more present — the mists less oppressive."
    /// </summary>
    private void ApplyTinVignette()
    {
        if (vignette == null || currentOverloadVisual > 0.05f) return;

        float targetIntensity = CurrentState == TinState.Flaring
            ? tinVignetteFlaringIntensity
            : tinVignetteIntensity;

        vignette.color.value     = tinVignetteColor;
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetIntensity, Time.deltaTime * 5f);
    }

    // ── Reflex Speed ──────────────────────────────────────────────────────────

    /// <summary>Applies subtle movement speed bonus when senses are sharp and not overloaded.</summary>
    private void UpdateReflexSpeed()
    {
        if (playerMove == null) return;

        float totalOverload = currentOverloadVisual + currentOverloadAudio;
        if (totalOverload < 0.2f)
            playerMove.externalSpeedMultiplier = reflexSpeedBoost;
        // Physical overload handles the penalty — don't double-apply
    }

    // ── Scent Detection — Abstract Smell ──────────────────────────────────────

    /// <summary>
    /// Performs a periodic proximity scan for nearby humans/enemies and plays a
    /// 3D spatialized audio ping at their location — even through walls.
    ///
    /// Lore: Tineyes smell sweat, metal, fear, and perfume. In-game this is
    /// abstracted as a directional audio awareness — you HEAR where enemies are,
    /// simulating the directional information that scent provides.
    ///
    /// If no scentPingClip is assigned, the scan still runs but produces no audio.
    /// </summary>
    private void UpdateScentDetection()
    {
        scentTimer -= Time.deltaTime;
        if (scentTimer > 0f) return;
        scentTimer = scentPingInterval;

        if (scentPingClip == null) return;

        foreach (var enemy in MistbornRegistry.ActiveEnemies)
        {
            if (enemy == null) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist > scentRadius) continue;

            // Closer enemies smell stronger (louder ping)
            float falloff = 1f - (dist / scentRadius);
            float vol     = scentPingVolume * falloff;

            // 3D spatialized at enemy world position — bleeds through geometry
            // because AudioSource.PlayClipAtPoint creates a temporary audio source
            // with 3D spatial blend, giving the player directional awareness
            AudioSource.PlayClipAtPoint(scentPingClip, enemy.transform.position, vol);
        }
    }

    // ── Vibration Detection — Abstract Touch ─────────────────────────────────

    /// <summary>
    /// Detects moving enemies via NavMeshAgent velocity and translates them into
    /// subtle ground-vibration camera pulses each frame.
    ///
    /// Lore: Tineyes feel vibrations in the stone floor — guards running down a
    /// corridor become a detectable tremor through the walls.
    /// </summary>
    private void UpdateVibrationDetection()
    {
        foreach (var enemy in MistbornRegistry.ActiveEnemies)
        {
            if (enemy == null) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist > vibrationRadius) continue;

            var nav = enemy.GetComponent<NavMeshAgent>();
            if (nav == null || !nav.isActiveAndEnabled) continue;

            float enemySpeed = nav.velocity.magnitude;
            if (enemySpeed < vibrationSpeedThreshold) continue;

            // Proximity and speed drive the shake magnitude — nearer + faster = stronger pulse
            float proximity = 1f - Mathf.Clamp01(dist / vibrationRadius);
            float shakeMag  = vibrationShakeMagnitude * enemySpeed * proximity;
            CameraShakeManager.Instance?.Shake(0.06f, shakeMag);
        }
    }

    // ── Sensory Overload ──────────────────────────────────────────────────────

    /// <summary>
    /// Checks all active SensorySource objects and accumulates overload.
    /// Bright lights fill visual overload; loud noises fill audio overload.
    ///
    /// Lore: A sudden torch in the face blinds a Tineye. A bell ringing next to
    /// them is genuinely painful and disorienting. This is the COST of Tin.
    /// </summary>
    private void HandleSensoryOverload(float flareMult)
    {
        foreach (var source in SensorySource.ActiveSources)
        {
            if (source == null) continue;

            float dist = Vector3.Distance(transform.position, source.transform.position);
            if (dist >= source.radius) continue;

            float falloff = source.falloff > 0f
                ? Mathf.Pow(1f - (dist / source.radius), source.falloff)
                : (1f - (dist / source.radius));

            float inputIntensity = falloff * source.intensity * flareMult;

            if (source.type == SensorySource.SourceType.BrightLight)
                currentOverloadVisual = Mathf.Clamp01(currentOverloadVisual + inputIntensity * Time.deltaTime * 5f);
            else
                currentOverloadAudio = Mathf.Clamp01(currentOverloadAudio + inputIntensity * Time.deltaTime * 5f);
        }
    }

    private void ApplyOverloadVisuals()
    {
        if (currentOverloadVisual <= 0.02f)
        {
            // No visual overload — if we're off and fully clear, ensure vignette is gone
            if (CurrentState == TinState.Off && vignette != null)
                vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0f, Time.deltaTime * 4f);
            return;
        }

        // Blinding white vignette overrides the blue-silver Tin vignette
        if (vignette != null)
        {
            vignette.color.value     = Color.white;
            vignette.intensity.value = currentOverloadVisual * 0.6f;
        }

        // Overload exposure (blinding flash) takes precedence over night vision
        if (exposure != null)
        {
            float nightVision = (CurrentState != TinState.Off)
                ? nightVisionIntensity * (FlareManager.Instance?.FlareMultiplier ?? 1f)
                : 0f;
            // Take the larger value — overload is brighter/worse than night vision
            exposure.compensation.value = Mathf.Max(nightVision, currentOverloadVisual * 8f);
        }

        // Camera shake at moderate and severe overload
        if (currentOverloadVisual > 0.45f)
        {
            float shakeMag = currentOverloadVisual * 0.12f;
            CameraShakeManager.Instance?.Shake(0.1f, shakeMag);
        }
    }

    private void ApplyOverloadAudio()
    {
        if (lowPass == null || highPass == null) return;

        if (currentOverloadAudio > 0.1f)
        {
            lowPass.enabled  = true;
            highPass.enabled = true;
            // Low-pass cuts highs (ringing); high-pass cuts lows (muffled underwater feel)
            lowPass.cutoffFrequency  = Mathf.Lerp(22000f,  800f, currentOverloadAudio);
            highPass.cutoffFrequency = Mathf.Lerp(   10f, 3500f, currentOverloadAudio);

            // Volume drop during audio pain — deafening loud sounds
            float targetVol = Mathf.Lerp(AudioListener.volume, originalAudioVolume * 0.25f, currentOverloadAudio);
            AudioListener.volume = targetVol;
        }
        else
        {
            lowPass.enabled  = false;
            highPass.enabled = false;
        }
    }

    private void ApplyPhysicalOverload()
    {
        if (playerMove == null) return;

        float totalOverload = Mathf.Clamp01(currentOverloadVisual + currentOverloadAudio);

        if (totalOverload > overloadImpairThreshold)
        {
            // Movement impaired by sensory overwhelm
            float speedFactor = Mathf.Lerp(
                reflexSpeedBoost,
                0.4f,
                (totalOverload - overloadImpairThreshold) / (1f - overloadImpairThreshold)
            );
            playerMove.externalSpeedMultiplier = speedFactor;

            // Camera roll/tilt at severe overload — disorienting
            if (totalOverload > 0.65f && playerCamera != null)
            {
                float tilt = Mathf.Sin(Time.time * 1.8f) * (totalOverload * 5f);
                playerCamera.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
            }

            // At 90%+ overload: intermittent movement stagger
            // Lore: Spook becomes almost nonfunctional in bright daylight without his blindfold
            if (totalOverload > 0.9f && Mathf.Sin(Time.time * 7f) > 0.5f)
                playerMove.externalSpeedMultiplier *= 0.1f;
        }
        else if (playerCamera != null)
        {
            // Smoothly return camera roll to neutral
            playerCamera.transform.localRotation = Quaternion.Lerp(
                playerCamera.transform.localRotation,
                Quaternion.identity,
                Time.deltaTime * 5f
            );
        }
    }

    // ── World Goes Dull ───────────────────────────────────────────────────────

    /// <summary>
    /// Coroutine triggered when Tin reserve hits zero.
    ///
    /// Lore (The Final Empire, ch. 22): "The world seemed to go dull and flat as her
    /// tin ran out, as if she had gone partially deaf and blind."
    ///
    /// Implements this as a brief 0.6s desaturation + low-pass filter, fading back to
    /// normal — the contrast between Tin-sharpened and baseline senses is jarring.
    /// </summary>
    private IEnumerator WorldGoesDullCoroutine()
    {
        const float duration = 0.6f;

        // Instant onset — the world snaps to dull as the metal runs out
        if (colorAdjustments != null) colorAdjustments.saturation.value = -45f;
        if (lowPass != null)
        {
            lowPass.enabled = true;
            lowPass.cutoffFrequency = 1100f;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (colorAdjustments != null)
                colorAdjustments.saturation.value = Mathf.Lerp(-45f, 0f, t);
            if (lowPass != null)
                lowPass.cutoffFrequency = Mathf.Lerp(1100f, 22000f, t);
            yield return null;
        }

        if (lowPass != null) lowPass.enabled = false;
        worldGoesDullCoroutine = null;
    }

    // ── Metal Drain ───────────────────────────────────────────────────────────

    private void DrainMetal(float flareMult)
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(
            AllomancySkill.MetalType.Tin,
            baseMetalCostPerSecond * flareMult * Time.deltaTime
        );
    }

    // ── Public Accessors ──────────────────────────────────────────────────────

    /// <summary>Current visual overload level (0-1) — for HUD display or animation.</summary>
    public float GetVisualOverload() => currentOverloadVisual;

    /// <summary>Current audio overload level (0-1) — for HUD display or animation.</summary>
    public float GetAudioOverload() => currentOverloadAudio;

    /// <summary>True when Tin is actively burning (Burning or Flaring state).</summary>
    public bool IsBurningTin() => CurrentState != TinState.Off;
}
