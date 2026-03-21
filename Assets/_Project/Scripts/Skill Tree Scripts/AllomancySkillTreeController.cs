using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Cursor;

public class AllomancySkillTreeController : MonoBehaviour
{
    [Header("Assets")]
    public VisualTreeAsset nodeTemplate; // Drag SkillNode.uxml here
    public AllomancySkill[] AllomancySkills;      // Drag your ScriptableObjects here

    private VisualElement _AllomancyTabContent;

    bool CheckIfPrerequisitesMet(AllomancySkill skill)
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
        _AllomancyTabContent = root.Q<VisualElement>("AllomancyTabContent");

        // 3. Clear existing items (prevents duplicates)
        _AllomancyTabContent.Clear();

        // 4. Generate the nodes
        GenerateSkillNodes();
    }

    void GenerateSkillNodes()
    {
        foreach (var skill in AllomancySkills)
        {
            // Create a clone of the SkillNode blueprint
            VisualElement node = nodeTemplate.Instantiate();

            // Setup labels and icons inside the node
            node.Q<Label>("SkillName").text = skill.skillName;
            node.Q<VisualElement>("SkillIcon").style.backgroundImage = new StyleBackground(skill.icon);

            // Add the click logic
            Button btn = node.Q<Button>();
            btn.clicked += () => OnSkillClicked(skill);

            // Finally, add it to the Allomancy Tab
            _AllomancyTabContent.Add(node);
        }
    }

    void UpdateNodeVisuals(VisualElement node, AllomancySkill skill)
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

    void OnSkillClicked(AllomancySkill skill)
    {
        Debug.Log($"Attempting to unlock: {skill.skillName}");
        // Here you will eventually call your 'Purchase' logic
    }
    
    void Update()
    {
        // Check if the Tab key was pressed this frame
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleSkillTree();
        }
    }

    void ToggleSkillTree()
    {
        var root = GetComponent<UI.SkillTree>().rootVisualElement;
    
        // Check current state: if it's 'Flex' (visible), change to 'None' (hidden)
        if (root.style.display == DisplayStyle.Flex)
        {
            root.style.display = DisplayStyle.None;
            Cursor.lockState = CursorLockMode.Locked; // Hide mouse for gameplay
            Cursor.visible = false;
        }
        else
        {
            root.style.display = DisplayStyle.Flex;
            Cursor.lockState = CursorLockMode.None; // Show mouse to click skills
            Cursor.visible = true;
        }
    }
}
