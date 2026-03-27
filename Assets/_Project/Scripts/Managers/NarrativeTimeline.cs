using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Tracks the player's progression through the Mistborn: The Final Empire story.
/// Major story beats trigger quests, unlock metals, spawn companions, change weather.
/// </summary>
public class NarrativeTimeline : MonoBehaviour
{
    public static NarrativeTimeline Instance { get; private set; }

    [System.Serializable]
    public class StoryBeat
    {
        public int chapter;
        public string title;
        [TextArea] public string description;
        public string[] flagsToSet;
        public string questToStart;
        public string metalToUnlock;
        public string companionToAdd;
    }

    [Header("Progression")]
    public int currentChapter = 0;
    public int totalChapters;

    public event Action<int, StoryBeat> OnStoryBeatReached;

    private List<StoryBeat> storyBeats = new List<StoryBeat>();
    private HashSet<int> completedBeats = new HashSet<int>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        PopulateStoryBeats();
        totalChapters = storyBeats.Count;
    }

    void PopulateStoryBeats()
    {
        // Act 1: The Crew Assembles
        AddBeat(1, "Snapping",
            "You have Snapped. Your Allomantic abilities awaken in a moment of trauma.",
            new[] { "PLAYER_SNAPPED" }, "main_01", "Steel");

        AddBeat(2, "The Survivor",
            "Kelsier, the Survivor of Hathsin, finds you. He recognizes your potential as a Mistborn.",
            new[] { "MET_KELSIER" }, null, "Iron", "Kelsier");

        AddBeat(3, "Clubs' Shop",
            "The crew gathers at Clubs' shop — the coppercloud keeps you hidden from Seekers.",
            new[] { "JOINED_CREW" }, "main_02", "Pewter");

        AddBeat(4, "Mist Training",
            "Kelsier takes you into the mists. Tonight you learn to fly.",
            new[] { "LEARNED_STEEL", "LEARNED_IRON" }, "main_03", "Tin");

        // Act 2: The Plan
        AddBeat(5, "The Nobleman's Ball",
            "Infiltrate Keep Venture. Breeze teaches you emotional Allomancy.",
            new[] { "FIRST_HEIST" }, "main_04", "Zinc");

        AddBeat(6, "Breeze's Lesson",
            "Master the art of Soothing and Rioting. The nobles suspect nothing.",
            new[] { "HAS_BRASS", "HAS_ZINC" }, null, "Brass");

        AddBeat(7, "Smoker's Cover",
            "Clubs shows you how his coppercloud works. Bronze Seekers can't find you now.",
            new[] { "CLUBS_TRUSTS_YOU" }, "side_05", "Copper");

        AddBeat(8, "Seeker Training",
            "Learn to feel the pulses of other Allomancers burning metals.",
            null, null, "Bronze");

        // Act 3: The Rebellion
        AddBeat(9, "The Atium Cache",
            "Kelsier reveals the Lord Ruler's secret — his power comes from Atium.",
            new[] { "DISCOVERED_ATIUM", "LEARNED_ABOUT_ATIUM" }, "main_05", "Atium");

        AddBeat(10, "The Pits of Hathsin",
            "You infiltrate the Pits where Atium geodes grow. Koloss guard the entrance.",
            null, null, null);

        AddBeat(11, "Sazed's Teaching",
            "Sazed explains Feruchemy and the Lord Ruler's weakness — Compounding.",
            new[] { "LEARNED_COMPOUNDING", "LEARNED_WEAKNESS" }, null, null, "Sazed");

        AddBeat(12, "Vin Joins",
            "Vin, a street urchin Mistborn, joins your crew. She's raw but powerful.",
            null, null, null, "Vin");

        // Act 4: The Assault
        AddBeat(13, "The Steel Inquisitor",
            "A Steel Inquisitor has been sent to hunt you. The spikes in its eyes see everything.",
            null, "main_06", "Gold");

        AddBeat(14, "Electrum Discovery",
            "You discover Electrum — the metal that lets you see your own possible futures.",
            null, null, "Electrum");

        AddBeat(15, "The Rebellion Rises",
            "The skaa are rising. Kelsier's plan is working. But the Lord Ruler knows.",
            null, null, "Bendalloy");

        AddBeat(16, "Time Bubbles",
            "You learn to create time bubbles — Bendalloy speeds, Cadmium slows.",
            null, null, "Cadmium");

        AddBeat(17, "Enhancement Metals",
            "The rarest metals: Aluminum purges, Duralumin supercharges, Chromium leeches, Nicrosil amplifies.",
            null, null, "Aluminum");

        AddBeat(18, "The Final Push",
            "Kredik Shaw. The Hill of a Thousand Spires. The Lord Ruler awaits.",
            null, "main_07", "Duralumin");

        AddBeat(19, "The Lord Ruler Falls",
            "Remove his metalminds. Stop his Compounding. End his thousand-year reign.",
            new[] { "LORD_RULER_DEFEATED" }, null, null);
    }

    void AddBeat(int chapter, string title, string desc, string[] flags,
        string quest = null, string metal = null, string companion = null)
    {
        storyBeats.Add(new StoryBeat
        {
            chapter = chapter,
            title = title,
            description = desc,
            flagsToSet = flags ?? new string[0],
            questToStart = quest ?? "",
            metalToUnlock = metal ?? "",
            companionToAdd = companion ?? ""
        });
    }

    /// <summary>
    /// Advance to the next story beat. Triggers all associated events.
    /// </summary>
    public void AdvanceStory()
    {
        if (currentChapter >= storyBeats.Count) return;

        StoryBeat beat = storyBeats[currentChapter];
        currentChapter++;

        // Set story flags
        foreach (string flag in beat.flagsToSet)
            GameFlowManager.Instance?.SetStoryFlag(flag, true);

        // Start quest
        if (!string.IsNullOrEmpty(beat.questToStart))
            QuestManager.Instance?.AddQuestById(beat.questToStart);

        // Unlock metal
        if (!string.IsNullOrEmpty(beat.metalToUnlock))
        {
            AllomancySkill.MetalType metal;
            if (System.Enum.TryParse(beat.metalToUnlock, out metal))
            {
                Allomancer player = FindObjectOfType<Allomancer>();
                if (player != null) player.UnlockMetal(metal);
            }
        }

        // Notification
        NotificationSystem.Instance?.ShowNotification($"Chapter {beat.chapter}: {beat.title}");

        OnStoryBeatReached?.Invoke(currentChapter, beat);
    }

    public StoryBeat GetCurrentBeat()
    {
        if (currentChapter < storyBeats.Count)
            return storyBeats[currentChapter];
        return null;
    }

    public StoryBeat GetBeat(int chapter)
    {
        if (chapter >= 0 && chapter < storyBeats.Count)
            return storyBeats[chapter];
        return null;
    }

    public int GetCurrentChapter() => currentChapter;
    public bool IsStoryComplete() => currentChapter >= storyBeats.Count;
}
