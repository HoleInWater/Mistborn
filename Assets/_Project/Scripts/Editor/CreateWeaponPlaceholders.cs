using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Ashwalker → Create Weapon Placeholders
/// Generates simple primitive-based weapon prefabs so weapons appear in-hand
/// immediately. Replace the prefab's mesh with a real model later — the
/// WeaponData hand offsets stay the same.
///
/// Wooden parts (handles, shafts) get a procedural wood-grain texture.
/// Re-run this menu item after any changes to regenerate all prefabs.
/// </summary>
public static class CreateWeaponPlaceholders
{
    const string PREFAB_FOLDER   = "Assets/_Project/Prefabs/Weapons";
    const string TEXTURE_FOLDER  = "Assets/_Project/Textures";
    const string WOOD_TEX_PATH   = "Assets/_Project/Textures/WoodGrain.png";
    const string WOOD_MAT_PATH   = "Assets/_Project/Textures/WoodGrain.mat";

    static Material _woodMat;   // cached for the current Create() run

    [MenuItem("Ashwalker/Weapons/Create Weapon Placeholders")]
    public static void Create()
    {
        EnsureFolder("Assets/_Project/Prefabs");
        EnsureFolder(PREFAB_FOLDER);
        EnsureFolder(TEXTURE_FOLDER);

        _woodMat = BuildWoodMaterial();

        MakeDagger();
        MakeSword();
        MakeAxe();
        MakeSpear();
        MakeMace();
        MakeBloodbruteBlade();

        _woodMat = null;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Auto-assign prefabs to WeaponData assets
        AssignPrefabsToWeaponData();

        EditorUtility.DisplayDialog("Done",
            "Placeholder weapon prefabs created in " + PREFAB_FOLDER + "\n\n" +
            "Prefabs have been auto-assigned to your WeaponData assets.\n" +
            "Replace the mesh with a real model whenever you're ready.",
            "OK");
    }

    // ── Weapon builders ───────────────────────────────────────────────────────

    static void MakeDagger()
    {
        var root = new GameObject("Dagger");

        MakePart(root, "Blade",  PrimitiveType.Cube,
            new Vector3(0.04f, 0.28f, 0.04f), new Vector3(0, 0.17f, 0), Color.gray);
        MakePart(root, "Guard",  PrimitiveType.Cube,
            new Vector3(0.14f, 0.03f, 0.06f), new Vector3(0, 0.03f, 0), new Color(0.5f, 0.4f, 0.2f));
        MakeWoodPart(root, "Handle", PrimitiveType.Cylinder,
            new Vector3(0.04f, 0.09f, 0.04f), new Vector3(0, -0.06f, 0));

        SavePrefab(root, "Dagger");
        GameObject.DestroyImmediate(root);
    }

    static void MakeSword()
    {
        var root = new GameObject("IronSword");

        MakePart(root, "Blade",  PrimitiveType.Cube,
            new Vector3(0.05f, 0.55f, 0.05f), new Vector3(0, 0.35f, 0), Color.gray);
        MakePart(root, "Guard",  PrimitiveType.Cube,
            new Vector3(0.22f, 0.04f, 0.07f), new Vector3(0, 0.06f, 0), new Color(0.55f, 0.45f, 0.2f));
        MakeWoodPart(root, "Handle", PrimitiveType.Cylinder,
            new Vector3(0.05f, 0.12f, 0.05f), new Vector3(0, -0.08f, 0));
        MakePart(root, "Pommel", PrimitiveType.Sphere,
            new Vector3(0.09f, 0.09f, 0.09f), new Vector3(0, -0.22f, 0), new Color(0.55f, 0.45f, 0.2f));

        SavePrefab(root, "IronSword");
        GameObject.DestroyImmediate(root);
    }

    static void MakeAxe()
    {
        var root = new GameObject("ObsidianAxe");

        MakeWoodPart(root, "Shaft", PrimitiveType.Cylinder,
            new Vector3(0.05f, 0.3f, 0.05f), new Vector3(0, 0.05f, 0));
        MakePart(root, "Head", PrimitiveType.Cube,
            new Vector3(0.22f, 0.18f, 0.06f), new Vector3(0.1f, 0.3f, 0), new Color(0.1f, 0.1f, 0.12f));

        SavePrefab(root, "ObsidianAxe");
        GameObject.DestroyImmediate(root);
    }

    static void MakeSpear()
    {
        var root = new GameObject("Spear");

        MakeWoodPart(root, "Shaft", PrimitiveType.Cylinder,
            new Vector3(0.04f, 0.7f, 0.04f), new Vector3(0, 0.3f, 0));
        MakePart(root, "Tip", PrimitiveType.Cube,
            new Vector3(0.05f, 0.2f, 0.05f), new Vector3(0, 1.0f, 0), Color.gray);

        SavePrefab(root, "Spear");
        GameObject.DestroyImmediate(root);
    }

    static void MakeMace()
    {
        var root = new GameObject("PewterMace");

        MakeWoodPart(root, "Handle", PrimitiveType.Cylinder,
            new Vector3(0.05f, 0.22f, 0.05f), new Vector3(0, 0.0f, 0));
        MakePart(root, "Head", PrimitiveType.Sphere,
            new Vector3(0.18f, 0.18f, 0.18f), new Vector3(0, 0.32f, 0), new Color(0.6f, 0.6f, 0.65f));

        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            MakePart(root, "Flange" + i, PrimitiveType.Cube,
                new Vector3(0.06f, 0.14f, 0.04f),
                new Vector3(Mathf.Cos(angle) * 0.14f, 0.32f, Mathf.Sin(angle) * 0.14f),
                new Color(0.55f, 0.55f, 0.6f));
        }

        SavePrefab(root, "PewterMace");
        GameObject.DestroyImmediate(root);
    }

    static void MakeBloodbruteBlade()
    {
        var root = new GameObject("BloodbruteBlade");

        MakePart(root, "Blade",  PrimitiveType.Cube,
            new Vector3(0.18f, 1.1f, 0.07f), new Vector3(0, 0.7f, 0), new Color(0.45f, 0.45f, 0.5f));
        MakePart(root, "Guard",  PrimitiveType.Cube,
            new Vector3(0.38f, 0.07f, 0.1f), new Vector3(0, 0.1f, 0), new Color(0.4f, 0.3f, 0.15f));
        MakeWoodPart(root, "Handle", PrimitiveType.Cylinder,
            new Vector3(0.07f, 0.2f, 0.07f), new Vector3(0, -0.12f, 0));

        SavePrefab(root, "BloodbruteBlade");
        GameObject.DestroyImmediate(root);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    // Wood part — uses the procedural wood material
    static GameObject MakeWoodPart(GameObject parent, string name, PrimitiveType shape,
                                   Vector3 scale, Vector3 localPos)
    {
        var go = MakePart(parent, name, shape, scale, localPos, Color.white);
        if (_woodMat != null)
            go.GetComponent<Renderer>().sharedMaterial = _woodMat;
        return go;
    }

    static GameObject MakePart(GameObject parent, string name, PrimitiveType shape,
                                Vector3 scale, Vector3 localPos, Color color)
    {
        var go = GameObject.CreatePrimitive(shape);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = localPos;
        go.transform.localScale    = scale;
        go.transform.localRotation = Quaternion.identity;

        Object.DestroyImmediate(go.GetComponent<Collider>());

        var rend = go.GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ??
                                   Shader.Find("Standard"));
            mat.color = color;
            rend.sharedMaterial = mat;
        }

        return go;
    }

    // ── Wood material + procedural texture ────────────────────────────────────

    static Material BuildWoodMaterial()
    {
        // Reuse existing material if already saved
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(WOOD_MAT_PATH);
        if (existing != null) return existing;

        Texture2D tex = GenerateWoodTexture(256, 256);

        // Save texture as PNG
        File.WriteAllBytes(
            Path.Combine(Application.dataPath, "../", WOOD_TEX_PATH),
            tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.Refresh();

        Texture2D savedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(WOOD_TEX_PATH);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ??
                               Shader.Find("Standard"));
        if (mat.HasProperty("_BaseMap"))    mat.SetTexture("_BaseMap",    savedTex);
        else if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", savedTex);

        AssetDatabase.CreateAsset(mat, WOOD_MAT_PATH);
        return mat;
    }

    static Texture2D GenerateWoodTexture(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGB24, true);

        Color light = new Color(0.76f, 0.55f, 0.28f);  // light honey oak
        Color dark  = new Color(0.40f, 0.24f, 0.09f);  // dark walnut

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float nx = x / (float)w;
                float ny = y / (float)h;

                // Coarse rings distorted by Perlin noise to look like real grain
                float coarse = Mathf.PerlinNoise(nx * 2.5f + 0.3f, ny * 2.5f + 0.7f) * 6f;
                float fine   = Mathf.PerlinNoise(nx * 9f  + 1.1f, ny * 9f  + 0.4f) * 1.5f;
                float grain  = Mathf.Sin((nx * 10f + coarse + fine) * Mathf.PI) * 0.5f + 0.5f;

                // Add subtle vertical streaks for long-grain fibres
                float streak = Mathf.PerlinNoise(nx * 40f, ny * 2f) * 0.12f;

                tex.SetPixel(x, y, Color.Lerp(dark, light, Mathf.Clamp01(grain + streak)));
            }
        }

        tex.Apply();
        return tex;
    }

    static void SavePrefab(GameObject root, string fileName)
    {
        string path = $"{PREFAB_FOLDER}/{fileName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            int slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }

    // ── Auto-assign prefabs to WeaponData assets ──────────────────────────────

    static void AssignPrefabsToWeaponData()
    {
        var pairs = new (string asset, string prefab)[]
        {
            ("Dagger",      "Dagger"),
            ("Iron Sword",  "IronSword"),
            ("Obsidian Axe","ObsidianAxe"),
            ("Spear",       "Spear"),
            ("Pewter Mace", "PewterMace"),
            ("Bloodbrute Blade","BloodbruteBlade"),
        };

        foreach (var (assetName, prefabName) in pairs)
        {
            string[] guids = AssetDatabase.FindAssets($"{assetName} t:WeaponData");
            if (guids.Length == 0) continue;

            string dataPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            WeaponData data = AssetDatabase.LoadAssetAtPath<WeaponData>(dataPath);
            if (data == null) continue;

            string prefabPath = $"{PREFAB_FOLDER}/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) continue;

            data.prefab = prefab;
            EditorUtility.SetDirty(data);
        }

        AssetDatabase.SaveAssets();
    }
}
