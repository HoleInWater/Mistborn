using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Central game state manager. Tracks chapters, story flags, objectives, game state.
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public enum GameState { Playing, Paused, Dialogue, Cutscene, Loading, GameOver }

    [Header("Game State")]
    public GameState currentState = GameState.Playing;

    [Header("Story Progress")]
    public int currentChapter = 0;
    public int currentSection = 0;
    public Dictionary<string, bool> storyFlags = new Dictionary<string, bool>();
    public List<string> completedObjectives = new List<string>();

    public event Action<int, int> OnChapterChanged;
    public event Action<string> OnObjectiveCompleted;
    public event Action<GameState> OnGameStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeStoryFlags();
    }

    void InitializeStoryFlags()
    {
        storyFlags["MET_KELSIER"] = false;
        storyFlags["LEARNED_STEEL"] = false;
        storyFlags["LEARNED_IRON"] = false;
        storyFlags["FIRST_HEIST"] = false;
        storyFlags["TRAINED_AT_KELSIERS_BASE"] = false;
        storyFlags["DISCOVERED_ATIUM"] = false;
        storyFlags["LORD_RULER_DEFEATED"] = false;
        storyFlags["JOINED_CREW"] = false;
        storyFlags["LEARNED_COMPOUNDING"] = false;
        storyFlags["LEARNED_WEAKNESS"] = false;
    }

    public void SetState(GameState state)
    {
        if (currentState == state) return;
        currentState = state;
        Time.timeScale = (state == GameState.Paused || state == GameState.Dialogue) ? 0f : 1f;
        OnGameStateChanged?.Invoke(state);
    }

    public void PauseGame() => SetState(GameState.Paused);
    public void ResumeGame() => SetState(GameState.Playing);

    public void SetStoryFlag(string flag, bool value)
    {
        storyFlags[flag] = value;
    }

    public bool GetStoryFlag(string flag)
    {
        return storyFlags.ContainsKey(flag) && storyFlags[flag];
    }

    public Dictionary<string, bool> GetStoryFlags() => storyFlags;
    public int GetChapterIndex() => currentChapter;

    public void SetChapterIndex(int chapter)
    {
        currentChapter = chapter;
        OnChapterChanged?.Invoke(currentChapter, currentSection);
    }

    public void AdvanceChapter()
    {
        currentChapter++;
        currentSection = 0;
        OnChapterChanged?.Invoke(currentChapter, currentSection);
    }

    public void CompleteObjective(string objectiveId)
    {
        if (!completedObjectives.Contains(objectiveId))
        {
            completedObjectives.Add(objectiveId);
            OnObjectiveCompleted?.Invoke(objectiveId);
        }
    }

    public bool IsObjectiveCompleted(string id) => completedObjectives.Contains(id);
}

/// <summary>
/// Tracks active objectives with progress, types, and events.
/// </summary>
public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    public List<Objective> activeObjectives = new List<Objective>();
    public List<Objective> completedObjectives = new List<Objective>();

    public event Action<Objective> OnObjectiveStarted;
    public event Action<Objective> OnObjectiveCompleted;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddObjective(Objective objective)
    {
        if (!activeObjectives.Contains(objective))
        {
            activeObjectives.Add(objective);
            OnObjectiveStarted?.Invoke(objective);
        }
    }

    public void AddObjective(ObjectiveData data)
    {
        if (data == null) return;
        AddObjective(new Objective
        {
            objectiveId = data.objectiveID,
            title = data.description,
            description = data.description,
            targetProgress = 1
        });
    }

    public void UpdateObjectiveProgress(string objectiveId, int progress)
    {
        Objective obj = activeObjectives.Find(o => o.objectiveId == objectiveId);
        if (obj == null) return;
        obj.currentProgress = Mathf.Min(obj.currentProgress + progress, obj.targetProgress);
        if (obj.currentProgress >= obj.targetProgress) CompleteObjective(obj);
    }

    void CompleteObjective(Objective objective)
    {
        activeObjectives.Remove(objective);
        completedObjectives.Add(objective);
        OnObjectiveCompleted?.Invoke(objective);
        GameFlowManager.Instance?.CompleteObjective(objective.objectiveId);
    }

    public Objective GetObjective(string id) => activeObjectives.Find(o => o.objectiveId == id);
    public List<Objective> GetActiveObjectives() => activeObjectives;
}

[System.Serializable]
public class Objective
{
    public string objectiveId;
    public string title;
    [TextArea] public string description;
    public ObjectiveType type;
    public int targetProgress = 1;
    public int currentProgress = 0;
    public float ProgressPercent => targetProgress > 0 ? (float)currentProgress / targetProgress : 0f;
}

public enum ObjectiveType { Kill, Collect, ReachLocation, TalkTo, UseAbility, Defend, Escort, Survive }
