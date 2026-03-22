using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for Gradient operations
    /// </summary>
    public static class GradientUtils
    {
        /// <summary>
        /// Creates a simple two-color gradient
        /// </summary>
        public static Gradient TwoColor(Color startColor, Color endColor)
        {
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            
            colorKeys[0] = new GradientColorKey(startColor, 0f);
            colorKeys[1] = new GradientColorKey(endColor, 1f);
            
            alphaKeys[0] = new GradientAlphaKey(startColor.a, 0f);
            alphaKeys[1] = new GradientAlphaKey(endColor.a, 1f);
            
            gradient.colorKeys = colorKeys;
            gradient.alphaKeys = alphaKeys;
            return gradient;
        }

        /// <summary>
        /// Creates a gradient with multiple colors
        /// </summary>
        public static Gradient MultiColor(params Color[] colors)
        {
            if (colors == null || colors.Length == 0) return new Gradient();
            
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[colors.Length];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[colors.Length];
            
            for (int i = 0; i < colors.Length; i++)
            {
                float time = (float)i / (colors.Length - 1);
                colorKeys[i] = new GradientColorKey(colors[i], time);
                alphaKeys[i] = new GradientAlphaKey(colors[i].a, time);
            }
            
            gradient.colorKeys = colorKeys;
            gradient.alphaKeys = alphaKeys;
            return gradient;
        }

        /// <summary>
        /// Gets the color of a gradient at a specific time
        /// </summary>
        public static Color Evaluate(Gradient gradient, float time)
        {
            if (gradient == null) return Color.white;
            return gradient.Evaluate(time);
        }

        /// <summary>
        /// Creates a gradient that cycles through rainbow colors
        /// </summary>
        public static Gradient Rainbow(float alpha = 1f)
        {
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[7];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            
            Color[] rainbowColors = new Color[]
            {
                Color.red,
                new Color(1f, 0.5f, 0f), // orange
                Color.yellow,
                Color.green,
                Color.cyan,
                Color.blue,
                new Color(0.5f, 0f, 0.5f) // purple
            };
            
            for (int i = 0; i < 7; i++)
            {
                float time = (float)i / 6f;
                colorKeys[i] = new GradientColorKey(rainbowColors[i], time);
            }
            
            alphaKeys[0] = new GradientAlphaKey(alpha, 0f);
            alphaKeys[1] = new GradientAlphaKey(alpha, 1f);
            
            gradient.colorKeys = colorKeys;
            gradient.alphaKeys = alphaKeys;
            return gradient;
        }

        /// <summary>
        /// Creates a gradient that fades from transparent to opaque
        /// </summary>
        public static Gradient FadeIn(Color color, float duration = 1f)
        {
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            
            colorKeys[0] = new GradientColorKey(color, 0f);
            colorKeys[1] = new GradientColorKey(color, 1f);
            
            alphaKeys[0] = new GradientAlphaKey(0f, 0f);
            alphaKeys[1] = new GradientAlphaKey(color.a, 1f);
            
            gradient.colorKeys = colorKeys;
            gradient.alphaKeys = alphaKeys;
            return gradient;
        }

        /// <summary>
        /// Creates a gradient that fades from opaque to transparent
        /// </summary>
        public static Gradient FadeOut(Color color, float duration = 1f)
        {
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            
            colorKeys[0] = new GradientColorKey(color, 0f);
            colorKeys[1] = new GradientColorKey(color, 1f);
            
            alphaKeys[0] = new GradientAlphaKey(color.a, 0f);
            alphaKeys[1] = new GradientAlphaKey(0f, 1f);
            
            gradient.colorKeys = colorKeys;
            gradient.alphaKeys = alphaKeys;
            return gradient;
        }

        /// <summary>
        /// Creates a gradient that pulses between transparent and opaque
        /// </summary>
        public static Gradient Pulse(Color color, int pulseCount = 3)
        {
            Gradient gradient = new Gradient();
            int keyCount = pulseCount * 2 + 1;
            GradientColorKey[] colorKeys = new GradientColorKey[keyCount];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[keyCount];
            
            for (int i = 0; i < keyCount; i++)
            {
                float time = (float)i / (keyCount - 1);
                colorKeys[i] = new GradientColorKey(color, time);
                
                // Alternate between 0 and full alpha
                float alpha = (i % 2 == 0) ? color.a : 0f;
                alphaKeys[i] = new GradientAlphaKey(alpha, time);
            }
            
            gradient.colorKeys = colorKeys;
            gradient.alphaKeys = alphaKeys;
            return gradient;
        }

        /// <summary>
        /// Reverses a gradient (time direction)
        /// </summary>
        public static Gradient Reverse(Gradient gradient)
        {
            if (gradient == null) return gradient;
            
            Gradient reversed = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[gradient.colorKeys.Length];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[gradient.alphaKeys.Length];
            
            for (int i = 0; i < gradient.colorKeys.Length; i++)
            {
                GradientColorKey key = gradient.colorKeys[i];
                colorKeys[i] = new GradientColorKey(key.color, 1f - key.time);
            }
            
            for (int i = 0; i < gradient.alphaKeys.Length; i++)
            {
                GradientAlphaKey key = gradient.alphaKeys[i];
                alphaKeys[i] = new GradientAlphaKey(key.alpha, 1f - key.time);
            }
            
            // Sort by time
            System.Array.Sort(colorKeys, (a, b) => a.time.CompareTo(b.time));
            System.Array.Sort(alphaKeys, (a, b) => a.time.CompareTo(b.time));
            
            reversed.colorKeys = colorKeys;
            reversed.alphaKeys = alphaKeys;
            return reversed;
        }

        /// <summary>
        /// Blends two gradients together
        /// </summary>
        public static Gradient Blend(Gradient a, Gradient b, float blendFactor)
        {
            if (a == null) return b;
            if (b == null) return a;
            
            blendFactor = Mathf.Clamp01(blendFactor);
            Gradient blended = new Gradient();
            
            // Simple blend: evaluate both gradients at the same time and lerp
            // This is a simplified approach; more sophisticated blending would require key merging
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            
            colorKeys[0] = new GradientColorKey(Color.Lerp(a.Evaluate(0f), b.Evaluate(0f), blendFactor), 0f);
            colorKeys[1] = new GradientColorKey(Color.Lerp(a.Evaluate(1f), b.Evaluate(1f), blendFactor), 1f);
            
            alphaKeys[0] = new GradientAlphaKey(Mathf.Lerp(a.Evaluate(0f).a, b.Evaluate(0f).a, blendFactor), 0f);
            alphaKeys[1] = new GradientAlphaKey(Mathf.Lerp(a.Evaluate(1f).a, b.Evaluate(1f).a, blendFactor), 1f);
            
            blended.colorKeys = colorKeys;
            blended.alphaKeys = alphaKeys;
            return blended;
        }

        /// <summary>
        /// Creates a gradient from a texture (samples colors across width)
        /// </summary>
        public static Gradient FromTexture(Texture2D texture, bool vertical = false)
        {
            if (texture == null) return new Gradient();
            
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[texture.width];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[texture.width];
            
            for (int i = 0; i < texture.width; i++)
            {
                float u = (float)i / (texture.width - 1);
                float v = vertical ? u : 0.5f;
                Color color = texture.GetPixelBilinear(u, v);
                
                float time = (float)i / (texture.width - 1);
                colorKeys[i] = new GradientColorKey(color, time);
                alphaKeys[i] = new GradientAlphaKey(color.a, time);
            }
            
            gradient.colorKeys = colorKeys;
            gradient.alphaKeys = alphaKeys;
            return gradient;
        }
    }
}
