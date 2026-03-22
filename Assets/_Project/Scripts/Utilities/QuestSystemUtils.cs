using System.Collections.Generic;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for managing quests in Mistborn
    /// </summary>
    public static class QuestSystemUtils
    {
        private static Dictionary<string, QuestData> activeQuests = new Dictionary<string, QuestData>();
        private static Dictionary<string, QuestData> completedQuests = new Dictionary<string, QuestData>();
        
        /// <summary>
        /// Quest data structure
        /// </summary>
        public class QuestData
        {
            public string questId;
            public string title;
            public string description;
            public QuestStatus status;
            public List<QuestObjective> objectives;
            public QuestReward reward;
            public string requiredLocation;
            public string requiredItem;
            public int requiredLevel;
            
            public QuestData(string id, string questTitle, string questDescription)
            {
                questId = id;
                title = questTitle;
                description = questDescription;
                status = QuestStatus.NotStarted;
                objectives = new List<QuestObjective>();
            }
        }
        
        public enum QuestStatus
        {
            NotStarted,
            InProgress,
            Completed,
            Failed
        }
        
        public class QuestObjective
        {
            public string objectiveId;
            public string description;
            public int requiredAmount;
            public int currentAmount;
            public bool isCompleted;
            
            public QuestObjective(string id, string desc, int required)
            {
                objectiveId = id;
                description = desc;
                requiredAmount = required;
                currentAmount = 0;
                isCompleted = false;
            }
            
            public void UpdateProgress(int amount)
            {
                currentAmount = Mathf.Min(currentAmount + amount, requiredAmount);
                isCompleted = currentAmount >= requiredAmount;
            }
        }
        
        public class QuestReward
        {
            public int experiencePoints;
            public int skillPoints;
            public string[] itemRewards;
            public int metalRewardAmount;
            public string metalType;
        }
        
        /// <summary>
        /// Starts a new quest
        /// </summary>
        public static bool StartQuest(string questId, string title, string description)
        {
            if (activeQuests.ContainsKey(questId) || completedQuests.ContainsKey(questId))
                return false;
            
            QuestData quest = new QuestData(questId, title, description);
            quest.status = QuestStatus.InProgress;
            activeQuests[questId] = quest;
            return true;
        }
        
        /// <summary>
        /// Adds an objective to a quest
        /// </summary>
        public static void AddObjective(string questId, string objectiveId, string description, int requiredAmount)
        {
            if (activeQuests.TryGetValue(questId, out QuestData quest))
            {
                QuestObjective objective = new QuestObjective(objectiveId, description, requiredAmount);
                quest.objectives.Add(objective);
            }
        }
        
        /// <summary>
        /// Updates quest objective progress
        /// </summary>
        public static bool UpdateObjectiveProgress(string questId, string objectiveId, int progressAmount)
        {
            if (activeQuests.TryGetValue(questId, out QuestData quest))
            {
                foreach (QuestObjective objective in quest.objectives)
                {
                    if (objective.objectiveId == objectiveId)
                    {
                        objective.UpdateProgress(progressAmount);
                        CheckQuestCompletion(questId);
                        return true;
                    }
                }
            }
            return false;
        }
        
        /// <summary>
        /// Checks if all objectives are completed
        /// </summary>
        private static void CheckQuestCompletion(string questId)
        {
            if (!activeQuests.TryGetValue(questId, out QuestData quest))
                return;
            
            bool allCompleted = true;
            foreach (QuestObjective objective in quest.objectives)
            {
                if (!objective.isCompleted)
                {
                    allCompleted = false;
                    break;
                }
            }
            
            if (allCompleted)
            {
                CompleteQuest(questId);
            }
        }
        
        /// <summary>
        /// Completes a quest
        /// </summary>
        public static bool CompleteQuest(string questId)
        {
            if (activeQuests.TryGetValue(questId, out QuestData quest))
            {
                quest.status = QuestStatus.Completed;
                completedQuests[questId] = quest;
                activeQuests.Remove(questId);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Fails a quest
        /// </summary>
        public static bool FailQuest(string questId)
        {
            if (activeQuests.TryGetValue(questId, out QuestData quest))
            {
                quest.status = QuestStatus.Failed;
                activeQuests.Remove(questId);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Gets quest status
        /// </summary>
        public static QuestStatus GetQuestStatus(string questId)
        {
            if (activeQuests.TryGetValue(questId, out QuestData quest))
                return quest.status;
            if (completedQuests.ContainsKey(questId))
                return QuestStatus.Completed;
            return QuestStatus.NotStarted;
        }
        
        /// <summary>
        /// Gets quest progress percentage (0-100)
        /// </summary>
        public static float GetQuestProgress(string questId)
        {
            if (!activeQuests.TryGetValue(questId, out QuestData quest))
                return 0f;
            
            if (quest.objectives.Count == 0) return 0f;
            
            float totalProgress = 0f;
            foreach (QuestObjective objective in quest.objectives)
            {
                totalProgress += Mathf.Clamp01((float)objective.currentAmount / objective.requiredAmount);
            }
            
            return (totalProgress / quest.objectives.Count) * 100f;
        }
        
        /// <summary>
        /// Gets all active quests
        /// </summary>
        public static List<QuestData> GetActiveQuests()
        {
            return new List<QuestData>(activeQuests.Values);
        }
        
        /// <summary>
        /// Gets all completed quests
        /// </summary>
        public static List<QuestData> GetCompletedQuests()
        {
            return new List<QuestData>(completedQuests.Values);
        }
        
        /// <summary>
        /// Sets quest reward
        /// </summary>
        public static void SetQuestReward(string questId, QuestReward reward)
        {
            if (activeQuests.TryGetValue(questId, out QuestData quest))
            {
                quest.reward = reward;
            }
        }
        
        /// <summary>
        /// Sets quest requirements
        /// </summary>
        public static void SetQuestRequirements(string questId, string location = null, string item = null, int level = 0)
        {
            if (activeQuests.TryGetValue(questId, out QuestData quest))
            {
                quest.requiredLocation = location;
                quest.requiredItem = item;
                quest.requiredLevel = level;
            }
        }
        
        /// <summary>
        /// Checks if player meets quest requirements
        /// </summary>
        public static bool MeetsRequirements(string questId, string currentLocation, string[] inventoryItems, int playerLevel)
        {
            if (!activeQuests.TryGetValue(questId, out QuestData quest))
                return false;
            
            // Check location requirement
            if (!string.IsNullOrEmpty(quest.requiredLocation) && quest.requiredLocation != currentLocation)
                return false;
            
            // Check item requirement
            if (!string.IsNullOrEmpty(quest.requiredItem))
            {
                bool hasItem = false;
                foreach (string item in inventoryItems)
                {
                    if (item == quest.requiredItem)
                    {
                        hasItem = true;
                        break;
                    }
                }
                if (!hasItem) return false;
            }
            
            // Check level requirement
            if (quest.requiredLevel > 0 && playerLevel < quest.requiredLevel)
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Resets all quest progress
        /// </summary>
        public static void ResetAllQuests()
        {
            activeQuests.Clear();
            completedQuests.Clear();
        }
        
        /// <summary>
        /// Gets quest count
        /// </summary>
        public static int GetActiveQuestCount()
        {
            return activeQuests.Count;
        }
        
        /// <summary>
        /// Gets completed quest count
        /// </summary>
        public static int GetCompletedQuestCount()
        {
            return completedQuests.Count;
        }
    }
}
