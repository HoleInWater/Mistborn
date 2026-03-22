using UnityEngine;
using UnityEngine.SceneManagement;

namespace MistbornGame.UI
{
    public class GameManager : MonoBehaviour
    {
        // Fix: Added type (GameManager), name (Instance), and proper property syntax
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        public bool isPaused = false;
        public bool isGameOver = false;

        [Header("References")]
        public GameObject pauseMenu;
        public GameObject gameOverScreen;

        void Awake()
        {
            // Singleton logic
            if (Instance == null)
            {
                Instance = this;
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

            // Fix: Cleaned up the parentheses and semicolons here
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
