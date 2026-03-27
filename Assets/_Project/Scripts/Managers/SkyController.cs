using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Links the scene's directional light as the HDRP sun so the sun disk
/// appears in the sky and DayNightCycle rotations are visible.
/// Does NOT modify sky type or volume profiles — all sky/colour/exposure
/// settings are left exactly as configured in the scene.
/// </summary>
public class SkyController : MonoBehaviour
{
    [Header("References")]
    public Light sunLight;

    void Start()
    {
        if (sunLight == null)
            sunLight = RenderSettings.sun;

        if (sunLight == null)
        {
            foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (l.type == LightType.Directional) { sunLight = l; break; }
            }
        }

        SetupSunLight();
    }

    void SetupSunLight()
    {
        if (sunLight == null) return;

        // Register as the environment sun so the sky system tracks this light.
        RenderSettings.sun = sunLight;

        // Allow the directional light to interact with the sky (shows sun disk in PBS).
        HDAdditionalLightData hd = sunLight.GetComponent<HDAdditionalLightData>();
        if (hd != null)
            hd.interactsWithSky = true;
    }
}
