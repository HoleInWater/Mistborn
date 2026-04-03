/* TitleLightFlicker.cs
 *
 * Makes point lights flicker realistically — torches, lanterns, window glows.
 * Attach to any GameObject with a Light component.
 * Uses layered Perlin noise for organic, non-repeating flicker.
 */

using UnityEngine;

public class TitleLightFlicker : MonoBehaviour
{
    public enum FlickerStyle { Torch, Lantern, WindowGlow, Ember }

    public FlickerStyle style = FlickerStyle.Torch;

    [Header("Override (0 = auto from style)")]
    public float intensityBase;
    public float intensityVariance;
    public float flickerSpeed;

    private Light _light;
    private float _seed;
    private float _baseIntensity;
    private Color _baseColor;

    void Start()
    {
        _light = GetComponent<Light>();
        if (_light == null) { enabled = false; return; }

        _seed = Random.Range(0f, 100f);
        _baseIntensity = _light.intensity;
        _baseColor = _light.color;

        // Auto-configure based on style if not overridden
        if (intensityBase <= 0f)
        {
            switch (style)
            {
                case FlickerStyle.Torch:
                    intensityBase = _baseIntensity;
                    intensityVariance = _baseIntensity * 0.35f;
                    flickerSpeed = 8f;
                    break;
                case FlickerStyle.Lantern:
                    intensityBase = _baseIntensity;
                    intensityVariance = _baseIntensity * 0.15f;
                    flickerSpeed = 4f;
                    break;
                case FlickerStyle.WindowGlow:
                    intensityBase = _baseIntensity;
                    intensityVariance = _baseIntensity * 0.08f;
                    flickerSpeed = 1.5f;
                    break;
                case FlickerStyle.Ember:
                    intensityBase = _baseIntensity;
                    intensityVariance = _baseIntensity * 0.5f;
                    flickerSpeed = 12f;
                    break;
            }
        }
    }

    void Update()
    {
        if (_light == null) return;

        float t = Time.time;

        // Layered noise for organic flicker
        float noise = Mathf.PerlinNoise(_seed + t * flickerSpeed, _seed * 0.7f);
        float noise2 = Mathf.PerlinNoise(_seed + t * flickerSpeed * 2.3f, _seed * 1.3f);
        float noise3 = Mathf.PerlinNoise(_seed + t * flickerSpeed * 0.4f, _seed * 2.1f);

        float combined = noise * 0.6f + noise2 * 0.25f + noise3 * 0.15f;

        _light.intensity = intensityBase + (combined - 0.5f) * 2f * intensityVariance;

        // Subtle color shift for torches (warmer on bright, cooler on dim)
        if (style == FlickerStyle.Torch || style == FlickerStyle.Ember)
        {
            float warmth = Mathf.Lerp(-0.05f, 0.05f, combined);
            _light.color = new Color(
                _baseColor.r + warmth,
                _baseColor.g + warmth * 0.3f,
                _baseColor.b - warmth * 0.2f);
        }

        // Occasional random spike (guttering torch)
        if (style == FlickerStyle.Torch && Random.Range(0f, 1f) < 0.003f)
            _light.intensity *= Random.Range(0.3f, 0.6f);
    }
}
