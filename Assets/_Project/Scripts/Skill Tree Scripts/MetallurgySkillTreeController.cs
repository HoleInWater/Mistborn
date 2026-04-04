// NOTE: Line 89 contains Debug.Log which should be removed for production
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic; // Required for Lists

public class MetallurgySkillTreeController : MonoBehaviour
{
    [Header("Assets")]
    public VisualTreeAsset nodeTemplate; // Drag SkillNode.uxml here
    public MetallurgySkill[] MetallurgySkills;      // Drag your ScriptableObjects here

    private VisualElement _MetallurgyTabContent;

    bool CheckIfPrerequisitesMet(MetallurgySkill skill)
    {
        // If there are no prerequisites, it's a starting skill!
        if (skill.prerequisites == null || skill.prerequisites.Count == 0)
        {
            return true;
        }
    
        // Check if EVERY prerequisite skill is unlocked
        foreach (var pre in skill.prerequisites)
        {
            if (!pre.isUnlocked)
            {
                return false; // Found one that isn't unlocked yet
            }
        }
    
        return true; // All requirements are met!
    }

    void OnEnable()
    {
        // 1. Get the root of your UI Document
        var root = GetComponent<UIDocument>().rootVisualElement;

        // 2. Find the container you named in UI Builder
        _MetallurgyTabContent = root.Q<VisualElement>("MetallurgyTabContent");

        // 3. Clear existing items (prevents duplicates)
        _MetallurgyTabContent.Clear();

        // 4. Generate the nodes
        GenerateSkillNodes();
    }

    void GenerateSkillNodes()
    {
        foreach (var skill in MetallurgySkills)
        {
            // Create a clone of the SkillNode blueprint
            VisualElement node = nodeTemplate.Instantiate();

            // Setup labels and icons inside the node
            node.Q<Label>("SkillName").text = skill.skillName;
            node.Q<VisualElement>("SkillIcon").style.backgroundImage = new StyleBackground(skill.icon);

            // Add the click logic
            Button btn = node.Q<Button>();
            btn.clicked += () => OnSkillClicked(skill);

            // Finally, add it to the Metallurgy Tab
            _MetallurgyTabContent.Add(node);
        }
    }

    void UpdateNodeVisuals(VisualElement node, MetallurgySkill skill)
    {
        // Find the button inside the template
        var button = node.Q<Button>();
    
        // Remove all state classes first to reset
        button.RemoveFromClassList("skill-node--unlocked");
        button.RemoveFromClassList("skill-node--available");
    
        if (skill.isUnlocked)
        {
            button.AddToClassList("skill-node--unlocked");
        }
        else if (CheckIfPrerequisitesMet(skill))
        {
            button.AddToClassList("skill-node--available");
        }
    }

    void OnSkillClicked(MetallurgySkill skill)
    {
        if (skill.isUnlocked)
        {
            Debug.Log($"[SKILL] {skill.skillName} is already unlocked.");
            return;
        }

        if (!CheckIfPrerequisitesMet(skill))
        {
            Debug.Log($"[SKILL] Prerequisites not met for {skill.skillName}.");
            return;
        }

        if (PlayerExperience.Instance != null && PlayerExperience.Instance.SpendSkillPoints(skill.skillPointCost))
        {
            UnlockSkill(skill);
        }
        else
        {
            Debug.Log("[SKILL] Not enough skill points!");
        }
    }

    private void UnlockSkill(MetallurgySkill skill)
    {
        skill.isUnlocked = true;
        
        // Notify Metallurgist via Registry (faster than FindObjectOfType)
        if (AshwalkerRegistry.ActiveMetallurgists.Count > 0)
            AshwalkerRegistry.ActiveMetallurgists[0].UnlockMetal(skill.metalType);

        Debug.Log($"[SKILL] UNLOCKED: {skill.skillName}!");
        
        // Refresh UI
        GenerateSkillNodes();
    }

    
    void Update()
    {
        // Check if the Tab key was pressed this frame
        if (Input.GetKeyDown(Keybinds.MetalWheel))
        {
            ToggleSkillTree();
        }
    }

    void ToggleSkillTree()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        if (root.style.display == DisplayStyle.Flex)
        {
            root.style.display = DisplayStyle.None;
            // Explicitly use UnityEngine.Cursor to avoid the ambiguous error
            UnityEngine.Cursor.lockState = CursorLockMode.Locked; 
            UnityEngine.Cursor.visible = false;
        }
        else
        {
            root.style.display = DisplayStyle.Flex;
            UnityEngine.Cursor.lockState = CursorLockMode.None; 
            UnityEngine.Cursor.visible = true;
        }
    }
}
