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
        if (gameOverScreen == null)
            gameOverScreen = BuildFallbackGameOverScreen();
        SetState(GameState.GameOver);
    }

    // Builds a minimal game-over overlay at runtime when no prefab is assigned.
    GameObject BuildFallbackGameOverScreen()
    {
        var canvasObj = new GameObject("GameOverScreen");
        DontDestroyOnLoad(canvasObj);

        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Dark overlay
        var bg = new GameObject("BG");
        bg.transform.SetParent(canvasObj.transform, false);
        var bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.85f);
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;

        // "YOU DIED" text
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(canvasObj.transform, false);
        var title = titleObj.AddComponent<UnityEngine.UI.Text>();
        title.text = "YOU DIED";
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 72;
        title.alignment = TextAnchor.MiddleCenter;
        title.color = new Color(0.85f, 0.1f, 0.1f, 1f);
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.2f, 0.55f);
        titleRect.anchorMax = new Vector2(0.8f, 0.75f);
        titleRect.offsetMin = titleRect.offsetMax = Vector2.zero;

        // Restart button
        AddButton(canvasObj.transform, "Restart", new Vector2(0.35f, 0.38f), new Vector2(0.65f, 0.5f),
            new Color(0.6f, 0.15f, 0.15f), () =>
            {
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            });

        // Quit button
        AddButton(canvasObj.transform, "Quit to Menu", new Vector2(0.35f, 0.22f), new Vector2(0.65f, 0.34f),
            new Color(0.25f, 0.25f, 0.25f), () =>
            {
                Time.timeScale = 1f;
                ReturnToMainMenu();
            });

        canvasObj.SetActive(false);
        return canvasObj;
    }

    static void AddButton(Transform parent, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color color,
        UnityEngine.Events.UnityAction onClick)
    {
        var btnObj = new GameObject(label);
        btnObj.transform.SetParent(parent, false);

        var img = btnObj.AddComponent<UnityEngine.UI.Image>();
        img.color = color;

        var btn = btnObj.AddComponent<UnityEngine.UI.Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        var txtObj = new GameObject("Label");
        txtObj.transform.SetParent(btnObj.transform, false);
        var txt = txtObj.AddComponent<UnityEngine.UI.Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 28;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        var txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = txtRect.offsetMax = Vector2.zero;
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
