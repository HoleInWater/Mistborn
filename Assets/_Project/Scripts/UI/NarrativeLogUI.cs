using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// A journal UI that displays summaries of the story chapters reached.
/// </summary>
public class NarrativeLogUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject logPanel;
    public TextMeshProUGUI logContentText;
    public ScrollRect scrollRect;

    void Start()
    {
        if (logPanel != null) logPanel.SetActive(false);
    }

    public void ToggleLog()
    {
        if (logPanel == null) return;
        bool active = !logPanel.activeSelf;
        logPanel.SetActive(active);
        if (active) RefreshLog();
    }

    public void RefreshLog()
    {
        if (NarrativeTimeline.Instance == null || logContentText == null) return;

        string fullLog = "--- JOURNALS OF THE SURVIVOR ---\n\n";
        int current = NarrativeTimeline.Instance.GetCurrentChapter();

        for (int i = 0; i < current; i++)
        {
            var beat = NarrativeTimeline.Instance.GetBeat(i);
            if (beat != null)
            {
                fullLog += $"Chapter {beat.chapter}: {beat.title}\n";
                fullLog += $"{beat.description}\n";
                fullLog += "\n--------------------------\n\n";
            }
        }

        logContentText.text = fullLog;
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0;
    }

    void Update()
    {
        // J opens quest menu instead of journal
        if (Input.GetKeyDown(KeyCode.J)) ToggleQuestMenu();
    }

    public void ToggleQuestMenu()
    {
        if (logPanel == null) return;
        bool active = !logPanel.activeSelf;
        logPanel.SetActive(active);
        if (active) RefreshQuestLog();
    }

    public void RefreshQuestLog()
    {
        if (QuestManager.Instance == null || logContentText == null) return;

        string text = "--- ACTIVE QUESTS ---\n\n";

        foreach (var quest in QuestManager.Instance.GetActiveQuests())
        {
            text += $"<b>{quest.title}</b>\n";
            text += $"{quest.description}\n";
            foreach (var obj in quest.objectives)
            {
                string check = obj.isCompleted ? "[x]" : "[ ]";
                text += $"  {check} {obj.description}";
                if (obj.targetCount > 1) text += $" ({obj.currentCount}/{obj.targetCount})";
                text += "\n";
            }
            text += "\n";
        }

        var completed = QuestManager.Instance.GetCompletedQuests();
        if (completed.Count > 0)
        {
            text += "--- COMPLETED ---\n\n";
            foreach (var quest in completed)
                text += $"[x] {quest.title}\n";
        }

        logContentText.text = text;
    }
}
