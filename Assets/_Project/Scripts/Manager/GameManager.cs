using UnityEngine;
using UnityEngine.SceneManagement;

// Ensure the file name in your Unity folder is exactly "GameManager.cs"
public class GameManager : MonoBehaviour
{
    // This MUST be named GameManagerInstance for your other scripts to find it
    public static GameManager GameManagerInstance { get; private set; }

    [Header("Game State")]
    public bool isPaused = false;
    public bool isGameOver = false;

    [Header("References")]
    public GameObject pauseMenu;
    public GameObject gameOverScreen;

    void Awake()
    {
        if (GameManagerInstance == null)
        {
            GameManagerInstance = this;
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
        if (pauseMenu != null) pauseMenu.SetActive(isPaused);
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        if (gameOverScreen != null) gameOverScreen.SetActive(true);
    }

    public void RestartGame()
    {
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
