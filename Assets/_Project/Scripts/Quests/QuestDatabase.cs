using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Populates the QuestManager with lore-accurate Mistborn quest content at runtime.
/// Quests follow the Final Empire story arc: skaa rebellion against the Lord Ruler.
/// </summary>
public class QuestDatabase : MonoBehaviour
{
    void Start()
    {
        if (QuestManager.Instance == null) return;
        PopulateQuests();
    }

    void PopulateQuests()
    {
        var quests = new List<Quest>();

        // ── Chapter 1: Awakening ─────────────────────────────────────────

        quests.Add(CreateQuest("main_01", "Snapping", Quest.QuestType.Main,
            "You have Snapped — your Allomantic abilities have awakened. " +
            "Learn to burn your first metals under Kelsier's guidance.",
            new List<QuestObjective>
            {
                Obj("main_01_a", "Burn Steel for the first time", QuestObjective.QuestObjectiveType.UseAbility, 1),
                Obj("main_01_b", "Burn Iron for the first time", QuestObjective.QuestObjectiveType.UseAbility, 1),
                Obj("main_01_c", "Push a coin at a target", QuestObjective.QuestObjectiveType.UseAbility, 1),
            },
            Reward(100, 20, 20)));

        quests.Add(CreateQuest("main_02", "The Crew Assembles", Quest.QuestType.Main,
            "Kelsier's crew is gathering at Clubs' shop. Meet the team that will " +
            "overthrow the Final Empire.",
            new List<QuestObjective>
            {
                Obj("main_02_a", "Travel to Clubs' shop in Luthadel", QuestObjective.QuestObjectiveType.ReachLocation, 1),
                Obj("main_02_b", "Speak with Kelsier", QuestObjective.QuestObjectiveType.TalkTo, 1),
                Obj("main_02_c", "Speak with Breeze", QuestObjective.QuestObjectiveType.TalkTo, 1),
                Obj("main_02_d", "Speak with Ham", QuestObjective.QuestObjectiveType.TalkTo, 1),
            },
            Reward(150, 0, 0, new List<string> { "Pewter" })));

        quests.Add(CreateQuest("main_03", "Mist Training", Quest.QuestType.Main,
            "Practice your Allomancy in the mists. A Mistborn must be comfortable " +
            "moving through Luthadel's rooftops at night.",
            new List<QuestObjective>
            {
                Obj("main_03_a", "Steel Push to reach a rooftop", QuestObjective.QuestObjectiveType.UseAbility, 1),
                Obj("main_03_b", "Iron Pull across 3 gaps", QuestObjective.QuestObjectiveType.UseAbility, 3),
                Obj("main_03_c", "Use Tin to see through the mists", QuestObjective.QuestObjectiveType.UseAbility, 1),
                Obj("main_03_d", "Burn Pewter and survive a high fall", QuestObjective.QuestObjectiveType.UseAbility, 1),
            },
            Reward(200, 30, 30, new List<string> { "Tin" })));

        quests.Add(CreateQuest("main_04", "The Noblemen's Ball", Quest.QuestType.Main,
            "Infiltrate a noble ball in Keep Venture. Use Zinc and Brass to manipulate " +
            "the nobles and gather intelligence on the Lord Ruler's defenses.",
            new List<QuestObjective>
            {
                Obj("main_04_a", "Enter Keep Venture undetected", QuestObjective.QuestObjectiveType.ReachLocation, 1),
                Obj("main_04_b", "Use Brass to Soothe a suspicious guard", QuestObjective.QuestObjectiveType.UseAbility, 1),
                Obj("main_04_c", "Use Zinc to Riot a noble into revealing secrets", QuestObjective.QuestObjectiveType.UseAbility, 1),
                Obj("main_04_d", "Escape the ball without raising alarm", QuestObjective.QuestObjectiveType.ReachLocation, 1),
            },
            Reward(300, 20, 20, new List<string> { "Zinc", "Brass" })));

        quests.Add(CreateQuest("main_05", "The Atium Cache", Quest.QuestType.Main,
            "Kelsier believes the Lord Ruler's power comes from a hidden Atium cache. " +
            "Investigate the Pits of Hathsin for clues.",
            new List<QuestObjective>
            {
                Obj("main_05_a", "Travel to the Pits of Hathsin", QuestObjective.QuestObjectiveType.ReachLocation, 1),
                Obj("main_05_b", "Defeat the Koloss guards", QuestObjective.QuestObjectiveType.Kill, 5),
                Obj("main_05_c", "Find the Atium geode chamber", QuestObjective.QuestObjectiveType.ReachLocation, 1),
                Obj("main_05_d", "Collect Atium samples", QuestObjective.QuestObjectiveType.Collect, 3),
            },
            Reward(500, 50, 50, new List<string> { "Atium" })));

        quests.Add(CreateQuest("main_06", "The Steel Inquisitor", Quest.QuestType.Main,
            "A Steel Inquisitor has been dispatched to hunt you. Survive the encounter " +
            "and learn the truth about their weakness.",
            new List<QuestObjective>
            {
                Obj("main_06_a", "Survive the Inquisitor ambush", QuestObjective.QuestObjectiveType.Kill, 1),
                Obj("main_06_b", "Remove the Inquisitor's linchpin spike", QuestObjective.QuestObjectiveType.Interact, 1),
            },
            Reward(600, 40, 40)));

        quests.Add(CreateQuest("main_07", "The Final Empire Falls", Quest.QuestType.Main,
            "The rebellion is ready. Lead the assault on Kredik Shaw, the Hill of a Thousand Spires. " +
            "Confront the Lord Ruler and end his thousand-year reign.",
            new List<QuestObjective>
            {
                Obj("main_07_a", "Rally the skaa rebels", QuestObjective.QuestObjectiveType.TalkTo, 3),
                Obj("main_07_b", "Breach Kredik Shaw", QuestObjective.QuestObjectiveType.ReachLocation, 1),
                Obj("main_07_c", "Defeat the Lord Ruler's Inquisitor guard", QuestObjective.QuestObjectiveType.Kill, 3),
                Obj("main_07_d", "Confront the Lord Ruler", QuestObjective.QuestObjectiveType.Kill, 1),
            },
            Reward(1000, 100, 100, new List<string> { "Duralumin", "Aluminum" })));

        // ── Side Quests ──────────────────────────────────────────────────

        quests.Add(CreateQuest("side_01", "Skaa Underground", Quest.QuestType.Side,
            "Help the skaa resistance establish a safe house in the slums of Luthadel.",
            new List<QuestObjective>
            {
                Obj("side_01_a", "Clear out the abandoned warehouse", QuestObjective.QuestObjectiveType.Kill, 3),
                Obj("side_01_b", "Find supplies for the rebels", QuestObjective.QuestObjectiveType.Collect, 5),
                Obj("side_01_c", "Report back to the rebellion leader", QuestObjective.QuestObjectiveType.TalkTo, 1),
            },
            Reward(150, 15, 15)));

        quests.Add(CreateQuest("side_02", "The Obligator's Ledger", Quest.QuestType.Side,
            "An Obligator has records that could expose the rebellion. " +
            "Steal the ledger from the Canton of Finance.",
            new List<QuestObjective>
            {
                Obj("side_02_a", "Infiltrate the Canton of Finance", QuestObjective.QuestObjectiveType.ReachLocation, 1),
                Obj("side_02_b", "Avoid or neutralize the Obligator guards", QuestObjective.QuestObjectiveType.Kill, 2),
                Obj("side_02_c", "Steal the ledger", QuestObjective.QuestObjectiveType.Collect, 1),
                Obj("side_02_d", "Escape without being detected", QuestObjective.QuestObjectiveType.ReachLocation, 1),
            },
            Reward(200, 25, 25)));

        quests.Add(CreateQuest("side_03", "Metal Vials", Quest.QuestType.Side,
            "Your metal reserves are running low. Find a skaa metalsmith who can " +
            "prepare proper Allomantic vials.",
            new List<QuestObjective>
            {
                Obj("side_03_a", "Locate the hidden metalsmith", QuestObjective.QuestObjectiveType.TalkTo, 1),
                Obj("side_03_b", "Collect raw metals from the market", QuestObjective.QuestObjectiveType.Collect, 4),
                Obj("side_03_c", "Return metals to the smith", QuestObjective.QuestObjectiveType.TalkTo, 1),
            },
            Reward(100, 50, 50)));

        quests.Add(CreateQuest("side_04", "The Mistwraith Hunt", Quest.QuestType.Side,
            "Mistwraiths have been spotted in the outskirts. Clear them before they " +
            "threaten the skaa settlements.",
            new List<QuestObjective>
            {
                Obj("side_04_a", "Hunt Mistwraiths in the mists", QuestObjective.QuestObjectiveType.Kill, 5),
                Obj("side_04_b", "Collect bone samples", QuestObjective.QuestObjectiveType.Collect, 3),
            },
            Reward(250, 20, 20)));

        quests.Add(CreateQuest("side_05", "Copper Cloud Training", Quest.QuestType.Side,
            "Clubs wants you to practice hiding your Allomantic pulses. " +
            "Sneak past Bronze Seekers without being detected.",
            new List<QuestObjective>
            {
                Obj("side_05_a", "Burn Copper to hide your pulses", QuestObjective.QuestObjectiveType.UseAbility, 1),
                Obj("side_05_b", "Sneak past 3 Seekers undetected", QuestObjective.QuestObjectiveType.UseAbility, 3),
                Obj("side_05_c", "Return to Clubs", QuestObjective.QuestObjectiveType.TalkTo, 1),
            },
            Reward(175, 10, 10, new List<string> { "Copper", "Bronze" })));

        // Register all quests
        QuestManager.Instance.allQuests = quests;

        // Re-index the database
        foreach (Quest q in quests)
        {
            if (q != null) QuestManager.Instance.AddQuestToDatabase(q);
        }

        // Auto-start the first quest
        QuestManager.Instance.AddQuest(quests[0]);
    }

    // ── Helper Methods ───────────────────────────────────────────────────

    Quest CreateQuest(string id, string title, Quest.QuestType type, string desc,
        List<QuestObjective> objectives, QuestReward reward)
    {
        Quest q = ScriptableObject.CreateInstance<Quest>();
        q.questId = id;
        q.title = title;
        q.questType = type;
        q.description = desc;
        q.objectives = objectives;
        q.reward = reward;
        q.prerequisiteQuestIds = new List<string>();
        return q;
    }

    QuestObjective Obj(string id, string desc, QuestObjective.QuestObjectiveType type, int target)
    {
        return new QuestObjective
        {
            objectiveId = id,
            description = desc,
            type = type,
            targetCount = target,
            currentCount = 0,
            isCompleted = false
        };
    }

    QuestReward Reward(int xp, int steel, int iron, List<string> abilities = null, List<string> items = null)
    {
        return new QuestReward
        {
            experiencePoints = xp,
            metalRewardSteel = steel,
            metalRewardIron = iron,
            unlockedAbilities = abilities ?? new List<string>(),
            itemRewards = items ?? new List<string>()
        };
    }
}
