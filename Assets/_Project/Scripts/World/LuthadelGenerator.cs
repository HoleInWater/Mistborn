using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procedural city generator for Luthadel — the capital of the Final Empire.
/// Creates a grid-based city with noble keeps, skaa slums, canals, and ashfall.
/// Buildings have metal fixtures (pushable/pullable) for Allomantic traversal.
///
/// Lore: Luthadel is divided into districts — noble keeps in the center,
/// skaa slums on the outskirts, canals cutting through, and Kredik Shaw
/// (the Lord Ruler's palace) at the heart.
/// </summary>
public class LuthadelGenerator : MonoBehaviour
{
    [Header("City Layout")]
    public int cityWidth = 20;
    public int cityDepth = 20;
    public float blockSize = 30f;
    public float streetWidth = 8f;
    public float canalWidth = 12f;

    [Header("Building Prefabs")]
    public GameObject[] skaaHousePrefabs;
    public GameObject[] nobleKeepPrefabs;
    public GameObject[] shopPrefabs;
    public GameObject[] warehousePrefabs;
    public GameObject[] wallPrefabs;

    [Header("Metal Fixtures")]
    public GameObject metalLamppostPrefab;
    public GameObject metalGatePrefab;
    public GameObject metalRailingPrefab;
    public GameObject coinPrefab;

    [Header("Environment")]
    public GameObject ashPilePrefab;
    public GameObject mistPrefab;
    public Material groundMaterial;
    public Material roadMaterial;
    public Material canalMaterial;

    [Header("Landmarks")]
    public GameObject kredikShawPrefab;
    public GameObject wellOfAscensionPrefab;

    [Header("Generation Settings")]
    public int seed = 0;
    public bool generateOnStart = true;
    [Range(0f, 1f)] public float metalFixtureDensity = 0.3f;
    [Range(0f, 1f)] public float ashDensity = 0.2f;

    [Header("District Colors")]
    public Color nobleDistrictColor = new Color(0.6f, 0.5f, 0.4f);
    public Color skaaDistrictColor = new Color(0.3f, 0.3f, 0.3f);

    // Generated data
    private CityBlock[,] cityGrid;
    private List<GameObject> generatedObjects = new List<GameObject>();
    private Transform cityParent;

    public enum DistrictType { Noble, Skaa, Canal, Market, Keep, Road, Fortress }

    [System.Serializable]
    public class CityBlock
    {
        public DistrictType type;
        public int x, z;
        public float buildingHeight;
        public bool hasMetalFixtures;
        public GameObject instance;
    }

    void Start()
    {
        if (generateOnStart)
            GenerateCity();
    }

    public void GenerateCity()
    {
        ClearCity();

        if (seed == 0) seed = Random.Range(1, 999999);
        Random.InitState(seed);

        cityParent = new GameObject("Luthadel").transform;
        cityParent.SetParent(transform);

        cityGrid = new CityBlock[cityWidth, cityDepth];

        // Phase 1: Assign district types
        AssignDistricts();

        // Phase 2: Generate terrain and roads
        GenerateGround();

        // Phase 3: Place buildings
        PlaceBuildings();

        // Phase 4: Place metal fixtures (for Allomantic traversal)
        PlaceMetalFixtures();

        // Phase 5: Scatter ash and mist
        ScatterEnvironment();

        // Phase 6: Place landmarks
        PlaceLandmarks();

    }

    void AssignDistricts()
    {
        int centerX = cityWidth / 2;
        int centerZ = cityDepth / 2;

        for (int x = 0; x < cityWidth; x++)
        {
            for (int z = 0; z < cityDepth; z++)
            {
                cityGrid[x, z] = new CityBlock { x = x, z = z };

                float distFromCenter = Vector2.Distance(new Vector2(x, z), new Vector2(centerX, centerZ));
                float normalizedDist = distFromCenter / (cityWidth * 0.5f);

                // Center: Kredik Shaw
                if (distFromCenter < 2f)
                {
                    cityGrid[x, z].type = DistrictType.Fortress;
                }
                // Inner ring: Noble keeps
                else if (normalizedDist < 0.35f)
                {
                    cityGrid[x, z].type = DistrictType.Noble;
                    cityGrid[x, z].buildingHeight = Random.Range(12f, 25f);
                }
                // Canals (every 5th column or row)
                else if (x % 5 == 0 || z % 5 == 0)
                {
                    cityGrid[x, z].type = (x % 5 == 0 && z % 5 == 0) ? DistrictType.Market : DistrictType.Canal;
                }
                // Roads (every 3rd)
                else if (x % 3 == 0 || z % 3 == 0)
                {
                    cityGrid[x, z].type = DistrictType.Road;
                }
                // Outer: Skaa slums
                else
                {
                    cityGrid[x, z].type = DistrictType.Skaa;
                    cityGrid[x, z].buildingHeight = Random.Range(3f, 8f);
                }
            }
        }
    }

    void GenerateGround()
    {
        // Create a large ground plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "GroundPlane";
        ground.transform.SetParent(cityParent);
        float totalSize = cityWidth * blockSize;
        ground.transform.localScale = new Vector3(totalSize * 0.1f, 1f, totalSize * 0.1f);
        ground.transform.position = new Vector3(totalSize * 0.5f, 0, totalSize * 0.5f);

        if (groundMaterial != null)
            ground.GetComponent<Renderer>().material = groundMaterial;

        generatedObjects.Add(ground);
    }

    void PlaceBuildings()
    {
        for (int x = 0; x < cityWidth; x++)
        {
            for (int z = 0; z < cityDepth; z++)
            {
                CityBlock block = cityGrid[x, z];
                Vector3 pos = new Vector3(x * blockSize, 0, z * blockSize);

                switch (block.type)
                {
                    case DistrictType.Noble:
                        PlaceBuilding(pos, nobleKeepPrefabs, block.buildingHeight, block);
                        break;
                    case DistrictType.Skaa:
                        PlaceBuilding(pos, skaaHousePrefabs, block.buildingHeight, block);
                        break;
                    case DistrictType.Market:
                        PlaceBuilding(pos, shopPrefabs, 5f, block);
                        break;
                    case DistrictType.Canal:
                        PlaceCanalSection(pos, block);
                        break;
                }
            }
        }
    }

    void PlaceBuilding(Vector3 pos, GameObject[] prefabs, float height, CityBlock block)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            // Procedural box building if no prefabs
            GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = $"Building_{block.x}_{block.z}";
            building.transform.SetParent(cityParent);

            float w = blockSize * Random.Range(0.5f, 0.9f);
            float d = blockSize * Random.Range(0.5f, 0.9f);
            float h = height > 0 ? height : Random.Range(4f, 15f);

            building.transform.localScale = new Vector3(w, h, d);
            building.transform.position = pos + new Vector3(blockSize * 0.5f, h * 0.5f, blockSize * 0.5f);

            // Color based on district
            Renderer r = building.GetComponent<Renderer>();
            if (r != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = block.type == DistrictType.Noble ? nobleDistrictColor : skaaDistrictColor;
                r.material = mat;
            }

            // Add collider for Allomantic traversal
            building.layer = LayerMask.NameToLayer("Default");
            block.instance = building;
            generatedObjects.Add(building);
        }
        else
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            GameObject building = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0, 4) * 90f, 0), cityParent);
            block.instance = building;
            generatedObjects.Add(building);
        }
    }

    void PlaceCanalSection(Vector3 pos, CityBlock block)
    {
        // Blue water plane for canal
        GameObject canal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        canal.name = $"Canal_{block.x}_{block.z}";
        canal.transform.SetParent(cityParent);
        canal.transform.localScale = new Vector3(blockSize, 0.2f, canalWidth);
        canal.transform.position = pos + new Vector3(blockSize * 0.5f, -0.5f, blockSize * 0.5f);
        canal.tag = "Water";

        Renderer r = canal.GetComponent<Renderer>();
        if (r != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.15f, 0.2f, 0.3f, 0.8f);
            r.material = mat;
        }

        generatedObjects.Add(canal);
    }

    void PlaceMetalFixtures()
    {
        for (int x = 0; x < cityWidth; x++)
        {
            for (int z = 0; z < cityDepth; z++)
            {
                if (Random.value > metalFixtureDensity) continue;

                CityBlock block = cityGrid[x, z];
                if (block.type == DistrictType.Canal || block.type == DistrictType.Fortress) continue;

                Vector3 pos = new Vector3(x * blockSize + Random.Range(2f, blockSize - 2f), 0,
                                          z * blockSize + Random.Range(2f, blockSize - 2f));

                // Place lamppost or railing
                GameObject fixture;
                if (metalLamppostPrefab != null && Random.value > 0.5f)
                {
                    fixture = Instantiate(metalLamppostPrefab, pos, Quaternion.identity, cityParent);
                }
                else
                {
                    // Procedural metal box if no prefab
                    fixture = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    fixture.name = "MetalLamppost";
                    fixture.transform.SetParent(cityParent);
                    fixture.transform.position = pos + Vector3.up * 2.5f;
                    fixture.transform.localScale = new Vector3(0.15f, 2.5f, 0.15f);

                    Renderer r = fixture.GetComponent<Renderer>();
                    if (r != null)
                    {
                        Material mat = new Material(Shader.Find("Standard"));
                        mat.color = new Color(0.4f, 0.4f, 0.45f);
                        r.material = mat;
                    }
                }

                // Make it pushable/pullable
                fixture.layer = LayerMask.NameToLayer("Metal");
                Rigidbody rb = fixture.GetComponent<Rigidbody>();
                if (rb == null) rb = fixture.AddComponent<Rigidbody>();
                rb.isKinematic = true; // Anchored — player pushes off these
                rb.mass = 500f;

                AllomanticTarget target = fixture.GetComponent<AllomanticTarget>();
                if (target == null) target = fixture.AddComponent<AllomanticTarget>();
                target.canBePushed = true;
                target.canBePulled = true;
                target.isAnchored = true;

                block.hasMetalFixtures = true;
                generatedObjects.Add(fixture);
            }
        }

        // Scatter some loose coins on the ground
        int coinCount = Mathf.RoundToInt(cityWidth * cityDepth * 0.1f);
        for (int i = 0; i < coinCount; i++)
        {
            float cx = Random.Range(0f, cityWidth * blockSize);
            float cz = Random.Range(0f, cityDepth * blockSize);
            Vector3 coinPos = new Vector3(cx, 0.05f, cz);

            GameObject coin;
            if (coinPrefab != null)
            {
                coin = Instantiate(coinPrefab, coinPos, Quaternion.identity, cityParent);
            }
            else
            {
                coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                coin.name = "Coin";
                coin.transform.SetParent(cityParent);
                coin.transform.position = coinPos;
                coin.transform.localScale = new Vector3(0.025f, 0.002f, 0.025f);
            }

            coin.layer = LayerMask.NameToLayer("Metal");
            coin.tag = "Coin";
            Rigidbody coinRb = coin.GetComponent<Rigidbody>();
            if (coinRb == null) coinRb = coin.AddComponent<Rigidbody>();
            coinRb.mass = AllomancyPhysicsFormulas.COIN_MASS;

            AllomanticTarget ct = coin.AddComponent<AllomanticTarget>();
            ct.canBePushed = true;
            ct.canBePulled = true;

            generatedObjects.Add(coin);
        }
    }

    void ScatterEnvironment()
    {
        // Ash piles throughout the city
        int ashCount = Mathf.RoundToInt(cityWidth * cityDepth * ashDensity);
        for (int i = 0; i < ashCount; i++)
        {
            float ax = Random.Range(0f, cityWidth * blockSize);
            float az = Random.Range(0f, cityDepth * blockSize);

            if (ashPilePrefab != null)
            {
                GameObject ash = Instantiate(ashPilePrefab, new Vector3(ax, 0, az),
                    Quaternion.identity, cityParent);
                generatedObjects.Add(ash);
            }
        }
    }

    void PlaceLandmarks()
    {
        // Kredik Shaw at center
        Vector3 center = new Vector3(cityWidth * blockSize * 0.5f, 0, cityDepth * blockSize * 0.5f);

        if (kredikShawPrefab != null)
        {
            GameObject ks = Instantiate(kredikShawPrefab, center, Quaternion.identity, cityParent);
            generatedObjects.Add(ks);
        }
        else
        {
            // Procedural "Hill of a Thousand Spires"
            for (int i = 0; i < 12; i++)
            {
                float angle = i * (360f / 12f);
                float radius = Random.Range(5f, 15f);
                Vector3 spirePos = center + Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;

                GameObject spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                spire.name = $"KredikShaw_Spire_{i}";
                spire.transform.SetParent(cityParent);
                float h = Random.Range(20f, 50f);
                spire.transform.localScale = new Vector3(1.5f, h, 1.5f);
                spire.transform.position = spirePos + Vector3.up * h;

                Renderer r = spire.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.2f, 0.2f, 0.25f);
                r.material = mat;

                // Metal spires — pushable/pullable anchors
                spire.layer = LayerMask.NameToLayer("Metal");
                Rigidbody rb = spire.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.mass = 10000f;
                AllomanticTarget at = spire.AddComponent<AllomanticTarget>();
                at.isAnchored = true;

                generatedObjects.Add(spire);
            }
        }
    }

    public void ClearCity()
    {
        foreach (var obj in generatedObjects)
            if (obj != null) Destroy(obj);
        generatedObjects.Clear();

        if (cityParent != null) Destroy(cityParent.gameObject);
    }

    public void RegenerateWithSeed(int newSeed)
    {
        seed = newSeed;
        GenerateCity();
    }

    // ── Public API ───────────────────────────────────────────────────────
    public CityBlock GetBlock(int x, int z)
    {
        if (x < 0 || x >= cityWidth || z < 0 || z >= cityDepth) return null;
        return cityGrid[x, z];
    }

    public Vector3 GetCityCenter() => new Vector3(cityWidth * blockSize * 0.5f, 0, cityDepth * blockSize * 0.5f);
    public float GetCitySize() => cityWidth * blockSize;
    public int GetSeed() => seed;
}
