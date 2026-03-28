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
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

/// <summary>
/// Tin Allomancy — enhances all five senses with lore-accurate benefits and costs.
/// </summary>
[PlayerComponent("Allomancy Metals", order: 40)]
public class Tin : MonoBehaviour
{
    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>Operational state of this Tineye's burn.</summary>
    public enum TinState { Off, Burning, Flaring }

    /// <summary>
    /// Current burn state. Readable by HUD, animations, and other systems.
    /// </summary>
    public TinState CurrentState { get; private set; } = TinState.Off;

    // E/Q key toggle — independent of B key and Left Ctrl burn gate
    private bool _tinToggled = false;

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

    // ── Heartbeat Detection (Flaring only) ───────────────────────────────────

    [Header("Heartbeat Detection — Flaring Only")]
    [SerializeField]
    [Tooltip("Radius within which enemy heartbeats are audible while Flaring. " +
             "Lore: Vin hears guard heartbeats through stone walls when burning hard.")]
    [Range(3f, 20f)]
    private float heartbeatRange = 9f;

    [SerializeField]
    [Tooltip("Seconds between heartbeat pulses (~0.85 = 70bpm). " +
             "Enemies close to the player should feel alive and threatening.")]
    [Range(0.4f, 1.5f)]
    private float heartbeatInterval = 0.85f;

    [SerializeField]
    [Tooltip("3D audio clip for the heartbeat cue. Assign a short, low thud. " +
             "Played at each enemy's position so the player hears direction.")]
    private AudioClip heartbeatClip;

    [SerializeField]
    [Tooltip("Volume of the heartbeat. Keep very subtle — this is awareness, not distraction.")]
    [Range(0f, 0.35f)]
    private float heartbeatVolume = 0.13f;

    // ── Flare Entry Effect ────────────────────────────────────────────────────

    [Header("Flare Entry — Zoom Snap")]
    [SerializeField]
    [Tooltip("Extra FOV reduction on entering Flaring state — a momentary snap-to-focus " +
             "that sells the sense amplification before settling to normal Flaring FOV.")]
    [Range(0f, 12f)]
    private float flareEntryFOVPulse = 7f;

    [SerializeField]
    [Tooltip("Duration of the flare entry zoom snap in seconds.")]
    [Range(0.1f, 0.5f)]
    private float flareEntryDuration = 0.25f;

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
    [Tooltip("HDRP Global Volume for post-processing. Optional — auto-found or auto-created if null.")]
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

    // Scent and heartbeat timers
    private float scentTimer     = 0f;
    private float heartbeatTimer = 0f;

    // Tracks SensorySource instances known from the previous frame.
    // Any source NOT in this set is "new this frame" — triggers an immediate overload spike.
    private readonly HashSet<SensorySource> knownSourcesLastFrame = new HashSet<SensorySource>();

    // Flare entry zoom coroutine
    private Coroutine flareEntryCoroutine;

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
    private Bloom            bloom;

    // Full-screen overexposure overlay (created at runtime)
    private Image overexposureOverlay;

    // Overload speed penalty (0–1). Written by ApplyPhysicalOverload, applied directly there.
    private float overloadSpeedFactor = 1f;

    // Vibration detection throttle + NavMeshAgent cache
    private float vibrationScanTimer = 0f;
    private const float VibrationScanInterval = 0.1f;
    private readonly Dictionary<AIController, NavMeshAgent> navAgentCache = new Dictionary<AIController, NavMeshAgent>();

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

        // Auto-find a Global Volume in the scene if none was assigned in the Inspector.
        // HDRP projects that switched from Built-in often have no scene Volume yet.
        if (globalVolume == null)
        {
            foreach (var vol in FindObjectsOfType<Volume>())
            {
                if (vol.isGlobal) { globalVolume = vol; break; }
            }
        }

        // If still null, create a runtime Global Volume so Tin post-processing always works.
        if (globalVolume == null)
        {
            GameObject volObj = new GameObject("Tin_GlobalVolume");
            globalVolume = volObj.AddComponent<Volume>();
            globalVolume.isGlobal = true;
            globalVolume.weight   = 1f;
            DontDestroyOnLoad(volObj);
        }

        // Ensure the volume has a profile (create one at runtime if missing).
        if (globalVolume.profile == null)
            globalVolume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

        // Obtain HDRP overrides from the profile.
        // Any missing override is added at runtime so Tin effects work
        // even on volumes that haven't been configured in advance.
        {
            var profile = globalVolume.profile;

            if (!profile.TryGet(out exposure))
                exposure = profile.Add<Exposure>(true);

            if (!profile.TryGet(out fog))
                fog = profile.Add<Fog>(true);

            if (!profile.TryGet(out colorAdjustments))
                colorAdjustments = profile.Add<ColorAdjustments>(true);

            if (!profile.TryGet(out vignette))
                vignette = profile.Add<Vignette>(true);

            if (!profile.TryGet(out bloom))
                bloom = profile.Add<Bloom>(true);
        }

        // Create full-screen overexposure overlay at runtime
        GameObject overlayCanvas = new GameObject("TinOverexposureCanvas");
        Canvas canvas = overlayCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        DontDestroyOnLoad(overlayCanvas);

        GameObject overlayObj = new GameObject("OverexposureImage");
        overlayObj.transform.SetParent(overlayCanvas.transform, false);
        RectTransform rt = overlayObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        overexposureOverlay = overlayObj.AddComponent<Image>();
        overexposureOverlay.color = new Color(1f, 1f, 1f, 0f);
        overlayCanvas.AddComponent<CanvasScaler>();
    }

    void Update()
    {
        // ── Determine target TinState ──────────────────────────────────────

        float tinReserve = allomancer != null
            ? allomancer.GetMetalReserve(AllomancySkill.MetalType.Tin)
            : 0f;

        // ── E / Q toggle input ─────────────────────────────────────────────────
        // E toggles Tin when it is the primary selected metal.
        // Q toggles Tin when it is the secondary selected metal.
        MetalSelector sel = allomancer?.GetComponent<MetalSelector>();
        bool ePressed = Input.GetKeyDown(Keybinds.Ability1);
        bool qPressed = Input.GetKeyDown(Keybinds.Ability2);

        if (sel != null)
        {
            bool tinIsPrimary   = sel.GetPrimaryMetal()   == AllomancySkill.MetalType.Tin;
            bool tinIsSecondary = sel.GetSecondaryMetal() == AllomancySkill.MetalType.Tin;

            if (tinIsPrimary   && ePressed) { _tinToggled = !_tinToggled; Debug.Log($"[TIN] E toggle → {_tinToggled}"); }
            if (tinIsSecondary && qPressed) { _tinToggled = !_tinToggled; Debug.Log($"[TIN] Q toggle → {_tinToggled}"); }
        }

        // Tin activates via TWO paths:
        //
        //   Path A — E/Q toggle: press once to turn on, press again to turn off.
        //             E when Tin is primary, Q when Tin is secondary.
        //   Path B — B key (Allomancer BurnToggle): fires when Tin is the active metal.
        //
        // Left Ctrl (FlareManager) intentionally does NOT control Tin — use E/Q.
        //
        bool burningViaSelection = allomancer.IsBurning()
                                && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Tin;

        bool burningTin = allomancer != null
                       && tinReserve > 0f
                       && (burningViaSelection || _tinToggled);

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
        {
            metalRanOut = true;
            _tinToggled = false;   // auto-off when reserve depleted
        }

        // ── State transition hooks ─────────────────────────────────────────

        if (targetState != CurrentState)
        {
            Debug.Log($"[TIN] State: {CurrentState} → {targetState}  toggled={_tinToggled}  reserve={tinReserve:F1}");

            if (targetState == TinState.Off)
            {
                // Stop flare entry zoom if it's still running
                if (flareEntryCoroutine != null) { StopCoroutine(flareEntryCoroutine); flareEntryCoroutine = null; }
                OnStopBurning(metalRanOut);
                metalRanOut = false;
            }
            else if (CurrentState == TinState.Off)
            {
                OnStartBurning();
                // If already above flare threshold on activation, trigger zoom immediately
                if (targetState == TinState.Flaring) TriggerFlareEntry();
            }
            else if (CurrentState == TinState.Burning && targetState == TinState.Flaring)
            {
                TriggerFlareEntry();
            }
            else if (CurrentState == TinState.Flaring && targetState == TinState.Burning)
            {
                // Dropped below flare threshold — cancel any pending zoom
                if (flareEntryCoroutine != null) { StopCoroutine(flareEntryCoroutine); flareEntryCoroutine = null; }
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
            if (CurrentState == TinState.Flaring) UpdateHeartbeatDetection();
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

        // Only clear the speed multiplier if Tin was the one penalizing it
        if (playerMove != null && overloadSpeedFactor < 1f)
            playerMove.externalSpeedMultiplier = 1f;
        overloadSpeedFactor = 1f;

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
    /// subtle ground-vibration camera pulses. Throttled to 10fps and caches
    /// NavMeshAgent lookups to avoid GetComponent every frame.
    ///
    /// Lore: Tineyes feel vibrations in the stone floor — guards running down a
    /// corridor become a detectable tremor through the walls.
    /// </summary>
    private void UpdateVibrationDetection()
    {
        vibrationScanTimer -= Time.deltaTime;
        if (vibrationScanTimer > 0f) return;
        vibrationScanTimer = VibrationScanInterval;

        // Purge cache entries for destroyed enemies (Unity null-check on MonoBehaviour keys)
        var deadKeys = new System.Collections.Generic.List<AIController>();
        foreach (var key in navAgentCache.Keys)
            if (key == null) deadKeys.Add(key);
        foreach (var key in deadKeys) navAgentCache.Remove(key);

        foreach (var enemy in MistbornRegistry.ActiveEnemies)
        {
            if (enemy == null) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist > vibrationRadius) continue;

            if (!navAgentCache.TryGetValue(enemy, out NavMeshAgent nav))
            {
                nav = enemy.GetComponent<NavMeshAgent>();
                navAgentCache[enemy] = nav;
            }
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

            // SUDDEN STIMULUS: a source that wasn't active last frame (e.g. an explosion,
            // a torch suddenly revealed) causes an immediate overload spike.
            // Lore: Vin is instantly blinded by Marsh's eye-spike gleam — no build-up.
            if (!knownSourcesLastFrame.Contains(source))
            {
                float spike = falloff * source.intensity * flareMult * 0.5f;
                if (source.type == SensorySource.SourceType.BrightLight)
                    currentOverloadVisual = Mathf.Clamp01(currentOverloadVisual + spike);
                else
                    currentOverloadAudio  = Mathf.Clamp01(currentOverloadAudio  + spike);
            }

            // GRADUAL ACCUMULATION: persistent sources (standing near a torch) build up over time
            float inputIntensity = falloff * source.intensity * flareMult;
            if (source.type == SensorySource.SourceType.BrightLight)
                currentOverloadVisual = Mathf.Clamp01(currentOverloadVisual + inputIntensity * Time.deltaTime * 5f);
            else
                currentOverloadAudio  = Mathf.Clamp01(currentOverloadAudio  + inputIntensity * Time.deltaTime * 5f);
        }

        // Update known sources for next frame's sudden-stimulus check
        knownSourcesLastFrame.Clear();
        foreach (var source in SensorySource.ActiveSources)
            if (source != null) knownSourcesLastFrame.Add(source);
    }

    private void ApplyOverloadVisuals()
    {
        if (currentOverloadVisual <= 0.02f)
        {
            // No visual overload — clear overlay and bloom, fade vignette if off
            if (overexposureOverlay != null)
                overexposureOverlay.color = new Color(1f, 1f, 1f, 0f);
            if (bloom != null) bloom.active = false;
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
            exposure.compensation.value = Mathf.Max(nightVision, currentOverloadVisual * 8f);
        }

        // Full-screen white overexposure overlay — fades in above 30% visual overload
        if (overexposureOverlay != null)
        {
            float alpha = Mathf.Clamp01((currentOverloadVisual - 0.3f) / 0.7f);
            overexposureOverlay.color = new Color(1f, 1f, 1f, alpha);
        }

        // Bloom glow — adds glare halo around bright areas during overload
        if (bloom != null)
        {
            bloom.active           = true;
            bloom.intensity.value  = currentOverloadVisual * 6f;
            bloom.threshold.value  = Mathf.Lerp(1.5f, 0.2f, currentOverloadVisual);
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
        // Tin does NOT boost speed — that is Pewter's domain.
        // Tin overload CAN slow you: sensory pain is genuinely debilitating.

        float totalOverload = Mathf.Clamp01(currentOverloadVisual + currentOverloadAudio);

        if (totalOverload > overloadImpairThreshold)
        {
            overloadSpeedFactor = Mathf.Lerp(
                1f, 0.4f,
                (totalOverload - overloadImpairThreshold) / (1f - overloadImpairThreshold)
            );
            if (totalOverload > 0.9f && Mathf.Sin(Time.time * 7f) > 0.5f)
                overloadSpeedFactor *= 0.1f;

            if (playerMove != null)
                playerMove.externalSpeedMultiplier = overloadSpeedFactor;

            if (totalOverload > 0.65f && playerCamera != null)
            {
                float tilt = Mathf.Sin(Time.time * 1.8f) * (totalOverload * 5f);
                playerCamera.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
            }
        }
        else
        {
            // Clear any overload speed penalty Tin applied — never write above 1
            if (playerMove != null && overloadSpeedFactor < 1f)
                playerMove.externalSpeedMultiplier = 1f;
            overloadSpeedFactor = 1f;

            if (playerCamera != null)
                playerCamera.transform.localRotation = Quaternion.Lerp(
                    playerCamera.transform.localRotation, Quaternion.identity, Time.deltaTime * 5f);
        }
    }

    // ── Heartbeat Detection ───────────────────────────────────────────────────

    /// <summary>
    /// While Flaring, plays a 3D heartbeat sound at nearby enemy positions.
    /// Lore: "She could hear their heartbeats — a soft, rhythmic thudding that
    /// let her know exactly where each guard was, even through the stone walls."
    /// Only fires when in Flaring state — burning hard enough to hear heartbeats
    /// is a deliberate cost, not a passive baseline.
    /// </summary>
    private void UpdateHeartbeatDetection()
    {
        heartbeatTimer -= Time.deltaTime;
        if (heartbeatTimer > 0f) return;
        heartbeatTimer = heartbeatInterval;

        if (heartbeatClip == null) return;

        foreach (var enemy in MistbornRegistry.ActiveEnemies)
        {
            if (enemy == null) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist > heartbeatRange) continue;

            float falloff = 1f - Mathf.Clamp01(dist / heartbeatRange);
            float vol     = heartbeatVolume * falloff;
            AudioSource.PlayClipAtPoint(heartbeatClip, enemy.transform.position, vol);
        }
    }

    // ── Flare Entry Zoom ──────────────────────────────────────────────────────

    private void TriggerFlareEntry()
    {
        if (flareEntryCoroutine != null) StopCoroutine(flareEntryCoroutine);
        if (playerCamera != null)
            flareEntryCoroutine = StartCoroutine(FlareEntryCoroutine());
    }

    /// <summary>
    /// Brief FOV snap on entering Flaring state.
    /// The camera punches in sharply then eases back to the settled Flaring FOV —
    /// selling the sensation of senses suddenly amplifying.
    /// </summary>
    private IEnumerator FlareEntryCoroutine()
    {
        float settledFOV  = originalFOV - fovFocusDegrees - fovFlaringExtra;
        float snapFOV     = settledFOV - flareEntryFOVPulse;
        float startFOV    = playerCamera.fieldOfView;
        float elapsed     = 0f;

        while (elapsed < flareEntryDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flareEntryDuration;
            // Quick snap in, ease back out (quadratic ease-out)
            float eased = 1f - (1f - t) * (1f - t);
            playerCamera.fieldOfView = Mathf.Lerp(snapFOV, settledFOV, eased);
            yield return null;
        }

        playerCamera.fieldOfView = settledFOV;
        flareEntryCoroutine = null;
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

    // ── Cleanup ───────────────────────────────────────────────────────────────

    void OnDestroy()
    {
        // Restore baselines in case the object is destroyed mid-burn (e.g. scene unload)
        if (CurrentState != TinState.Off)
            OnStopBurning(false);

        // Clear stale entries from the NavMeshAgent cache (destroyed enemies)
        navAgentCache.Clear();
    }

    // ── Public Accessors ──────────────────────────────────────────────────────

    /// <summary>Current visual overload level (0-1) — for HUD display or animation.</summary>
    public float GetVisualOverload() => currentOverloadVisual;

    /// <summary>Current audio overload level (0-1) — for HUD display or animation.</summary>
    public float GetAudioOverload() => currentOverloadAudio;

    /// <summary>True when Tin is actively burning (Burning or Flaring state).</summary>
    public bool IsBurningTin() => CurrentState != TinState.Off;
}
