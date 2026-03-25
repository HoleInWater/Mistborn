using UnityEngine;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    private static EventManager _instance;
    public static EventManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<EventManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("EventManager");
                    _instance = go.AddComponent<EventManager>();
                }
            }
            return _instance;
        }
    }

    private Dictionary<string, System.Action<Dictionary<string, object>>> eventListeners = new Dictionary<string, System.Action<Dictionary<string, object>>>();
    private Dictionary<string, List<System.Delegate>> listeners = new Dictionary<string, List<System.Delegate>>();

    public static void RegisterEvent(string eventName, System.Action callback)
    {
        Instance.AddListener(eventName, callback);
    }

    public static void RegisterEvent<T>(string eventName, System.Action<T> callback)
    {
        Instance.AddListener<T>(eventName, callback);
    }

    public static void RegisterEvent<T1, T2>(string eventName, System.Action<T1, T2> callback)
    {
        Instance.AddListener<T1, T2>(eventName, callback);
    }

    public static void UnregisterEvent(string eventName, System.Action callback)
    {
        Instance.RemoveListener(eventName, callback);
    }

    public static void UnregisterEvent<T>(string eventName, System.Action<T> callback)
    {
        Instance.RemoveListener<T>(eventName, callback);
    }

    public static void TriggerEvent(string eventName, Dictionary<string, object> data = null)
    {
        Instance.InvokeEvent(eventName, data);
    }

    public static void TriggerEvent<T>(string eventName, T param)
    {
        Instance.InvokeEvent<T>(eventName, param);
    }

    void AddListener(string eventName, System.Action callback)
    {
        if (!listeners.ContainsKey(eventName))
        {
            listeners[eventName] = new List<System.Delegate>();
        }
        listeners[eventName].Add(callback);
    }

    void AddListener<T>(string eventName, System.Action<T> callback)
    {
        if (!listeners.ContainsKey(eventName))
        {
            listeners[eventName] = new List<System.Delegate>();
        }
        listeners[eventName].Add(callback);
    }

    void AddListener<T1, T2>(string eventName, System.Action<T1, T2> callback)
    {
        if (!listeners.ContainsKey(eventName))
        {
            listeners[eventName] = new List<System.Delegate>();
        }
        listeners[eventName].Add(callback);
    }

    void RemoveListener(string eventName, System.Action callback)
    {
        if (listeners.ContainsKey(eventName))
        {
            listeners[eventName].Remove(callback);
        }
    }

    void RemoveListener<T>(string eventName, System.Action<T> callback)
    {
        if (listeners.ContainsKey(eventName))
        {
            listeners[eventName].Remove(callback);
        }
    }

    void InvokeEvent(string eventName, Dictionary<string, object> data)
    {
        if (listeners.ContainsKey(eventName))
        {
            foreach (System.Delegate d in listeners[eventName])
            {
                try
                {
                    ((System.Action) d)?.Invoke();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[EVENT] Error invoking {eventName}: {e.Message}");
                }
            }
        }
    }

    void InvokeEvent<T>(string eventName, T param)
    {
        if (listeners.ContainsKey(eventName))
        {
            foreach (System.Delegate d in listeners[eventName])
            {
                try
                {
                    ((System.Action<T>) d)?.Invoke(param);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[EVENT] Error invoking {eventName}: {e.Message}");
                }
            }
        }
    }
}

public class SaveLoadManager : MonoBehaviour
{
    [Header("Save Settings")]
    public string saveFolder = "Saves";
    public int maxSaveSlots = 10;

    [System.Serializable]
    public class SaveData
    {
        public string saveName;
        public System.DateTime saveTime;
        public Vector3 playerPosition;
        public Quaternion playerRotation;
        public List<InventoryItem> inventory;
        public List<string> completedQuests;
        public Dictionary<string, float> metalReserves;
        public Dictionary<string, bool> flags;
    }

    public void SaveGame(int slot, string saveName)
    {
        SaveData data = new SaveData();
        data.saveName = saveName;
        data.saveTime = System.DateTime.Now;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosition = player.transform.position;
            data.playerRotation = player.transform.rotation;

            Allomancer allomancer = player.GetComponent<Allomancer>();
            if (allomancer != null)
            {
                data.metalReserves = new Dictionary<string, float>();
                for (int i = 0; i < allomancer.metalReserves.Length; i++)
                {
                    data.metalReserves[((AllomancySkill.MetalType)i).ToString()] = allomancer.metalReserves[i];
                }
            }
        }

        if (Inventory.Instance != null)
        {
            data.inventory = Inventory.Instance.items;
        }

        if (QuestManager.Instance != null)
        {
            data.completedQuests = new List<string>();
            foreach (Quest q in QuestManager.Instance.GetCompletedQuests())
            {
                data.completedQuests.Add(q.questId);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        string path = $"{Application.persistentDataPath}/{saveFolder}/save_{slot}.json";
        
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
        System.IO.File.WriteAllText(path, json);

        Debug.Log($"[SAVE] Game saved to slot {slot}: {saveName}");
    }

    public SaveData LoadGame(int slot)
    {
        string path = $"{Application.persistentDataPath}/{saveFolder}/save_{slot}.json";
        
        if (!System.IO.File.Exists(path))
        {
            Debug.Log($"[SAVE] No save found in slot {slot}");
            return null;
        }

        string json = System.IO.File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        ApplySaveData(data);

        Debug.Log($"[SAVE] Game loaded from slot {slot}");
        return data;
    }

    void ApplySaveData(SaveData data)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && data.playerPosition != null)
        {
            player.transform.position = data.playerPosition;
            player.transform.rotation = data.playerRotation;
        }

        if (data.metalReserves != null && player != null)
        {
            Allomancer allomancer = player.GetComponent<Allomancer>();
            if (allomancer != null)
            {
                foreach (var kvp in data.metalReserves)
                {
                    if (System.Enum.TryParse<AllomancySkill.MetalType>(kvp.Key, out AllomancySkill.MetalType metal))
                    {
                        allomancer.metalReserves[(int)metal] = kvp.Value;
                    }
                }
            }
        }
    }

    public bool HasSaveData(int slot)
    {
        string path = $"{Application.persistentDataPath}/{saveFolder}/save_{slot}.json";
        return System.IO.File.Exists(path);
    }

    public void DeleteSave(int slot)
    {
        string path = $"{Application.persistentDataPath}/{saveFolder}/save_{slot}.json";
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
    }
}