using UnityEngine;
using UnityEngine.UIElements;

public class AllomancySkillUI : MonoBehaviour
{
    public VisualTreeAsset nodeTemplate; // Drag your SkillNode UXML here
    public AllomancySkill[] allAllomancySkills;   // Drag your Skill assets here

    void OnEnable()
    {
        // Get the root of your UI Document
        var root = GetComponent<UIDocument>().rootVisualElement;
        var container = root.Q<VisualElement>("AllomancyTabContent"); // Name must match UI Builder

        foreach (var skill in allAllomancySkills)
        {
            // Instantiate (Clone) the template
            VisualElement node = nodeTemplate.Instantiate();
            
            // Fill the labels and icons
            node.Q<Label>("SkillName").text = skill.skillName;
            node.Q<VisualElement>("SkillIcon").style.backgroundImage = new StyleBackground(skill.icon);
            
            // Add click logic
            node.Q<Button>().clicked += () => Debug.Log("Clicked " + skill.skillName);

            container.Add(node);
        }
    }
}
