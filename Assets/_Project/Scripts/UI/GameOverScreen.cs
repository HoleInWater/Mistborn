/// <summary>
/// Game over screen controller.
/// Usage: Attach to game over panel.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    // UI ELEMENTS
    public GameObject gameOverPanel;
    public UnityEngine.UI.Text titleText;
    public UnityEngine.UI.Text statsText;
    public UnityEngine.UI.Button restartButton;
    public UnityEngine.UI.Button mainMenuButton;
    
    // STATE
    private bool isShowing = false;
    
    void Start()
    {
        gameObject.SetActive(false);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestart);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);
    }
    
    public void ShowGameOver()
    {
        if (isShowing) return;
        
        isShowing = true;
        gameObject.SetActive(true);
        gameOverPanel.SetActive(true);
        
        Time.timeScale = 0f;
        
        // Update stats
        UpdateStats();
    }
    
    void UpdateStats()
    {
        if (statsText != null)
        {
            string stats = $"Time Survived: {GetTimeString()}\n";
            stats += $"Enemies Defeated: 0\n"; // TODO: Track this
            stats += $"Metals Collected: 0\n"; // TODO: Track this
            statsText.text = stats;
        }
    }
    
    string GetTimeString()
    {
        float time = Time.timeSinceLevelLoad;
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }
    
    public void OnRestart()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
    
    public void OnMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    public void Hide()
    {
        isShowing = false;
        gameObject.SetActive(false);
    }
}
