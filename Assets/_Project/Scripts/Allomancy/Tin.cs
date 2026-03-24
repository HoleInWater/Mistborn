using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;

/// <summary>
/// Implements the Tin Allomancy ability (enhanced senses).
/// Lore: Increases all senses, allows seeing through mist/darkness, but risks sensory overload.
/// </summary>
public class Tin : MonoBehaviour
{
    [Header("Settings")]
    public float baseMetalCostPerSecond = 1f;
    public float fovIncreaseBase = 5f;
    public float fovIncreaseMax = 20f;
    public float audioVolumeMultiplierBase = 1.2f;
    public float audioVolumeMultiplierMax = 3.0f;
    
    [Header("Enhancement Scales")]
    [Range(0, 1)] public float nightVisionIntensity = 0.5f;
    [Range(0, 1)] public float mistPiercingStrength = 0.7f;
    public float reflexSpeedBoost = 1.15f;

    [Header("Sensory Overload")]
    public float overloadRecoveryRate = 2f;
    public float visualOverloadThreshold = 0.8f;
    public float audioOverloadThreshold = 0.8f;

    [Header("References")]
    public Camera playerCamera;
    public Allomancer allomancer;
    public Volume globalVolume;
    
    // Internal State
    private bool isBurning = false;
    private float originalFOV;
    private float originalAudioVolume;
    private float currentOverloadVisual = 0f;
    private float currentOverloadAudio = 0f;
    // References & Cache
    private BasicPlayerMove playerMove;
    private Vector3 originalCameraLocalPos;
    private float shakeIntensity = 0f;
    private AudioLowPassFilter lowPass;
    private AudioHighPassFilter highPass;

    // HDRP Overrides
    private Exposure exposure;
    private Fog fog;
    private ColorAdjustments colorAdjustments;
    private Vignette vignette;

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
        
        // Audio filters (optional but used if they exist)
        lowPass = GetComponent<AudioLowPassFilter>();
        highPass = GetComponent<AudioHighPassFilter>();

        // Initialize HDRP Volume components
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out exposure);
            globalVolume.profile.TryGet(out fog);
            globalVolume.profile.TryGet(out colorAdjustments);
            globalVolume.profile.TryGet(out vignette);
        }

        // Initialize Components
        playerMove = GetComponentInParent<BasicPlayerMove>();
        
        if (playerCamera != null)
        {
            lowPass = playerCamera.GetComponent<AudioLowPassFilter>();
            if (lowPass == null) lowPass = playerCamera.gameObject.AddComponent<AudioLowPassFilter>();
            lowPass.enabled = false;

            highPass = playerCamera.GetComponent<AudioHighPassFilter>();
            if (highPass == null) highPass = playerCamera.gameObject.AddComponent<AudioHighPassFilter>();
            highPass.enabled = false;

            originalCameraLocalPos = playerCamera.transform.localPosition;
        }
    }

    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Tin;

        if (isBurning)
        {
            float flareMult = (FlareManager.Instance != null) ? FlareManager.Instance.FlareMultiplier : 1.0f;
            ApplyTinEffects(flareMult);
            HandleSensoryOverload(flareMult);
            DrainMetal(flareMult);
        }
        else if (wasBurning)
        {
            ResetTinEffects();
        }

        // Recover from overload over time
        float recoveryScale = isBurning ? 0.5f : 1.5f; // Slower recovery while burning
        if (currentOverloadVisual > 0) currentOverloadVisual -= overloadRecoveryRate * recoveryScale * Time.deltaTime;
        if (currentOverloadAudio > 0) currentOverloadAudio -= overloadRecoveryRate * recoveryScale * Time.deltaTime;
        
        ApplyOverloadVisuals();
        ApplyOverloadAudio();
        ApplyPhysicalOverload();
    }

    // Removed local GetFlareMultiplier in favor of unified FlareManager state

    void ApplyTinEffects(float flareMult)
    {
        // FOV
        if (playerCamera != null)
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, originalFOV + (fovIncreaseBase * flareMult), Time.deltaTime * 5f);
        
        // Audio Base Volume
        AudioListener.volume = originalAudioVolume * Mathf.Lerp(audioVolumeMultiplierBase, audioVolumeMultiplierMax, (flareMult - 1f) / 1.5f);

        // HDRP Enhancements
        if (exposure != null) exposure.compensation.value = nightVisionIntensity * flareMult;

        if (fog != null) fog.meanFreePath.value = 100f * (1f + mistPiercingStrength * flareMult);

        if (colorAdjustments != null)
        {
            colorAdjustments.contrast.value = 10f * flareMult;
            colorAdjustments.saturation.value = 5f * flareMult;
        }

        // Slight speed boost from heightened reflexes if NOT overloaded
        if (playerMove != null && (currentOverloadVisual + currentOverloadAudio) < 0.2f)
        {
            playerMove.externalSpeedMultiplier = reflexSpeedBoost;
        }
    }

    void ResetTinEffects()
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
    }

    void HandleSensoryOverload(float flareMult)
    {
        foreach (var source in SensorySource.ActiveSources)
        {
            if (source == null) continue;
            float dist = Vector3.Distance(transform.position, source.transform.position);
            if (dist < source.radius)
            {
                float falloff = source.falloff > 0 ? Mathf.Pow(1f - (dist / source.radius), source.falloff) : (1f - (dist / source.radius));
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
        
        if (exposure != null)
        {
            // Set overload exposure — NOT additive (would accumulate infinitely)
            exposure.compensation.value = currentOverloadVisual * 8f;
        }

        // Camera Shake for intense overload
        if (currentOverloadVisual > 0.5f || currentOverloadAudio > 0.5f)
        {
            shakeIntensity = Mathf.Max(currentOverloadVisual, currentOverloadAudio) * 0.1f;
            CameraShakeManager.Instance?.Shake(0.1f, shakeIntensity);
        }
        else if (playerCamera != null)
        {
            if (CameraShakeManager.Instance == null)
                playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, originalCameraLocalPos, Time.deltaTime * 5f);
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

        float totalOverload = Mathf.Clamp01(currentOverloadVisual + currentOverloadAudio);
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
                // Severe stagger: intermittent near-stop (no Debug.Log in production)
                if (Mathf.Sin(Time.time * 8f) > 0.5f)
                    playerMove.externalSpeedMultiplier *= 0.1f;
            }
        }
        else if (playerCamera != null)
        {
             playerCamera.transform.localRotation = Quaternion.Lerp(playerCamera.transform.localRotation, Quaternion.identity, Time.deltaTime * 3f);
        }
    }


    void DrainMetal(float flareMult)
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(AllomancySkill.MetalType.Tin, baseMetalCostPerSecond * flareMult * Time.deltaTime);
    }
}
