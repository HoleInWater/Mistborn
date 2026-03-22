using UnityEngine;

namespace MistbornGame.Utilities
{
    public static class ColorUtils
    {
        /// <summary>
        /// Creates a color with specified alpha while keeping RGB values
        /// </summary>
        public static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
        }

        /// <summary>
        /// Creates a grayscale color from a value
        /// </summary>
        public static Color Gray(float value)
        {
            float clamped = Mathf.Clamp01(value);
            return new Color(clamped, clamped, clamped);
        }

        /// <summary>
        /// Creates a color from HSV values
        /// </summary>
        public static Color FromHSV(float h, float s, float v)
        {
            return Color.HSVToRGB(h, s, v);
        }

        /// <summary>
        /// Gets the luminance/brightness of a color
        /// </summary>
        public static float GetLuminance(Color color)
        {
            return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
        }

        /// <summary>
        /// Returns the complementary color
        /// </summary>
        public static Color Complementary(Color color)
        {
            return new Color(1f - color.r, 1f - color.g, 1f - color.b);
        }

        /// <summary>
        /// Linearly interpolates between two colors
        /// </summary>
        public static Color LerpUnclamped(Color a, Color b, float t)
        {
            return new Color(
                Mathf.LerpUnclamped(a.r, b.r, t),
                Mathf.LerpUnclamped(a.g, b.g, t),
                Mathf.LerpUnclamped(a.b, b.b, t),
                Mathf.LerpUnclamped(a.a, b.a, t)
            );
        }

        /// <summary>
        /// Creates a random color with optional brightness control
        /// </summary>
        public static Color RandomColor(float minBrightness = 0f, float maxBrightness = 1f)
        {
            float brightness = Random.Range(minBrightness, maxBrightness);
            return new Color(Random.value, Random.value, Random.value) * brightness;
        }
    }
}