using UnityEngine;
using System;

/// <summary>
/// Tracks and manages the player's progress through the Book 1 narrative.
/// </summary>
public class NarrativeTimeline : MonoBehaviour
{
    public static NarrativeTimeline Instance { get; private set; }

    [Header("Progression State")]
    public int currentChapter = 1;
    public int totalChapters = 38;

    [Header("Data")]
    public DialogueData[] chapterSummaries = new DialogueData[38];

    public event Action<int> OnChapterChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void AdvanceChapter()
    {
        if (currentChapter < totalChapters)
        {
            currentChapter++;
            OnChapterChanged?.Invoke(currentChapter);
            var activeData = chapterSummaries[currentChapter - 1];
            string speaker = activeData != null ? activeData.speakerName : "Unknown";
            Debug.Log($"[NARRATIVE] Story Advanced to Chapter {currentChapter}: {speaker}");
        }
    }

    public DialogueData GetCurrentChapterData()
    {
        if (currentChapter > 0 && currentChapter <= chapterSummaries.Length)
            return chapterSummaries[currentChapter - 1];
        return null;
    }
}
