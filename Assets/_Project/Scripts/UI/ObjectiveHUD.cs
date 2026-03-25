using UnityEngine;
using TMPro;

/// <summary>
/// Displays the current active objectives on the HUD.
/// </summary>
public class ObjectiveHUD : MonoBehaviour
{
    public TextMeshProUGUI objectiveText;
    public string header = "Current Tasks:\n";

    void Start()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveStarted += OnObjectiveChanged;
            ObjectiveManager.Instance.OnObjectiveCompleted += OnObjectiveChanged;
        }
        RefreshUI();
    }

    void OnObjectiveChanged(Objective obj)
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (objectiveText == null) return;

        if (ObjectiveManager.Instance == null || ObjectiveManager.Instance.GetActiveObjectives().Count == 0)
        {
            objectiveText.text = "";
            return;
        }

        string fullText = header;
        foreach (var obj in ObjectiveManager.Instance.GetActiveObjectives())
        {
            fullText += $"- {obj.title}\n";
        }
        objectiveText.text = fullText;
    }

    void OnDestroy()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveStarted -= OnObjectiveChanged;
            ObjectiveManager.Instance.OnObjectiveCompleted -= OnObjectiveChanged;
        }
    }
}
