using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Day/night cycle with directional light rotation, sky color changes,
/// and mist intensity tied to time. The mists come at night (lore-accurate).
/// Connects to WeatherGameplayIntegration for gameplay effects.
///
/// HDRP note: intensity is driven through SkyController.SetSunIntensityFraction()
/// which uses HDAdditionalLightData.intensity (physical lux). The legacy
/// Light.intensity field has no effect on HDRP brightness.
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }

    [Header("Time")]
    [Range(0f, 24f)] public float currentHour = 12f;
    public float dayLengthMinutes = 20f; // Real minutes per in-game day
    public bool autoAdvance = true;

    [Header("Sun")]
    public Light directionalLight;
    public Gradient sunColorGradient;
    // Intensity is now driven through SkyController (physical lux).
    // These fields are kept for the Inspector to show tuning is done there.

    [Header("Ambient")]
    public Gradient ambientColorGradient;
    public AnimationCurve fogDensityCurve;
    public float maxFogDensity = 0.02f;

    [Header("Mist — Lore: Mists come every night")]
    public AnimationCurve mistIntensityCurve;
    public float nightStart = 20f;
    public float nightEnd = 6f;
    public float duskStart = 18f;
    public float dawnEnd = 8f;

    [Header("Scadrial Lore — Ash & Sun")]
    [Tooltip("Luthadel's implied latitude (degrees). Higher = lower noon sun. ~45 matches temperate Final Empire.")]
    [Range(20f, 65f)]
    public float luthedalLatitudeDeg = 45f;

    [Tooltip("Solar declination (degrees). 0 = equinox. Positive = summer solstice arc (higher noon sun).")]
    [Range(-23.5f, 23.5f)]
    public float solarDeclination = 0f;

    [Tooltip("Ash attenuation from the Ashmounts. 0 = no ash (clear sky), 1 = maximum ash cover.")]
    [Range(0f, 1f)]
    public float ashAttenuation = 0.45f;

    [Tooltip("How much the ash tints the sun color toward sepia/red during the day.")]
    [Range(0f, 1f)]
    public float ashColorTint = 0.6f;

    private SkyController skyController;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sunColorGradient == null)
        {
            // Lore-accurate Final Empire palette:
            // Ash particles scatter blue light → sunrises/sunsets are deep red-orange.
            // Midday is not clean white but a washed-out sepia from constant ash haze.
            sunColorGradient = new Gradient();
            sunColorGradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.08f, 0.06f, 0.12f), 0f),    // midnight — cold dark
                    new GradientColorKey(new Color(0.75f, 0.22f, 0.05f), 0.22f), // pre-dawn — dark blood red
                    new GradientColorKey(new Color(0.95f, 0.50f, 0.15f), 0.26f), // sunrise — fiery orange (ash scatter)
                    new GradientColorKey(new Color(0.98f, 0.82f, 0.58f), 0.5f),  // noon — warm sepia (ash filter)
                    new GradientColorKey(new Color(0.95f, 0.45f, 0.10f), 0.74f), // sunset — deep amber-orange
                    new GradientColorKey(new Color(0.60f, 0.12f, 0.05f), 0.78f), // post-sunset — dying ember red
                    new GradientColorKey(new Color(0.08f, 0.06f, 0.12f), 1f)     // midnight
                },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
            );
        }

        if (ambientColorGradient == null)
        {
            // Ambient reflects the ash-filtered sky: brown-grey during day, cold dark at night.
            ambientColorGradient = new Gradient();
            ambientColorGradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.04f, 0.03f, 0.06f), 0f),   // midnight
                    new GradientColorKey(new Color(0.25f, 0.15f, 0.10f), 0.25f), // dawn — reddish
                    new GradientColorKey(new Color(0.42f, 0.38f, 0.30f), 0.5f),  // noon — ashy brown-grey
                    new GradientColorKey(new Color(0.28f, 0.18f, 0.10f), 0.75f), // dusk — warm brown
                    new GradientColorKey(new Color(0.04f, 0.03f, 0.06f), 1f)
                },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
            );
        }

        if (mistIntensityCurve == null || mistIntensityCurve.length == 0)
        {
            mistIntensityCurve = new AnimationCurve(
                new Keyframe(0f, 0.9f), new Keyframe(6f, 0.8f), new Keyframe(8f, 0.1f),
                new Keyframe(16f, 0.05f), new Keyframe(18f, 0.3f), new Keyframe(20f, 0.85f),
                new Keyframe(24f, 0.9f)
            );
        }

        if (fogDensityCurve == null || fogDensityCurve.length == 0)
        {
            fogDensityCurve = new AnimationCurve(
                new Keyframe(0f, 1f), new Keyframe(6f, 0.8f), new Keyframe(10f, 0.1f),
                new Keyframe(16f, 0.15f), new Keyframe(20f, 0.7f), new Keyframe(24f, 1f)
            );
        }
    }

    void Start()
    {
        skyController = FindObjectOfType<SkyController>();
    }

    void Update()
    {
        if (autoAdvance)
        {
            float hoursPerSecond = 24f / (dayLengthMinutes * 60f);
            currentHour += hoursPerSecond * Time.deltaTime;
            if (currentHour >= 24f) currentHour -= 24f;
        }

        float timeNormalized = currentHour / 24f;

        UpdateSun(timeNormalized);
        UpdateAmbient(timeNormalized);
        UpdateFog();
        SyncWeatherSystem();
    }

    void UpdateSun(float t)
    {
        // Re-acquire after scene transitions
        if (skyController == null) skyController = FindObjectOfType<SkyController>();
        if (directionalLight == null && skyController != null)
            directionalLight = skyController.GetSunLight();
        if (directionalLight == null) return;

        // ── Lore-accurate latitude-based sun arc ─────────────────────────
        //
        // Luthadel is at a temperate mid-latitude (~45°N implied by lore).
        // The Lord Ruler preserved a normal day/night cycle but Scadrial was
        // nudged closer to the sun, making it inherently more intense.
        //
        // Astronomical formula (simplified, assumes circular orbit / equinox):
        //   hour angle H = (hour - 12) * 15°
        //   sin(altitude) = sin(lat)*sin(decl) + cos(lat)*cos(decl)*cos(H)
        //
        // This produces the CORRECT non-linear rate of change:
        //   - Sun moves faster near the horizon (sunrise/sunset)
        //   - Sun moves slower near zenith (around noon)
        //   - At 45°N latitude, noon elevation ≈ 45° (not straight overhead)
        //
        float latRad  = luthedalLatitudeDeg * Mathf.Deg2Rad;
        float declRad = solarDeclination * Mathf.Deg2Rad;
        float hourAngleRad = (currentHour - 12f) * 15f * Mathf.Deg2Rad;

        float sinAlt = Mathf.Sin(latRad)  * Mathf.Sin(declRad)
                     + Mathf.Cos(latRad)  * Mathf.Cos(declRad) * Mathf.Cos(hourAngleRad);
        float altitudeDeg = Mathf.Asin(Mathf.Clamp(sinAlt, -1f, 1f)) * Mathf.Rad2Deg;

        // Azimuth: sun rises east (~90°), transits south (~170°), sets west (~250°).
        // Approximate with a smooth east→south→west sweep over the day arc.
        // When sun is below horizon the azimuth still tracks but intensity is 0.
        float azimuthDeg;
        {
            // cosAz = (sin(decl) - sin(alt)*sin(lat)) / (cos(alt)*cos(lat))
            float cosAlt = Mathf.Cos(altitudeDeg * Mathf.Deg2Rad);
            float denom = cosAlt * Mathf.Cos(latRad);
            float cosAz = denom > 0.0001f
                ? (Mathf.Sin(declRad) - sinAlt * Mathf.Sin(latRad)) / denom
                : 0f;
            cosAz = Mathf.Clamp(cosAz, -1f, 1f);
            float az = Mathf.Acos(cosAz) * Mathf.Rad2Deg;
            // Before solar noon the sun is in the east half (az < 180°),
            // after noon it is in the west half (az > 180°)
            azimuthDeg = currentHour < 12f ? az : (360f - az);
        }

        // X = altitude (0° horizon → positive = above), Y = azimuth
        // HDRP directional light: positive X pitch tilts the light downward
        directionalLight.transform.rotation = Quaternion.Euler(altitudeDeg, azimuthDeg, 0f);

        // Color — lore-accurate ash-filtered palette
        if (sunColorGradient != null)
            directionalLight.color = sunColorGradient.Evaluate(t);

        // ── Intensity: raw solar + ash attenuation ────────────────────────
        //
        // Scadrial is closer to its star → raw solar flux is higher than Earth.
        // The Ashmounts spew constant particulates that scatter/absorb ~45% of
        // daylight, keeping the surface from overheating. This is modeled as:
        //   effectiveFraction = rawFraction × (1 − ashAttenuation)
        //
        float rawFraction = Mathf.Clamp01(sinAlt);   // 0 below horizon, linear above

        // Atmospheric reddening near horizon: ash scatter is stronger at low angles
        // (longer path through ash layer). Tint the light color warmer when the sun
        // is near the horizon — this is already baked into the gradient but we can
        // amplify it based on actual computed altitude.
        if (sunColorGradient != null && altitudeDeg < 15f && altitudeDeg > -5f)
        {
            float horizonBlend = 1f - Mathf.Clamp01((altitudeDeg + 5f) / 20f);
            Color ashHorizon = new Color(0.95f, 0.35f, 0.05f); // deep ash-red
            directionalLight.color = Color.Lerp(directionalLight.color, ashHorizon, horizonBlend * ashColorTint);
        }

        float effectiveFraction = rawFraction * (1f - ashAttenuation);

        // Smooth the fraction slightly so rapid sun dips don't cause intensity flicker
        effectiveFraction = Mathf.SmoothStep(0f, 1f, effectiveFraction);

        skyController?.SetSunIntensityFraction(effectiveFraction);
    }

    void UpdateAmbient(float t)
    {
        RenderSettings.ambientLight = ambientColorGradient.Evaluate(t);
    }

    void UpdateFog()
    {
        float fogAmount = fogDensityCurve.Evaluate(currentHour) * maxFogDensity;
        RenderSettings.fogDensity = fogAmount;
        RenderSettings.fog = fogAmount > 0.001f;
    }

    void SyncWeatherSystem()
    {
        WeatherGameplayIntegration wgi = WeatherGameplayIntegration.Instance;
        if (wgi != null)
        {
            wgi.timeOfDay = currentHour;
            wgi.mistIntensity = mistIntensityCurve.Evaluate(currentHour);
        }
    }

    // ── Public API ───────────────────────────────────────────────────────

    /// <summary>True when the sun is geometrically above the horizon (computed from latitude/declination).</summary>
    public bool IsSunAboveHorizon()
    {
        float latRad  = luthedalLatitudeDeg * Mathf.Deg2Rad;
        float declRad = solarDeclination * Mathf.Deg2Rad;
        float H = (currentHour - 12f) * 15f * Mathf.Deg2Rad;
        float sinAlt = Mathf.Sin(latRad) * Mathf.Sin(declRad)
                     + Mathf.Cos(latRad) * Mathf.Cos(declRad) * Mathf.Cos(H);
        return sinAlt > 0f;
    }

    public bool IsNight() => currentHour < nightEnd || currentHour > nightStart;
    public bool IsDusk() => currentHour > duskStart && currentHour <= nightStart;
    public bool IsDawn() => currentHour >= nightEnd && currentHour < dawnEnd;
    public float GetMistIntensity() => mistIntensityCurve.Evaluate(currentHour);
    public float GetHour() => currentHour;

    public void SetTime(float hour) => currentHour = Mathf.Clamp(hour, 0f, 24f);
    public void SetDaySpeed(float realMinutesPerDay) => dayLengthMinutes = realMinutesPerDay;
}
