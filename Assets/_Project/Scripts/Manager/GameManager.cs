using UnityEngine;
using UnityEngine.SceneManagement;

// Removed the 'namespace' line so other scripts can see this easily
public class GameManagerInstance : MonoBehaviour 
{
    // Changed 'Instance' to 'instance' to match standard naming if needed, 
    // but kept the class name as GameManagerInstance per your original code.
    public static GameManagerInstance instance { get; private set; }

    [Header("Game State")]
    public bool isPaused = false;
    public bool isGameOver = false;

    [Header("References")]
    public GameObject pauseMenu;
    public GameObject gameOverScreen;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(isPaused);
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }
    }

    public void RestartGame()
    {
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
