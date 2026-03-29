using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Allomantic skill tree with 3 tiers per metal: Efficiency, Power, Mastery.
/// Plus passive branches for Combat, Movement, and Survival.
/// Integrates with PlayerExperience for skill point spending.
/// </summary>
public class AllomanticSkillTree : MonoBehaviour
{
    public static AllomanticSkillTree Instance { get; private set; }

    [System.Serializable]
    public class SkillNode
    {
        public string skillId;
        public string displayName;
        public string description;
        public SkillCategory category;
        public SkillTier tier;
        public AllomancySkill.MetalType metal;
        public int cost = 1;
        public bool unlocked = false;
        public string[] prerequisites;

        // Effect values
        public float effectValue;
    }

    public enum SkillCategory { MetalEfficiency, MetalPower, MetalMastery, CombatPassive, MovementPassive, SurvivalPassive }
    public enum SkillTier { Tier1, Tier2, Tier3 }

    [Header("Skill Database")]
    public List<SkillNode> allSkills = new List<SkillNode>();

    private Dictionary<string, SkillNode> skillLookup = new Dictionary<string, SkillNode>();
    private HashSet<string> unlockedSkillIds = new HashSet<string>();

    public System.Action<SkillNode> OnSkillUnlocked;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        PopulateSkillTree();
    }

    void PopulateSkillTree()
    {
        allSkills.Clear();

        // Generate 3-tier skills for each of the 16 metals
        string[] metalNames = { "Steel", "Iron", "Pewter", "Tin", "Zinc", "Brass", "Copper", "Bronze",
                                "Gold", "Electrum", "Atium", "Malatium", "Aluminum", "Duralumin",
                                "Bendalloy", "Cadmium", "Chromium", "Nicrosil" };

        for (int i = 0; i < 16; i++)
        {
            AllomancySkill.MetalType metal = (AllomancySkill.MetalType)i;
            string name = metalNames[i];

            // Tier 1: Efficiency — reduce metal drain
            AddSkill($"{name}_Eff1", $"{name} Efficiency I", $"Reduce {name} burn rate by 15%",
                SkillCategory.MetalEfficiency, SkillTier.Tier1, metal, 1, 0.15f);

            // Tier 2: Power — increase effect strength
            AddSkill($"{name}_Pow1", $"{name} Power I", $"Increase {name} effect strength by 20%",
                SkillCategory.MetalPower, SkillTier.Tier2, metal, 2, 0.20f,
                new[] { $"{name}_Eff1" });

            // Tier 3: Mastery — unlock special ability
            AddSkill($"{name}_Mas1", $"{name} Mastery", $"Unlock {name} special ability",
                SkillCategory.MetalMastery, SkillTier.Tier3, metal, 3, 1f,
                new[] { $"{name}_Pow1" });
        }

        // Passive branches
        // Combat
        AddSkill("Combat_Damage1", "Striking Force I", "Increase melee damage by 10%",
            SkillCategory.CombatPassive, SkillTier.Tier1, AllomancySkill.MetalType.Pewter, 1, 0.10f);
        AddSkill("Combat_Damage2", "Striking Force II", "Increase melee damage by 20%",
            SkillCategory.CombatPassive, SkillTier.Tier2, AllomancySkill.MetalType.Pewter, 2, 0.20f,
            new[] { "Combat_Damage1" });
        AddSkill("Combat_CritChance", "Critical Edge", "10% critical hit chance",
            SkillCategory.CombatPassive, SkillTier.Tier2, AllomancySkill.MetalType.Pewter, 2, 0.10f,
            new[] { "Combat_Damage1" });
        AddSkill("Combat_ParryWindow", "Perfect Parry", "Increase parry window by 50%",
            SkillCategory.CombatPassive, SkillTier.Tier3, AllomancySkill.MetalType.Atium, 3, 0.50f,
            new[] { "Combat_CritChance" });
        AddSkill("Combat_ComboExtend", "Relentless Combos", "Extend combo timeout by 1 second",
            SkillCategory.CombatPassive, SkillTier.Tier3, AllomancySkill.MetalType.Steel, 3, 1f,
            new[] { "Combat_Damage2" });

        // Movement
        AddSkill("Move_Speed1", "Fleet Feet I", "Increase movement speed by 10%",
            SkillCategory.MovementPassive, SkillTier.Tier1, AllomancySkill.MetalType.Steel, 1, 0.10f);
        AddSkill("Move_Speed2", "Fleet Feet II", "Increase movement speed by 20%",
            SkillCategory.MovementPassive, SkillTier.Tier2, AllomancySkill.MetalType.Steel, 2, 0.20f,
            new[] { "Move_Speed1" });
        AddSkill("Move_FallDamage", "Pewter Landing", "Reduce fall damage by 50%",
            SkillCategory.MovementPassive, SkillTier.Tier1, AllomancySkill.MetalType.Pewter, 1, 0.50f);
        AddSkill("Move_WallRunDuration", "Wall Runner", "Increase wall run duration by 50%",
            SkillCategory.MovementPassive, SkillTier.Tier2, AllomancySkill.MetalType.Steel, 2, 0.50f,
            new[] { "Move_Speed1" });
        AddSkill("Move_DoubleJump", "Allomantic Leap", "Steel Push off ground for double jump",
            SkillCategory.MovementPassive, SkillTier.Tier3, AllomancySkill.MetalType.Steel, 3, 1f,
            new[] { "Move_WallRunDuration" });

        // Survival
        AddSkill("Surv_HealthRegen", "Vital Reserves I", "Increase health regen by 25%",
            SkillCategory.SurvivalPassive, SkillTier.Tier1, AllomancySkill.MetalType.Gold, 1, 0.25f);
        AddSkill("Surv_HealthRegen2", "Vital Reserves II", "Increase health regen by 50%",
            SkillCategory.SurvivalPassive, SkillTier.Tier2, AllomancySkill.MetalType.Gold, 2, 0.50f,
            new[] { "Surv_HealthRegen" });
        AddSkill("Surv_MetalRegen", "Metal Conservation", "Passive metal reserve recovery +25%",
            SkillCategory.SurvivalPassive, SkillTier.Tier1, AllomancySkill.MetalType.Copper, 1, 0.25f);
        AddSkill("Surv_DamageReduction", "Iron Skin", "Reduce all damage taken by 10%",
            SkillCategory.SurvivalPassive, SkillTier.Tier2, AllomancySkill.MetalType.Pewter, 2, 0.10f,
            new[] { "Surv_HealthRegen" });
        AddSkill("Surv_Immunity", "Allomantic Resilience", "Immune to poison and sensory overload",
            SkillCategory.SurvivalPassive, SkillTier.Tier3, AllomancySkill.MetalType.Tin, 3, 1f,
            new[] { "Surv_DamageReduction" });

        // Build lookup
        foreach (var skill in allSkills)
            skillLookup[skill.skillId] = skill;
    }

    void AddSkill(string id, string name, string desc, SkillCategory cat, SkillTier tier,
        AllomancySkill.MetalType metal, int cost, float value, string[] prereqs = null)
    {
        allSkills.Add(new SkillNode
        {
            skillId = id,
            displayName = name,
            description = desc,
            category = cat,
            tier = tier,
            metal = metal,
            cost = cost,
            effectValue = value,
            prerequisites = prereqs ?? new string[0]
        });
    }

    // ── Public API ───────────────────────────────────────────────────────

    public bool TryUnlockSkill(string skillId)
    {
        if (!skillLookup.ContainsKey(skillId)) return false;
        SkillNode skill = skillLookup[skillId];

        if (skill.unlocked) return false;
        if (!ArePrerequisitesMet(skill)) return false;

        PlayerExperience xp = PlayerExperience.Instance;
        if (xp == null || !xp.SpendSkillPoints(skill.cost)) return false;

        skill.unlocked = true;
        unlockedSkillIds.Add(skillId);
        ApplySkillEffect(skill);
        OnSkillUnlocked?.Invoke(skill);

        SoundManager.Instance?.PlaySkillUnlock();
        return true;
    }

    bool ArePrerequisitesMet(SkillNode skill)
    {
        foreach (string prereq in skill.prerequisites)
        {
            if (!unlockedSkillIds.Contains(prereq)) return false;
        }
        return true;
    }

    void ApplySkillEffect(SkillNode skill)
    {
        // Effects are applied by querying IsSkillUnlocked / GetSkillValue
        // from the relevant gameplay systems (Allomancer, PlayerHealth, etc.)
    }

    public bool IsSkillUnlocked(string skillId) => unlockedSkillIds.Contains(skillId);

    public float GetSkillValue(string skillId)
    {
        if (skillLookup.ContainsKey(skillId) && skillLookup[skillId].unlocked)
            return skillLookup[skillId].effectValue;
        return 0f;
    }

    /// <summary>
    /// Get total efficiency bonus for a metal (sum of all unlocked efficiency skills).
    /// </summary>
    public float GetMetalEfficiencyBonus(AllomancySkill.MetalType metal)
    {
        float bonus = 0f;
        foreach (var skill in allSkills)
        {
            if (skill.unlocked && skill.metal == metal && skill.category == SkillCategory.MetalEfficiency)
                bonus += skill.effectValue;
        }
        return bonus;
    }

    /// <summary>
    /// Get total power bonus for a metal.
    /// </summary>
    public float GetMetalPowerBonus(AllomancySkill.MetalType metal)
    {
        float bonus = 0f;
        foreach (var skill in allSkills)
        {
            if (skill.unlocked && skill.metal == metal && skill.category == SkillCategory.MetalPower)
                bonus += skill.effectValue;
        }
        return bonus;
    }

    public bool HasMastery(AllomancySkill.MetalType metal)
    {
        foreach (var skill in allSkills)
        {
            if (skill.unlocked && skill.metal == metal && skill.category == SkillCategory.MetalMastery)
                return true;
        }
        return false;
    }

    public List<SkillNode> GetSkillsByCategory(SkillCategory category)
    {
        return allSkills.FindAll(s => s.category == category);
    }

    public List<string> GetUnlockedSkillIds() => new List<string>(unlockedSkillIds);

    public void LoadUnlockedSkills(List<string> ids)
    {
        foreach (string id in ids)
        {
            if (skillLookup.ContainsKey(id))
            {
                skillLookup[id].unlocked = true;
                unlockedSkillIds.Add(id);
            }
        }
    }
}
