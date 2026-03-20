// ============================================================
// FILE: TestArenaSetup.cs
// SYSTEM: World
// STATUS: PLANNED
// AUTHOR: 
//
// PURPOSE:
//   Editor utility to quickly set up the TestArena scene with
//   metal objects at various positions for testing push/pull.
//
// TODO (AI Agent):
//   - Create actual Unity scene file
//   - Add menu option to auto-generate test environment
//
// TODO (Team):
//   - Design arena layout
//   - Place metal objects
//   - Set up camera and player start
//
// LAST UPDATED: 2026-03-20
// ============================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Mistborn.World
{
    public class TestArenaSetup : EditorWindow
    {
        [MenuItem("Mistborn/Setup Test Arena")]
        public static void ShowWindow()
        {
            GetWindow<TestArenaSetup>("Test Arena Setup");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Test Arena Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Generate Basic Arena"))
            {
                GenerateBasicArena();
            }
            
            if (GUILayout.Button("Add Metal Coins"))
            {
                AddMetalCoins();
            }
            
            if (GUILayout.Button("Add Anchored Metal"))
            {
                AddAnchoredMetal();
            }
            
            if (GUILayout.Button("Clear Arena"))
            {
                ClearArena();
            }
        }
        
        private void GenerateBasicArena()
        {
            // Create floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(10, 1, 10);
            
            // Create walls
            CreateWall(new Vector3(0, 5, 25), new Vector3(50, 10, 1), "North Wall");
            CreateWall(new Vector3(0, 5, -25), new Vector3(50, 10, 1), "South Wall");
            CreateWall(new Vector3(25, 5, 0), new Vector3(1, 10, 50), "East Wall");
            CreateWall(new Vector3(-25, 5, 0), new Vector3(1, 10, 50), "West Wall");
            
            Debug.Log("Basic arena generated!");
        }
        
        private void CreateWall(Vector3 position, Vector3 scale, string name)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            
            // Make wall metal
            wall.AddComponent<AllomanticTarget>();
            wall.GetComponent<AllomanticTarget>().metalType = AllomanticMetal.Steel;
            wall.GetComponent<AllomanticTarget>().isAnchored = true;
            wall.GetComponent<AllomanticTarget>().metalMass = 500f;
        }
        
        private void AddMetalCoins()
        {
            for (int i = 0; i < 10; i++)
            {
                GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                coin.name = $"Coin_{i}";
                coin.transform.position = new Vector3(
                    Random.Range(-10f, 10f),
                    1f,
                    Random.Range(-10f, 10f)
                );
                coin.transform.localScale = Vector3.one * 0.1f;
                
                coin.AddComponent<Rigidbody>();
                coin.AddComponent<AllomanticTarget>();
                coin.GetComponent<AllomanticTarget>().metalType = AllomanticMetal.Steel;
                coin.GetComponent<AllomanticTarget>().isAnchored = false;
                coin.GetComponent<AllomanticTarget>().metalMass = 0.1f;
                
                // Set silver material
                Renderer renderer = coin.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = Color.gray;
            }
            
            Debug.Log("Added 10 metal coins!");
        }
        
        private void AddAnchoredMetal()
        {
            // Add metal brackets on walls
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f;
                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                
                GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bracket.name = $"Bracket_{i}";
                bracket.transform.position = dir * 20f + Vector3.up * Random.Range(2f, 8f);
                bracket.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                
                bracket.AddComponent<AllomanticTarget>();
                bracket.GetComponent<AllomanticTarget>().metalType = AllomanticMetal.Steel;
                bracket.GetComponent<AllomanticTarget>().isAnchored = true;
                bracket.GetComponent<AllomanticTarget>().metalMass = 50f;
            }
            
            // Add floor metal plate
            GameObject floorPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorPlate.name = "FloorPlate_Center";
            floorPlate.transform.position = Vector3.zero;
            floorPlate.transform.localScale = new Vector3(2f, 0.1f, 2f);
            
            floorPlate.AddComponent<AllomanticTarget>();
            floorPlate.GetComponent<AllomanticTarget>().metalType = AllomanticMetal.Steel;
            floorPlate.GetComponent<AllomanticTarget>().isAnchored = true;
            floorPlate.GetComponent<AllomanticTarget>().metalMass = 100f;
            
            Debug.Log("Added anchored metal!");
        }
        
        private void ClearArena()
        {
            string[] excludeNames = { "Player", "Main Camera", "Directional Light" };
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                bool exclude = false;
                foreach (string name in excludeNames)
                {
                    if (obj.name.Contains(name))
                    {
                        exclude = true;
                        break;
                    }
                }
                
                if (!exclude)
                {
                    DestroyImmediate(obj);
                }
            }
            
            Debug.Log("Arena cleared!");
        }
    }
}
#endif
