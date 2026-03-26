using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Achievement tracking and notification system.
/// </summary>
public class AchievementSystem : MonoBehaviour
{
    public static AchievementSystem Instance { get; private set; }

    [System.Serializable]
    public class Achievement
    {
        public string id;
        public string title;
        public string description;
        public bool unlocked = false;
        public AchievementCategory category;
    }

    public enum AchievementCategory { Allomancy, Combat, Exploration, Survival, Story, Feruchemy }

    public List<Achievement> achievements = new List<Achievement>();
    private Dictionary<string, Achievement> lookup = new Dictionary<string, Achievement>();

    public System.Action<Achievement> OnAchievementUnlocked;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        PopulateAchievements();
    }

    void PopulateAchievements()
    {
        Add("first_push", "Coinshot", "Push a coin for the first time", AchievementCategory.Allomancy);
        Add("first_pull", "Lurcher", "Pull a metal object for the first time", AchievementCategory.Allomancy);
        Add("burn_all_16", "Full Mistborn", "Burn all 16 metals at least once", AchievementCategory.Allomancy);
        Add("flare_master", "Flare Master", "Flare a metal to maximum intensity", AchievementCategory.Allomancy);
        Add("compound_gold", "Immortal", "Compound gold for the first time", AchievementCategory.Feruchemy);
        Add("kill_inquisitor", "Spike Puller", "Defeat a Steel Inquisitor by removing the linchpin", AchievementCategory.Combat);
        Add("kill_koloss_10", "Koloss Slayer", "Defeat 10 Koloss", AchievementCategory.Combat);
        Add("combo_10", "Relentless", "Achieve a 10-hit combo", AchievementCategory.Combat);
        Add("parry_perfect", "Perfect Timing", "Land a perfect parry", AchievementCategory.Combat);
        Add("survive_ashstorm", "Ash Walker", "Survive an ash storm without taking cover", AchievementCategory.Survival);
        Add("all_lore", "Scholar", "Collect all lore entries", AchievementCategory.Exploration);
        Add("lord_ruler_defeated", "The Ash Stops", "Defeat the Lord Ruler", AchievementCategory.Story);
        Add("mist_spirit", "Mist Touched", "Encounter the mist spirit", AchievementCategory.Exploration);

        foreach (var a in achievements) lookup[a.id] = a;
    }

    void Add(string id, string title, string desc, AchievementCategory cat)
    {
        achievements.Add(new Achievement { id = id, title = title, description = desc, category = cat });
    }

    public void TryUnlock(string id)
    {
        if (!lookup.ContainsKey(id)) return;
        Achievement a = lookup[id];
        if (a.unlocked) return;

        a.unlocked = true;
        OnAchievementUnlocked?.Invoke(a);
        SoundManager.Instance?.PlaySkillUnlock();
    }

    public bool IsUnlocked(string id) => lookup.ContainsKey(id) && lookup[id].unlocked;
    public int GetUnlockedCount() => achievements.FindAll(a => a.unlocked).Count;
    public int GetTotalCount() => achievements.Count;
}
