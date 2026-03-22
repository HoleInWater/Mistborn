using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Experience")]
    public int currentLevel = 1;
    public int currentExperience = 0;
    public int experienceToNextLevel = 100;
    public int experiencePerLevel = 50;
    
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    
    [Header("Attributes")]
    public int strength = 10;
    public int agility = 10;
    public int intelligence = 10;
    public int vitality = 10;
    
    [Header("Derived Stats")]
    public float maxHealthBase = 100f;
    public float damageBase = 10f;
    public float armorBase = 0f;
    public float staminaRegenBase = 5f;
    
    [Header("Combat Stats")]
    public float criticalChance = 5f;
    public float criticalDamage = 150f;
    public float attackSpeed = 1f;
    public float movementSpeed = 5f;
    
    [Header("Metal Affinity")]
    public float physicalMetalBonus = 1f;
    public float mentalMetalBonus = 1f;
    public float temporalMetalBonus = 1f;
    public float enhancementMetalBonus = 1f;
    
    [Header("UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI experienceText;
    public Image experienceBar;
    
    public System.Action<int> OnLevelUp;
    public System.Action<int, int> OnExperienceGained;
    
    void Start()
    {
        CalculateStats();
        UpdateUI();
    }
    
    public void AddExperience(int amount)
    {
        currentExperience += amount;
        OnExperienceGained?.Invoke(amount, currentExperience);
        
        while (currentExperience >= experienceToNextLevel)
        {
            LevelUp();
        }
        
        UpdateUI();
    }
    
    void LevelUp()
    {
        currentExperience -= experienceToNextLevel;
        currentLevel++;
        experienceToNextLevel = CalculateExperienceToNextLevel();
        
        CalculateStats();
        Heal(maxHealth);
        RestoreStamina(maxStamina);
        
        OnLevelUp?.Invoke(currentLevel);
        Debug.Log($"Level up! Now level {currentLevel}");
    }
    
    int CalculateExperienceToNextLevel()
    {
        return experiencePerLevel * currentLevel * currentLevel;
    }
    
    void CalculateStats()
    {
        maxHealth = maxHealthBase + (vitality * 10f);
        damageBase = damageBase + (strength * 2f);
        armorBase = vitality * 0.5f;
        staminaRegenBase = 5f + (agility * 0.5f);
        movementSpeed = 5f + (agility * 0.1f);
        criticalChance = 5f + (agility * 0.5f);
    }
    
    public void TakeDamage(float damage)
    {
        float actualDamage = damage - armorBase;
        actualDamage = Mathf.Max(0, actualDamage);
        
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
    
    public void RestoreStamina(float amount)
    {
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
    }
    
    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            return true;
        }
        return false;
    }
    
    void Die()
    {
        Debug.Log("Player died!");
    }
    
    public float GetDamage()
    {
        return damageBase;
    }
    
    public float GetArmor()
    {
        return armorBase;
    }
    
    public float GetCriticalChance()
    {
        return criticalChance;
    }
    
    public float CalculateCriticalDamage(float baseDamage)
    {
        if (Random.Range(0f, 100f) <= criticalChance)
        {
            return baseDamage * (criticalDamage / 100f);
        }
        return baseDamage;
    }
    
    void UpdateUI()
    {
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevel}";
        }
        
        if (experienceText != null)
        {
            experienceText.text = $"{currentExperience}/{experienceToNextLevel}";
        }
        
        if (experienceBar != null)
        {
            experienceBar.fillAmount = (float)currentExperience / experienceToNextLevel;
        }
    }
    
    public void AddAttributePoint(string attribute, int points = 1)
    {
        switch (attribute.ToLower())
        {
            case "strength":
                strength += points;
                break;
            case "agility":
                agility += points;
                break;
            case "intelligence":
                intelligence += points;
                break;
            case "vitality":
                vitality += points;
                break;
        }
        
        CalculateStats();
    }
}

public class AllomancySkillTree : MonoBehaviour
{
    [Header("Skill Points")]
    public int totalSkillPoints = 0;
    public int spentSkillPoints = 0;
    public int availableSkillPoints = 0;
    
    [Header("Skill Categories")]
    public SkillNode[] physicalSkills;
    public SkillNode[] mentalSkills;
    public SkillNode[] temporalSkills;
    public SkillNode[] enhancementSkills;
    public SkillNode[] godMetalSkills;
    
    [Header("Unlockables")]
    public bool unlocksSteelPush = false;
    public bool unlocksIronPull = false;
    public bool unlocksPewterBurn = false;
    public bool unlocksTinEnhance = false;
    public bool unlocksAllPhysicalMetals = false;
    public bool unlocksAllMentalMetals = false;
    public bool unlocksAllomancy = false;
    public bool unlocksFeruchemy = false;
    public bool unlocksHemalurgy = false;
    
    public void UnlockSkill(SkillNode skill)
    {
        if (skill.isUnlocked) return;
        if (availableSkillPoints < skill.cost) return;
        
        foreach (SkillNode prereq in skill.prerequisites)
        {
            if (!prereq.isUnlocked) return;
        }
        
        skill.isUnlocked = true;
        spentSkillPoints += skill.cost;
        availableSkillPoints -= skill.cost;
        
        ApplySkillEffects(skill);
        
        Debug.Log($"Unlocked skill: {skill.skillName}");
    }
    
    void ApplySkillEffects(SkillNode skill)
    {
        foreach (SkillEffect effect in skill.effects)
        {
            switch (effect.type)
            {
                case SkillEffectType.UnlockMetal:
                    ApplyMetalUnlock(effect.value);
                    break;
                case SkillEffectType.StatBonus:
                    ApplyStatBonus(effect.statName, effect.value);
                    break;
                case SkillEffectType.AbilityUnlock:
                    ApplyAbilityUnlock(effect.value);
                    break;
            }
        }
    }
    
    void ApplyMetalUnlock(float metalId)
    {
        int metal = (int)metalId;
        
        if (metal == 0) unlocksSteelPush = true;
        else if (metal == 1) unlocksIronPull = true;
        else if (metal == 2) unlocksPewterBurn = true;
        else if (metal == 3) unlocksTinEnhance = true;
        
        if (metal < 4) unlocksAllPhysicalMetals = true;
    }
    
    void ApplyStatBonus(string stat, float value)
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats == null) return;
        
        switch (stat)
        {
            case "strength":
                stats.strength += (int)value;
                break;
            case "agility":
                stats.agility += (int)value;
                break;
            case "intelligence":
                stats.intelligence += (int)value;
                break;
            case "vitality":
                stats.vitality += (int)value;
                break;
        }
        
        stats.CalculateStats();
    }
    
    void ApplyAbilityUnlock(float abilityId)
    {
        int ability = (int)abilityId;
        
        if (ability == 0) unlocksAllomancy = true;
        else if (ability == 1) unlocksFeruchemy = true;
        else if (ability == 2) unlocksHemalurgy = true;
    }
}

[System.Serializable]
public class SkillNode
{
    public string skillId;
    public string skillName;
    [TextArea(2, 4)]
    public string description;
    public int cost = 1;
    public bool isUnlocked = false;
    public SkillNode[] prerequisites;
    public SkillEffect[] effects;
    public Sprite icon;
}

[System.Serializable]
public class SkillEffect
{
    public SkillEffectType type;
    public string statName;
    public float value;
}

public enum SkillEffectType
{
    UnlockMetal,
    StatBonus,
    AbilityUnlock,
    PassiveBonus,
    SkillUnlock
}
