using UnityEngine;

/// <summary>
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
        if (Input.GetKeyDown(Keybinds.Pause))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameFlowManager.Instance?.PauseGame();
    }

    public void ResumeGame()
    {
        isPaused = false;
        CloseAllPanels();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameFlowManager.Instance?.ResumeGame();
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
    }
}
