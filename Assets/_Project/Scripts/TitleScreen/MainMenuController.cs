/* MainMenuController.cs
 *
 * PURPOSE:
 * Main menu after the title sequence. Handles Start/Load/Settings/Quit buttons.
 * Continues playing the main theme from where the title sequence left off
 * (or loops it if the player skipped).
 */

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Scene Names")]
    public string gameSceneName = "GameWorld";
    public string settingsSceneName = "";

    [Header("Audio")]
    [Tooltip("If assigned, continues/loops the main theme. If null, finds existing AudioSource.")]
    public AudioSource musicSource;
    public AudioClip mainThemeClip;
    public bool loopMusic = true;

    [Header("Transition")]
    public CanvasGroup fadeOverlay;
    public float fadeOutDuration = 1.5f;

    [Header("Settings Panel (inline)")]
    [Tooltip("Optional: settings panel that overlays the menu instead of loading a scene")]
    public GameObject settingsPanel;

    [Header("Mist Particle Effect")]
    [Tooltip("Ambient mist particles behind the menu")]
    public ParticleSystem mistParticles;

    void Start()
    {
        // Wire buttons
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGame);
        if (loadGameButton != null) loadGameButton.onClick.AddListener(OnLoadGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

        // Disable load if no save exists
        if (loadGameButton != null && !SaveExists())
            loadGameButton.interactable = false;

        // Music
        if (musicSource != null && mainThemeClip != null && !musicSource.isPlaying)
        {
            musicSource.clip = mainThemeClip;
            musicSource.loop = loopMusic;
            musicSource.Play();
        }

        // Ensure fade overlay starts transparent
        if (fadeOverlay != null) fadeOverlay.alpha = 0f;

        // Hide settings panel
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Cursor visible on menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void OnNewGame()
    {
        StartCoroutine(TransitionToScene(gameSceneName));
    }

    void OnLoadGame()
    {
        // Load the saved game state, then transition
        // SaveManager integration point — for now just loads the game scene
        StartCoroutine(TransitionToScene(gameSceneName));
    }

    void OnSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        else if (!string.IsNullOrEmpty(settingsSceneName))
        {
            SceneManager.LoadScene(settingsSceneName);
        }
    }

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator TransitionToScene(string sceneName)
    {
        // Fade music
        if (musicSource != null)
            StartCoroutine(FadeAudio(musicSource, 0f, fadeOutDuration));

        // Fade to black
        if (fadeOverlay != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeOutDuration);
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }

        SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeAudio(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }

    bool SaveExists()
    {
        // Check if a save file exists — hook into your save system
        return PlayerPrefs.HasKey("SaveSlot0") || System.IO.File.Exists(
            System.IO.Path.Combine(Application.persistentDataPath, "save.dat"));
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
}
