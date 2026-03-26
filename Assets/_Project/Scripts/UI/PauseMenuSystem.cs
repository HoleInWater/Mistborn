using UnityEngine;

/// <summary>
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
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameFlowManager.Instance?.PauseGame();
        GameManager.Instance?.SetState(GameManager.GameState.Paused);
    }

    public void ResumeGame()
    {
        isPaused = false;
        HideAllPanels();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameFlowManager.Instance?.ResumeGame();
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
    }
}
