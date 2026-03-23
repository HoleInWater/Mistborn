using UnityEngine;

/// <summary>
/// Handles particle effects for metal burning and flaring
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class MetalBurnEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    public ParticleSystem burnParticles;
    public ParticleSystem flareParticles;
    
    [Header("References")]
    public Allomancer allomancer;
    
    [Header("Effects")]
    public Gradient burnColorGradient;
    public Gradient flareColorGradient;
    public float baseSize = 1f;
    public float flareSizeMultiplier = 2f;
    
    void Awake()
    {
        if (burnParticles == null)
            burnParticles = GetComponent<ParticleSystem>();
        
        if (flareParticles == null)
        {
            // Create a separate particle system for flare effects
            GameObject flareObj = new GameObject("FlareParticles");
            flareObj.transform.SetParent(transform);
            flareParticles = flareObj.AddComponent<ParticleSystem>();
            
            // Copy settings from burn particles
            var main = burnParticles.main;
            var flareMain = flareParticles.main;
            flareMain.duration = main.duration;
            flareMain.looping = main.looping;
            flareMain.startLifetime = main.startLifetime;
            flareMain.startSpeed = main.startSpeed;
            flareMain.startSize = main.startSize;
            flareMain.startRotation = main.startRotation;
            flareMain.startColor = main.startColor;
            flareMain.gravityModifier = main.gravityModifier;
        }
    }
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponent<Allomancer>();
        
        // Start with burn particles off
        var burnMain = burnParticles.main;
        burnMain.looping = false;
        
        var flareMain = flareParticles.main;
        flareMain.looping = false;
    }
    
    void Update()
    {
        UpdateBurnEffect();
        UpdateFlareEffect();
    }
    
    void UpdateBurnEffect()
    {
        bool isBurning = allomancer != null && allomancer.IsBurning();
        
        var main = burnParticles.main;
        if (isBurning != burnParticles.isPlaying)
        {
            if (isBurning)
            {
                burnParticles.Play();
                // Set color based on current metal
                if (allomancer != null && burnColorGradient != null)
                {
                    float t = (float)allomancer.GetCurrentMetal() / 15f; // 16 metals = 0-15 range
                    main.startColor = burnColorGradient.Evaluate(t);
                }
            }
            else
            {
                burnParticles.Stop();
            }
        }
        
        // Adjust size based on burn intensity
        if (isBurning && allomancer != null)
        {
            float reserve = allomancer.GetMetalReserve(allomancer.GetCurrentMetal());
            float intensity = Mathf.Clamp01(reserve / 100f);
            main.startSize = baseSize * (0.5f + intensity * 0.5f); // 0.5x to 1x size
        }
    }
    
    void UpdateFlareEffect()
    {
        bool isFlaring = false;
        if (allomancer != null)
        {
            // Check if either iron or steel is flaring
            var flareMan = FindObjectOfType<FlareManager>();
            if (flareMan != null)
            {
                isFlaring = flareMan.IsIronFlaring || flareMan.IsSteelFlaring;
            }
        }
        
        var main = flareParticles.main;
        if (isFlaring != flareParticles.isPlaying)
        {
            if (isFlaring)
            {
                flareParticles.Play();
                // Set color based on which metal is flaring
                if (allomancer != null && flareColorGradient != null)
                {
                    float t = 0f;
                    var flareMan = FindObjectOfType<FlareManager>();
                    if (flareMan != null)
                    {
                        if (flareMan.IsIronFlaring)
                            t = (float)AllomancySkill.MetalType.Iron / 15f;
                        else if (flareMan.IsSteelFlaring)
                            t = (float)AllomancySkill.MetalType.Steel / 15f;
                    }
                    main.startColor = flareColorGradient.Evaluate(t);
                }
                
                // Increase size when flaring
                main.startSize = baseSize * flareSizeMultiplier;
            }
            else
            {
                flareParticles.Stop();
            }
        }
    }
    
    // Optional: Emit burst when flaring starts/stops
    public void EmitFlareBurst()
    {
        if (flareParticles != null)
        {
            flareParticles.Emit(10);
        }
    }
    
    // Optional: Emit burst when burning starts
    public void EmitBurnBurst()
    {
        if (burnParticles != null)
        {
            burnParticles.Emit(5);
        }
    }
}