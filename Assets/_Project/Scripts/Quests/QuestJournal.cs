using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class QuestJournal : MonoBehaviour
    {
        [Header("Quest Settings")]
        [SerializeField] private int maxActiveQuests = 10;
        [SerializeField] private int maxCompletedQuests = 50;
        
        [Header("UI References")]
        [SerializeField] private GameObject journalUI;
        [SerializeField] private UnityEngine.UI.Text questTitleText;
        [SerializeField] private UnityEngine.UI.Text questDescriptionText;
        [SerializeField] private UnityEngine.UI.Text questObjectivesText;
        
        private List<Quest> activeQuests = new List<Quest>();
        private List<Quest> completedQuests = new List<Quest>();
        private Quest currentlySelectedQuest;
        
        public struct Quest
        {
            public string questID;
            public string questName;
            public string description;
            public List<Objective> objectives;
            public int levelRequirement;
            public int rewardGold;
            public List<string> rewardItems;
            public int rewardExperience;
            public bool isMainStory;
            public string giverNPC;
        }
        
        public struct Objective
        {
            public string description;
            public int targetCount;
            public int currentCount;
            public bool isComplete;
            public ObjectiveType type;
            public string targetID;
        }
        
        public enum ObjectiveType
        {
            Kill,
            Collect,
            Talk,
            Visit,
            Escort,
            Craft,
            Use
        }
        
        private void Start()
        {
            if (journalUI != null)
            {
                journalUI.SetActive(false);
            }
        }
        
        public void OpenJournal()
        {
            if (journalUI != null)
            {
                journalUI.SetActive(true);
            }
            
            if (OnJournalOpened != null)
            {
                OnJournalOpened();
            }
        }
        
        public void CloseJournal()
        {
            if (journalUI != null)
            {
                journalUI.SetActive(false);
            }
            
            if (OnJournalClosed != null)
            {
                OnJournalClosed();
            }
        }
        
        public bool AddQuest(Quest quest)
        {
            if (activeQuests.Count >= maxActiveQuests)
            {
                return false;
            }
            
            foreach (Quest existingQuest in activeQuests)
            {
                if (existingQuest.questID == quest.questID)
                {
                    return false;
                }
            }
            
            activeQuests.Add(quest);
            
            if (OnQuestAdded != null)
            {
                OnQuestAdded(quest);
            }
            
            return true;
        }
        
        public bool RemoveQuest(string questID)
        {
            for (int i = 0; i < activeQuests.Count; i++)
            {
                if (activeQuests[i].questID == questID)
                {
                    Quest quest = activeQuests[i];
                    activeQuests.RemoveAt(i);
                    
                    if (OnQuestRemoved != null)
                    {
                        OnQuestRemoved(quest);
                    }
                    
                    return true;
                }
            }
            
            return false;
        }
        
        public void UpdateQuestObjective(string questID, string objectiveTargetID, int progress = 1)
        {
            for (int i = 0; i < activeQuests.Count; i++)
            {
                if (activeQuests[i].questID == questID)
                {
                    Quest quest = activeQuests[i];
                    
                    for (int j = 0; j < quest.objectives.Count; j++)
                    {
                        if (quest.objectives[j].targetID == objectiveTargetID && !quest.objectives[j].isComplete)
                        {
                            quest.objectives[j].currentCount += progress;
                            
                            if (quest.objectives[j].currentCount >= quest.objectives[j].targetCount)
                            {
                                quest.objectives[j].isComplete = true;
                            }
                            
                            activeQuests[i] = quest;
                            
                            if (OnObjectiveUpdated != null)
                            {
                                OnObjectiveUpdated(quest, quest.objectives[j]);
                            }
                            
                            if (IsQuestComplete(quest))
                            {
                                CompleteQuest(questID);
                            }
                            
                            return;
                        }
                    }
                }
            }
        }
        
        public bool IsQuestComplete(Quest quest)
        {
            foreach (Objective objective in quest.objectives)
            {
                if (!objective.isComplete)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public void CompleteQuest(string questID)
        {
            for (int i = 0; i < activeQuests.Count; i++)
            {
                if (activeQuests[i].questID == questID)
                {
                    Quest quest = activeQuests[i];
                    
                    AwardQuestRewards(quest);
                    
                    activeQuests.RemoveAt(i);
                    
                    if (completedQuests.Count >= maxCompletedQuests)
                    {
                        completedQuests.RemoveAt(0);
                    }
                    
                    completedQuests.Add(quest);
                    
                    if (OnQuestCompleted != null)
                    {
                        OnQuestCompleted(quest);
                    }
                    
                    return;
                }
            }
        }
        
        private void AwardQuestRewards(Quest quest)
        {
            PlayerStats stats = GetComponent<PlayerStats>();
            if (stats != null)
            {
                if (quest.rewardGold > 0)
                {
                    stats.AddGold(quest.rewardGold);
                }
                
                if (quest.rewardExperience > 0)
                {
                    stats.AddExperience(quest.rewardExperience);
                }
            }
            
            InventorySystem inventory = GetComponent<InventorySystem>();
            if (inventory != null && quest.rewardItems != null)
            {
                foreach (string itemName in quest.rewardItems)
                {
                    inventory.AddItem(itemName, 1);
                }
            }
        }
        
        public void SelectQuest(Quest quest)
        {
            currentlySelectedQuest = quest;
            
            if (questTitleText != null)
            {
                questTitleText.text = quest.questName;
            }
            
            if (questDescriptionText != null)
            {
                questDescriptionText.text = quest.description;
            }
            
            if (questObjectivesText != null)
            {
                questObjectivesText.text = "";
                
                foreach (Objective objective in quest.objectives)
                {
                    string status = objective.isComplete ? "[X] " : "[ ] ";
                    questObjectivesText.text += status + objective.description + " (" + objective.currentCount + "/" + objective.targetCount + ")\n";
                }
            }
            
            if (OnQuestSelected != null)
            {
                OnQuestSelected(quest);
            }
        }
        
        public List<Quest> GetActiveQuests()
        {
            return new List<Quest>(activeQuests);
        }
        
        public List<Quest> GetCompletedQuests()
        {
            return new List<Quest>(completedQuests);
        }
        
        public Quest? GetQuest(string questID)
        {
            foreach (Quest quest in activeQuests)
            {
                if (quest.questID == questID)
                {
                    return quest;
                }
            }
            
            foreach (Quest quest in completedQuests)
            {
                if (quest.questID == questID)
                {
                    return quest;
                }
            }
            
            return null;
        }
        
        public bool HasQuest(string questID)
        {
            return GetQuest(questID).HasValue;
        }
        
        public bool IsQuestCompleted(string questID)
        {
            foreach (Quest quest in completedQuests)
            {
                if (quest.questID == questID)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        public void ClearAllQuests()
        {
            activeQuests.Clear();
            completedQuests.Clear();
            currentlySelectedQuest = default(Quest);
        }
        
        public event System.Action OnJournalOpened;
        public event System.Action OnJournalClosed;
        public event System.Action<Quest> OnQuestAdded;
        public event System.Action<Quest> OnQuestRemoved;
        public event System.Action<Quest> OnQuestCompleted;
        public event System.Action<Quest, Objective> OnObjectiveUpdated;
        public event System.Action<Quest> OnQuestSelected;
    }
}
