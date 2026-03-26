using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu controller — New Game, Load Game, Settings, Quit.
/// Shows Mistborn-themed background with mist particles.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject loadGamePanel;
    public GameObject creditsPanel;

    [Header("Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Scene Names")]
    public string gameplayScene = "Luthadel";

    [Header("Background")]
    public ParticleSystem mistBackground;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        ShowMainPanel();

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
        ShowMainPanel();
        if (loadGamePanel != null) loadGamePanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false);
    }

    public void OnSettings()
    {
        ShowMainPanel();
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false);
    }

    public void OnCredits()
    {
        ShowMainPanel();
        if (creditsPanel != null) creditsPanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false);
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
}
