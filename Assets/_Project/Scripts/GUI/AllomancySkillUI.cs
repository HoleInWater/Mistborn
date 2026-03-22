using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AllomancySkillUI : MonoBehaviour
{
    [Header("Skill Tree References")]
    public AllomancySkillTreeController skillTreeController;
    public GameObject skillButtonPrefab;
    public Transform skillContainer;
    
    [Header("UI Elements")]
    public TextMeshProUGUI skillPointsText;
    public TextMeshProUGUI selectedSkillName;
    public TextMeshProUGUI selectedSkillDescription;
    public Image selectedSkillIcon;
    public Button unlockButton;
    public Button closeButton;
    
    [Header("Skill Categories")]
    public GameObject physicalPanel;
    public GameObject mentalPanel;
    public GameObject enhancementPanel;
    public GameObject temporalPanel;
    public GameObject godMetalPanel;
    
    [Header("Settings")]
    public Color unlockedColor = Color.green;
    public Color lockedColor = Color.gray;
    public Color availableColor = Color.yellow;
    
    private AllomancySkill[] allSkills;
    private AllomancySkill selectedSkill;
    
    void Start()
    {
        if (skillTreeController == null)
        {
            skillTreeController = FindObjectOfType<AllomancySkillTreeController>();
        }
        
        InitializeSkillUI();
        SetupButtons();
    }
    
    void InitializeSkillUI()
    {
        if (skillTreeController == null) return;
        
        allSkills = skillTreeController.allSkills;
        
        if (skillContainer == null) return;
        
        foreach (AllomancySkill skill in allSkills)
        {
            CreateSkillButton(skill);
        }
    }
    
    void CreateSkillButton(AllomancySkill skill)
    {
        if (skillButtonPrefab == null || skillContainer == null) return;
        
        GameObject buttonObj = Instantiate(skillButtonPrefab, skillContainer);
        SkillButton button = buttonObj.GetComponent<SkillButton>();
        
        if (button != null)
        {
            button.Initialize(skill, this);
        }
        
        buttonObj.SetActive(true);
    }
    
    void SetupButtons()
    {
        if (unlockButton != null)
        {
            unlockButton.onClick.AddListener(UnlockSelectedSkill);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseUI);
        }
    }
    
    public void SelectSkill(AllomancySkill skill)
    {
        selectedSkill = skill;
        
        if (selectedSkillName != null)
        {
            selectedSkillName.text = skill.skillName;
        }
        
        if (selectedSkillDescription != null)
        {
            selectedSkillDescription.text = skill.description;
        }
        
        if (selectedSkillIcon != null && skill.icon != null)
        {
            selectedSkillIcon.sprite = skill.icon;
        }
        
        UpdateUnlockButton();
    }
    
    void UpdateUnlockButton()
    {
        if (unlockButton == null || selectedSkill == null) return;
        
        if (selectedSkill.isUnlocked)
        {
            unlockButton.interactable = false;
            unlockButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unlocked";
        }
        else
        {
            int availablePoints = skillTreeController != null ? skillTreeController.skillPoints : 0;
            float[] reserves = new float[16];
            
            MetalReserveManager metalManager = FindObjectOfType<MetalReserveManager>();
            if (metalManager != null)
            {
                for (int i = 0; i < 16; i++)
                {
                    reserves[i] = metalManager.GetReserve((MetalType)i);
                }
            }
            
            bool canUnlock = selectedSkill.CanUnlock(availablePoints, reserves);
            unlockButton.interactable = canUnlock;
            unlockButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Unlock ({selectedSkill.skillPointCost} pts)";
        }
    }
    
    void UnlockSelectedSkill()
    {
        if (selectedSkill == null || skillTreeController == null) return;
        
        int availablePoints = skillTreeController.skillPoints;
        float[] reserves = new float[16];
        
        MetalReserveManager metalManager = FindObjectOfType<MetalReserveManager>();
        if (metalManager != null)
        {
            for (int i = 0; i < 16; i++)
            {
                reserves[i] = metalManager.GetReserve((MetalType)i);
            }
        }
        
        if (selectedSkill.CanUnlock(availablePoints, reserves))
        {
            skillTreeController.skillPoints -= selectedSkill.skillPointCost;
            selectedSkill.Unlock();
            metalManager?.Drain(selectedSkill.metalType, selectedSkill.metalCost);
            
            UpdateUI();
            UpdateUnlockButton();
        }
    }
    
    public void UpdateUI()
    {
        if (skillPointsText != null && skillTreeController != null)
        {
            skillPointsText.text = $"Skill Points: {skillTreeController.skillPoints}";
        }
        
        foreach (Transform child in skillContainer)
        {
            SkillButton button = child.GetComponent<SkillButton>();
            if (button != null)
            {
                button.UpdateState();
            }
        }
    }
    
    public void ShowCategory(MetalCategory category)
    {
        HideAllCategories();
        
        switch (category)
        {
            case MetalCategory.Physical:
                if (physicalPanel != null) physicalPanel.SetActive(true);
                break;
            case MetalCategory.Mental:
                if (mentalPanel != null) mentalPanel.SetActive(true);
                break;
            case MetalCategory.Enhancement:
                if (enhancementPanel != null) enhancementPanel.SetActive(true);
                break;
            case MetalCategory.Temporal:
                if (temporalPanel != null) temporalPanel.SetActive(true);
                break;
            case MetalCategory.GodMetal:
                if (godMetalPanel != null) godMetalPanel.SetActive(true);
                break;
        }
    }
    
    void HideAllCategories()
    {
        if (physicalPanel != null) physicalPanel.SetActive(false);
        if (mentalPanel != null) mentalPanel.SetActive(false);
        if (enhancementPanel != null) enhancementPanel.SetActive(false);
        if (temporalPanel != null) temporalPanel.SetActive(false);
        if (godMetalPanel != null) godMetalPanel.SetActive(false);
    }
    
    void CloseUI()
    {
        gameObject.SetActive(false);
    }
    
    void OnEnable()
    {
        UpdateUI();
    }
}

public class SkillButton : MonoBehaviour
{
    public Image iconImage;
    public Image backgroundImage;
    public TextMeshProUGUI nameText;
    public GameObject lockedOverlay;
    public GameObject unlockedIndicator;
    
    private AllomancySkill skill;
    private AllomancySkillUI parentUI;
    
    public void Initialize(AllomancySkill skillData, AllomancySkillUI parent)
    {
        skill = skillData;
        parentUI = parent;
        
        if (nameText != null)
        {
            nameText.text = skill.skillName;
        }
        
        if (iconImage != null && skill.icon != null)
        {
            iconImage.sprite = skill.icon;
        }
        
        UpdateState();
        
        GetComponent<Button>()?.onClick.AddListener(OnClick);
    }
    
    public void UpdateState()
    {
        if (skill == null) return;
        
        if (backgroundImage != null)
        {
            if (skill.isUnlocked)
            {
                backgroundImage.color = new Color(0.2f, 0.8f, 0.2f);
            }
            else
            {
                backgroundImage.color = new Color(0.5f, 0.5f, 0.5f);
            }
        }
        
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!skill.isUnlocked);
        }
        
        if (unlockedIndicator != null)
        {
            unlockedIndicator.SetActive(skill.isUnlocked);
        }
    }
    
    void OnClick()
    {
        if (parentUI != null && skill != null)
        {
            parentUI.SelectSkill(skill);
        }
    }
}
