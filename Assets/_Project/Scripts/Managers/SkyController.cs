using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Ensures the HDRP sky is configured correctly at runtime:
/// - Switches to PhysicallyBasedSky (shows sun disk from directional light)
/// - Ensures the directional light interacts with the sky
/// Fixes: no sun visible, sky incorrectly showing gradient-only.
/// Assign sunLight to the Directional Light in the Inspector.
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
        SetupSky();
    }

    void SetupSunLight()
    {
        if (sunLight == null) return;

        // Link as the environment sun so PhysicallyBasedSky can find it
        RenderSettings.sun = sunLight;

        HDAdditionalLightData hd = sunLight.GetComponent<HDAdditionalLightData>();
        if (hd != null)
            hd.interactsWithSky = true;
    }

    void SetupSky()
    {
        // Find the highest-priority global volume
        Volume globalVolume = null;
        float bestPriority = float.MinValue;
        foreach (Volume v in FindObjectsByType<Volume>(FindObjectsSortMode.None))
        {
            if (v.isGlobal && v.priority >= bestPriority)
            {
                bestPriority = v.priority;
                globalVolume = v;
            }
        }

        if (globalVolume == null) return;

        // Create an instanced copy so we don't permanently modify the shared asset on disk.
        // volume.profile returns sharedProfile if no instance exists yet; we must explicitly clone it.
        if (globalVolume.sharedProfile != null && globalVolume.profile == globalVolume.sharedProfile)
            globalVolume.profile = Object.Instantiate(globalVolume.sharedProfile);
        VolumeProfile profile = globalVolume.profile;

        // Ensure VisualEnvironment uses PhysicallyBasedSky
        if (!profile.TryGet(out VisualEnvironment visualEnv))
            visualEnv = profile.Add<VisualEnvironment>(true);
        visualEnv.skyType.Override(SkySettings.GetUniqueID<PhysicallyBasedSky>());
        visualEnv.skyAmbientMode.Override(SkyAmbientMode.Dynamic);

        // Add/configure PhysicallyBasedSky
        if (!profile.TryGet(out PhysicallyBasedSky sky))
            sky = profile.Add<PhysicallyBasedSky>(true);
        sky.active = true;

        // Disable GradientSky so it doesn't override
        if (profile.TryGet(out GradientSky gradientSky))
            gradientSky.active = false;
    }
}
