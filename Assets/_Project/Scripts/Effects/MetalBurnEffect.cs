using UnityEngine;

/// <summary>
/// Handles particle effects for metal burning and flaring.
/// Creates particle systems at runtime if prefabs are not assigned.
/// Attaches to the Allomancer and shows visual feedback for active metal burning.
/// </summary>
public class MetalBurnEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    public ParticleSystem burnParticles;
    public ParticleSystem flareParticles;

    [Header("References")]
    public Allomancer allomancer;

    [Header("Effects")]
    public float baseSize = 0.15f;
    public float flareSizeMultiplier = 2.5f;
    public float burnEmissionRate = 15f;
    public float flareEmissionRate = 40f;

    private bool wasFlaring = false;
    private AllomancySkill.MetalType lastMetal;

    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();

        // Create particle systems at runtime if none assigned
        if (burnParticles == null)
            burnParticles = CreateBurnParticleSystem("MetalBurnVFX");

        if (flareParticles == null)
            flareParticles = CreateFlareParticleSystem("MetalFlareVFX");
    }

    void Update()
    {
        if (allomancer == null) return;

        UpdateBurnEffect();
        UpdateFlareEffect();
    }

    void UpdateBurnEffect()
    {
        if (burnParticles == null) return;

        bool isBurning = allomancer.IsBurning();
        AllomancySkill.MetalType currentMetal = allomancer.GetCurrentMetal();

        if (isBurning && !burnParticles.isPlaying)
        {
            SetParticleColor(burnParticles, GetMetalColor(currentMetal));
            burnParticles.Play();
            lastMetal = currentMetal;
        }
        else if (!isBurning && burnParticles.isPlaying)
        {
            burnParticles.Stop();
        }

        // Update color if metal changed
        if (isBurning && currentMetal != lastMetal)
        {
            SetParticleColor(burnParticles, GetMetalColor(currentMetal));
            lastMetal = currentMetal;
        }

        // Adjust size based on reserve
        if (isBurning)
        {
            float reserve = allomancer.GetMetalReserve(currentMetal);
            float intensity = Mathf.Clamp01(reserve / 100f);
            var main = burnParticles.main;
            main.startSize = baseSize * (0.5f + intensity * 0.5f);

            var emission = burnParticles.emission;
            emission.rateOverTime = burnEmissionRate * intensity;
        }
    }

    void UpdateFlareEffect()
    {
        if (flareParticles == null) return;

        bool isFlaring = FlareManager.Instance != null && FlareManager.Instance.IsFlaring;

        if (isFlaring && !flareParticles.isPlaying)
        {
            AllomancySkill.MetalType currentMetal = allomancer.GetCurrentMetal();
            Color metalColor = GetMetalColor(currentMetal);
            metalColor.a = 0.8f;
            SetParticleColor(flareParticles, metalColor);
            flareParticles.Play();
        }
        else if (!isFlaring && flareParticles.isPlaying)
        {
            flareParticles.Stop();
        }

        // Pulse size based on flare intensity
        if (isFlaring && FlareManager.Instance != null)
        {
            float pulse = 0.8f + Mathf.PingPong(Time.time * 2f, 0.4f);
            var main = flareParticles.main;
            main.startSize = baseSize * flareSizeMultiplier * pulse * FlareManager.Instance.FlareIntensity;

            var emission = flareParticles.emission;
            emission.rateOverTime = flareEmissionRate * FlareManager.Instance.FlareIntensity;
        }

        wasFlaring = isFlaring;
    }

    public void EmitFlareBurst()
    {
        if (flareParticles != null)
            flareParticles.Emit(15);
    }

    public void EmitBurnBurst()
    {
        if (burnParticles != null)
            burnParticles.Emit(8);
    }

    // ── Runtime Particle Creation ────────────────────────────────────────────

    ParticleSystem CreateBurnParticleSystem(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0, 1f, 0); // Chest height

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.8f;
        main.startSpeed = 0.5f;
        main.startSize = baseSize;
        main.startColor = Color.white;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 50;
        main.loop = true;
        main.gravityModifier = -0.3f; // Float upward

        var emission = ps.emission;
        emission.rateOverTime = burnEmissionRate;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.6f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0.2f));

        // Set up renderer with default particle material
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = GetDefaultParticleMaterial();

        ps.Stop();
        return ps;
    }

    ParticleSystem CreateFlareParticleSystem(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0, 1f, 0);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 1.5f;
        main.startSize = baseSize * flareSizeMultiplier;
        main.startColor = Color.white;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 100;
        main.loop = true;
        main.gravityModifier = -0.5f;

        var emission = ps.emission;
        emission.rateOverTime = flareEmissionRate;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.yellow, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1.5f, 1, 0.1f));

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = GetDefaultParticleMaterial();

        ps.Stop();
        return ps;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void SetParticleColor(ParticleSystem ps, Color color)
    {
        var main = ps.main;
        main.startColor = color;
    }

    Material GetDefaultParticleMaterial()
    {
        // Try HDRP particle shader first, fall back to default
        Shader shader = Shader.Find("HDRP/Particles/Unlit")
                     ?? Shader.Find("Particles/Standard Unlit")
                     ?? Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_Blend", 0);   // Alpha blend
        return mat;
    }

    Color GetMetalColor(AllomancySkill.MetalType metal)
    {
        switch (metal)
        {
            case AllomancySkill.MetalType.Steel:    return new Color(0.3f, 0.5f, 1f);
            case AllomancySkill.MetalType.Iron:     return new Color(0.2f, 0.8f, 1f);
            case AllomancySkill.MetalType.Pewter:   return new Color(0.8f, 0.2f, 0.2f);
            case AllomancySkill.MetalType.Tin:      return new Color(1f, 1f, 0.5f);
            case AllomancySkill.MetalType.Zinc:     return new Color(1f, 0.5f, 0f);
            case AllomancySkill.MetalType.Brass:    return new Color(0.2f, 0.9f, 0.5f);
            case AllomancySkill.MetalType.Copper:   return new Color(0.2f, 0.8f, 0.2f);
            case AllomancySkill.MetalType.Bronze:   return new Color(0.8f, 0.3f, 0.8f);
            case AllomancySkill.MetalType.Atium:    return new Color(0.9f, 0.9f, 1f);
            case AllomancySkill.MetalType.Gold:     return new Color(1f, 0.85f, 0.2f);
            case AllomancySkill.MetalType.Electrum: return new Color(0.8f, 0.8f, 0.3f);
            case AllomancySkill.MetalType.Aluminum: return new Color(0.7f, 0.7f, 0.7f);
            case AllomancySkill.MetalType.Duralumin: return new Color(0.6f, 0.2f, 1f);
            case AllomancySkill.MetalType.Bendalloy: return new Color(0.2f, 0.6f, 1f);
            case AllomancySkill.MetalType.Cadmium:  return new Color(1f, 0.4f, 0.2f);
            case AllomancySkill.MetalType.Chromium:  return new Color(0.3f, 0.3f, 0.3f);
            case AllomancySkill.MetalType.Nicrosil:  return new Color(0.5f, 1f, 0.8f);
            default:                                 return Color.white;
        }
    }
}
