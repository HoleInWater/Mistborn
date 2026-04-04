/* LuthadelCityBuilder.cs
 *
 * Editor tool: Mistborn → World → Build Luthadel City in Current Scene
 *
 * Generates a playable Luthadel city using the same visual style as the
 * title sequence — primitives with TitleSequenceMaterialOverride for colors.
 * No prefabs needed. Creates:
 *   - Kredik Shaw at the center (spires, walls, gate)
 *   - Noble district (tall buildings, keeps, wide streets)
 *   - Skaa quarter (dense short hovels, narrow alleys)
 *   - Market squares, canals with bridges, city wall
 *   - Street-level details (lanterns, barrels, signs, etc.)
 *   - Metal objects tagged for Allomantic interaction
 *   - Ash + mist particle systems
 *
 * All buildings get colliders so the player can walk around.
 * Metal fixtures get the "Metal" layer for Push/Pull targeting.
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LuthadelCityBuilder
{
    // Color palette (matches title sequence)
    static readonly Color COL_GROUND      = new Color(0.12f, 0.10f, 0.08f);
    static readonly Color COL_ROAD        = new Color(0.22f, 0.20f, 0.18f);
    static readonly Color COL_CANAL       = new Color(0.04f, 0.06f, 0.10f);
    static readonly Color COL_METAL       = new Color(0.35f, 0.35f, 0.40f);
    static readonly Color COL_WOOD        = new Color(0.25f, 0.18f, 0.10f);
    static readonly Color COL_LANTERN     = new Color(1.0f, 0.55f, 0.15f);

    static readonly Color[] NOBLE_COLORS = {
        new Color(0.30f, 0.26f, 0.22f),
        new Color(0.38f, 0.34f, 0.28f),
        new Color(0.32f, 0.18f, 0.14f),
        new Color(0.28f, 0.28f, 0.30f),
    };

    static readonly Color[] SKAA_COLORS = {
        new Color(0.14f, 0.12f, 0.10f),
        new Color(0.16f, 0.13f, 0.11f),
        new Color(0.12f, 0.11f, 0.10f),
        new Color(0.18f, 0.15f, 0.12f),
    };

    static readonly Color[] ROOF_COLORS = {
        new Color(0.20f, 0.22f, 0.25f),
        new Color(0.35f, 0.20f, 0.12f),
        new Color(0.15f, 0.15f, 0.18f),
    };

    [MenuItem("Mistborn/World/Build Luthadel City in Current Scene")]
    public static void Build()
    {
        if (!EditorUtility.DisplayDialog("Build Luthadel",
            "This will create a procedural Luthadel city in the current scene.\n\n" +
            "It generates ~500+ objects. Continue?", "Build", "Cancel"))
            return;

        // Ensure Metal layer exists
        EnsureLayer("Metal", 8);

        var root = new GameObject("Luthadel_City");
        var t = root.transform;

        int gridSize = 12;
        float blockSize = 25f;
        float streetWidth = 6f;
        float totalSize = gridSize * (blockSize + streetWidth);

        // ── Ground ───────────────────────────────────────────────────────
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "CityGround";
        ground.transform.SetParent(t);
        ground.transform.localScale = new Vector3(totalSize * 0.12f, 1f, totalSize * 0.12f);
        ApplyColor(ground, COL_GROUND);

        // ── City Grid ────────────────────────────────────────────────────
        int center = gridSize / 2;

        for (int gx = 0; gx < gridSize; gx++)
        {
            for (int gz = 0; gz < gridSize; gz++)
            {
                float worldX = (gx - center) * (blockSize + streetWidth);
                float worldZ = (gz - center) * (blockSize + streetWidth);
                Vector3 blockCenter = new Vector3(worldX, 0f, worldZ);

                float dist = Vector2.Distance(new Vector2(gx, gz), new Vector2(center, center));

                // Kredik Shaw at center
                if (dist < 1.5f)
                {
                    if (gx == center && gz == center)
                        BuildKredikShaw(t, blockCenter);
                    continue;
                }

                // Roads (ground strips)
                BuildRoad(t, blockCenter, blockSize, streetWidth);

                // Canal (every 4th row/column)
                if (gx % 4 == 0 || gz % 4 == 0)
                {
                    if (gx % 4 == 0 && gz % 4 == 0)
                        BuildMarketSquare(t, blockCenter, blockSize * 0.6f);
                    else
                        BuildCanal(t, blockCenter, gx % 4 == 0, blockSize);
                    continue;
                }

                // Noble district (inner ring)
                if (dist < gridSize * 0.35f)
                    BuildNobleBlock(t, blockCenter, blockSize);
                // Skaa quarter (outer ring)
                else
                    BuildSkaaBlock(t, blockCenter, blockSize);

                // Lanterns along streets
                if (Random.Range(0f, 1f) < 0.5f)
                    BuildLantern(t, blockCenter + new Vector3(blockSize * 0.5f + 1f, 0f, Random.Range(-blockSize * 0.3f, blockSize * 0.3f)));
            }
        }

        // ── City Wall ────────────────────────────────────────────────────
        float wallRadius = totalSize * 0.48f;
        BuildCityWall(t, Vector3.zero, wallRadius, 24);

        // ── Ash + Mist ───────────────────────────────────────────────────
        BuildAshSystem(t, totalSize);
        BuildMistSystem(t, totalSize);

        // ── Select the root ──────────────────────────────────────────────
        Selection.activeGameObject = root;

        Debug.Log($"[LuthadelCityBuilder] Built {root.transform.childCount} root objects, " +
                  $"grid {gridSize}x{gridSize}, total size ~{totalSize:F0}m");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // BLOCK BUILDERS
    // ═════════════════════════════════════════════════════════════════════════

    static void BuildKredikShaw(Transform parent, Vector3 center)
    {
        var ks = new GameObject("KredikShaw");
        ks.transform.SetParent(parent);
        ks.transform.position = center;

        Color spireCol = new Color(0.14f, 0.13f, 0.16f);

        // Central spire
        BuildSpire(ks.transform, Vector3.zero, 2.5f, 40f, spireCol);

        // Inner ring (8 spires)
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2f / 8f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * 8f, 0f, Mathf.Sin(angle) * 8f);
            BuildSpire(ks.transform, pos, Random.Range(1.2f, 2f), Random.Range(20f, 30f), spireCol);
        }

        // Outer ring (12 spires)
        for (int i = 0; i < 12; i++)
        {
            float angle = i * Mathf.PI * 2f / 12f + 0.15f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * 14f, 0f, Mathf.Sin(angle) * 14f);
            BuildSpire(ks.transform, pos, Random.Range(0.8f, 1.5f), Random.Range(12f, 20f), spireCol);
        }

        // Base platform
        var basePlat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        basePlat.name = "KredikShawBase";
        basePlat.transform.SetParent(ks.transform);
        basePlat.transform.localPosition = new Vector3(0f, 2f, 0f);
        basePlat.transform.localScale = new Vector3(20f, 2f, 20f);
        ApplyColor(basePlat, new Color(0.12f, 0.11f, 0.13f));

        // Perimeter walls
        BuildPerimeterWalls(ks.transform, Vector3.zero, 18f, 8);
    }

    static void BuildSpire(Transform parent, Vector3 pos, float radius, float height, Color color)
    {
        var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Spire";
        body.transform.SetParent(parent);
        body.transform.localPosition = pos + new Vector3(0f, height * 0.5f, 0f);
        body.transform.localScale = new Vector3(radius, height * 0.5f, radius);
        body.transform.rotation = Quaternion.Euler(Random.Range(-2f, 2f), Random.Range(0f, 360f), Random.Range(-2f, 2f));
        ApplyColor(body, color);

        var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.name = "SpireTip";
        tip.transform.SetParent(body.transform, false);
        tip.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        tip.transform.localScale = new Vector3(0.5f, 1.8f, 0.5f);
        ApplyColor(tip, new Color(color.r + 0.05f, color.g + 0.05f, color.b + 0.06f));
    }

    static void BuildPerimeterWalls(Transform parent, Vector3 center, float radius, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float a1 = i * Mathf.PI * 2f / segments;
            float a2 = (i + 1) * Mathf.PI * 2f / segments;
            Vector3 p1 = center + new Vector3(Mathf.Cos(a1) * radius, 0f, Mathf.Sin(a1) * radius);
            Vector3 p2 = center + new Vector3(Mathf.Cos(a2) * radius, 0f, Mathf.Sin(a2) * radius);
            Vector3 mid = (p1 + p2) * 0.5f;
            float length = Vector3.Distance(p1, p2);
            float angle = Mathf.Atan2(p2.x - p1.x, p2.z - p1.z) * Mathf.Rad2Deg;

            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.SetParent(parent);
            wall.transform.localPosition = mid + new Vector3(0f, 4f, 0f);
            wall.transform.localScale = new Vector3(0.6f, 8f, length);
            wall.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            ApplyColor(wall, new Color(0.13f, 0.12f, 0.14f));
        }
    }

    static void BuildNobleBlock(Transform parent, Vector3 center, float size)
    {
        int count = Random.Range(2, 4);
        for (int i = 0; i < count; i++)
        {
            float x = center.x + Random.Range(-size * 0.35f, size * 0.35f);
            float z = center.z + Random.Range(-size * 0.35f, size * 0.35f);
            float h = Random.Range(10f, 22f);
            float w = Random.Range(5f, 10f);
            float d = Random.Range(5f, 10f);

            var bldg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bldg.name = "NobleBuilding";
            bldg.transform.SetParent(parent);
            bldg.transform.position = new Vector3(x, h * 0.5f, z);
            bldg.transform.localScale = new Vector3(w, h, d);
            bldg.transform.rotation = Quaternion.Euler(0f, Random.Range(-5f, 5f), 0f);
            ApplyColor(bldg, NOBLE_COLORS[Random.Range(0, NOBLE_COLORS.Length)]);

            // Roof
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(bldg.transform, false);
            roof.transform.localPosition = new Vector3(0f, 0.52f, 0f);
            roof.transform.localScale = new Vector3(1.05f, 0.05f, 1.05f);
            ApplyColor(roof, ROOF_COLORS[Random.Range(0, ROOF_COLORS.Length)]);

            // Window light
            if (Random.Range(0f, 1f) < 0.4f)
            {
                var wl = new GameObject("WindowLight");
                wl.transform.SetParent(bldg.transform, false);
                wl.transform.localPosition = new Vector3(0.5f, 0f, 0f);
                var light = wl.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(0.9f, 0.6f, 0.2f);
                light.intensity = 1f;
                light.range = 8f;
            }
        }
    }

    static void BuildSkaaBlock(Transform parent, Vector3 center, float size)
    {
        int count = Random.Range(4, 8);
        for (int i = 0; i < count; i++)
        {
            float x = center.x + Random.Range(-size * 0.4f, size * 0.4f);
            float z = center.z + Random.Range(-size * 0.4f, size * 0.4f);
            float h = Random.Range(3f, 7f);
            float w = Random.Range(3f, 6f);
            float d = Random.Range(3f, 6f);

            var bldg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bldg.name = "SkaaHovel";
            bldg.transform.SetParent(parent);
            bldg.transform.position = new Vector3(x, h * 0.5f, z);
            bldg.transform.localScale = new Vector3(w, h, d);
            bldg.transform.rotation = Quaternion.Euler(0f, Random.Range(-10f, 10f), 0f);
            ApplyColor(bldg, SKAA_COLORS[Random.Range(0, SKAA_COLORS.Length)]);
        }
    }

    static void BuildRoad(Transform parent, Vector3 center, float blockSize, float streetWidth)
    {
        // Horizontal road
        var roadH = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roadH.name = "Road";
        roadH.transform.SetParent(parent);
        roadH.transform.position = new Vector3(center.x, 0.02f, center.z + blockSize * 0.5f + streetWidth * 0.5f);
        roadH.transform.localScale = new Vector3(blockSize, 0.04f, streetWidth);
        ApplyColor(roadH, COL_ROAD);
    }

    static void BuildCanal(Transform parent, Vector3 center, bool northSouth, float length)
    {
        var canal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        canal.name = "Canal";
        canal.transform.SetParent(parent);
        canal.transform.position = center + new Vector3(0f, -0.5f, 0f);
        if (northSouth)
            canal.transform.localScale = new Vector3(4f, 1f, length);
        else
            canal.transform.localScale = new Vector3(length, 1f, 4f);
        ApplyColor(canal, COL_CANAL);

        // Bridge
        var bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge.name = "Bridge";
        bridge.transform.SetParent(parent);
        bridge.transform.position = center + new Vector3(0f, 0.5f, 0f);
        bridge.transform.localScale = northSouth
            ? new Vector3(6f, 0.5f, 4f)
            : new Vector3(4f, 0.5f, 6f);
        ApplyColor(bridge, new Color(0.25f, 0.22f, 0.18f));

        // Metal railing — tagged as Metal for Allomancy
        for (int side = -1; side <= 1; side += 2)
        {
            var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "BridgeRail";
            rail.transform.SetParent(parent);
            Vector3 railPos = center + new Vector3(0f, 1.2f, 0f);
            if (northSouth)
                railPos += new Vector3(side * 2.8f, 0f, 0f);
            else
                railPos += new Vector3(0f, 0f, side * 2.8f);
            rail.transform.position = railPos;
            rail.transform.localScale = northSouth
                ? new Vector3(0.08f, 0.8f, 4f)
                : new Vector3(4f, 0.8f, 0.08f);
            ApplyColor(rail, COL_METAL);
            SetLayer(rail, "Metal");
        }
    }

    static void BuildMarketSquare(Transform parent, Vector3 center, float size)
    {
        var square = GameObject.CreatePrimitive(PrimitiveType.Cube);
        square.name = "MarketSquare";
        square.transform.SetParent(parent);
        square.transform.position = center + new Vector3(0f, 0.03f, 0f);
        square.transform.localScale = new Vector3(size, 0.06f, size);
        ApplyColor(square, COL_ROAD);

        // Fountain
        var fountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fountain.name = "Fountain";
        fountain.transform.SetParent(parent);
        fountain.transform.position = center + new Vector3(0f, 0.5f, 0f);
        fountain.transform.localScale = new Vector3(2f, 0.5f, 2f);
        ApplyColor(fountain, new Color(0.28f, 0.26f, 0.22f));

        // Market stalls
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad + 45f * Mathf.Deg2Rad;
            Vector3 stallPos = center + new Vector3(Mathf.Cos(angle) * size * 0.3f, 0f, Mathf.Sin(angle) * size * 0.3f);
            var stall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stall.name = "Stall";
            stall.transform.SetParent(parent);
            stall.transform.position = stallPos + new Vector3(0f, 1f, 0f);
            stall.transform.localScale = new Vector3(2.5f, 2f, 2f);
            stall.transform.rotation = Quaternion.Euler(0f, i * 90f, 0f);
            ApplyColor(stall, COL_WOOD);
        }
    }

    static void BuildLantern(Transform parent, Vector3 pos)
    {
        // Post
        var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = "LampPost";
        post.transform.SetParent(parent);
        post.transform.position = pos + new Vector3(0f, 2f, 0f);
        post.transform.localScale = new Vector3(0.1f, 2f, 0.1f);
        ApplyColor(post, COL_METAL);
        SetLayer(post, "Metal");

        // Lantern body
        var lantern = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lantern.name = "Lantern";
        lantern.transform.SetParent(parent);
        lantern.transform.position = pos + new Vector3(0f, 4.2f, 0f);
        lantern.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
        ApplyColor(lantern, COL_METAL);
        SetLayer(lantern, "Metal");

        // Light
        var lightObj = new GameObject("LanternLight");
        lightObj.transform.SetParent(parent);
        lightObj.transform.position = pos + new Vector3(0f, 4f, 0f);
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = COL_LANTERN;
        light.intensity = 2f;
        light.range = 12f;
    }

    static void BuildCityWall(Transform parent, Vector3 center, float radius, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float a1 = i * Mathf.PI * 2f / segments;
            float a2 = (i + 1) * Mathf.PI * 2f / segments;
            Vector3 p1 = center + new Vector3(Mathf.Cos(a1) * radius, 0f, Mathf.Sin(a1) * radius);
            Vector3 p2 = center + new Vector3(Mathf.Cos(a2) * radius, 0f, Mathf.Sin(a2) * radius);
            Vector3 mid = (p1 + p2) * 0.5f;
            float length = Vector3.Distance(p1, p2);
            float angle = Mathf.Atan2(p2.x - p1.x, p2.z - p1.z) * Mathf.Rad2Deg;

            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "CityWall";
            wall.transform.SetParent(parent);
            wall.transform.position = mid + new Vector3(0f, 5f, 0f);
            wall.transform.localScale = new Vector3(1.5f, 10f, length);
            wall.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            ApplyColor(wall, new Color(0.18f, 0.16f, 0.14f));

            // Tower at every 4th segment
            if (i % (segments / 6) == 0)
            {
                var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tower.name = "WallTower";
                tower.transform.SetParent(parent);
                tower.transform.position = p1 + new Vector3(0f, 8f, 0f);
                tower.transform.localScale = new Vector3(3f, 8f, 3f);
                ApplyColor(tower, new Color(0.16f, 0.14f, 0.13f));
            }
        }
    }

    static void BuildAshSystem(Transform parent, float spread)
    {
        var obj = new GameObject("CityAsh");
        obj.transform.SetParent(parent);
        obj.transform.position = new Vector3(0f, 20f, 0f);
        var ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 15f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.30f, 0.27f, 0.23f, 0.6f),
            new Color(0.45f, 0.40f, 0.33f, 0.9f));
        main.maxParticles = 1000;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = new ParticleSystem.MinMaxCurve(0.03f, 0.12f);
        var em = ps.emission;
        em.rateOverTime = 80f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(spread, 1f, spread);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.5f;
        noise.octaveCount = 2;
    }

    static void BuildMistSystem(Transform parent, float spread)
    {
        var obj = new GameObject("CityMist");
        obj.transform.SetParent(parent);
        obj.transform.position = new Vector3(0f, 1f, 0f);
        var ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(10f, 18f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.1f);
        main.startSize = new ParticleSystem.MinMaxCurve(5f, 15f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.65f, 0.68f, 0.75f, 0.08f),
            new Color(0.80f, 0.80f, 0.85f, 0.15f));
        main.maxParticles = 60;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = ps.emission;
        em.rateOverTime = 5f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(spread * 0.8f, 0.5f, spread * 0.8f);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.1f;
        noise.frequency = 0.2f;
        noise.octaveCount = 1;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.5f, 0.2f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═════════════════════════════════════════════════════════════════════════

    static Material _citySourceMat;
    static Dictionary<string, Material> _cityMatCache = new Dictionary<string, Material>();

    static void ApplyColor(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;

        string hex = ColorUtility.ToHtmlStringRGB(color);
        string key = $"City_{hex}";

        if (!_cityMatCache.TryGetValue(key, out Material mat))
        {
            if (_citySourceMat == null)
            {
                string[] sources = {
                    "Assets/_Project/Materials/Ground(Temp).mat",
                    "Assets/_Project/Materials/Metal.mat",
                    "Assets/_Project/Materials/Wood.mat",
                };
                foreach (var p in sources)
                {
                    _citySourceMat = AssetDatabase.LoadAssetAtPath<Material>(p);
                    if (_citySourceMat != null) break;
                }
            }
            if (_citySourceMat == null) return;

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Materials/City"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Materials"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Materials");
                AssetDatabase.CreateFolder("Assets/_Project/Materials", "City");
            }

            mat = new Material(_citySourceMat);
            mat.name = key;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color", color);
            mat.color = color;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);
            if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic", 0f);

            string path = $"Assets/_Project/Materials/City/{key}.mat";
            AssetDatabase.CreateAsset(mat, path);
            _cityMatCache[key] = mat;
        }

        rend.sharedMaterial = mat;
    }

    static void SetLayer(GameObject go, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer >= 0) go.layer = layer;
    }

    static void EnsureLayer(string name, int index)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        if (layers.GetArrayElementAtIndex(index).stringValue == name) return;
        if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(index).stringValue))
        {
            layers.GetArrayElementAtIndex(index).stringValue = name;
            tagManager.ApplyModifiedProperties();
            Debug.Log($"[LuthadelCityBuilder] Created layer '{name}' at index {index}");
        }
    }
}
#endif
