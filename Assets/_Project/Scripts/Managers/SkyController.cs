using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Ensures a properly-configured HDRP directional sun light exists in the scene.
///
/// HDRP requires three things for a visible sun:
///   1. A Directional Light with an HDAdditionalLightData component.
///   2. interactsWithSky = true so the sky shader draws the sun disk.
///   3. Physical intensity in lux (real sun ≈ 100,000 lux; game-tuned default: 80,000).
///      The built-in Light.intensity field does NOT control HDRP brightness —
///      HDAdditionalLightData.intensity does.
///
/// If no directional light is present in the scene, this script creates one at runtime.
/// Assign the created "Sun" GameObject to DayNightCycle.directionalLight in the Inspector.
/// </summary>
public class SkyController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the scene Directional Light here. Auto-found or auto-created if null.")]
    public Light sunLight;

    [Header("HDRP Physical Intensity (lux)")]
    [Tooltip("Brightness at solar noon. Real sun ≈ 100,000 lux. Keep 60,000–100,000 for realism.")]
    public float sunLuxIntensity  = 80000f;
    [Tooltip("Brightness at night (moon approximation).")]
    public float moonLuxIntensity = 0.5f;

    void Start()
    {
        AcquireOrCreateSunLight();
        SetupSunLight();
    }

    void AcquireOrCreateSunLight()
    {
        // 1. Use assigned reference
        if (sunLight != null) return;

        // 2. Try RenderSettings.sun
        sunLight = RenderSettings.sun;
        if (sunLight != null) return;

        // 3. Find any directional light in the scene
        foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l.type == LightType.Directional) { sunLight = l; break; }
        }
        if (sunLight != null) return;

        // 4. None found — create one at runtime
        GameObject sunObj = new GameObject("Sun");
        sunObj.transform.SetParent(null);

        sunLight = sunObj.AddComponent<Light>();
        sunLight.type      = LightType.Directional;
        sunLight.shadows   = LightShadows.Soft;
        sunLight.color     = new Color(1f, 0.95f, 0.8f); // warm daylight white

        // HDRP data is auto-added with the Light component in HDRP projects,
        // but AddComponent handles the missing case.
        if (sunObj.GetComponent<HDAdditionalLightData>() == null)
            sunObj.AddComponent<HDAdditionalLightData>();

        // Aim southeast and 50° down — a mid-morning sun angle
        sunObj.transform.rotation = Quaternion.Euler(50f, 170f, 0f);

        Debug.Log("[SkyController] No directional light found — created 'Sun' at runtime. " +
                  "Drag it into DayNightCycle.directionalLight to enable full day/night cycle.");
    }

    void SetupSunLight()
    {
        if (sunLight == null) return;

        // Register as the environment sun so the sky shader tracks it
        RenderSettings.sun = sunLight;

        HDAdditionalLightData hd = sunLight.GetComponent<HDAdditionalLightData>();
        if (hd == null) hd = sunLight.gameObject.AddComponent<HDAdditionalLightData>();

        // Must be true for the sun disk to appear in Physically Based Sky
        hd.interactsWithSky = true;

        // Set physical lux intensity — this is what HDRP actually uses for brightness
        hd.intensity = sunLuxIntensity;
    }

    /// <summary>
    /// Called by DayNightCycle to drive intensity in physical lux rather than the
    /// legacy 0-1 Light.intensity that HDRP ignores.
    /// </summary>
    public void SetSunIntensityFraction(float fraction)
    {
        if (sunLight == null) return;
        HDAdditionalLightData hd = sunLight.GetComponent<HDAdditionalLightData>();
        if (hd == null) return;
        hd.intensity = Mathf.Lerp(moonLuxIntensity, sunLuxIntensity, fraction);
    }

    public Light GetSunLight() => sunLight;
}
