// ============================================================
// FILE: SaveLoadSystem.cs
// SYSTEM: Utilities
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Handles saving and loading game state.
//   Saves player position, metal reserves, and progress.
//
// TODO:
//   - Add encryption for save data
//   - Add auto-save functionality
//   - Add multiple save slots
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;
using System;

namespace Mistborn.Utilities
{
    [Serializable]
    public class GameSaveData
    {
        public string saveName;
        public DateTime saveTime;
        
        // Player data
        public float playerPositionX;
        public float playerPositionY;
        public float playerPositionZ;
        public float playerRotationY;
        
        // Metal reserves
        public float steelReserve;
        public float ironReserve;
        public float pewterReserve;
        public float tinReserve;
        
        // Progress
        public string currentScene;
        public int storyProgress;
    }
    
    public class SaveLoadSystem : MonoBehaviour
    {
        public static SaveLoadSystem Instance { get; private set; }
        
        [Header("Save Settings")]
        public string saveFileName = "mistborn_save.json";
        public int maxSaveSlots = 3;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void SaveGame(int slot = 0)
        {
            GameSaveData data = CollectSaveData();
            data.saveName = $"Save Slot {slot}";
            data.saveTime = DateTime.Now;
            
            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath(slot);
            
            System.IO.File.WriteAllText(path, json);
            Debug.Log($"Game saved to {path}");
        }
        
        public bool LoadGame(int slot = 0)
        {
            string path = GetSavePath(slot);
            
            if (!System.IO.File.Exists(path))
            {
                Debug.Log($"No save found at {path}");
                return false;
            }
            
            string json = System.IO.File.ReadAllText(path);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            
            if (data == null)
            {
                Debug.LogError("Failed to parse save file");
                return false;
            }
            
            ApplySaveData(data);
            Debug.Log($"Game loaded from {path}");
            return true;
        }
        
        public bool HasSave(int slot = 0)
        {
            return System.IO.File.Exists(GetSavePath(slot));
        }
        
        public void DeleteSave(int slot = 0)
        {
            string path = GetSavePath(slot);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                Debug.Log($"Save deleted: {path}");
            }
        }
        
        private string GetSavePath(int slot)
        {
            return Application.persistentDataPath + $"/{saveFileName.Replace(".json", "")}_{slot}.json";
        }
        
        private GameSaveData CollectSaveData()
        {
            GameSaveData data = new GameSaveData();
            
            // Get player data
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                data.playerPositionX = player.transform.position.x;
                data.playerPositionY = player.transform.position.y;
                data.playerPositionZ = player.transform.position.z;
                data.playerRotationY = player.transform.eulerAngles.y;
            }
            
            // Get metal reserves
            AllomancerController allomancer = player?.GetComponent<AllomancerController>();
            if (allomancer != null)
            {
                data.steelReserve = allomancer.GetReserve(AllomanticMetal.Steel)?.currentAmount ?? 0;
                data.ironReserve = allomancer.GetReserve(AllomanticMetal.Iron)?.currentAmount ?? 0;
                data.pewterReserve = allomancer.GetReserve(AllomanticMetal.Pewter)?.currentAmount ?? 0;
                data.tinReserve = allomancer.GetReserve(AllomanticMetal.Tin)?.currentAmount ?? 0;
            }
            
            // Get current scene
            data.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            return data;
        }
        
        private void ApplySaveData(GameSaveData data)
        {
            // Restore player position
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(
                    data.playerPositionX,
                    data.playerPositionY,
                    data.playerPositionZ
                );
                player.transform.eulerAngles = new Vector3(0, data.playerRotationY, 0);
            }
            
            // Restore metal reserves
            AllomancerController allomancer = player?.GetComponent<AllomancerController>();
            if (allomancer != null)
            {
                allomancer.GetReserve(AllomanticMetal.Steel).currentAmount = data.steelReserve;
                allomancer.GetReserve(AllomanticMetal.Iron).currentAmount = data.ironReserve;
                allomancer.GetReserve(AllomanticMetal.Pewter).currentAmount = data.pewterReserve;
                allomancer.GetReserve(AllomanticMetal.Tin).currentAmount = data.tinReserve;
            }
            
            // Load scene if different
            if (!string.IsNullOrEmpty(data.currentScene))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(data.currentScene);
            }
        }
    }
}
