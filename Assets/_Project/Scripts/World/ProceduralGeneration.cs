using UnityEngine;
using System.Collections.Generic;

public class ProceduralGeneration : MonoBehaviour
{
    [Header("Generation Settings")]
    public int seed = 0;
    public bool randomSeed = true;
    
    [Header("Terrain")]
    public int terrainWidth = 100;
    public int terrainHeight = 100;
    public float terrainScale = 1f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    
    [Header("Objects")]
    public int objectCount = 50;
    public float objectSpacing = 5f;
    public LayerMask spawnLayer;
    
    [Header("Prefabs")]
    public GameObject[] buildingPrefabs;
    public GameObject[] debrisPrefabs;
    public GameObject[] metalPieces;
    
    private System.Random rng;
    private float[,] noiseMap;
    
    void Start()
    {
        if (randomSeed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        
        rng = new System.Random(seed);
        GenerateNoiseMap();
        GenerateWorld();
    }
    
    void GenerateNoiseMap()
    {
        noiseMap = new float[terrainWidth, terrainHeight];
        
        float halfWidth = terrainWidth / 2f;
        float halfHeight = terrainHeight / 2f;
        
        for (int x = 0; x < terrainWidth; x++)
        {
            for (int z = 0; z < terrainHeight; z++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;
                
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / terrainScale * frequency;
                    float sampleZ = (z - halfHeight) / terrainScale * frequency;
                    
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2f - 1f;
                    noiseHeight += perlinValue * amplitude;
                    
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                
                noiseMap[x, z] = noiseHeight;
            }
        }
    }
    
    void GenerateWorld()
    {
        GenerateBuildings();
        GenerateDebris();
        GenerateMetalPieces();
    }
    
    void GenerateBuildings()
    {
        if (buildingPrefabs == null || buildingPrefabs.Length == 0) return;
        
        for (int i = 0; i < objectCount / 3; i++)
        {
            Vector3 position = GetRandomPosition();
            position.y = GetHeightAt(position.x, position.z);
            
            Quaternion rotation = Quaternion.Euler(0, rng.Next(0, 360), 0);
            GameObject building = buildingPrefabs[rng.Next(0, buildingPrefabs.Length)];
            
            Instantiate(building, position, rotation);
        }
    }
    
    void GenerateDebris()
    {
        if (debrisPrefabs == null || debrisPrefabs.Length == 0) return;
        
        for (int i = 0; i < objectCount / 2; i++)
        {
            Vector3 position = GetRandomPosition();
            position.y = GetHeightAt(position.x, position.z);
            
            Quaternion rotation = Quaternion.Euler(
                rng.Next(-15, 15),
                rng.Next(0, 360),
                rng.Next(-15, 15)
            );
            
            GameObject debris = debrisPrefabs[rng.Next(0, debrisPrefabs.Length)];
            Instantiate(debris, position, rotation);
        }
    }
    
    void GenerateMetalPieces()
    {
        if (metalPieces == null || metalPieces.Length == 0) return;
        
        for (int i = 0; i < objectCount; i++)
        {
            Vector3 position = GetRandomPosition();
            position.y = GetHeightAt(position.x, position.z) + 0.5f;
            
            Quaternion rotation = Quaternion.Euler(0, rng.Next(0, 360), 90);
            GameObject metal = metalPieces[rng.Next(0, metalPieces.Length)];
            
            GameObject obj = Instantiate(metal, position, rotation);
            
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddTorque(Vector3.one * rng.Next(1, 10));
            }
        }
    }
    
    Vector3 GetRandomPosition()
    {
        float x = rng.Next(0, terrainWidth);
        float z = rng.Next(0, terrainHeight);
        return new Vector3(x, 0, z);
    }
    
    float GetHeightAt(float x, float z)
    {
        int xi = Mathf.RoundToInt(x);
        int zi = Mathf.RoundToInt(z);
        
        xi = Mathf.Clamp(xi, 0, terrainWidth - 1);
        zi = Mathf.Clamp(zi, 0, terrainHeight - 1);
        
        return noiseMap[xi, zi];
    }
    
    public float[,] GetNoiseMap()
    {
        return noiseMap;
    }
    
    public void Regenerate()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        if (randomSeed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        
        rng = new System.Random(seed);
        GenerateNoiseMap();
        GenerateWorld();
    }
}

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public int width = 50;
    public int height = 50;
    public int roomCount = 10;
    public int minRoomSize = 5;
    public int maxRoomSize = 15;
    
    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject doorPrefab;
    public GameObject chestPrefab;
    
    private int[,] dungeonMap;
    private List<Rect> rooms = new List<Rect>();
    
    void Start()
    {
        GenerateDungeon();
    }
    
    void GenerateDungeon()
    {
        dungeonMap = new int[width, height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                dungeonMap[x, y] = 1;
            }
        }
        
        for (int i = 0; i < roomCount; i++)
        {
            CreateRoom();
        }
        
        ConnectRooms();
        PlaceObjects();
        BuildDungeon();
    }
    
    void CreateRoom()
    {
        int roomWidth = Random.Range(minRoomSize, maxRoomSize);
        int roomHeight = Random.Range(minRoomSize, maxRoomSize);
        int x = Random.Range(1, width - roomWidth - 1);
        int y = Random.Range(1, height - roomHeight - 1);
        
        Rect room = new Rect(x, y, roomWidth, roomHeight);
        rooms.Add(room);
        
        for (int rx = x; rx < x + roomWidth; rx++)
        {
            for (int ry = y; ry < y + roomHeight; ry++)
            {
                dungeonMap[rx, ry] = 0;
            }
        }
    }
    
    void ConnectRooms()
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Vector2 center1 = new Vector2(rooms[i].x + rooms[i].width / 2, rooms[i].y + rooms[i].height / 2);
            Vector2 center2 = new Vector2(rooms[i + 1].x + rooms[i + 1].width / 2, rooms[i + 1].y + rooms[i + 1].height / 2);
            
            if (Random.value > 0.5f)
            {
                CreateHorizontalTunnel((int)center1.x, (int)center2.x, (int)center1.y);
                CreateVerticalTunnel((int)center1.y, (int)center2.y, (int)center2.x);
            }
            else
            {
                CreateVerticalTunnel((int)center1.y, (int)center2.y, (int)center1.x);
                CreateHorizontalTunnel((int)center1.x, (int)center2.x, (int)center2.y);
            }
        }
    }
    
    void CreateHorizontalTunnel(int x1, int x2, int y)
    {
        for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
        {
            dungeonMap[x, y] = 0;
        }
    }
    
    void CreateVerticalTunnel(int y1, int y2, int x)
    {
        for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
        {
            dungeonMap[x, y] = 0;
        }
    }
    
    void PlaceObjects()
    {
        foreach (Rect room in rooms)
        {
            if (Random.value > 0.7f && chestPrefab != null)
            {
                int x = (int)(room.x + room.width / 2);
                int z = (int)(room.y + room.height / 2);
                Instantiate(chestPrefab, new Vector3(x, 1, z), Quaternion.identity);
            }
        }
    }
    
    void BuildDungeon()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = new Vector3(x, 0, z);
                
                if (dungeonMap[x, z] == 0)
                {
                    if (floorPrefab != null)
                    {
                        Instantiate(floorPrefab, position, Quaternion.identity);
                    }
                }
                else if (dungeonMap[x, z] == 1)
                {
                    if (wallPrefab != null)
                    {
                        Instantiate(wallPrefab, position, Quaternion.identity);
                    }
                }
            }
        }
    }
}
