using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    
    [Header("UI References")]
    public string firstSceneName = "Scene 1";
    
    private void Start()
    {
        ShowMainPanel();
    }
    
    public void StartGame()
    {
        SceneManager.LoadScene(firstSceneName);
    }
    
    public void ShowSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    
    public void ShowCredits()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }
    
    public void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }
    
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
