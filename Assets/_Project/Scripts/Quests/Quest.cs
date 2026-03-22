using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Quests
{
    public class Quest : MonoBehaviour
    {
        [Header("Quest Configuration")]
        [SerializeField] private string questId;
        [SerializeField] private string questTitle;
        [SerializeField] [TextArea] private string questDescription;
        [SerializeField] private QuestType questType = QuestType.Main;
        [SerializeField] private QuestDifficulty difficulty = QuestDifficulty.Standard;
        
        [Header("Requirements")]
        [SerializeField] private QuestObjective[] objectives;
        [SerializeField] private QuestRequirement[] requirements;
        
        [Header("Rewards")]
        [SerializeField] private int experienceReward = 100;
        [SerializeField] private int metalReward = 50;
        [SerializeField] private int skillPointReward = 1;
        [SerializeField] private string[] itemRewards;
        
        [Header("Quest Giver")]
        [SerializeField] private string questGiverId;
        [SerializeField] private string questGiverName;
        
        [Header("Timing")]
        [SerializeField] private bool hasTimeLimit = false;
        [SerializeField] private float timeLimit = 600f;
        [SerializeField] private float elapsedTime = 0f;
        
        [Header("Branching")]
        [SerializeField] private bool hasBranchingPaths = false;
        [SerializeField] private Quest[] branchQuests;
        [SerializeField] private string selectedBranch;
        
        [Header("State")]
        [SerializeField] private QuestState currentState = QuestState.Inactive;
        
        private Dictionary<string, int> objectiveProgress = new Dictionary<string, int>();
        private bool isTracking = false;
        private float startTime = 0f;
        
        public string QuestId => questId;
        public string QuestTitle => questTitle;
        public string QuestDescription => questDescription;
        public QuestType QuestType => questType;
        public QuestDifficulty Difficulty => difficulty;
        public QuestState CurrentState => currentState;
        public float ElapsedTime => elapsedTime;
        
        public event System.Action<Quest> OnQuestStarted;
        public event System.Action<Quest> OnQuestCompleted;
        public event System.Action<Quest, QuestObjective> OnObjectiveCompleted;
        public event System.Action<Quest, QuestObjective> OnObjectiveUpdated;
        
        private void Start()
        {
            InitializeObjectives();
        }
        
        private void Update()
        {
            if (currentState == QuestState.Active)
            {
                UpdateQuest();
            }
        }
        
        private void InitializeObjectives()
        {
            if (objectives == null)
                return;
            
            foreach (var objective in objectives)
            {
                objectiveProgress[objective.objectiveId] = 0;
            }
        }
        
        public void StartQuest()
        {
            if (currentState != QuestState.Inactive && currentState != QuestState.Available)
                return;
            
            if (!CheckRequirements())
                return;
            
            currentState = QuestState.Active;
            startTime = Time.time;
            elapsedTime = 0f;
            
            OnQuestStarted?.Invoke(this);
            
            Debug.Log($"Quest Started: {questTitle}");
        }
        
        private bool CheckRequirements()
        {
            if (requirements == null)
                return true;
            
            foreach (var requirement in requirements)
            {
                if (!requirement.IsMet())
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public void UpdateQuest()
        {
            if (currentState != QuestState.Active)
                return;
            
            elapsedTime = Time.time - startTime;
            
            if (hasTimeLimit && elapsedTime >= timeLimit)
            {
                FailQuest();
                return;
            }
            
            CheckObjectives();
        }
        
        private void CheckObjectives()
        {
            bool allComplete = true;
            
            foreach (var objective in objectives)
            {
                int current = GetObjectiveProgress(objective.objectiveId);
                
                if (current < objective.targetCount)
                {
                    allComplete = false;
                }
            }
            
            if (allComplete)
            {
                CompleteQuest();
            }
        }
        
        public void UpdateObjectiveProgress(string objectiveId, int amount)
        {
            if (!objectiveProgress.ContainsKey(objectiveId))
                return;
            
            int currentProgress = objectiveProgress[objectiveId];
            int newProgress = currentProgress + amount;
            
            objectiveProgress[objectiveId] = newProgress;
            
            QuestObjective objective = GetObjective(objectiveId);
            if (objective != null)
            {
                OnObjectiveUpdated?.Invoke(this, objective);
                
                if (newProgress >= objective.targetCount && currentProgress < objective.targetCount)
                {
                    CompleteObjective(objectiveId);
                }
            }
        }
        
        public void SetObjectiveProgress(string objectiveId, int amount)
        {
            if (!objectiveProgress.ContainsKey(objectiveId))
                return;
            
            objectiveProgress[objectiveId] = amount;
            
            QuestObjective objective = GetObjective(objectiveId);
            if (objective != null && amount >= objective.targetCount)
            {
                CompleteObjective(objectiveId);
            }
        }
        
        public int GetObjectiveProgress(string objectiveId)
        {
            if (objectiveProgress.ContainsKey(objectiveId))
                return objectiveProgress[objectiveId];
            return 0;
        }
        
        public QuestObjective GetObjective(string objectiveId)
        {
            if (objectives == null)
                return null;
            
            foreach (var obj in objectives)
            {
                if (obj.objectiveId == objectiveId)
                    return obj;
            }
            return null;
        }
        
        public void CompleteObjective(string objectiveId)
        {
            QuestObjective objective = GetObjective(objectiveId);
            if (objective != null)
            {
                objectiveProgress[objectiveId] = objective.targetCount;
                OnObjectiveCompleted?.Invoke(this, objective);
                
                Debug.Log($"Objective Completed: {objective.objectiveName}");
            }
        }
        
        public void CompleteQuest()
        {
            if (currentState != QuestState.Active)
                return;
            
            currentState = QuestState.Completed;
            
            GrantRewards();
            
            OnQuestCompleted?.Invoke(this);
            
            Debug.Log($"Quest Completed: {questTitle}");
        }
        
        private void GrantRewards()
        {
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddExperience(experienceReward);
                playerStats.AddMetal(metalReward);
                playerStats.AddSkillPoints(skillPointReward);
            }
            
            InventorySystem inventory = FindObjectOfType<InventorySystem>();
            if (inventory != null && itemRewards != null)
            {
                foreach (var itemId in itemRewards)
                {
                    inventory.AddItem(itemId, 1);
                }
            }
        }
        
        public void FailQuest()
        {
            if (currentState != QuestState.Active)
                return;
            
            currentState = QuestState.Failed;
            
            Debug.Log($"Quest Failed: {questTitle}");
        }
        
        public void AbandonQuest()
        {
            currentState = QuestState.Abandoned;
            
            Debug.Log($"Quest Abandoned: {questTitle}");
        }
        
        public void StartTracking()
        {
            isTracking = true;
        }
        
        public void StopTracking()
        {
            isTracking = false;
        }
        
        public bool IsTracking()
        {
            return isTracking;
        }
        
        public float GetTimeRemaining()
        {
            if (!hasTimeLimit)
                return float.MaxValue;
            
            return Mathf.Max(0, timeLimit - elapsedTime);
        }
        
        public float GetTimeProgress()
        {
            if (!hasTimeLimit)
                return 1f;
            
            return 1f - (elapsedTime / timeLimit);
        }
        
        public QuestData GetQuestData()
        {
            return new QuestData
            {
                questId = questId,
                state = currentState,
                objectiveProgress = new Dictionary<string, int>(objectiveProgress),
                elapsedTime = elapsedTime,
                isTracking = isTracking
            };
        }
        
        public void LoadQuestData(QuestData data)
        {
            currentState = data.state;
            objectiveProgress = new Dictionary<string, int>(data.objectiveProgress);
            elapsedTime = data.elapsedTime;
            isTracking = data.isTracking;
            
            if (currentState == QuestState.Active)
            {
                startTime = Time.time - elapsedTime;
            }
        }
    }
    
    public enum QuestType
    {
        Main,
        Side,
        Bounty,
        Collection,
        Escort,
        Crafting
    }
    
    public enum QuestDifficulty
    {
        Trivial,
        Standard,
        Hard,
        Epic,
        Legendary
    }
    
    public enum QuestState
    {
        Inactive,
        Available,
        Active,
        Completed,
        Failed,
        Abandoned
    }
    
    [System.Serializable]
    public class QuestObjective
    {
        public string objectiveId;
        public string objectiveName;
        public ObjectiveType type;
        public int targetCount;
        public string targetId;
        public string targetName;
    }
    
    [System.Serializable]
    public class QuestRequirement
    {
        public RequirementType type;
        public string requirementId;
        public int requiredAmount;
        
        public bool IsMet()
        {
            switch (type)
            {
                case RequirementType.Level:
                    PlayerStats stats = Object.FindObjectOfType<PlayerStats>();
                    return stats != null && stats.Level >= requiredAmount;
                    
                case RequirementType.Quest:
                    QuestManager qm = Object.FindObjectOfType<QuestManager>();
                    return qm != null && qm.IsQuestCompleted(requirementId);
                    
                case RequirementType.Item:
                    InventorySystem inv = Object.FindObjectOfType<InventorySystem>();
                    return inv != null && inv.HasItem(requirementId, requiredAmount);
                    
                default:
                    return true;
            }
        }
    }
    
    public enum ObjectiveType
    {
        Kill,
        Collect,
        Talk,
        Explore,
        Escort,
        Defend,
        Craft,
        Deliver
    }
    
    public enum RequirementType
    {
        None,
        Level,
        Quest,
        Item,
        Faction
    }
    
    [System.Serializable]
    public class QuestData
    {
        public string questId;
        public QuestState state;
        public Dictionary<string, int> objectiveProgress;
        public float elapsedTime;
        public bool isTracking;
    }
}
