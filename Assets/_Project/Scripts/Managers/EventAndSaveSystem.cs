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
    public static SaveLoadManager Instance { get; private set; }

    public enum SaveSlot
    {
        Slot1 = 0, Slot2 = 1, Slot3 = 2, Slot4 = 3, Slot5 = 4,
        Slot6 = 5, Slot7 = 6, Slot8 = 7, Slot9 = 8, Slot10 = 9
    }

    [Header("Save Settings")]
    public string saveFolder = "Saves";
    public int maxSaveSlots = 10;

    // JsonUtility doesn't support Dictionary<>, so use serializable list pairs
    [System.Serializable]
    public class SerializableKeyValue<T>
    {
        public string key;
        public T value;
        public SerializableKeyValue(string k, T v) { key = k; value = v; }
    }

    [System.Serializable]
    public class SaveData
    {
        // Meta
        public string saveName;
        public string saveTimeString; // DateTime as ISO string (JsonUtility can't serialize DateTime)
        public float playTimeSeconds;
        public int chapterIndex;

        // Player Transform
        public Vector3 playerPosition;
        public Quaternion playerRotation;
        public float playerHealth;
        public float playerStamina;

        // Allomancy State
        public float[] metalReserves = new float[20];
        public bool[] unlockedMetals = new bool[20];
        public bool isDuraluminPrimed;
        public int flareIntensity;

        // Inventory
        public List<InventoryItem> inventory = new List<InventoryItem>();

        // Quests
        public List<string> activeQuestIds = new List<string>();
        public List<string> completedQuestIds = new List<string>();

        // Story / Dialogue Flags
        public List<SerializableKeyValue<bool>> storyFlags = new List<SerializableKeyValue<bool>>();

        // Skill Tree
        public List<string> unlockedSkillIds = new List<string>();

        // Player Progression
        public int playerLevel;
        public float playerExperience;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void SaveGame(int slot, string saveName)
    {
        SaveData data = new SaveData();
        data.saveName = saveName;
        data.saveTimeString = System.DateTime.Now.ToString("o");
        data.chapterIndex = 0;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosition = player.transform.position;
            data.playerRotation = player.transform.rotation;

            // Allomancy state
            Allomancer allomancer = player.GetComponent<Allomancer>();
            if (allomancer != null)
            {
                System.Array.Copy(allomancer.metalReserves, data.metalReserves,
                    Mathf.Min(allomancer.metalReserves.Length, data.metalReserves.Length));
                System.Array.Copy(allomancer.unlockedMetals, data.unlockedMetals,
                    Mathf.Min(allomancer.unlockedMetals.Length, data.unlockedMetals.Length));
                data.isDuraluminPrimed = allomancer.isDuraluminPrimed;
            }

            // Flare state
            if (FlareManager.Instance != null)
                data.flareIntensity = FlareManager.Instance.Intensity;

            // Health
            IDamageable damageable = player.GetComponent<IDamageable>();
            if (damageable != null)
                data.playerHealth = damageable.GetCurrentHealth();

            // Stamina
            PlayerStamina stamina = player.GetComponent<PlayerStamina>();
            if (stamina != null)
                data.playerStamina = stamina.GetCurrentStamina();

            // Experience
            PlayerExperience xp = player.GetComponent<PlayerExperience>();
            if (xp != null)
            {
                data.playerLevel = xp.GetLevel();
                data.playerExperience = xp.GetExperience();
            }
        }

        // Inventory
        if (Inventory.Instance != null)
            data.inventory = Inventory.Instance.items;

        // Quests
        if (QuestManager.Instance != null)
        {
            data.completedQuestIds = new List<string>();
            foreach (Quest q in QuestManager.Instance.GetCompletedQuests())
                data.completedQuestIds.Add(q.questId);

            data.activeQuestIds = new List<string>();
            foreach (Quest q in QuestManager.Instance.GetActiveQuests())
                data.activeQuestIds.Add(q.questId);
        }

        // Story flags from GameFlowManager
        GameFlowManager gfm = FindObjectOfType<GameFlowManager>();
        if (gfm != null)
        {
            data.chapterIndex = gfm.GetChapterIndex();
            foreach (var kvp in gfm.GetStoryFlags())
                data.storyFlags.Add(new SerializableKeyValue<bool>(kvp.Key, kvp.Value));
        }

        // Write to disk
        string json = JsonUtility.ToJson(data, true);
        string path = GetSavePath(slot);

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
        System.IO.File.WriteAllText(path, json);

        Debug.Log($"[SAVE] Game saved to slot {slot}: {saveName}");
    }

    public SaveData LoadGame(int slot)
    {
        string path = GetSavePath(slot);

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
        if (player != null)
        {
            player.transform.position = data.playerPosition;
            player.transform.rotation = data.playerRotation;

            // Restore Allomancy
            Allomancer allomancer = player.GetComponent<Allomancer>();
            if (allomancer != null)
            {
                System.Array.Copy(data.metalReserves, allomancer.metalReserves,
                    Mathf.Min(data.metalReserves.Length, allomancer.metalReserves.Length));
                System.Array.Copy(data.unlockedMetals, allomancer.unlockedMetals,
                    Mathf.Min(data.unlockedMetals.Length, allomancer.unlockedMetals.Length));
                allomancer.isDuraluminPrimed = data.isDuraluminPrimed;
            }

            // Restore flare
            if (FlareManager.Instance != null)
                FlareManager.Instance.SetIntensity(data.flareIntensity);

            // Restore experience
            PlayerExperience xp = player.GetComponent<PlayerExperience>();
            if (xp != null)
                xp.SetLevelAndExperience(data.playerLevel, data.playerExperience);
        }

        // Restore inventory
        if (Inventory.Instance != null && data.inventory != null)
            Inventory.Instance.items = data.inventory;

        // Restore quests
        if (QuestManager.Instance != null)
        {
            foreach (string questId in data.activeQuestIds)
                QuestManager.Instance.AddQuestById(questId);
        }

        // Restore story flags
        GameFlowManager gfm = FindObjectOfType<GameFlowManager>();
        if (gfm != null)
        {
            gfm.SetChapterIndex(data.chapterIndex);
            foreach (var flag in data.storyFlags)
                gfm.SetStoryFlag(flag.key, flag.value);
        }
    }

    public bool HasSaveData(int slot)
    {
        return System.IO.File.Exists(GetSavePath(slot));
    }

    public void DeleteSave(int slot)
    {
        string path = GetSavePath(slot);
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }

    public SaveData PeekSaveData(int slot)
    {
        string path = GetSavePath(slot);
        if (!System.IO.File.Exists(path)) return null;

        string json = System.IO.File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    string GetSavePath(int slot)
    {
        return $"{Application.persistentDataPath}/{saveFolder}/save_{slot}.json";
    }
}