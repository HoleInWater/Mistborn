using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mistborn.Quests
{
    [CreateAssetMenu(fileName = "Quest", menuName = "Mistborn/Quest")]
    public class Quest : ScriptableObject
    {
        public string questId;
        public string title;
        [TextArea(2, 4)]
        public string description;
        public QuestType type;
        public QuestDifficulty difficulty;
        public int recommendedLevel = 1;

        [Header("Objectives")]
        public List<Objective> objectives = new List<Objective>();
        public List<string> prerequisiteQuestIds;

        [Header("Rewards")]
        public int experienceReward;
        public int goldReward;
        public List<string> itemRewards;
        public List<string> achievementRewards;

        [Header("Settings")]
        public bool repeatable;
        public float timeLimit;
        public bool trackAutomatically = true;

        [NonSerialized] public bool isActive;
        [NonSerialized] public bool isCompleted;
        [NonSerialized] public bool isFailed;
        [NonSerialized] public Dictionary<string, int> objectiveProgress = new Dictionary<string, int>();

        public enum QuestType
        {
            Main,
            Side,
            Contract,
            Collection,
            Escort,
            Combat
        }

        public enum QuestDifficulty
        {
            Trivial,
            Easy,
            Normal,
            Hard,
            Elite
        }

        [Serializable]
        public class Objective
        {
            public string objectiveId;
            public string description;
            public ObjectiveType type;
            public int targetAmount = 1;
            public string targetId;
            public Vector3 targetPosition;
            public float radius = 5f;

            public enum ObjectiveType
            {
                Kill,
                Collect,
                Interact,
                Talk,
                Explore,
                Escort,
                Defend,
                Craft
            }
        }

        public bool CanStart()
        {
            if (isActive || isCompleted || isFailed) return false;
            return true;
        }

        public void StartQuest()
        {
            isActive = true;
            isCompleted = false;
            isFailed = false;

            foreach (Objective obj in objectives)
            {
                objectiveProgress[obj.objectiveId] = 0;
            }
        }

        public void UpdateObjective(string objectiveId, int amount = 1)
        {
            if (!objectiveProgress.ContainsKey(objectiveId))
                objectiveProgress[objectiveId] = 0;

            objectiveProgress[objectiveId] += amount;

            Objective obj = objectives.Find(o => o.objectiveId == objectiveId);
            if (obj != null && objectiveProgress[objectiveId] >= obj.targetAmount)
            {
                CompleteObjective(objectiveId);
            }
        }

        public void CompleteObjective(string objectiveId)
        {
            Objective obj = objectives.Find(o => o.objectiveId == objectiveId);
            if (obj != null)
            {
                objectiveProgress[objectiveId] = obj.targetAmount;
            }

            CheckCompletion();
        }

        public void CheckCompletion()
        {
            bool allComplete = true;
            foreach (Objective obj in objectives)
            {
                if (!objectiveProgress.ContainsKey(obj.objectiveId) || objectiveProgress[obj.objectiveId] < obj.targetAmount)
                {
                    allComplete = false;
                    break;
                }
            }

            if (allComplete)
            {
                CompleteQuest();
            }
        }

        public void CompleteQuest()
        {
            isActive = false;
            isCompleted = true;
        }

        public void FailQuest()
        {
            isActive = false;
            isFailed = true;
        }

        public void ResetQuest()
        {
            isActive = false;
            isCompleted = false;
            isFailed = false;
            objectiveProgress.Clear();
        }

        public float GetProgress()
        {
            if (objectives.Count == 0) return 0f;

            float total = 0f;
            foreach (Objective obj in objectives)
            {
                int progress = objectiveProgress.ContainsKey(obj.objectiveId) ? objectiveProgress[obj.objectiveId] : 0;
                total += Mathf.Clamp01((float)progress / obj.targetAmount);
            }

            return total / objectives.Count;
        }
    }

    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [Header("Active Quests")]
        [SerializeField] private int m_maxActiveQuests = 10;
        [SerializeField] private List<Quest> m_availableQuests = new List<Quest>();
        [SerializeField] private List<Quest> m_completedQuests = new List<Quest>();

        private List<Quest> m_activeQuests = new List<Quest>();
        private Quest m_currentQuest;

        public event Action<Quest> OnQuestStarted;
        public event Action<Quest> OnQuestCompleted;
        public event Action<Quest> OnQuestFailed;
        public event Action<Quest> OnObjectiveCompleted;

        public List<Quest> ActiveQuests => m_activeQuests;
        public Quest CurrentQuest => m_currentQuest;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool StartQuest(Quest quest)
        {
            if (quest == null) return false;
            if (!quest.CanStart()) return false;
            if (m_activeQuests.Count >= m_maxActiveQuests) return false;

            foreach (string prereqId in quest.prerequisiteQuestIds)
            {
                if (!HasCompletedQuest(prereqId))
                    return false;
            }

            quest.StartQuest();
            m_activeQuests.Add(quest);

            OnQuestStarted?.Invoke(quest);
            return true;
        }

        public void AbandonQuest(Quest quest)
        {
            if (quest == null || !quest.isActive) return;

            quest.FailQuest();
            m_activeQuests.Remove(quest);

            if (m_currentQuest == quest)
                m_currentQuest = null;
        }

        public void CompleteQuest(Quest quest)
        {
            if (quest == null) return;

            quest.CompleteQuest();
            m_activeQuests.Remove(quest);
            m_completedQuests.Add(quest);

            GrantRewards(quest);

            if (m_currentQuest == quest)
                m_currentQuest = null;

            OnQuestCompleted?.Invoke(quest);
        }

        public void UpdateObjective(string objectiveId, int amount = 1)
        {
            foreach (Quest quest in m_activeQuests)
            {
                if (ContainsObjective(quest, objectiveId))
                {
                    int previousProgress = quest.objectiveProgress.ContainsKey(objectiveId) ? quest.objectiveProgress[objectiveId] : 0;
                    quest.UpdateObjective(objectiveId, amount);

                    if (quest.objectiveProgress[objectiveId] != previousProgress)
                    {
                        Objective obj = quest.objectives.Find(o => o.objectiveId == objectiveId);
                        if (obj != null && quest.objectiveProgress[objectiveId] >= obj.targetAmount)
                        {
                            OnObjectiveCompleted?.Invoke(quest);
                        }
                    }
                    break;
                }
            }
        }

        private bool ContainsObjective(Quest quest, string objectiveId)
        {
            foreach (Quest.Objective obj in quest.objectives)
            {
                if (obj.objectiveId == objectiveId)
                    return true;
            }
            return false;
        }

        private void GrantRewards(Quest quest)
        {
            if (quest.experienceReward > 0)
            {
            }

            if (quest.goldReward > 0)
            {
                PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
                inventory?.AddGold(quest.goldReward);
            }

            foreach (string itemId in quest.itemRewards)
            {
                PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
                inventory?.AddItem(itemId);
            }

            foreach (string achievementId in quest.achievementRewards)
            {
                AchievementManager.Instance?.Unlock(achievementId);
            }
        }

        public bool HasCompletedQuest(string questId)
        {
            return m_completedQuests.Exists(q => q.questId == questId);
        }

        public bool HasActiveQuest(string questId)
        {
            return m_activeQuests.Exists(q => q.questId == questId);
        }

        public List<Quest> GetAvailableQuests()
        {
            List<Quest> available = new List<Quest>();

            foreach (Quest quest in m_availableQuests)
            {
                if (!quest.isActive && !quest.isCompleted && !quest.isFailed)
                {
                    if (quest.prerequisiteQuestIds.Count == 0 || ArePrerequisitesMet(quest))
                    {
                        available.Add(quest);
                    }
                }
            }

            return available;
        }

        private bool ArePrerequisitesMet(Quest quest)
        {
            foreach (string prereqId in quest.prerequisiteQuestIds)
            {
                if (!HasCompletedQuest(prereqId))
                    return false;
            }
            return true;
        }

        public void SetCurrentQuest(Quest quest)
        {
            m_currentQuest = quest;
        }

        public void SaveQuestProgress()
        {
            List<string> activeIds = new List<string>();
            foreach (Quest quest in m_activeQuests)
            {
                activeIds.Add(quest.questId);
            }
            PlayerPrefs.SetString("ActiveQuests", string.Join(",", activeIds));

            List<string> completedIds = new List<string>();
            foreach (Quest quest in m_completedQuests)
            {
                completedIds.Add(quest.questId);
            }
            PlayerPrefs.SetString("CompletedQuests", string.Join(",", completedIds));

            PlayerPrefs.Save();
        }

        public void LoadQuestProgress()
        {
            string activeStr = PlayerPrefs.GetString("ActiveQuests", "");
            if (!string.IsNullOrEmpty(activeStr))
            {
                string[] ids = activeStr.Split(',');
                foreach (string id in ids)
                {
                    Quest quest = m_availableQuests.Find(q => q.questId == id);
                    if (quest != null)
                    {
                        quest.StartQuest();
                        m_activeQuests.Add(quest);
                    }
                }
            }

            string completedStr = PlayerPrefs.GetString("CompletedQuests", "");
            if (!string.IsNullOrEmpty(completedStr))
            {
                string[] ids = completedStr.Split(',');
                foreach (string id in ids)
                {
                    Quest quest = m_availableQuests.Find(q => q.questId == id);
                    if (quest != null)
                    {
                        quest.CompleteQuest();
                        m_completedQuests.Add(quest);
                    }
                }
            }
        }
    }

    public class QuestGiver : MonoBehaviour
    {
        [Header("Quests")]
        [SerializeField] private List<Quest> m_quests = new List<Quest>();
        [SerializeField] private bool m_giveAllOnFirstTalk;

        [Header("Dialogue")]
        [SerializeField] private string[] m_greetingLines;
        [SerializeField] private string[] m_questCompleteLines;
        [SerializeField] private string[] m_noQuestLines;

        private bool m_hasGivenQuest;

        public void Talk()
        {
            QuestManager questManager = QuestManager.Instance;
            if (questManager == null) return;

            foreach (Quest quest in m_quests)
            {
                if (questManager.HasCompletedQuest(quest.questId))
                    continue;

                if (questManager.HasActiveQuest(quest.questId))
                {
                    if (quest.isCompleted)
                    {
                        TurnInQuest(quest);
                        return;
                    }
                    continue;
                }

                if (quest.CanStart())
                {
                    if (m_giveAllOnFirstTalk || !m_hasGivenQuest)
                    {
                        questManager.StartQuest(quest);
                        m_hasGivenQuest = true;
                        ShowQuestAcceptedDialogue(quest);
                        return;
                    }
                }
            }

            ShowNoQuestDialogue();
        }

        private void TurnInQuest(Quest quest)
        {
            QuestManager questManager = QuestManager.Instance;
            if (questManager == null) return;

            questManager.CompleteQuest(quest);
            ShowQuestCompleteDialogue(quest);
        }

        private void ShowQuestAcceptedDialogue(Quest quest)
        {
            Debug.Log($"Quest accepted: {quest.title}");
        }

        private void ShowQuestCompleteDialogue(Quest quest)
        {
            Debug.Log($"Quest completed: {quest.title}");
        }

        private void ShowNoQuestDialogue()
        {
            if (m_noQuestLines.Length > 0)
            {
                Debug.Log(m_noQuestLines[Random.Range(0, m_noQuestLines.Length)]);
            }
        }
    }
}
