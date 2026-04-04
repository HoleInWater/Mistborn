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
    public Metallurgist metallurgist;
    
    [Header("Effects")]
    public Gradient burnColorGradient;
    public Gradient flareColorGradient;
    public float baseSize = 1f;
    public float flareSizeMultiplier = 2f;
    
    void Awake()
    {
        // [AGENT REVIEW] Permanently disable particles per user request
        if (burnParticles) { burnParticles.Stop(); var em = burnParticles.emission; em.enabled = false; }
        if (flareParticles) { flareParticles.Stop(); var em = flareParticles.emission; em.enabled = false; }
        
        // Ensure any default particle systems automatically added to this GameObject are killed
        var localPs = GetComponent<ParticleSystem>();
        if (localPs) { localPs.Stop(); var localEm = localPs.emission; localEm.enabled = false; }
        
        this.enabled = false; // Disable ONLY the script, not the entire Player GameObject!
    }

    void Start()
    {
        if (metallurgist == null)
            metallurgist = GetComponent<Metallurgist>();
        
        // Start with burn particles off
        var burnMain = burnParticles.main;
        burnMain.loop = false;
        
        if (flareParticles != null)
        {
            var flareMain = flareParticles.main;
            flareMain.loop = false;
        }
    }
    
    void Update()
    {
        UpdateBurnEffect();
        UpdateFlareEffect();
    }
    
    void UpdateBurnEffect()
    {
        // [AGENT REVIEW] Particle emissions disabled globally per user request
        return;

        if (metallurgist == null) return;

        bool isBurning = metallurgist.IsBurning();
        var main = burnParticles.main;

        if (isBurning != burnParticles.isPlaying)
        {
            if (isBurning)
            {
                burnParticles.Play();
                // Set color based on current metal
                if (burnColorGradient != null)
                {
                    float t = (float)metallurgist.GetCurrentMetal() / 15f; 
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
            float reserve = metallurgist.GetMetalReserve(metallurgist.GetCurrentMetal());
            float intensity = Mathf.Clamp01(reserve / 100f);
            main.startSize = baseSize * (0.5f + intensity * 0.5f);
        }
    }
    
    void UpdateFlareEffect()
    {
        // [AGENT REVIEW] Particle emissions disabled globally per user request
        return;

        if (metallurgist == null || flareParticles == null) return;

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
                    float t = (float)metallurgist.GetCurrentMetal() / 15f;
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
