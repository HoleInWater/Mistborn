using UnityEngine;
using UnityEngine.UIElements;

public class AllomancySkillTreeController : MonoBehaviour
{
    [Header("Assets")]
    public VisualTreeAsset nodeTemplate; // Drag SkillNode.uxml here
    public AllomancySkill[] AllomancySkills;      // Drag your ScriptableObjects here

    private VisualElement _AllomancyTabContent;

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

    void UpdateNodeVisuals(VisualElement node, MagicSkill skill)
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
}
