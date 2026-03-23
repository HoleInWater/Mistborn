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
            flareMain.loop = main.loop;
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
        burnMain.loop = false;
        
        var flareMain = flareParticles.main;
        flareMain.loop = false;
    }
    
    void Update()
    {
        UpdateBurnEffect();
        UpdateFlareEffect();
    }
    
    void UpdateBurnEffect()
    {
        if (allomancer == null) return;

        bool isBurning = allomancer.IsBurning();
        var main = burnParticles.main;

        if (isBurning != burnParticles.isPlaying)
        {
            if (isBurning)
            {
                burnParticles.Play();
                // Set color based on current metal
                if (burnColorGradient != null)
                {
                    float t = (float)allomancer.GetCurrentMetal() / 15f; 
                    main.startColor = burnColorGradient.Evaluate(t);
                }
            }
            else
            {
                burnParticles.Stop();
            }
        }
        
        // Adjust size based on reserve
        if (isBurning)
        {
            float reserve = allomancer.GetMetalReserve(allomancer.GetCurrentMetal());
            float intensity = Mathf.Clamp01(reserve / 100f);
            main.startSize = baseSize * (0.5f + intensity * 0.5f);
        }
    }
    
    void UpdateFlareEffect()
    {
        if (allomancer == null) return;

        // Check global flare state from FlareManager
        bool isFlaring = FlareManager.Instance != null && FlareManager.Instance.IsFlaring;
        
        var main = flareParticles.main;
        if (isFlaring != flareParticles.isPlaying)
        {
            if (isFlaring)
            {
                flareParticles.Play();
                // Set color based on active metal (assuming flaring corresponds to active metal)
                if (flareColorGradient != null)
                {
                    float t = (float)allomancer.GetCurrentMetal() / 15f;
                    main.startColor = flareColorGradient.Evaluate(t);
                }
                
                main.startSize = baseSize * flareSizeMultiplier;
            }
            else
            {
                flareParticles.Stop();
            }
        }

        // Pulse size or intensity based on flare intensity if active
        if (isFlaring && FlareManager.Instance != null)
        {
            float pulse = 0.8f + Mathf.PingPong(Time.time * 2f, 0.4f);
            main.startSize = baseSize * flareSizeMultiplier * pulse * FlareManager.Instance.FlareIntensity;
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
