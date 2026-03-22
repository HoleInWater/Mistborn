using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Extension methods for Color struct
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Returns a new color with the specified alpha
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
        }

        /// <summary>
        /// Returns a grayscale version of the color
        /// </summary>
        public static Color Grayscale(this Color color)
        {
            float gray = color.grayscale;
            return new Color(gray, gray, gray, color.a);
        }

        /// <summary>
        /// Returns the complementary color
        /// </summary>
        public static Color Complementary(this Color color)
        {
            return new Color(1f - color.r, 1f - color.g, 1f - color.b, color.a);
        }

        /// <summary>
        /// Linearly interpolates between this color and another
        /// </summary>
        public static Color LerpTo(this Color color, Color target, float t)
        {
            return Color.Lerp(color, target, t);
        }

        /// <summary>
        /// Linearly interpolates between this color and another (unclamped)
        /// </summary>
        public static Color LerpToUnclamped(this Color color, Color target, float t)
        {
            return new Color(
                Mathf.LerpUnclamped(color.r, target.r, t),
                Mathf.LerpUnclamped(color.g, target.g, t),
                Mathf.LerpUnclamped(color.b, target.b, t),
                Mathf.LerpUnclamped(color.a, target.a, t)
            );
        }

        /// <summary>
        /// Returns the luminance/brightness of the color
        /// </summary>
        public static float Luminance(this Color color)
        {
            return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
        }

        /// <summary>
        /// Returns a random color with the same alpha
        /// </summary>
        public static Color Randomize(this Color color)
        {
            return new Color(Random.value, Random.value, Random.value, color.a);
        }

        /// <summary>
        /// Converts color to hex string (without alpha)
        /// </summary>
        public static string ToHex(this Color color)
        {
            Color32 c = color;
            return $"#{c.r:X2}{c.g:X2}{c.b:X2}";
        }

        /// <summary>
        /// Converts color to hex string with alpha
        /// </summary>
        public static string ToHexWithAlpha(this Color color)
        {
            Color32 c = color;
            return $"#{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}";
        }

        /// <summary>
        /// Parses a hex color string to Color
        /// </summary>
        public static Color FromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex))
            {
                Debug.LogError("ColorExtensions: Hex string is null or empty");
                return Color.white;
            }
            
            hex = hex.Replace("#", "");
            
            if (hex.Length == 6)
            {
                hex += "FF";
            }
            
            if (hex.Length != 8)
            {
                Debug.LogError("ColorExtensions: Invalid hex string length");
                return Color.white;
            }
            
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            
            return new Color32(r, g, b, a);
        }
    }
}
