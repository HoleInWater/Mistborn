/* CoinPrefabBuilder.cs
 *
 * Editor tool: Mistborn → Items → Create Coin Prefabs
 *
 * Creates Clip and Boxing prefabs with correct physics properties:
 *   Clip:   2cm diameter, 3g copper (standard Coinshot ammo)
 *   Boxing: 3cm diameter, 15.5g brass (heavy projectile)
 *
 * Both get: Rigidbody (correct mass), SphereCollider, AllomanticTarget,
 * Metal layer, "Coin" tag, and a colored material.
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CoinPrefabBuilder
{
    [MenuItem("Mistborn/Items/Create Coin Prefabs")]
    public static void CreateCoins()
    {
        string folder = "Assets/_Project/Prefabs/Items";
        EnsureFolder(folder);

        CreateCoinPrefab(folder, "Clip",
            AllomancyPhysicsFormulas.CLIP_MASS,           // 0.003 kg
            0.01f,                                        // radius 1cm = 0.01 units (actual), scaled up for visibility
            new Color(0.72f, 0.45f, 0.20f));              // copper color

        CreateCoinPrefab(folder, "Boxing",
            AllomancyPhysicsFormulas.BOXING_MASS,          // 0.0155 kg
            0.015f,                                       // radius 1.5cm
            new Color(0.80f, 0.68f, 0.20f));              // brass/gold color

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Coin Prefabs Created",
            $"Created at {folder}/:\n\n" +
            "Clip.prefab — 2cm copper, 3g (standard ammo)\n" +
            "Boxing.prefab — 3cm brass, 15.5g (heavy shot)\n\n" +
            "Assign Clip to CoinPouch → Coin Prefab field.\n" +
            "Both have: Rigidbody, SphereCollider, AllomanticTarget, Metal layer.",
            "OK");
    }

    static void CreateCoinPrefab(string folder, string name, float mass, float radius, Color color)
    {
        // Visual: scaled-up cylinder so it's actually visible in-game
        // Real coins are 1-1.5cm radius — invisible at game scale
        // Scale up 5× for visibility (still small)
        float visualScale = radius * 5f;

        var coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.name = name;
        coin.transform.localScale = new Vector3(visualScale * 2f, visualScale * 0.15f, visualScale * 2f);

        // Physics
        var rb = coin.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // fast-moving projectile
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Collider — use sphere for simpler physics
        Object.DestroyImmediate(coin.GetComponent<CapsuleCollider>()); // cylinder default
        var col = coin.AddComponent<SphereCollider>();
        col.radius = 0.5f; // unit sphere, scaled by transform

        // Allomantic target
        var target = coin.AddComponent<AllomanticTarget>();
        target.canBePushed = true;
        target.canBePulled = true;

        // Layer
        int metalLayer = LayerMask.NameToLayer("Metal");
        if (metalLayer >= 0) coin.layer = metalLayer;

        // Tag
        EnsureTag("Coin");
        coin.tag = "Coin";

        // Material — clone from existing project material
        Material sourceMat = null;
        string[] matPaths = {
            "Assets/_Project/Materials/Metal.mat",
            "Assets/_Project/Materials/Ground(Temp).mat",
        };
        foreach (var p in matPaths)
        {
            sourceMat = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (sourceMat != null) break;
        }

        if (sourceMat != null)
        {
            string matFolder = "Assets/_Project/Materials/Items";
            EnsureFolder(matFolder);

            var mat = new Material(sourceMat);
            mat.name = $"Coin_{name}";
            mat.SetColor("_BaseColor", color);
            mat.SetColor("_Color", color);
            mat.color = color;
            mat.SetFloat("_Smoothness", 0.6f); // coins are somewhat shiny
            mat.SetFloat("_Metallic", 0.8f);   // they ARE metal
            // Kill inherited emission
            mat.DisableKeyword("_EMISSION");
            mat.SetColor("_EmissiveColor", Color.black);
            mat.SetColor("_EmissiveColorLDR", Color.black);
            mat.SetColor("_EmissionColor", Color.black);
            mat.SetFloat("_UseEmissiveIntensity", 0f);

            string matPath = $"{matFolder}/Coin_{name}.mat";
            AssetDatabase.CreateAsset(mat, matPath);
            coin.GetComponent<Renderer>().sharedMaterial = mat;
        }

        // Save as prefab
        string prefabPath = $"{folder}/{name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(coin, prefabPath);
        Object.DestroyImmediate(coin);

        Debug.Log($"[CoinPrefabBuilder] Created {prefabPath} — mass={mass}kg, radius={radius}m");
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

    static void EnsureTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tagName) return;
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tagName;
        tagManager.ApplyModifiedProperties();
        Debug.Log($"[CoinPrefabBuilder] Created tag '{tagName}'");
    }
}
#endif
