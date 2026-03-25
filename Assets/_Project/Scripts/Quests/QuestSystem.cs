using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class QuestObjective
{
    public string objectiveId;
    public string description;
    public QuestObjectiveType type;
    public int targetCount;
    public int currentCount;
    public bool isCompleted;

    public enum QuestObjectiveType { Kill, Collect, TalkTo, ReachLocation, UseAbility, Interact }
}

[System.Serializable]
public class QuestReward
{
    public int experiencePoints;
    public int metalRewardSteel;
    public int metalRewardIron;
    public List<string> unlockedAbilities;
    public List<string> itemRewards;
}

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/New Quest")]
public class Quest : ScriptableObject
{
    [Header("Quest Info")]
    public string questId;
    public string title;
    [TextArea] public string description;
    public QuestType questType;

    [Header("Objectives")]
    public List<QuestObjective> objectives;

    [Header("Rewards")]
    public QuestReward reward;

    [Header("Chain")]
    public Quest nextQuest;
    public List<string> prerequisiteQuestIds;

    public enum QuestType { Main, Side, Secondary }

    public bool IsComplete()
    {
        foreach (var obj in objectives)
        {
            if (!obj.isCompleted) return false;
        }
        return true;
    }

    public void UpdateObjective(string objectiveId, int progress = 1)
    {
        foreach (var obj in objectives)
        {
            if (obj.objectiveId == objectiveId)
            {
                obj.currentCount += progress;
                if (obj.currentCount >= obj.targetCount)
                {
                    obj.isCompleted = true;
                }
                break;
            }
        }
    }
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Database")]
    public List<Quest> allQuests;

    [Header("State")]
    public List<Quest> activeQuests = new List<Quest>();
    public List<Quest> completedQuests = new List<Quest>();
    public List<string> availableQuestIds = new List<string>();

    private Dictionary<string, Quest> questDatabase = new Dictionary<string, Quest>();
    private QuestEventSystem questEvents;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;

        questEvents = GetComponent<QuestEventSystem>();
        if (questEvents == null) questEvents = gameObject.AddComponent<QuestEventSystem>();

        foreach (Quest q in allQuests)
        {
            if (q != null) questDatabase[q.questId] = q;
        }
    }

    public void AddQuest(Quest quest)
    {
        if (quest == null || activeQuests.Exists(q => q.questId == quest.questId)) return;

        activeQuests.Add(quest);
        questEvents.OnQuestStarted(quest);

        Debug.Log($"[QUEST] Started: {quest.title}");
    }

    public void AddQuestById(string questId)
    {
        if (questDatabase.ContainsKey(questId))
        {
            AddQuest(questDatabase[questId]);
        }
    }

    public void CompleteObjective(string questId, string objectiveId, int progress = 1)
    {
        Quest quest = activeQuests.Find(q => q.questId == questId);
        if (quest == null) return;

        quest.UpdateObjective(objectiveId, progress);
        
        if (quest.IsComplete())
        {
            CompleteQuest(quest);
        }
    }

    public void CompleteQuest(Quest quest)
    {
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        if (quest.reward != null)
        {
            AwardRewards(quest.reward);
        }

        if (quest.nextQuest != null)
        {
            AddQuest(quest.nextQuest);
        }

        questEvents.OnQuestCompleted(quest);
        Debug.Log($"[QUEST] Completed: {quest.title}");
    }

    void AwardRewards(QuestReward reward)
    {
        Allomancer player = FindObjectOfType<Allomancer>();
        if (player != null)
        {
            player.RefillMetal(AllomancySkill.MetalType.Steel, reward.metalRewardSteel);
            player.RefillMetal(AllomancySkill.MetalType.Iron, reward.metalRewardIron);
        }

        foreach (string ability in reward.unlockedAbilities)
        {
            Debug.Log($"[QUEST] Unlocked ability: {ability}");
        }
    }

    public Quest GetQuest(string questId)
    {
        return questDatabase.ContainsKey(questId) ? questDatabase[questId] : null;
    }

    public List<Quest> GetActiveQuests() => activeQuests;
    public List<Quest> GetCompletedQuests() => completedQuests;
}

public class QuestEventSystem : MonoBehaviour
{
    public System.Action<Quest> OnQuestStartedEvent;
    public System.Action<Quest> OnQuestCompletedEvent;
    public System.Action<string, string> OnObjectiveUpdatedEvent;

    public void OnQuestStarted(Quest quest)
    {
        OnQuestStartedEvent?.Invoke(quest);
    }

    public void OnQuestCompleted(Quest quest)
    {
        OnQuestCompletedEvent?.Invoke(quest);
    }

    public void OnObjectiveUpdated(string questId, string objectiveId)
    {
        OnObjectiveUpdatedEvent?.Invoke(questId, objectiveId);
    }
}