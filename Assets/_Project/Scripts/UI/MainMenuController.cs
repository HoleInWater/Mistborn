using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
<<<<<<< HEAD
/// Main menu controller — New Game, Load Game, Settings, Quit.
/// Shows Mistborn-themed background with mist particles.
=======
/// Main menu: New Game, Load Game, Settings, Credits, Quit.
/// Settings are handled by the attached SettingsPanelController.
/// Load Game delegates to SaveGameUI in load-only mode.
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
<<<<<<< HEAD
    public GameObject settingsPanel;
=======
    public SettingsPanelController settingsController;  // root of the settings panel
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    public GameObject loadGamePanel;
    public GameObject creditsPanel;

    [Header("Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button quitButton;

<<<<<<< HEAD
=======
    [Header("Load Game")]
    public SaveGameUI saveGameUI;   // assign if load panel uses SaveGameUI component

>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    [Header("Scene Names")]
    public string gameplayScene = "Luthadel";

    [Header("Background")]
    public ParticleSystem mistBackground;

<<<<<<< HEAD
=======
    // ─────────────────────────────────────────────────────────────────────

>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        ShowMainPanel();

<<<<<<< HEAD
        // Wire buttons
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGame);
        if (loadGameButton != null) loadGameButton.onClick.AddListener(OnLoadGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
        if (creditsButton != null) creditsButton.onClick.AddListener(OnCredits);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
    }

    void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (loadGamePanel != null) loadGamePanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }
=======
        if (newGameButton  != null) newGameButton.onClick.AddListener(OnNewGame);
        if (loadGameButton != null) loadGameButton.onClick.AddListener(OnLoadGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
        if (creditsButton  != null) creditsButton.onClick.AddListener(OnCredits);
        if (quitButton     != null) quitButton.onClick.AddListener(OnQuit);
    }

    // ── Button Handlers ───────────────────────────────────────────────────
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a

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
<<<<<<< HEAD
        ShowMainPanel();
        if (loadGamePanel != null) loadGamePanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false);
=======
        ShowPanel(loadGamePanel);
        // If a SaveGameUI is wired up, open it in load mode
        if (saveGameUI != null)
            saveGameUI.OpenLoadMenu();
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    }

    public void OnSettings()
    {
<<<<<<< HEAD
        ShowMainPanel();
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false);
=======
        ShowPanel(settingsController?.gameObject);
        settingsController?.RefreshUI();
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    }

    public void OnCredits()
    {
<<<<<<< HEAD
        ShowMainPanel();
        if (creditsPanel != null) creditsPanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false);
=======
        ShowPanel(creditsPanel);
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    }

    public void OnBack()
    {
        ShowMainPanel();
    }

    public void OnQuit()
    {
<<<<<<< HEAD
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
=======
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
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    }
}
