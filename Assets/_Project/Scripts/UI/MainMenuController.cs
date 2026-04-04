using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Main menu — New Game, Load Game, Settings, Credits, Quit.
/// Arrived at from the title sequence or from "Quit to Menu" in pause.
/// Music continues from the title sequence (SoundManager persists across scenes).
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
    public Button continueButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Scene Names")]
    public string gameplayScene = "Cinderhold";

    [Header("Background")]
    public ParticleSystem mistBackground;

    [Header("Transition")]
    public CanvasGroup fadeOverlay;
    public float fadeOutDuration = 1.5f;

    void Start()
    {
        // Cursor visible on menu
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetState(CursorManager.CursorState.Menu);
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Time.timeScale = 1f;
        ShowMainPanel();

        // Wire buttons
        if (newGameButton != null)    newGameButton.onClick.AddListener(OnNewGame);
        if (continueButton != null)   continueButton.onClick.AddListener(OnContinue);
        if (loadGameButton != null)   loadGameButton.onClick.AddListener(OnLoadGame);
        if (settingsButton != null)   settingsButton.onClick.AddListener(OnSettings);
        if (creditsButton != null)    creditsButton.onClick.AddListener(OnCredits);
        if (quitButton != null)       quitButton.onClick.AddListener(OnQuit);

        // Disable continue/load if no save exists
        bool hasSave = SaveLoadManager.Instance != null && SaveLoadManager.Instance.HasSave(0);
        if (continueButton != null) continueButton.interactable = hasSave;
        if (loadGameButton != null) loadGameButton.interactable = hasSave;

        // Ensure music is playing (main theme continues from title sequence)
        if (SoundManager.Instance != null && SoundManager.Instance.musicSource != null
            && !SoundManager.Instance.musicSource.isPlaying)
        {
            SoundManager.Instance.PlayMainTheme();
        }

        // Fade overlay starts transparent (title sequence already faded to black,
        // the scene load clears it)
        if (fadeOverlay != null) fadeOverlay.alpha = 0f;
    }

    void ShowMainPanel()
    {
        if (mainPanel != null)     mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (loadGamePanel != null) loadGamePanel.SetActive(false);
        if (creditsPanel != null)  creditsPanel.SetActive(false);
    }

    public void OnNewGame()
    {
        StartCoroutine(TransitionToGame(gameplayScene));
    }

    public void OnContinue()
    {
        // Load the most recent save, then transition
        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.LoadGame(0);
        StartCoroutine(TransitionToGame(gameplayScene));
    }

    public void OnLoadGame()
    {
        ShowMainPanel();
        if (loadGamePanel != null) loadGamePanel.SetActive(true);
        if (mainPanel != null)     mainPanel.SetActive(false);
    }

    public void OnSettings()
    {
        ShowMainPanel();
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (mainPanel != null)     mainPanel.SetActive(false);
    }

    public void OnCredits()
    {
        ShowMainPanel();
        if (creditsPanel != null) creditsPanel.SetActive(true);
        if (mainPanel != null)    mainPanel.SetActive(false);
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

    IEnumerator TransitionToGame(string sceneName)
    {
        // Hide cursor for gameplay
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetState(CursorManager.CursorState.Gameplay);

        // Fade music to exploration track
        if (SoundManager.Instance != null)
            SoundManager.Instance.TransitionToExploration();

        // Fade to black
        if (fadeOverlay != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeOutDuration);
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }

        // Use loading screen if available
        LoadingScreen ls = FindObjectOfType<LoadingScreen>();
        if (ls != null)
            ls.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}
