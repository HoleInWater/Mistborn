using UnityEngine;

/// <summary>
<<<<<<< HEAD
/// Pause menu with resume, settings, save/load, and quit.
/// Integrates with GameFlowManager for state control.
/// </summary>
public class PauseMenuSystem : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject saveLoadPanel;
    public GameObject loreCodexPanel;

    [Header("Settings")]
    public bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
=======
/// Pause menu: Esc toggles pause.
/// Pause panel shows Resume / Settings / Save &amp; Quit.
/// Settings are handled by the attached SettingsPanelController.
/// Wire panels and the settings controller in the Inspector.
/// </summary>
public class PauseMenuSystem : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public SettingsPanelController settingsController;  // root of the settings panel

    [HideInInspector] public bool isPaused;

    // ─────────────────────────────────────────────────────────────────────

    void Start()
    {
        HideAllPanels();
    }

    void Update()
    {
        if (KeybindRebinder.IsRebinding) return;

        if (Input.GetKeyDown(Keybinds.Pause))
        {
            if (isPaused) ResumeGame();
            else          PauseGame();
        }
    }

    // ── Pause / Resume ────────────────────────────────────────────────────

    public void PauseGame()
    {
        isPaused = true;
        ShowPanel(pausePanel);
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameFlowManager.Instance?.PauseGame();
<<<<<<< HEAD
=======
        GameManager.Instance?.SetState(GameManager.GameState.Paused);
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    }

    public void ResumeGame()
    {
        isPaused = false;
<<<<<<< HEAD
        CloseAllPanels();
=======
        HideAllPanels();
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameFlowManager.Instance?.ResumeGame();
<<<<<<< HEAD
    }

    public void OpenSettings()
    {
        CloseAllPanels();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OpenSaveLoad()
    {
        CloseAllPanels();
        if (saveLoadPanel != null) saveLoadPanel.SetActive(true);
    }

    public void OpenLoreCodex()
    {
        CloseAllPanels();
        if (loreCodexPanel != null) loreCodexPanel.SetActive(true);
    }

    public void BackToPause()
    {
        CloseAllPanels();
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void CloseAllPanels()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (saveLoadPanel != null) saveLoadPanel.SetActive(false);
        if (loreCodexPanel != null) loreCodexPanel.SetActive(false);
=======
        GameManager.Instance?.SetState(GameManager.GameState.Playing);
    }

    // ── Pause Panel Buttons ───────────────────────────────────────────────

    public void OnResumeClicked() => ResumeGame();

    public void OnSettingsClicked()
    {
        ShowPanel(settingsController?.gameObject);
        settingsController?.RefreshUI();
    }

    /// <summary>Save to slot 0 (AutoSave) then return to main menu.</summary>
    public void OnSaveAndQuitClicked()
    {
        SaveLoadManager.Instance?.SaveGame(0, "AutoSave");
        NotificationSystem.Instance?.ShowNotification("Game Saved");
        Time.timeScale = 1f;
        GameManager.Instance?.ReturnToMainMenu();
    }

    // ── Settings Back Button ──────────────────────────────────────────────

    public void OnSettingsBackClicked()
    {
        ShowPanel(pausePanel);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    void ShowPanel(GameObject panel)
    {
        HideAllPanels();
        if (panel != null) panel.SetActive(true);
    }

    void HideAllPanels()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsController != null) settingsController.gameObject.SetActive(false);
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    }
}
