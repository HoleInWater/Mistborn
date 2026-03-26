using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// File-based save/load with comprehensive state persistence.
/// Saves player position, Allomancy state, inventory, quests, story flags, XP.
/// </summary>
public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    public enum SaveSlot { Slot1 = 0, Slot2 = 1, Slot3 = 2, Slot4 = 3, Slot5 = 4 }

    [Header("Settings")]
    public string saveFolder = "Saves";

    [System.Serializable]
    public class SaveData
    {
        public string saveName;
        public string saveTimeString;
        public int chapterIndex;

        // Player
        public Vector3 playerPosition;
        public Quaternion playerRotation;
        public float playerHealth;

        // Allomancy
        public float[] metalReserves = new float[20];
        public bool[] unlockedMetals = new bool[20];

        // Quests
        public List<string> activeQuestIds = new List<string>();
        public List<string> completedQuestIds = new List<string>();

        // Progression
        public int playerLevel;
        public float playerXP;
        public List<string> unlockedSkills = new List<string>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveGame(int slot, string saveName)
    {
        SaveData data = new SaveData();
        data.saveName = saveName;
        data.saveTimeString = System.DateTime.Now.ToString("o");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosition = player.transform.position;
            data.playerRotation = player.transform.rotation;

            Allomancer allo = player.GetComponent<Allomancer>();
            if (allo != null)
            {
                System.Array.Copy(allo.metalReserves, data.metalReserves,
                    Mathf.Min(allo.metalReserves.Length, 20));
                System.Array.Copy(allo.unlockedMetals, data.unlockedMetals,
                    Mathf.Min(allo.unlockedMetals.Length, 20));
            }

            PlayerHealth hp = player.GetComponent<PlayerHealth>();
            if (hp != null) data.playerHealth = hp.GetCurrentHealth();

            PlayerExperience xp = player.GetComponent<PlayerExperience>();
            if (xp != null) { data.playerLevel = xp.GetLevel(); data.playerXP = xp.GetExperience(); }
        }

        if (QuestManager.Instance != null)
        {
            foreach (var q in QuestManager.Instance.GetCompletedQuests())
                data.completedQuestIds.Add(q.questId);
            foreach (var q in QuestManager.Instance.GetActiveQuests())
                data.activeQuestIds.Add(q.questId);
        }

        if (GameFlowManager.Instance != null)
            data.chapterIndex = GameFlowManager.Instance.GetChapterIndex();

        if (AllomanticSkillTree.Instance != null)
            data.unlockedSkills = AllomanticSkillTree.Instance.GetUnlockedSkillIds();

        string json = JsonUtility.ToJson(data, true);
        string path = GetPath(slot);
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
        System.IO.File.WriteAllText(path, json);
    }

    public SaveData LoadGame(int slot)
    {
        string path = GetPath(slot);
        if (!System.IO.File.Exists(path)) return null;

        string json = System.IO.File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        ApplyData(data);
        return data;
    }

    void ApplyData(SaveData data)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = data.playerPosition;
            player.transform.rotation = data.playerRotation;

            Allomancer allo = player.GetComponent<Allomancer>();
            if (allo != null)
            {
                System.Array.Copy(data.metalReserves, allo.metalReserves,
                    Mathf.Min(data.metalReserves.Length, allo.metalReserves.Length));
                System.Array.Copy(data.unlockedMetals, allo.unlockedMetals,
                    Mathf.Min(data.unlockedMetals.Length, allo.unlockedMetals.Length));
            }

            PlayerExperience xp = player.GetComponent<PlayerExperience>();
            if (xp != null) xp.SetLevelAndExperience(data.playerLevel, data.playerXP);
        }

        if (QuestManager.Instance != null)
            foreach (string id in data.activeQuestIds)
                QuestManager.Instance.AddQuestById(id);

        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.SetChapterIndex(data.chapterIndex);

        if (AllomanticSkillTree.Instance != null)
            AllomanticSkillTree.Instance.LoadUnlockedSkills(data.unlockedSkills);
    }

    public bool HasSave(int slot) => System.IO.File.Exists(GetPath(slot));
    public void DeleteSave(int slot) { string p = GetPath(slot); if (System.IO.File.Exists(p)) System.IO.File.Delete(p); }

    public SaveData PeekSaveData(int slot)
    {
        string path = GetPath(slot);
        if (!System.IO.File.Exists(path)) return null;
        return JsonUtility.FromJson<SaveData>(System.IO.File.ReadAllText(path));
    }

    string GetPath(int slot) => $"{Application.persistentDataPath}/{saveFolder}/save_{slot}.json";
}
