/* ParticleTextureGenerator.cs
 *
 * Generates soft circular particle textures for mist, smoke, ash, etc.
 * These replace the default hard-edged square particle with smooth
 * radial falloff circles that look like actual fog/smoke.
 *
 * Run via: Ashwalker → Effects → Generate Particle Textures
 * Creates textures at Assets/_Project/Textures/Particles/
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ParticleTextureGenerator
{
    [MenuItem("Ashwalker/Effects/Generate Particle Textures")]
    public static void Generate()
    {
        string folder = "Assets/_Project/Textures/Particles";
        EnsureFolder(folder);

        // Soft circle — main mist/fog/smoke texture
        CreateSoftCircle(folder, "SoftCircle_128", 128, 2.5f);

        // Softer, larger falloff for distant fog
        CreateSoftCircle(folder, "SoftCircle_64", 64, 3f);

        // Harder edge for ash flakes
        CreateSoftCircle(folder, "AshFlake_32", 32, 1.5f);

        AssetDatabase.Refresh();

        Debug.Log("[ParticleTextureGenerator] Created particle textures at " + folder);
        EditorUtility.DisplayDialog("Particle Textures Generated",
            $"Created at {folder}:\n\n" +
            "SoftCircle_128.png — mist, fog, smoke\n" +
            "SoftCircle_64.png — distant fog\n" +
            "AshFlake_32.png — ash particles\n\n" +
            "Now rebuild the Title Sequence scene to apply them.",
            "OK");
    }

    static void CreateSoftCircle(string folder, string name, int size, float falloffPower)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Smooth radial falloff
                float alpha = Mathf.Clamp01(1f - Mathf.Pow(dist, falloffPower));

                // Slight variation for organic feel
                alpha *= Mathf.Lerp(0.85f, 1f,
                    Mathf.PerlinNoise(x * 0.1f, y * 0.1f));

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        string path = $"{folder}/{name}.png";
        System.IO.File.WriteAllBytes(path, png);
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path);

        // Set texture import settings for particles
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
        }
    }

    static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
