using UnityEngine;
using UnityEditor;

/// <summary>
/// Mistborn → Create Weapon Placeholders
/// Generates simple primitive-based weapon prefabs so weapons appear in-hand
/// immediately. Replace the prefab's mesh with a real model later — the
/// WeaponData hand offsets stay the same.
/// </summary>
public static class CreateWeaponPlaceholders
{
    const string PREFAB_FOLDER = "Assets/_Project/Prefabs/Weapons";

    [MenuItem("Mistborn/Create Weapon Placeholders")]
    public static void Create()
    {
        EnsureFolder("Assets/_Project/Prefabs");
        EnsureFolder(PREFAB_FOLDER);

        MakeDagger();
        MakeSword();
        MakeAxe();
        MakeSpear();
        MakeMace();
        MakeKolossBlade();

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
        // Blade: thin tall cube. Handle: shorter cube below.
        var root = new GameObject("Dagger");

        var blade = MakePart(root, "Blade", PrimitiveType.Cube,
            new Vector3(0.04f, 0.28f, 0.04f), new Vector3(0, 0.17f, 0), Color.gray);

        var guard = MakePart(root, "Guard", PrimitiveType.Cube,
            new Vector3(0.14f, 0.03f, 0.06f), new Vector3(0, 0.03f, 0), new Color(0.5f, 0.4f, 0.2f));

        var handle = MakePart(root, "Handle", PrimitiveType.Cylinder,
            new Vector3(0.04f, 0.09f, 0.04f), new Vector3(0, -0.06f, 0), new Color(0.35f, 0.2f, 0.1f));

        SavePrefab(root, "Dagger");
        GameObject.DestroyImmediate(root);
    }

    static void MakeSword()
    {
        var root = new GameObject("IronSword");

        MakePart(root, "Blade", PrimitiveType.Cube,
            new Vector3(0.05f, 0.55f, 0.05f), new Vector3(0, 0.35f, 0), Color.gray);

        MakePart(root, "Guard", PrimitiveType.Cube,
            new Vector3(0.22f, 0.04f, 0.07f), new Vector3(0, 0.06f, 0), new Color(0.55f, 0.45f, 0.2f));

        MakePart(root, "Handle", PrimitiveType.Cylinder,
            new Vector3(0.05f, 0.12f, 0.05f), new Vector3(0, -0.08f, 0), new Color(0.3f, 0.18f, 0.08f));

        MakePart(root, "Pommel", PrimitiveType.Sphere,
            new Vector3(0.09f, 0.09f, 0.09f), new Vector3(0, -0.22f, 0), new Color(0.55f, 0.45f, 0.2f));

        SavePrefab(root, "IronSword");
        GameObject.DestroyImmediate(root);
    }

    static void MakeAxe()
    {
        var root = new GameObject("ObsidianAxe");

        // Shaft
        MakePart(root, "Shaft", PrimitiveType.Cylinder,
            new Vector3(0.05f, 0.3f, 0.05f), new Vector3(0, 0.05f, 0), new Color(0.25f, 0.15f, 0.08f));

        // Axe head — dark obsidian-ish
        MakePart(root, "Head", PrimitiveType.Cube,
            new Vector3(0.22f, 0.18f, 0.06f), new Vector3(0.1f, 0.3f, 0), new Color(0.1f, 0.1f, 0.12f));

        SavePrefab(root, "ObsidianAxe");
        GameObject.DestroyImmediate(root);
    }

    static void MakeSpear()
    {
        var root = new GameObject("Spear");

        // Long shaft
        MakePart(root, "Shaft", PrimitiveType.Cylinder,
            new Vector3(0.04f, 0.7f, 0.04f), new Vector3(0, 0.3f, 0), new Color(0.4f, 0.25f, 0.1f));

        // Iron tip
        MakePart(root, "Tip", PrimitiveType.Cube,
            new Vector3(0.05f, 0.2f, 0.05f), new Vector3(0, 1.0f, 0), Color.gray);

        SavePrefab(root, "Spear");
        GameObject.DestroyImmediate(root);
    }

    static void MakeMace()
    {
        var root = new GameObject("PewterMace");

        MakePart(root, "Handle", PrimitiveType.Cylinder,
            new Vector3(0.05f, 0.22f, 0.05f), new Vector3(0, 0.0f, 0), new Color(0.3f, 0.18f, 0.08f));

        // Heavy pewter head
        MakePart(root, "Head", PrimitiveType.Sphere,
            new Vector3(0.18f, 0.18f, 0.18f), new Vector3(0, 0.32f, 0), new Color(0.6f, 0.6f, 0.65f));

        // Flanges
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            var flange = MakePart(root, "Flange" + i, PrimitiveType.Cube,
                new Vector3(0.06f, 0.14f, 0.04f),
                new Vector3(Mathf.Cos(angle) * 0.14f, 0.32f, Mathf.Sin(angle) * 0.14f),
                new Color(0.55f, 0.55f, 0.6f));
        }

        SavePrefab(root, "PewterMace");
        GameObject.DestroyImmediate(root);
    }

    static void MakeKolossBlade()
    {
        var root = new GameObject("KolossBlade");

        // Massive wide blade
        MakePart(root, "Blade", PrimitiveType.Cube,
            new Vector3(0.18f, 1.1f, 0.07f), new Vector3(0, 0.7f, 0), new Color(0.45f, 0.45f, 0.5f));

        MakePart(root, "Guard", PrimitiveType.Cube,
            new Vector3(0.38f, 0.07f, 0.1f), new Vector3(0, 0.1f, 0), new Color(0.4f, 0.3f, 0.15f));

        MakePart(root, "Handle", PrimitiveType.Cylinder,
            new Vector3(0.07f, 0.2f, 0.07f), new Vector3(0, -0.12f, 0), new Color(0.25f, 0.15f, 0.08f));

        SavePrefab(root, "KolossBlade");
        GameObject.DestroyImmediate(root);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static GameObject MakePart(GameObject parent, string name, PrimitiveType shape,
                                Vector3 scale, Vector3 localPos, Color color)
    {
        var go = GameObject.CreatePrimitive(shape);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = localPos;
        go.transform.localScale    = scale;
        go.transform.localRotation = Quaternion.identity;

        // Remove colliders — weapon parts shouldn't block raycasts
        Object.DestroyImmediate(go.GetComponent<Collider>());

        // Apply colour via material property block to avoid asset leaks
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
            ("Koloss Blade","KolossBlade"),
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
