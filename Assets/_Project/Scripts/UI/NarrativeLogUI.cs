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
        if (NarrativeTimeline.Instance == null) return;

        string fullLog = "--- JOURNALS OF THE SURVIVOR ---\n\n";
        int current = NarrativeTimeline.Instance.currentChapter;

        for (int i = 0; i < current; i++)
        {
            var data = NarrativeTimeline.Instance.chapterSummaries[i];
            if (data != null)
            {
                fullLog += $"Chapter {i + 1}: {data.speakerName}\n";
                foreach (string line in data.lines)
                {
                    fullLog += $"{line}\n";
                }
                fullLog += "\n--------------------------\n\n";
            }
        }

        logContentText.text = fullLog;
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0; // Scroll to bottom
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) ToggleLog();
    }
}
