using UnityEngine;
using System.Collections.Generic;
using System;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Game State")]
    public GameState currentState = GameState.Menu;
    public enum GameState { Menu, Playing, Paused, Dialogue, Cutscene, Death, Transition }

    [Header("Current Chapter")]
    public int currentChapter = 0;
    public int currentSection = 0;
    public string currentSceneName = "";

    [Header("Progress")]
    public Dictionary<string, bool> storyFlags = new Dictionary<string, bool>();
    public Dictionary<string, int> stats = new Dictionary<string, int>();
    public List<string> completedObjectives = new List<string>();

    [Header("Settings")]
    public bool pauseOnCutscene = true;
    public bool pauseOnDialogue = true;

    private GameState previousState;
    private float gameTime = 0f;
    private int deathCount = 0;

    public event Action<GameState> OnStateChanged;
    public event Action<int, int> OnChapterChanged;
    public event Action<string> OnObjectiveCompleted;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;

        DontDestroyOnLoad(gameObject);
        InitializeStoryFlags();
    }

    void Update()
    {
        if (currentState == GameState.Playing)
        {
            gameTime += Time.deltaTime;
        }

        HandlePauseInput();
    }

    void InitializeStoryFlags()
    {
        // Story progression flags
        storyFlags["MET_KELSIER"] = false;
        storyFlags["LEARNED_STEEL"] = false;
        storyFlags["LEARNED_IRON"] = false;
        storyFlags["FIRST_HEIST"] = false;
        storyFlags["TRAINED_AT_KELSIERS_BASE"] = false;
        storyFlags["RESCUED_MOASH"] = false;
        storyFlags["DISCOVERED_ATIUM"] = false;
        storyFlags["LORD_RULER_DEFEATED"] = false;
    }

    void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }

    public void StartNewGame()
    {
        currentChapter = 0;
        currentSection = 0;
        gameTime = 0f;
        deathCount = 0;

        foreach (var flag in storyFlags.Keys)
        {
            storyFlags[flag] = false;
        }

        completedObjectives.Clear();
        stats.Clear();

        SetState(GameState.Playing);
        
        Debug.Log("[GAME] New game started");
    }

    public void LoadGame(int slot)
    {
        SaveLoadManager.Instance?.LoadGame(slot);
        SetState(GameState.Playing);
        
        Debug.Log($"[GAME] Game loaded from slot {slot}");
    }

    public void SaveGame(int slot)
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame(slot, $"Chapter {currentChapter}-{currentSection}");
        }
    }

    public void SetState(GameState newState)
    {
        previousState = currentState;
        currentState = newState;

        switch (newState)
        {
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Dialogue:
                if (pauseOnDialogue) Time.timeScale = 0f;
                break;
            case GameState.Cutscene:
                if (pauseOnCutscene) Time.timeScale = 0f;
                break;
        }

        OnStateChanged?.Invoke(newState);
        
        Debug.Log($"[GAME] State: {previousState} -> {newState}");
    }

    public void PauseGame()
    {
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        SetState(GameState.Playing);
    }

    public void SetStoryFlag(string flag, bool value)
    {
        storyFlags[flag] = value;
        
        Debug.Log($"[STORY] Flag '{flag}' set to {value}");
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

    public void CompleteObjective(string objectiveId)
    {
        if (!completedObjectives.Contains(objectiveId))
        {
            completedObjectives.Add(objectiveId);
            OnObjectiveCompleted?.Invoke(objectiveId);
            
            Debug.Log($"[GAME] Objective completed: {objectiveId}");
        }
    }

    public bool IsObjectiveCompleted(string objectiveId)
    {
        return completedObjectives.Contains(objectiveId);
    }

    public void AdvanceChapter()
    {
        currentChapter++;
        currentSection = 0;
        
        OnChapterChanged?.Invoke(currentChapter, currentSection);
        
        Debug.Log($"[GAME] Advanced to Chapter {currentChapter}");
    }

    public void AdvanceSection()
    {
        currentSection++;
        
        OnChapterChanged?.Invoke(currentChapter, currentSection);
        
        Debug.Log($"[GAME] Chapter {currentChapter}, Section {currentSection}");
    }

    public void OnPlayerDeath()
    {
        deathCount++;
        SetState(GameState.Death);
    }

    public float GetGameTime() => gameTime;
    public int GetDeathCount() => deathCount;
    public int GetCurrentChapter() => currentChapter;
    public int GetCurrentSection() => currentSection;
    public GameState GetState() => currentState;
}

public class ChapterManager : MonoBehaviour
{
    [Header("Chapter Settings")]
    public string[] chapterNames;
    public GameObject[] chapterPrefabs;
    public string[] chapterDescriptions;

    [Header("Current")]
    public int currentChapterIndex = 0;

    private GameFlowManager gameFlow;

    void Start()
    {
        gameFlow = GameFlowManager.Instance;
        
        if (chapterNames.Length > 0)
        {
            LoadChapter(0);
        }
    }

    public void LoadChapter(int index)
    {
        if (index < 0 || index >= chapterNames.Length) return;

        currentChapterIndex = index;
        
        if (chapterPrefabs.Length > index && chapterPrefabs[index] != null)
        {
            Instantiate(chapterPrefabs[index]);
        }

        Debug.Log($"[CHAPTER] Loaded: {chapterNames[index]}");
        
        EventManager.TriggerEvent("ChapterLoaded", new Dictionary<string, object> {
            { "chapterIndex", index },
            { "chapterName", chapterNames[index] }
        });
    }

    public void CompleteChapter()
    {
        gameFlow?.AdvanceChapter();
        LoadChapter(currentChapterIndex + 1);
    }

    public string GetCurrentChapterName()
    {
        return currentChapterIndex < chapterNames.Length ? chapterNames[currentChapterIndex] : "";
    }
}

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("Current Objectives")]
    public List<Objective> activeObjectives = new List<Objective>();

    [Header("Completed")]
    public List<Objective> completedObjectives = new List<Objective>();

    public event Action<Objective> OnObjectiveStarted;
    public event Action<Objective> OnObjectiveCompleted;
    public event Action<Objective> OnObjectiveFailed;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    public void AddObjective(Objective objective)
    {
        if (!activeObjectives.Contains(objective))
        {
            activeObjectives.Add(objective);
            OnObjectiveStarted?.Invoke(objective);
            
            Debug.Log($"[OBJECTIVE] Started: {objective.title}");
        }
    }

    public void UpdateObjectiveProgress(string objectiveId, int progress)
    {
        Objective obj = activeObjectives.Find(o => o.objectiveId == objectiveId);
        if (obj == null) return;

        obj.currentProgress = Mathf.Min(obj.currentProgress + progress, obj.targetProgress);

        if (obj.currentProgress >= obj.targetProgress)
        {
            CompleteObjective(obj);
        }
    }

    void CompleteObjective(Objective objective)
    {
        activeObjectives.Remove(objective);
        completedObjectives.Add(objective);
        OnObjectiveCompleted?.Invoke(objective);

        GameFlowManager.Instance?.CompleteObjective(objective.objectiveId);

        Debug.Log($"[OBJECTIVE] Completed: {objective.title}");
    }

    public void FailObjective(Objective objective)
    {
        activeObjectives.Remove(objective);
        OnObjectiveFailed?.Invoke(objective);
        
        Debug.Log($"[OBJECTIVE] Failed: {objective.title}");
    }

    public Objective GetObjective(string objectiveId)
    {
        return activeObjectives.Find(o => o.objectiveId == objectiveId);
    }

    public List<Objective> GetActiveObjectives() => activeObjectives;
    public bool HasActiveObjective(string objectiveId) => activeObjectives.Exists(o => o.objectiveId == objectiveId);
}

[System.Serializable]
public class Objective
{
    public string objectiveId;
    public string title;
    [TextArea] public string description;
    public ObjectiveType type;
    public int targetProgress;
    public int currentProgress;
    public bool isOptional = false;
    public bool autoCompleteOnReachTarget = true;

    public enum ObjectiveType
    {
        Kill,
        Collect,
        ReachLocation,
        TalkToNPC,
        CompleteMission,
        UseAbility,
        DefendArea,
        Escort,
        Timer
    }

    public bool IsComplete => currentProgress >= targetProgress;
    public float ProgressPercent => targetProgress > 0 ? (float)currentProgress / targetProgress : 0f;
}