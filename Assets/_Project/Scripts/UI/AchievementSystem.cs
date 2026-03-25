using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AchievementSystem : MonoBehaviour
{
    public static AchievementSystem Instance { get; private set; }

    [Header("Achievements")]
    public List<Achievement> allAchievements = new List<Achievement>();
    public List<string> unlockedAchievements = new List<string>();

    [Header("Stats")]
    public Dictionary<string, int> stats = new Dictionary<string, int>();

    [Header("Settings")]
    public float notificationDuration = 3f;

    public event System.Action<Achievement> OnAchievementUnlocked;
    public event System.Action<string, int> OnStatUpdated;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;

        DontDestroyOnLoad(gameObject);
        InitializeAchievements();
    }

    void InitializeAchievements()
    {
        // Metal Mastery
        allAchievements.Add(new Achievement
        {
            achievementId = "FIRST_BURN",
            name = "First Flame",
            description = "Burn metal for the first time",
            icon = null,
            isHidden = false,
            triggerStat = "metals_burned",
            targetValue = 1
        });

        allAchievements.Add(new Achievement
        {
            achievementId = "STEEL_MASTER",
            name = "Steel Master",
            description = "Push 100 metal objects",
            icon = null,
            isHidden = false,
            triggerStat = "steel_pushes",
            targetValue = 100
        });

        allAchievements.Add(new Achievement
        {
            achievementId = "IRON_MASTER",
            name = "Iron Master",
            description = "Pull 100 metal objects",
            icon = null,
            isHidden = false,
            triggerStat = "iron_pulls",
            targetValue = 100
        });

        allAchievements.Add(new Achievement
        {
            achievementId = "COIN_MASTER",
            name = "Coin Shot",
            description = "Launch 500 coins",
            icon = null,
            isHidden = false,
            triggerStat = "coins_launched",
            targetValue = 500
        });

        // Combat
        allAchievements.Add(new Achievement
        {
            achievementId = "FIRST_KILL",
            name = "First Blood",
            description = "Defeat your first enemy",
            icon = null,
            isHidden = false,
            triggerStat = "enemies_killed",
            targetValue = 1
        });

        allAchievements.Add(new Achievement
        {
            achievementId = "MASS_KILL",
            name = "Mass Destruction",
            description = "Defeat 50 enemies",
            icon = null,
            isHidden = false,
            triggerStat = "enemies_killed",
            targetValue = 50
        });

        allAchievements.Add(new Achievement
        {
            achievementId = "COMBO_MASTER",
            name = "Combo Master",
            description = "Achieve a 10-hit combo",
            icon = null,
            isHidden = false,
            triggerStat = "max_combo",
            targetValue = 10
        });

        // Survival
        allAchievements.Add(new Achievement
        {
            achievementId = "SURVIVOR",
            name = "Survivor",
            description = "Die 10 times",
            icon = null,
            isHidden = false,
            triggerStat = "deaths",
            targetValue = 10
        });

        allAchievements.Add(new Achievement
        {
            achievementId = "NO_DEATH",
            name = "Mistborn",
            description = "Complete the game without dying",
            icon = null,
            isHidden = true,
            triggerStat = "chapter_completions",
            targetValue = 8
        });

        // Exploration
        allAchievements.Add(new Achievement
        {
            achievementId = "EXPLORED_ALL",
            name = "Explorer",
            description = "Visit all areas",
            icon = null,
            isHidden = false,
            triggerStat = "areas_visited",
            targetValue = 15
        });

        // Time
        allAchievements.Add(new Achievement
        {
            achievementId = "TIME_BUBBLE",
            name = "Time Bender",
            description = "Use time bubbles 50 times",
            icon = null,
            isHidden = false,
            triggerStat = "time_bubbles_used",
            targetValue = 50
        });

        // Training
        allAchievements.Add(new Achievement
        {
            achievementId = "ALL_METALS",
            name = "Full Mistborn",
            description = "Unlock all 16 metals",
            icon = null,
            isHidden = false,
            triggerStat = "metals_unlocked",
            targetValue = 16
        });
    }

    public void UpdateStat(string statName, int value)
    {
        if (!stats.ContainsKey(statName))
            stats[statName] = 0;

        stats[statName] += value;
        OnStatUpdated?.Invoke(statName, stats[statName]);

        CheckAchievementsForStat(statName);
    }

    void CheckAchievementsForStat(string statName)
    {
        foreach (var achievement in allAchievements)
        {
            if (unlockedAchievements.Contains(achievement.achievementId)) continue;
            if (achievement.triggerStat != statName) continue;

            if (stats[statName] >= achievement.targetValue)
            {
                UnlockAchievement(achievement);
            }
        }
    }

    public void UnlockAchievement(Achievement achievement)
    {
        if (unlockedAchievements.Contains(achievement.achievementId)) return;

        unlockedAchievements.Add(achievement.achievementId);
        achievement.isUnlocked = true;

        OnAchievementUnlocked?.Invoke(achievement);

        Debug.Log($"[ACHIEVEMENT] Unlocked: {achievement.name} - {achievement.description}");
    }

    public void CheckAchievement(string achievementId)
    {
        Achievement achievement = allAchievements.Find(a => a.achievementId == achievementId);
        if (achievement != null && !achievement.isUnlocked)
        {
            UnlockAchievement(achievement);
        }
    }

    public int GetStat(string statName)
    {
        return stats.ContainsKey(statName) ? stats[statName] : 0;
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        return unlockedAchievements.Contains(achievementId);
    }

    public List<Achievement> GetUnlockedAchievements()
    {
        return allAchievements.Where(a => a.isUnlocked).ToList();
    }

    public float GetCompletionPercentage()
    {
        return (float)unlockedAchievements.Count / allAchievements.Count * 100f;
    }
}

[System.Serializable]
public class Achievement
{
    public string achievementId;
    public string name;
    [TextArea] public string description;
    public Sprite icon;
    public bool isHidden = false;
    public bool isUnlocked = false;
    public string triggerStat = "";
    public int targetValue = 1;
}

public class AchievementNotification : MonoBehaviour
{
    [Header("UI")]
    public TMPro.TextMeshProUGUI achievementName;
    public TMPro.TextMeshProUGUI achievementDescription;
    public UnityEngine.UI.Image achievementIcon;
    public Animator animator;

    [Header("Queue")]
    private Queue<Achievement> notificationQueue = new Queue<Achievement>();
    private bool isShowingNotification = false;

    void Start()
    {
        if (AchievementSystem.Instance != null)
        {
            AchievementSystem.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
        }
    }

    void OnAchievementUnlocked(Achievement achievement)
    {
        notificationQueue.Enqueue(achievement);
        
        if (!isShowingNotification)
        {
            ShowNextNotification();
        }
    }

    void ShowNextNotification()
    {
        if (notificationQueue.Count == 0)
        {
            isShowingNotification = false;
            return;
        }

        isShowingNotification = true;
        Achievement achievement = notificationQueue.Dequeue();

        if (achievementName != null) achievementName.text = achievement.name;
        if (achievementDescription != null) achievementDescription.text = achievement.description;
        if (achievementIcon != null && achievement.icon != null) achievementIcon.sprite = achievement.icon;

        if (animator != null)
        {
            animator.SetTrigger("Show");
        }

        StartCoroutine(HideAfterDelay(3f));
    }

    System.Collections.IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (animator != null)
        {
            animator.SetTrigger("Hide");
        }

        yield return new WaitForSeconds(0.5f);

        ShowNextNotification();
    }
}

public class StatsTracker : MonoBehaviour
{
    [Header("Tracked Stats")]
    private int metalsBurned = 0;
    private int steelPushes = 0;
    private int ironPulls = 0;
    private int coinsLaunched = 0;
    private int enemiesKilled = 0;
    private int deaths = 0;
    private int areasVisited = 0;
    private int timeBubblesUsed = 0;
    private int maxCombo = 0;
    private int currentCombo = 0;
    private int metalsUnlocked = 0;

    private AchievementSystem achievementSystem;

    void Start()
    {
        achievementSystem = AchievementSystem.Instance;
        RegisterEventListeners();
    }

    void RegisterEventListeners()
    {
        EventManager.RegisterEvent("MetalBurned", OnMetalBurned);
        EventManager.RegisterEvent("SteelPush", OnSteelPush);
        EventManager.RegisterEvent("IronPull", OnIronPull);
        EventManager.RegisterEvent("CoinLaunched", OnCoinLaunched);
        EventManager.RegisterEvent("EnemyKilled", OnEnemyKilled);
        EventManager.RegisterEvent("PlayerDied", OnPlayerDied);
        EventManager.RegisterEvent("AreaVisited", OnAreaVisited);
        EventManager.RegisterEvent("TimeBubbleUsed", OnTimeBubbleUsed);
        EventManager.RegisterEvent("ComboHit", OnComboHit);
        EventManager.RegisterEvent("MetalUnlocked", OnMetalUnlocked);
    }

    void OnMetalBurned()
    {
        metalsBurned++;
        achievementSystem?.UpdateStat("metals_burned", 1);
    }

    void OnSteelPush()
    {
        steelPushes++;
        achievementSystem?.UpdateStat("steel_pushes", 1);
    }

    void OnIronPull()
    {
        ironPulls++;
        achievementSystem?.UpdateStat("iron_pulls", 1);
    }

    void OnCoinLaunched()
    {
        coinsLaunched++;
        achievementSystem?.UpdateStat("coins_launched", 1);
    }

    void OnEnemyKilled()
    {
        enemiesKilled++;
        achievementSystem?.UpdateStat("enemies_killed", 1);
    }

    void OnPlayerDied()
    {
        deaths++;
        achievementSystem?.UpdateStat("deaths", 1);
    }

    void OnAreaVisited()
    {
        areasVisited++;
        achievementSystem?.UpdateStat("areas_visited", 1);
    }

    void OnTimeBubbleUsed()
    {
        timeBubblesUsed++;
        achievementSystem?.UpdateStat("time_bubbles_used", 1);
    }

    void OnComboHit()
    {
        currentCombo++;
        if (currentCombo > maxCombo)
        {
            maxCombo = currentCombo;
            achievementSystem?.UpdateStat("max_combo", maxCombo);
        }
    }

    void OnMetalUnlocked()
    {
        metalsUnlocked++;
        achievementSystem?.UpdateStat("metals_unlocked", 1);
    }
}