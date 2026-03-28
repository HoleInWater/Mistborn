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

    private SkyController skyController;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sunColorGradient == null)
        {
            sunColorGradient = new Gradient();
            sunColorGradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 0f),    // midnight
                    new GradientColorKey(new Color(0.8f, 0.4f, 0.2f), 0.25f), // dawn
                    new GradientColorKey(new Color(1f, 0.95f, 0.8f), 0.5f),   // noon
                    new GradientColorKey(new Color(0.9f, 0.5f, 0.2f), 0.75f), // dusk
                    new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 1f)     // midnight
                },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
            );
        }

        if (ambientColorGradient == null)
        {
            ambientColorGradient = new Gradient();
            ambientColorGradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.05f, 0.05f, 0.1f), 0f),
                    new GradientColorKey(new Color(0.3f, 0.3f, 0.35f), 0.25f),
                    new GradientColorKey(new Color(0.5f, 0.5f, 0.5f), 0.5f),
                    new GradientColorKey(new Color(0.3f, 0.25f, 0.2f), 0.75f),
                    new GradientColorKey(new Color(0.05f, 0.05f, 0.1f), 1f)
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

        // Rotate: 0h = below horizon, 6h = sunrise, 12h = overhead, 18h = sunset
        float sunAngle = (t * 360f) - 90f;
        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // Color
        if (sunColorGradient != null)
            directionalLight.color = sunColorGradient.Evaluate(t);

        // Intensity fraction (0=moon, 1=full sun) — SkyController converts to lux
        float intensityFraction;
        if (currentHour >= dawnEnd && currentHour <= duskStart)
        {
            intensityFraction = 1f;
        }
        else if (currentHour > duskStart && currentHour <= nightStart)
        {
            float fade = (currentHour - duskStart) / Mathf.Max(nightStart - duskStart, 0.001f);
            intensityFraction = Mathf.Lerp(1f, 0f, fade);
        }
        else if (currentHour >= nightEnd && currentHour < dawnEnd)
        {
            float fade = (currentHour - nightEnd) / Mathf.Max(dawnEnd - nightEnd, 0.001f);
            intensityFraction = Mathf.Lerp(0f, 1f, fade);
        }
        else
        {
            intensityFraction = 0f; // night
        }

        // Drive HDRP physical lux through SkyController
        skyController?.SetSunIntensityFraction(intensityFraction);
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
    public bool IsNight() => currentHour < nightEnd || currentHour > nightStart;
    public bool IsDusk() => currentHour > duskStart && currentHour <= nightStart;
    public bool IsDawn() => currentHour >= nightEnd && currentHour < dawnEnd;
    public float GetMistIntensity() => mistIntensityCurve.Evaluate(currentHour);
    public float GetHour() => currentHour;

    public void SetTime(float hour) => currentHour = Mathf.Clamp(hour, 0f, 24f);
    public void SetDaySpeed(float realMinutesPerDay) => dayLengthMinutes = realMinutesPerDay;
}
