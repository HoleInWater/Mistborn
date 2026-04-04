/* MistSystem.cs
 *
 * Lore-accurate Ashara mist system.
 *
 * From the books:
 *   - Thick, ground-covering fog — only a few feet deep
 *   - Obscures vision at night (can't see more than 10 feet ahead)
 *   - Rises at night, disappears during the day
 *   - Physical manifestation of The Warden's power
 *   - Clings to the surface, low-lying
 *   - Does NOT tower into the sky like clouds
 *
 * This system creates multiple layered particle emitters at ground level
 * with procedurally generated soft textures (no squares). It generates
 * its own soft circle texture at runtime if none exists.
 *
 * Attach to any GameObject. Call Initialize() or it auto-starts.
 */

using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class MistSystem : MonoBehaviour
{
    [Header("Mist Coverage")]
    [Tooltip("Radius of the mist coverage area")]
    public float coverageRadius = 40f;
    [Tooltip("Maximum height of mist above ground (lore: few feet)")]
    public float mistHeight = 3f;
    [Tooltip("Number of particle emitter layers")]
    public int layerCount = 3;

    [Header("Particle Settings")]
    public int maxParticlesPerLayer = 60;
    public float emissionRate = 8f;
    public float particleLifetime = 15f;
    public float particleSize = 6f;
    public float particleSizeVariation = 3f;

    [Header("Movement")]
    public float driftSpeed = 0.08f;
    public float noiseStrength = 0.12f;
    public float noiseFrequency = 0.12f;

    [Header("Appearance")]
    public Color mistColor = new Color(0.75f, 0.78f, 0.85f, 0.12f);
    public Color mistColorBright = new Color(0.88f, 0.88f, 0.92f, 0.18f);

    [Header("The Warden's Pulse")]
    [Tooltip("Subtle opacity pulse — The Warden's heartbeat")]
    public float pulseFrequency = 0.12f;
    public float pulseAmplitude = 0.03f;

    private List<ParticleSystem> layers = new List<ParticleSystem>();
    private Material mistMaterial;
    private Texture2D softTexture;
    private bool initialized;

    void OnEnable()
    {
        if (!initialized) Initialize();
    }

    public void Initialize()
    {
        if (initialized) return;
        initialized = true;

        CreateSoftTexture();
        CreateMistMaterial();
        CreateLayers();
    }

    /// <summary>
    /// Generates a 128x128 soft radial circle texture at runtime.
    /// This is what makes particles look like fog instead of squares.
    /// Radial falloff with Perlin noise for organic edges.
    /// </summary>
    void CreateSoftTexture()
    {
        int size = 128;
        softTexture = new Texture2D(size, size, TextureFormat.RGBA32, true);
        softTexture.name = "MistSoftCircle";
        softTexture.wrapMode = TextureWrapMode.Clamp;
        softTexture.filterMode = FilterMode.Bilinear;

        float center = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Smooth radial falloff — soft edges, no hard square boundary
                float alpha = Mathf.Clamp01(1f - dist * dist);
                // Extra smoothing at the edge
                alpha = alpha * alpha;
                // Perlin noise for organic variation
                alpha *= Mathf.Lerp(0.7f, 1f, Mathf.PerlinNoise(x * 0.05f + 7.3f, y * 0.05f + 2.1f));

                softTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        softTexture.Apply(true);
    }

    /// <summary>
    /// Creates an HDRP-compatible particle material using the soft texture.
    /// Tries HDRP particle shaders first, falls back to standard.
    /// </summary>
    void CreateMistMaterial()
    {
        // Try HDRP particle shaders
        string[] shaderNames = {
            "HDRP/Particles/Unlit",
            "HDRP/Particles/Lit",
            "Particles/Standard Unlit",
            "Universal Render Pipeline/Particles/Unlit",
            "Sprites/Default",
            "Unlit/Transparent",
        };

        Shader shader = null;
        foreach (var sn in shaderNames)
        {
            shader = Shader.Find(sn);
            if (shader != null) break;
        }

        if (shader == null)
        {
            // Absolute fallback: use whatever the default particle uses
            var tempPS = new GameObject("_tempPS").AddComponent<ParticleSystem>();
            var tempRenderer = tempPS.GetComponent<ParticleSystemRenderer>();
            if (tempRenderer.sharedMaterial != null)
                shader = tempRenderer.sharedMaterial.shader;
            DestroyImmediate(tempPS.gameObject);
        }

        if (shader == null)
        {
            Debug.LogError("[MistSystem] Cannot find any particle shader!");
            return;
        }

        mistMaterial = new Material(shader);
        mistMaterial.name = "MistMaterial";

        // Assign the soft texture
        if (mistMaterial.HasProperty("_MainTex"))
            mistMaterial.SetTexture("_MainTex", softTexture);
        if (mistMaterial.HasProperty("_BaseMap"))
            mistMaterial.SetTexture("_BaseMap", softTexture);
        if (mistMaterial.HasProperty("_BaseColorMap"))
            mistMaterial.SetTexture("_BaseColorMap", softTexture);
        if (mistMaterial.HasProperty("_UnlitColorMap"))
            mistMaterial.SetTexture("_UnlitColorMap", softTexture);

        // Set color
        mistMaterial.color = Color.white;
        if (mistMaterial.HasProperty("_BaseColor"))
            mistMaterial.SetColor("_BaseColor", Color.white);

        // Ensure transparency/additive blending
        mistMaterial.renderQueue = 3000; // Transparent
    }

    /// <summary>
    /// Creates layered particle emitters at different heights.
    /// Multiple layers overlap to create dense, realistic fog.
    /// </summary>
    void CreateLayers()
    {
        for (int i = 0; i < layerCount; i++)
        {
            float layerHeight = (float)i / Mathf.Max(1, layerCount - 1) * mistHeight;
            float layerAlpha = Mathf.Lerp(1f, 0.5f, (float)i / layerCount); // thicker at ground

            var layerObj = new GameObject($"MistLayer_{i}");
            layerObj.transform.SetParent(transform);
            layerObj.transform.localPosition = new Vector3(0f, layerHeight, 0f);

            var ps = layerObj.AddComponent<ParticleSystem>();
            ConfigureParticleSystem(ps, layerAlpha, i);

            // Set the material with soft texture
            var renderer = layerObj.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && mistMaterial != null)
            {
                renderer.sharedMaterial = mistMaterial;
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.sortMode = ParticleSystemSortMode.Distance;
                renderer.minParticleSize = 0f;
                renderer.maxParticleSize = 0.5f;
            }

            layers.Add(ps);
        }
    }

    void ConfigureParticleSystem(ParticleSystem ps, float alphaMultiplier, int layerIndex)
    {
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(particleLifetime * 0.8f, particleLifetime * 1.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(driftSpeed * 0.5f, driftSpeed * 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(
            particleSize - particleSizeVariation,
            particleSize + particleSizeVariation);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.maxParticles = maxParticlesPerLayer;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f; // mist doesn't fall

        // Color: tint with layer-specific alpha
        Color layerColor = Color.Lerp(mistColor, mistColorBright, (float)layerIndex / Mathf.Max(1, layerCount - 1));
        layerColor.a *= alphaMultiplier;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(layerColor.r * 0.9f, layerColor.g * 0.9f, layerColor.b, layerColor.a * 0.7f),
            layerColor);

        // Emission
        var emission = ps.emission;
        emission.rateOverTime = emissionRate;

        // Shape: flat box at ground level
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(coverageRadius * 2f, 0.5f, coverageRadius * 2f);

        // Noise: organic swirling movement
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = noiseStrength;
        noise.frequency = noiseFrequency;
        noise.scrollSpeed = 0.04f;
        noise.octaveCount = 2;
        noise.damping = true;

        // Slow rotation for organic feel
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);

        // Size over lifetime: grow then shrink
        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.3f),
            new Keyframe(0.2f, 0.9f),
            new Keyframe(0.7f, 1f),
            new Keyframe(1f, 0.2f)));

        // Alpha over lifetime: fade in and out
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.7f, 0.15f),
                new GradientAlphaKey(0.6f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);
    }

    void Update()
    {
        if (!initialized || layers.Count == 0) return;

        // The Warden's pulse — subtle global opacity shift
        float pulse = 1f + Mathf.Sin(Time.time * pulseFrequency * Mathf.PI * 2f) * pulseAmplitude;

        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i] == null) continue;
            var em = layers[i].emission;
            em.rateOverTime = emissionRate * pulse;
        }
    }

    void OnDestroy()
    {
        if (softTexture != null) DestroyImmediate(softTexture);
        if (mistMaterial != null) DestroyImmediate(mistMaterial);
    }
}
