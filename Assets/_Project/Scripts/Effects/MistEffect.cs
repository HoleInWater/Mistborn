using UnityEngine;

/// <summary>
/// Atmospheric mist particles that appear at night.
/// Lore: The mists come every night in the Final Empire. They're thick,
/// reduce visibility, and most people fear them. Mistborn move freely through them.
/// Tin burning pierces the mist. Copper burning feels "warm" in the mist.
/// </summary>
public class MistEffect : MonoBehaviour
{
    [Header("Mist Particles")]
    public ParticleSystem mistParticleSystem;
    public float maxEmissionRate = 100f;
    public float mistHeight = 3f;
    public float mistRadius = 30f;

    [Header("Fog Settings")]
    public float maxFogDensity = 0.03f;
    public Color mistFogColor = new Color(0.6f, 0.6f, 0.7f);
    public float fogTransitionSpeed = 1f;

    [Header("References")]
    public Transform player;

    private float currentMistIntensity = 0f;
    private float targetFogDensity = 0f;
    private Color originalFogColor;
    private bool originalFogEnabled;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        originalFogColor = RenderSettings.fogColor;
        originalFogEnabled = RenderSettings.fog;

        if (mistParticleSystem == null)
            mistParticleSystem = CreateMistParticles();
    }

    void Update()
    {
        // Get mist intensity from DayNightCycle or WeatherSystem
        float targetIntensity = 0f;

        if (DayNightCycle.Instance != null)
            targetIntensity = DayNightCycle.Instance.GetMistIntensity();
        else if (WeatherGameplayIntegration.Instance != null)
            targetIntensity = WeatherGameplayIntegration.Instance.GetMistIntensity();

        currentMistIntensity = Mathf.Lerp(currentMistIntensity, targetIntensity, Time.deltaTime * fogTransitionSpeed);

        UpdateParticles();
        UpdateFog();
        FollowPlayer();
    }

    void UpdateParticles()
    {
        if (mistParticleSystem == null) return;

        var emission = mistParticleSystem.emission;
        emission.rateOverTime = maxEmissionRate * currentMistIntensity;

        if (currentMistIntensity > 0.05f && !mistParticleSystem.isPlaying)
            mistParticleSystem.Play();
        else if (currentMistIntensity <= 0.05f && mistParticleSystem.isPlaying)
            mistParticleSystem.Stop();
    }

    void UpdateFog()
    {
        targetFogDensity = maxFogDensity * currentMistIntensity;
        RenderSettings.fog = targetFogDensity > 0.001f;
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, Time.deltaTime * fogTransitionSpeed);
        RenderSettings.fogColor = Color.Lerp(originalFogColor, mistFogColor, currentMistIntensity);
    }

    void FollowPlayer()
    {
        if (player == null) return;
        transform.position = new Vector3(player.position.x, player.position.y + mistHeight * 0.5f, player.position.z);
    }

    ParticleSystem CreateMistParticles()
    {
        GameObject go = new GameObject("MistParticles");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 8f;
        main.startSpeed = 0.3f;
        main.startSize = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startColor = new Color(0.7f, 0.7f, 0.8f, 0.15f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;
        main.loop = true;
        main.gravityModifier = -0.05f;

        var emission = ps.emission;
        emission.rateOverTime = 0;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(mistRadius * 2f, mistHeight, mistRadius * 2f);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.15f, 0.3f), new GradientAlphaKey(0.15f, 0.7f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        Shader shader = Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
        renderer.material = new Material(shader);

        ps.Stop();
        return ps;
    }

    public float GetMistIntensity() => currentMistIntensity;
}
