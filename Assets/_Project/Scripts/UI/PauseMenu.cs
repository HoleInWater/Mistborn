using UnityEngine;

/// <summary>
/// Pause menu controller.
/// Usage: Attach to pause menu panel.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    // UI PANELS
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject quitConfirmPanel;
    
    void Start()
    {
        // Start with all panels hidden
        HideAll();
    }
    
    void Update()
    {
        // Toggle pause with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameObject.activeSelf)
                Resume();
            else
                Pause();
        }
    }
    
    public void Pause()
    {
        gameObject.SetActive(true);
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        quitConfirmPanel.SetActive(false);
        Time.timeScale = 0f;
    }
    
    public void Resume()
    {
        Time.timeScale = 1f;
        HideAll();
        gameObject.SetActive(false);
    }
    
    public void ShowOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }
    
    public void HideOptions()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }
    
    public void ShowQuitConfirm()
    {
        pausePanel.SetActive(false);
        quitConfirmPanel.SetActive(true);
    }
    
    public void HideQuitConfirm()
    {
        quitConfirmPanel.SetActive(false);
        pausePanel.SetActive(true);
    }
    
    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    public void QuitToDesktop()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    void HideAll()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);
    }
}
