using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu: New Game, Load Game, Settings, Credits, Quit.
/// Settings are handled by the attached SettingsPanelController.
/// Load Game delegates to SaveGameUI in load-only mode.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public SettingsPanelController settingsController;  // root of the settings panel
    public GameObject loadGamePanel;
    public GameObject creditsPanel;

    [Header("Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Load Game")]
    public SaveGameUI saveGameUI;   // assign if load panel uses SaveGameUI component

    [Header("Scene Names")]
    public string gameplayScene = "Luthadel";

    [Header("Background")]
    public ParticleSystem mistBackground;

    // ─────────────────────────────────────────────────────────────────────

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        ShowMainPanel();

        if (newGameButton  != null) newGameButton.onClick.AddListener(OnNewGame);
        if (loadGameButton != null) loadGameButton.onClick.AddListener(OnLoadGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
        if (creditsButton  != null) creditsButton.onClick.AddListener(OnCredits);
        if (quitButton     != null) quitButton.onClick.AddListener(OnQuit);
    }

    // ── Button Handlers ───────────────────────────────────────────────────

    public void OnNewGame()
    {
        LoadingScreen ls = FindObjectOfType<LoadingScreen>();
        if (ls != null)
            ls.LoadScene(gameplayScene);
        else
            SceneManager.LoadScene(gameplayScene);
    }

    public void OnLoadGame()
    {
        ShowPanel(loadGamePanel);
        // If a SaveGameUI is wired up, open it in load mode
        if (saveGameUI != null)
            saveGameUI.OpenLoadMenu();
    }

    public void OnSettings()
    {
        ShowPanel(settingsController?.gameObject);
        settingsController?.RefreshUI();
    }

    public void OnCredits()
    {
        ShowPanel(creditsPanel);
    }

    public void OnBack()
    {
        ShowMainPanel();
    }

    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsController != null) settingsController.gameObject.SetActive(false);
        if (loadGamePanel != null) loadGamePanel.SetActive(false);
        if (creditsPanel  != null) creditsPanel.SetActive(false);
    }

    void ShowPanel(GameObject panel)
    {
        ShowMainPanel();
        if (mainPanel != null) mainPanel.SetActive(false);
        if (panel != null) panel.SetActive(true);
    }
}
