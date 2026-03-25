using UnityEngine;
using System.Collections.Generic;

public class SkillTreeSystem : MonoBehaviour
{
    public static SkillTreeSystem Instance { get; private set; }

    [Header("Skill Points")]
    public int availableSkillPoints = 5;
    public int totalSkillPointsEarned = 0;

    [Header("Skills")]
    public List<SkillNode> allSkills = new List<SkillNode>();
    public List<string> unlockedSkills = new List<string>();

    [Header("References")]
    public SkillTreeUI ui;

    public event System.Action<SkillNode> OnSkillUnlocked;
    public event System.Action<int> OnSkillPointsChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    public bool CanUnlockSkill(SkillNode skill)
    {
        if (unlockedSkills.Contains(skill.skillId)) return false;
        if (skill.requiredSkillPoints > availableSkillPoints) return false;

        foreach (string prereqId in skill.prerequisiteSkills)
        {
            if (!unlockedSkills.Contains(prereqId)) return false;
        }

        return true;
    }

    public bool UnlockSkill(SkillNode skill)
    {
        if (!CanUnlockSkill(skill)) return false;

        availableSkillPoints -= skill.requiredSkillPoints;
        unlockedSkills.Add(skill.skillId);

        skill.isUnlocked = true;
        ApplySkillEffects(skill);

        OnSkillUnlocked?.Invoke(skill);
        OnSkillPointsChanged?.Invoke(availableSkillPoints);

        Debug.Log($"[SKILL] Unlocked: {skill.skillName}");
        return true;
    }

    void ApplySkillEffects(SkillNode skill)
    {
        Allomancer player = FindObjectOfType<Allomancer>();
        if (player == null) return;

        switch (skill.skillType)
        {
            case SkillNode.SkillType.Metal:
                if (skill.metalType != AllomancySkill.MetalType.Steel)
                {
                    player.UnlockMetal(skill.metalType);
                }
                break;
            case SkillNode.SkillType.Upgrade:
                ApplyUpgrade(skill);
                break;
            case SkillNode.SkillType.Passive:
                ApplyPassive(skill);
                break;
        }
    }

    void ApplyUpgrade(SkillNode skill)
    {
        switch (skill.upgradeType)
        {
            case SkillNode.UpgradeType.Damage:
                PlayerCombat combat = FindObjectOfType<PlayerCombat>();
                if (combat != null) combat.damageMultiplier += skill.upgradeValue;
                break;
            case SkillNode.UpgradeType.Speed:
                BasicPlayerMove move = FindObjectOfType<BasicPlayerMove>();
                if (move != null) move.externalSpeedMultiplier += skill.upgradeValue * 0.1f;
                break;
            case SkillNode.UpgradeType.Health:
                PlayerHealth health = FindObjectOfType<PlayerHealth>();
                if (health != null) health.maxHealth += skill.upgradeValue * 10;
                break;
            case SkillNode.UpgradeType.MetalDrain:
                Allomancer allomancer = FindObjectOfType<Allomancer>();
                if (allomancer != null) allomancer.baseBurnRate -= skill.upgradeValue * 0.5f;
                break;
        }
    }

    void ApplyPassive(SkillNode skill)
    {
        switch (skill.passiveType)
        {
            case SkillNode.PassiveType.Regeneration:
                PlayerHealth health = FindObjectOfType<PlayerHealth>();
                if (health != null) health.healthRegenRate += skill.passiveValue;
                break;
            case SkillNode.PassiveType.DamageReduction:
                // Add damage reduction passive
                break;
            case SkillNode.PassiveType.MovementSpeed:
                BasicPlayerMove move = FindObjectOfType<BasicPlayerMove>();
                if (move != null) move.externalSpeedMultiplier += skill.passiveValue * 0.2f;
                break;
        }
    }

    public void AddSkillPoints(int amount)
    {
        availableSkillPoints += amount;
        totalSkillPointsEarned += amount;
        OnSkillPointsChanged?.Invoke(availableSkillPoints);
    }

    public bool IsSkillUnlocked(string skillId)
    {
        return unlockedSkills.Contains(skillId);
    }

    public int GetUnlockedSkillCount() => unlockedSkills.Count;
}

[System.Serializable]
public class SkillNode
{
    public string skillId;
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;
    public SkillType skillType;

    public enum SkillType { Metal, Upgrade, Passive }

    [Header("Requirements")]
    public int requiredSkillPoints = 1;
    public List<string> prerequisiteSkills = new List<string>();
    public int level = 1;

    [Header("Metal Unlock")]
    public AllomancySkill.MetalType metalType;

    [Header("Upgrade")]
    public UpgradeType upgradeType;
    public float upgradeValue = 1f;

    [Header("Passive")]
    public PassiveType passiveType;
    public float passiveValue = 0.1f;

    [Header("State")]
    public bool isUnlocked = false;

    public enum UpgradeType { Damage, Speed, Health, MetalDrain, Range, Flaring }
    public enum PassiveType { Regeneration, DamageReduction, MovementSpeed, MetalRegen, Immunity }
}

public class SkillTreeUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform skillTreeContainer;
    public GameObject skillNodePrefab;
    public TMPro.TextMeshProUGUI skillNameText;
    public TMPro.TextMeshProUGUI skillDescriptionText;
    public TMPro.TextMeshProUGUI skillPointsText;

    [Header("Skill Nodes")]
    public List<SkillNodeUI> nodeUIs = new List<SkillNodeUI>();

    private SkillTreeSystem skillTree;

    void Start()
    {
        skillTree = SkillTreeSystem.Instance;
        if (skillTree != null)
        {
            skillTree.OnSkillUnlocked += OnSkillUnlocked;
            skillTree.OnSkillPointsChanged += OnSkillPointsChanged;
        }

        InitializeUI();
    }

    void InitializeUI()
    {
        if (skillTree == null) return;

        foreach (var skill in skillTree.allSkills)
        {
            CreateSkillNodeUI(skill);
        }

        UpdateUI();
    }

    void CreateSkillNodeUI(SkillNode skill)
    {
        if (skillNodePrefab == null || skillTreeContainer == null) return;

        GameObject node = Instantiate(skillNodePrefab, skillTreeContainer);
        SkillNodeUI nodeUI = node.GetComponent<SkillNodeUI>();
        if (nodeUI != null)
        {
            nodeUI.Initialize(skill);
            nodeUIs.Add(nodeUI);
        }
    }

    void UpdateUI()
    {
        if (skillPointsText != null)
        {
            skillPointsText.text = $"Skill Points: {skillTree?.availableSkillPoints ?? 0}";
        }

        foreach (var nodeUI in nodeUIs)
        {
            nodeUI.UpdateState();
        }
    }

    void OnSkillUnlocked(SkillNode skill)
    {
        UpdateUI();
    }

    void OnSkillPointsChanged(int points)
    {
        UpdateUI();
    }

    public void OnSkillNodeSelected(SkillNode skill)
    {
        if (skillNameText != null) skillNameText.text = skill.skillName;
        if (skillDescriptionText != null) skillDescriptionText.text = skill.description;
    }

    public void OnUnlockButtonClicked(SkillNode skill)
    {
        if (skillTree != null && skillTree.CanUnlockSkill(skill))
        {
            skillTree.UnlockSkill(skill);
        }
    }
}

public class SkillNodeUI : MonoBehaviour
{
    [Header("UI Elements")]
    public UnityEngine.UI.Image icon;
    public TMPro.TextMeshProUGUI nameText;
    public UnityEngine.UI.Button unlockButton;
    public UnityEngine.UI.Image lockOverlay;

    private SkillNode skill;
    private SkillTreeSystem skillTree;

    public void Initialize(SkillNode node)
    {
        skill = node;
        skillTree = SkillTreeSystem.Instance;

        if (nameText != null) nameText.text = node.skillName;
        if (icon != null && node.icon != null) icon.sprite = node.icon;

        UpdateState();
    }

    public void UpdateState()
    {
        if (skill == null || skillTree == null) return;

        bool isUnlocked = skillTree.IsSkillUnlocked(skill.skillId);
        bool canUnlock = skillTree.CanUnlockSkill(skill);

        if (lockOverlay != null)
        {
            lockOverlay.enabled = !isUnlocked;
        }

        if (unlockButton != null)
        {
            unlockButton.interactable = canUnlock;
            unlockButton.onClick.AddListener(() => skillTree.UnlockSkill(skill));
        }
    }
}