using UnityEngine;
using System;

/// <summary>
/// Manages player experience, level, and skill points.
/// </summary>
public class PlayerExperience : MonoBehaviour
{
    public static PlayerExperience Instance { get; private set; }

    [Header("Leveling Settings")]
    public int currentLevel = 1;
    public float currentXP = 0;
    public float xpToNextLevel = 100;
    public float xpScaleFactor = 1.2f;

    [Header("Skill Points")]
    public int skillPoints = 0;

    public event Action OnLevelUp;
    public event Action OnXPChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddXP(float amount)
    {
        currentXP += amount;
        
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        OnXPChanged?.Invoke();
    }

    private void LevelUp()
    {
        currentXP -= xpToNextLevel;
        currentLevel++;
        skillPoints++;
        
        // Scale next level requirement
        xpToNextLevel = Mathf.Round(xpToNextLevel * xpScaleFactor);
        
        OnLevelUp?.Invoke();
    }

    public bool SpendSkillPoints(int amount)
    {
        if (skillPoints >= amount)
        {
            skillPoints -= amount;
            OnXPChanged?.Invoke();
            return true;
        }
        return false;
    }

    public int GetLevel() => currentLevel;
    public float GetExperience() => currentXP;

    public void SetLevelAndExperience(int level, float xp)
    {
        currentLevel = level;
        currentXP = xp;
        xpToNextLevel = Mathf.Round(100f * Mathf.Pow(xpScaleFactor, level - 1));
        OnXPChanged?.Invoke();
    }
}
