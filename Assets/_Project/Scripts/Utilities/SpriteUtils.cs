using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for handling common texture and sprite operations
    /// </summary>
    public class SpriteUtils : MonoBehaviour
    {
        /// <summary>
        /// Creates a sprite from a texture
        /// </summary>
        public static Sprite CreateSprite(Texture2D texture, Rect? rect = null, Vector2? pivot = null, float pixelsPerUnit = 100f)
        {
            if (texture == null) return null;
            
            Rect spriteRect = rect ?? new Rect(0, 0, texture.width, texture.height);
            Vector2 spritePivot = pivot ?? new Vector2(0.5f, 0.5f);
            
            return Sprite.Create(texture, spriteRect, spritePivot, pixelsPerUnit);
        }

        /// <summary>
        /// Creates a sprite from a texture with specified dimensions
        /// </summary>
        public static Sprite CreateSprite(Texture2D texture, int x, int y, int width, int height, float pixelsPerUnit = 100f)
        {
            if (texture == null) return null;
            
            Rect rect = new Rect(x, y, width, height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            
            return Sprite.Create(texture, rect, pivot, pixelsPerUnit);
        }

        /// <summary>
        /// Gets the pixels per unit setting for a sprite
        /// </summary>
        public static float GetPixelsPerUnit(Sprite sprite)
        {
            if (sprite == null) return 100f;
            return sprite.pixelsPerUnit;
        }

        /// <summary>
        /// Sets the pixels per unit setting for a sprite (by creating a new sprite)
        /// </summary>
        public static Sprite SetPixelsPerUnit(Sprite sprite, float pixelsPerUnit)
        {
            if (sprite == null) return null;
            
            Rect rect = sprite.rect;
            Vector2 pivot = sprite.pivot;
            
            return Sprite.Create(sprite.texture, rect, pivot, pixelsPerUnit);
        }

        /// <summary>
        /// Creates a sprite atlas from multiple textures
        /// </summary>
        public static Sprite[] CreateSpriteAtlas(Texture2D[] textures, int columns = 0, int rows = 0, float padding = 2f, float pixelsPerUnit = 100f)
        {
            if (textures == null || textures.Length == 0) return new Sprite[0];
            
            // If columns/rows not specified, calculate a roughly square layout
            if (columns <= 0) columns = Mathf.CeilToInt(Mathf.Sqrt(textures.Length));
            if (rows <= 0) rows = Mathf.CeilToInt((float)textures.Length / columns);
            
            // Calculate atlas dimensions
            int atlasWidth = 0;
            int atlasHeight = 0;
            int maxWidth = 0;
            int maxHeight = 0;
            
            // Find max dimensions
            foreach (var texture in textures)
            {
                if (texture.width > maxWidth) maxWidth = texture.width;
                if (texture.height > maxHeight) maxHeight = texture.height;
            }
            
            atlasWidth = maxWidth * columns + (int)(padding * (columns - 1));
            atlasHeight = maxHeight * rows + (int)(padding * (rows - 1));
            
            // Create atlas texture
            Texture2D atlas = new Texture2D(atlasWidth, atlasHeight, TextureFormat.RGBA32, false);
            
            // Pack textures and create sprites
            Sprite[] sprites = new Sprite[textures.Length];
            Rect[] rects = atlas.PackTextures(textures, (int)padding);
            
            for (int i = 0; i < textures.Length; i++)
            {
                if (i < rects.Length)
                {
                    sprites[i] = Sprite.Create(atlas, rects[i], new Vector2(0.5f, 0.5f), pixelsPerUnit);
                }
            }
            
            return sprites;
        }

        /// <summary>
        /// Gets the main texture of a sprite
        /// </summary>
        public static Texture2D GetTexture(Sprite sprite)
        {
            if (sprite == null) return null;
            return sprite.texture;
        }

        /// <summary>
        /// Creates a circular mask for a sprite
        /// </summary>
        public static Sprite CreateCircularSprite(Sprite sourceSprite)
        {
            if (sourceSprite == null) return null;
            
            // Create a new texture with alpha channel for circular mask
            Texture2D sourceTexture = sourceSprite.texture;
            Rect sourceRect = sourceSprite.rect;
            Vector2 sourcePivot = sourceSprite.pivot;
            float pixelsPerUnit = sourceSprite.pixelsPerUnit;
            
            // Create circular mask texture
            Texture2D maskTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.ARGB32, false);
            Color[] pixels = new Color[sourceTexture.width * sourceTexture.height];
            sourceTexture.GetPixels(0, 0, sourceTexture.width, sourceTexture.height, pixels);
            
            int centerX = sourceTexture.width / 2;
            int centerY = sourceTexture.height / 2;
            float radiusSquared = Mathf.Min(centerX, centerY) * Mathf.Min(centerX, centerY);
            
            for (int y = 0; y < sourceTexture.height; y++)
            {
                for (int x = 0; x < sourceTexture.width; x++)
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    float distanceSquared = dx * dx + dy * dy;
                    
                    if (distanceSquared > radiusSquared)
                    {
                        // Outside circle - set alpha to 0
                        int index = y * sourceTexture.width + x;
                        pixels[index] = new Color(pixels[index].r, pixels[index].g, pixels[index].b, 0f);
                    }
                }
            }
            
            maskTexture.SetPixels(pixels);
            maskTexture.Apply();
            
            // Apply mask to source texture
            Color[] resultPixels = new Color[sourceTexture.width * sourceTexture.height];
            sourceTexture.GetPixels(0, 0, sourceTexture.width, sourceTexture.height, resultPixels);
            
            for (int i = 0; i < resultPixels.Length; i++)
            {
                resultPixels[i] = new Color(
                    resultPixels[i].r,
                    resultPixels[i].g,
                    resultPixels[i].b,
                    resultPixels[i].a * maskTexture.GetPixels()[i].a
                );
            }
            
            Texture2D resultTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.ARGB32, false);
            resultTexture.SetPixels(resultPixels);
            resultTexture.Apply();
            
            return Sprite.Create(resultTexture, sourceRect, sourcePivot, pixelsPerUnit);
        }

        /// <summary>
        /// Creates a sprite from a RenderTexture
        /// </summary>
        public static Sprite CreateSpriteFromRenderTexture(RenderTexture renderTexture)
        {
            if (renderTexture == null) return null;
            
            // Create temporary texture to read render texture
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = renderTexture;
            
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            
            RenderTexture.active = prev;
            
            // Create sprite from texture
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Gets the bounds of a sprite in world space
        /// </summary>
        public static Bounds GetBounds(Sprite sprite, Vector2 scale = default(Vector2))
        {
            if (sprite == null)
                return new Bounds(Vector3.zero, Vector3.zero);
                
            if (scale == default(Vector2))
                scale = Vector2.one;
                
            // Sprite bounds are in local space, convert to world space
            Vector2 size = new Vector2(sprite.rect.width, sprite.rect.height) / sprite.pixelsPerUnit;
            Vector3 worldSize = new Vector3(size.x * scale.x, size.y * scale.y, 0f);
            return new Bounds(Vector3.zero, worldSize);
        }
    }
}