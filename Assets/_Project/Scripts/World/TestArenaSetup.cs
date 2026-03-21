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
                GenerateBasicArena();
            
            if (GUILayout.Button("Add Metal Coins"))
                AddMetalCoins();
            
            if (GUILayout.Button("Add Anchored Metal"))
                AddAnchoredMetal();
            
            if (GUILayout.Button("Clear Arena"))
                ClearArena();
        }
        
        private void GenerateBasicArena()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(10, 1, 10);
            
            CreateWall(new Vector3(0, 5, 25), new Vector3(50, 10, 1), "North Wall");
            CreateWall(new Vector3(0, 5, -25), new Vector3(50, 10, 1), "South Wall");
            CreateWall(new Vector3(25, 5, 0), new Vector3(1, 10, 50), "East Wall");
            CreateWall(new Vector3(-25, 5, 0), new Vector3(1, 10, 50), "West Wall");
            
            Debug.Log("Basic arena generated!");
        }
        
        private void CreateWall(Vector3 pos, Vector3 scale, string name)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            
            AllomanticTarget target = wall.AddComponent<AllomanticTarget>();
            target.SetAnchored(true);
            target.SetMetalMass(500f);
        }
        
        private void AddMetalCoins()
        {
            for (int i = 0; i < 10; i++)
            {
                GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                coin.name = $"Coin_{i}";
                coin.transform.position = new Vector3(Random.Range(-10f, 10f), 1f, Random.Range(-10f, 10f));
                coin.transform.localScale = Vector3.one * 0.1f;
                
                coin.AddComponent<Rigidbody>();
                AllomanticTarget target = coin.AddComponent<AllomanticTarget>();
                target.SetAnchored(false);
                target.SetMetalMass(0.1f);
                
                Renderer renderer = coin.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = Color.gray;
            }
            
            Debug.Log("Added 10 metal coins!");
        }
        
        private void AddAnchoredMetal()
        {
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f;
                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                
                GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bracket.name = $"Bracket_{i}";
                bracket.transform.position = dir * 20f + Vector3.up * Random.Range(2f, 8f);
                bracket.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                
                AllomanticTarget target = bracket.AddComponent<AllomanticTarget>();
                target.SetAnchored(true);
                target.SetMetalMass(50f);
            }
            
            GameObject floorPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorPlate.name = "FloorPlate_Center";
            floorPlate.transform.position = Vector3.zero;
            floorPlate.transform.localScale = new Vector3(2f, 0.1f, 2f);
            
            AllomanticTarget target = floorPlate.AddComponent<AllomanticTarget>();
            target.SetAnchored(true);
            target.SetMetalMass(100f);
            
            Debug.Log("Added anchored metal!");
        }
        
        private void ClearArena()
        {
            string[] exclude = { "Player", "Main Camera", "Directional Light" };
            
            foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                bool skip = false;
                foreach (string name in exclude)
                    if (obj.name.Contains(name)) { skip = true; break; }
                
                if (!skip)
                    DestroyImmediate(obj);
            }
            
            Debug.Log("Arena cleared!");
        }
    }
}
#endif
