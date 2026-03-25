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
    public List<string> unlockedAbilities = new List<string>();
    public List<string> itemRewards = new List<string>();
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
    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Rewards")]
    public QuestReward reward;

    [Header("Chain")]
    public Quest nextQuest;
    public List<string> prerequisiteQuestIds = new List<string>();

    public enum QuestType { Main, Side, Secondary }

    public bool IsComplete()
    {
        foreach (var obj in objectives)
            if (!obj.isCompleted) return false;
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
                    obj.isCompleted = true;
                break;
            }
        }
    }
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Database")]
    public List<Quest> allQuests = new List<Quest>();

    public List<Quest> activeQuests = new List<Quest>();
    public List<Quest> completedQuests = new List<Quest>();

    private Dictionary<string, Quest> questDatabase = new Dictionary<string, Quest>();

    public System.Action<Quest> OnQuestStarted;
    public System.Action<Quest> OnQuestCompleted;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        foreach (Quest q in allQuests)
            if (q != null) questDatabase[q.questId] = q;
    }

    public void AddQuest(Quest quest)
    {
        if (quest == null || activeQuests.Exists(q => q.questId == quest.questId)) return;
        activeQuests.Add(quest);
        OnQuestStarted?.Invoke(quest);
    }

    public void AddQuestById(string questId)
    {
        if (questDatabase.ContainsKey(questId))
            AddQuest(questDatabase[questId]);
    }

    public void AddQuestToDatabase(Quest quest)
    {
        if (quest != null) questDatabase[quest.questId] = quest;
    }

    public void CompleteObjective(string questId, string objectiveId, int progress = 1)
    {
        Quest quest = activeQuests.Find(q => q.questId == questId);
        if (quest == null) return;
        quest.UpdateObjective(objectiveId, progress);
        if (quest.IsComplete()) CompleteQuest(quest);
    }

    public void CompleteQuest(Quest quest)
    {
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        if (quest.reward != null)
        {
            Allomancer player = FindObjectOfType<Allomancer>();
            if (player != null)
            {
                player.RefillMetal(AllomancySkill.MetalType.Steel, quest.reward.metalRewardSteel);
                player.RefillMetal(AllomancySkill.MetalType.Iron, quest.reward.metalRewardIron);
            }
        }

        if (quest.nextQuest != null)
            AddQuest(quest.nextQuest);

        OnQuestCompleted?.Invoke(quest);
    }

    public Quest GetQuest(string questId)
    {
        return questDatabase.ContainsKey(questId) ? questDatabase[questId] : null;
    }

    public List<Quest> GetActiveQuests() => activeQuests;
    public List<Quest> GetCompletedQuests() => completedQuests;
}
