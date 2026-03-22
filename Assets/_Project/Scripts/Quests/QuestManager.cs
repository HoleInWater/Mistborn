using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Quests
{
    public class QuestManager : MonoBehaviour
    {
        [Header("Quest Management")]
        [SerializeField] private List<Quest> activeQuests = new List<Quest>();
        [SerializeField] private List<Quest> completedQuests = new List<Quest>();
        [SerializeField] private List<Quest> availableQuests = new List<Quest>();
        [SerializeField] private int maxActiveQuests = 10;
        
        [Header("Quest Database")]
        [SerializeField] private Quest[] allQuests;
        
        [Header("Tracking")]
        [SerializeField] private Quest trackedQuest;
        [SerializeField] private bool showQuestMarkers = true;
        
        [Header("Events")]
        public System.Action<Quest> OnQuestStarted;
        public System.Action<Quest> OnQuestCompleted;
        public System.Action<Quest> OnQuestFailed;
        public System.Action<Quest> OnQuestTracked;
        
        private Dictionary<string, Quest> questDatabase = new Dictionary<string, Quest>();
        
        public static QuestManager instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }
        
        private void Start()
        {
            InitializeQuestDatabase();
            LoadQuests();
        }
        
        private void InitializeQuestDatabase()
        {
            if (allQuests == null || allQuests.Length == 0)
                return;
            
            foreach (var quest in allQuests)
            {
                if (quest != null)
                {
                    questDatabase[quest.QuestId] = quest;
                    
                    if (quest.CurrentState == Quest.QuestState.Available)
                    {
                        availableQuests.Add(quest);
                    }
                }
            }
        }
        
        public void AcceptQuest(Quest quest)
        {
            if (quest == null)
                return;
            
            if (activeQuests.Count >= maxActiveQuests)
            {
                Debug.Log("Maximum active quests reached!");
                return;
            }
            
            if (quest.CurrentState != Quest.QuestState.Available && 
                quest.CurrentState != Quest.QuestState.Inactive)
            {
                return;
            }
            
            quest.StartQuest();
            activeQuests.Add(quest);
            availableQuests.Remove(quest);
            
            OnQuestStarted?.Invoke(quest);
            
            SaveQuests();
        }
        
        public void CompleteQuest(Quest quest)
        {
            if (quest == null)
                return;
            
            quest.CompleteQuest();
            
            activeQuests.Remove(quest);
            completedQuests.Add(quest);
            
            CheckForNewAvailableQuests();
            
            OnQuestCompleted?.Invoke(quest);
            
            SaveQuests();
        }
        
        public void FailQuest(Quest quest)
        {
            if (quest == null)
                return;
            
            quest.FailQuest();
            
            activeQuests.Remove(quest);
            
            OnQuestFailed?.Invoke(quest);
            
            SaveQuests();
        }
        
        public void AbandonQuest(Quest quest)
        {
            if (quest == null)
                return;
            
            quest.AbandonQuest();
            
            activeQuests.Remove(quest);
            
            SaveQuests();
        }
        
        public void TrackQuest(Quest quest)
        {
            if (trackedQuest != null)
            {
                trackedQuest.StopTracking();
            }
            
            trackedQuest = quest;
            
            if (quest != null)
            {
                quest.StartTracking();
            }
            
            OnQuestTracked?.Invoke(quest);
        }
        
        public void StopTrackingQuest()
        {
            if (trackedQuest != null)
            {
                trackedQuest.StopTracking();
                trackedQuest = null;
            }
        }
        
        public void UpdateQuestProgress(string questId, string objectiveId, int amount)
        {
            Quest quest = GetQuest(questId);
            if (quest != null)
            {
                quest.UpdateObjectiveProgress(objectiveId, amount);
            }
        }
        
        public void UpdateQuestProgress(string objectiveId, int amount)
        {
            if (trackedQuest != null)
            {
                trackedQuest.UpdateObjectiveProgress(objectiveId, amount);
            }
            
            foreach (var quest in activeQuests)
            {
                if (quest.GetObjective(objectiveId) != null)
                {
                    quest.UpdateObjectiveProgress(objectiveId, amount);
                    break;
                }
            }
        }
        
        public void OnLocationDiscovered(string locationName)
        {
            foreach (var quest in activeQuests)
            {
                Quest.QuestObjective[] objectives = new Quest.QuestObjective[0];
                var field = quest.GetType().GetField("objectives", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    objectives = (Quest.QuestObjective[])field.GetValue(quest);
                }
                
                if (objectives != null)
                {
                    foreach (var obj in objectives)
                    {
                        if (obj.type == Quest.ObjectiveType.Explore && 
                            obj.targetName == locationName)
                        {
                            quest.UpdateObjectiveProgress(obj.objectiveId, 1);
                        }
                    }
                }
            }
        }
        
        public void OnNPCKilled(string npcId)
        {
            foreach (var quest in activeQuests)
            {
                Quest.QuestObjective[] objectives = new Quest.QuestObjective[0];
                var field = quest.GetType().GetField("objectives", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    objectives = (Quest.QuestObjective[])field.GetValue(quest);
                }
                
                if (objectives != null)
                {
                    foreach (var obj in objectives)
                    {
                        if (obj.type == Quest.ObjectiveType.Kill && obj.targetId == npcId)
                        {
                            quest.UpdateObjectiveProgress(obj.objectiveId, 1);
                        }
                    }
                }
            }
        }
        
        public void OnItemCollected(string itemId, int amount)
        {
            foreach (var quest in activeQuests)
            {
                Quest.QuestObjective[] objectives = new Quest.QuestObjective[0];
                var field = quest.GetType().GetField("objectives", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    objectives = (Quest.QuestObjective[])field.GetValue(quest);
                }
                
                if (objectives != null)
                {
                    foreach (var obj in objectives)
                    {
                        if (obj.type == Quest.ObjectiveType.Collect && obj.targetId == itemId)
                        {
                            quest.UpdateObjectiveProgress(obj.objectiveId, amount);
                        }
                    }
                }
            }
        }
        
        public void OnQuestTriggered(string eventId)
        {
        }
        
        public void OnCompoundAlerted(string compoundName)
        {
        }
        
        public void OnVaultOpened(string houseName)
        {
        }
        
        public void OnNobleBetrayed(string houseName)
        {
        }
        
        private void CheckForNewAvailableQuests()
        {
            foreach (var kvp in questDatabase)
            {
                Quest quest = kvp.Value;
                
                if (quest.CurrentState == Quest.QuestState.Inactive)
                {
                    if (!activeQuests.Contains(quest) && !completedQuests.Contains(quest))
                    {
                        quest.CurrentState = Quest.QuestState.Available;
                        availableQuests.Add(quest);
                    }
                }
            }
        }
        
        public Quest GetQuest(string questId)
        {
            if (questDatabase.ContainsKey(questId))
            {
                return questDatabase[questId];
            }
            
            foreach (var quest in activeQuests)
            {
                if (quest.QuestId == questId)
                    return quest;
            }
            
            return null;
        }
        
        public bool IsQuestActive(string questId)
        {
            Quest quest = GetQuest(questId);
            return quest != null && quest.CurrentState == Quest.QuestState.Active;
        }
        
        public bool IsQuestCompleted(string questId)
        {
            foreach (var quest in completedQuests)
            {
                if (quest.QuestId == questId)
                    return true;
            }
            return false;
        }
        
        public List<Quest> GetActiveQuests()
        {
            return new List<Quest>(activeQuests);
        }
        
        public List<Quest> GetAvailableQuests()
        {
            return new List<Quest>(availableQuests);
        }
        
        public List<Quest> GetCompletedQuests()
        {
            return new List<Quest>(completedQuests);
        }
        
        public Quest GetTrackedQuest()
        {
            return trackedQuest;
        }
        
        public int GetActiveQuestCount()
        {
            return activeQuests.Count;
        }
        
        public int GetCompletedQuestCount()
        {
            return completedQuests.Count;
        }
        
        public int GetAvailableQuestCount()
        {
            return availableQuests.Count;
        }
        
        private void SaveQuests()
        {
            QuestSaveData saveData = new QuestSaveData();
            
            saveData.activeQuests = new List<QuestData>();
            foreach (var quest in activeQuests)
            {
                saveData.activeQuests.Add(quest.GetQuestData());
            }
            
            saveData.completedQuestIds = new List<string>();
            foreach (var quest in completedQuests)
            {
                saveData.completedQuestIds.Add(quest.QuestId);
            }
            
            saveData.trackedQuestId = trackedQuest?.QuestId;
            
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("QuestSaveData", json);
            PlayerPrefs.Save();
        }
        
        private void LoadQuests()
        {
            if (!PlayerPrefs.HasKey("QuestSaveData"))
                return;
            
            string json = PlayerPrefs.GetString("QuestSaveData");
            QuestSaveData saveData = JsonUtility.FromJson<QuestSaveData>(json);
            
            if (saveData.activeQuests != null)
            {
                foreach (var data in saveData.activeQuests)
                {
                    Quest quest = GetQuest(data.questId);
                    if (quest != null)
                    {
                        quest.LoadQuestData(data);
                        activeQuests.Add(quest);
                        availableQuests.Remove(quest);
                    }
                }
            }
            
            if (saveData.completedQuestIds != null)
            {
                foreach (var questId in saveData.completedQuestIds)
                {
                    Quest quest = GetQuest(questId);
                    if (quest != null)
                    {
                        quest.CurrentState = Quest.QuestState.Completed;
                        completedQuests.Add(quest);
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(saveData.trackedQuestId))
            {
                TrackQuest(GetQuest(saveData.trackedQuestId));
            }
        }
        
        public void NewGamePlus()
        {
            activeQuests.Clear();
            completedQuests.Clear();
            availableQuests.Clear();
            
            foreach (var kvp in questDatabase)
            {
                kvp.Value.CurrentState = Quest.QuestState.Inactive;
            }
            
            trackedQuest = null;
            
            PlayerPrefs.DeleteKey("QuestSaveData");
        }
    }
    
    [System.Serializable]
    public class QuestSaveData
    {
        public List<QuestData> activeQuests;
        public List<string> completedQuestIds;
        public string trackedQuestId;
    }
}
