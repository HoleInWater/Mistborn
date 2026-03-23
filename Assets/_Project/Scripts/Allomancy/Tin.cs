using UnityEngine;

/// <summary>
/// Implements the Tin Allomancy ability (enhanced senses).
/// </summary>
public class Tin : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Base metal burn rate per second")]
    public float metalCostPerSecond = 1f;
    [Tooltip("Field of view increase when burning Tin (degrees)")]
    public float fovIncrease = 10f;
    [Tooltip("Audio volume multiplier when burning Tin")]
    public float audioVolumeMultiplier = 1.5f;
    [Tooltip("Cooldown time in seconds after stopping burn")]
    public float burnCooldown = 0.1f;
    
    [Header("References")]
    public Camera playerCamera;
    public AudioListener audioListener;
    public Allomancer allomancer;
    
    private bool isBurning = false;
    private float cooldownTimer = 0f;
    private float originalFOV;
    private float originalAudioVolume;
    
    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        if (audioListener == null)
            audioListener = FindObjectOfType<AudioListener>();
        
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
        
        originalFOV = playerCamera.fieldOfView;
        originalAudioVolume = audioListener.volume;
    }
    
    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
        
        // Check if we can burn tin
        if (allomancer != null && !allomancer.canBurnMetal)
        {
            if (isBurning) StopBurning();
            return;
        }
        
        // T key to burn Tin (as per common Allomancy key bindings)
        if (Input.GetKeyDown(KeyCode.T) && cooldownTimer <= 0f)
        {
            if (!isBurning) StartBurning();
        }
        
        if (Input.GetKeyUp(KeyCode.T))
        {
            if (isBurning) StopBurning();
        }
        
        // Continuous metal drain while burning
        if (isBurning)
        {
            DrainMetal();
        }
    }
    
    void StartBurning()
    {
        if (isBurning) return;
        isBurning = true;
        cooldownTimer = burnCooldown;
        allomancer.StartBurning(AllomancySkill.MetalType.Tin);
        ApplyTinEffects();
    }
    
    void StopBurning()
    {
        if (!isBurning) return;
        isBurning = false;
        cooldownTimer = burnCooldown;
        allomancer.StopBurning();
        ResetTinEffects();
    }
    
    void ApplyTinEffects()
    {
        if (playerCamera != null)
            playerCamera.fieldOfView = originalFOV + fovIncrease;
        
        if (audioListener != null)
            audioListener.volume = originalAudioVolume * audioVolumeMultiplier;
    }
    
    void ResetTinEffects()
    {
        if (playerCamera != null)
            playerCamera.fieldOfView = originalFOV;
        
        if (audioListener != null)
            audioListener.volume = originalAudioVolume;
    }
    
    void DrainMetal()
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(AllomancySkill.MetalType.Tin, metalCostPerSecond * Time.deltaTime);
    }
}