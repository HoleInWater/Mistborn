using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Master game manager. Handles game state transitions, death/respawn flow,
/// victory conditions, and scene management. DontDestroyOnLoad singleton.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, Dialogue, Cutscene, GameOver, Victory }

    [Header("State")]
    public GameState currentState = GameState.Playing;

    [Header("UI References")]
    public GameObject gameOverScreen;
    public GameObject victoryScreen;

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string gameplayScene = "Luthadel";

    [Header("Settings")]
    public float gameOverDelay = 2f;
    public bool autoSaveOnCheckpoint = true;

    private float playTime = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Listen for player death
        EventManager.RegisterEvent("PlayerDied", OnPlayerDied);
        EventManager.RegisterEvent("LordRuler_Defeated", OnVictory);
        EventManager.RegisterEvent("InquisitorDefeated", OnInquisitorDefeated);
    }

    void Update()
    {
        if (currentState == GameState.Playing)
            playTime += Time.deltaTime;
    }

    // ── State Transitions ────────────────────────────────────────────────

    public void SetState(GameState state)
    {
        currentState = state;

        switch (state)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case GameState.Paused:
            case GameState.Dialogue:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (gameOverScreen != null) gameOverScreen.SetActive(true);
                break;

            case GameState.Victory:
                Time.timeScale = 0.3f; // Slow-mo for dramatic effect
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (victoryScreen != null) victoryScreen.SetActive(true);
                break;
        }
    }

    // ── Events ───────────────────────────────────────────────────────────

    void OnPlayerDied()
    {
        if (currentState == GameState.GameOver) return;
        StartCoroutine(ShowGameOverDelayed());
    }

    System.Collections.IEnumerator ShowGameOverDelayed()
    {
        yield return new WaitForSecondsRealtime(gameOverDelay);
        ShowGameOver();
    }

    void ShowGameOver()
    {
        SetState(GameState.GameOver);
    }

    void OnVictory()
    {
        SetState(GameState.Victory);
        AchievementSystem.Instance?.TryUnlock("lord_ruler_defeated");
    }

    void OnInquisitorDefeated()
    {
        AchievementSystem.Instance?.TryUnlock("kill_inquisitor");
        NotificationSystem.Instance?.ShowNotification("Steel Inquisitor defeated!");
    }

    // ── Scene Management ─────────────────────────────────────────────────

    public void StartNewGame()
    {
        playTime = 0f;
        LoadingScreen.Instance?.LoadScene(gameplayScene);
        SetState(GameState.Playing);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
        SetState(GameState.MainMenu);
    }

    public void RestartFromCheckpoint()
    {
        SetState(GameState.Playing);
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        CheckpointSystem.Instance?.RespawnPlayer();
    }

    public void QuickSave()
    {
        SaveLoadManager.Instance?.SaveGame(0, "QuickSave");
        NotificationSystem.Instance?.ShowNotification("Game Saved");
    }

    public void QuickLoad()
    {
        SaveLoadManager.Instance?.LoadGame(0);
        NotificationSystem.Instance?.ShowNotification("Game Loaded");
        SetState(GameState.Playing);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // ── Public API ───────────────────────────────────────────────────────

    public float GetPlayTime() => playTime;
    public bool IsPlaying() => currentState == GameState.Playing;
    public bool IsPaused() => currentState == GameState.Paused;
}
