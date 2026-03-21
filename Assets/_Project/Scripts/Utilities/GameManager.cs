using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mistborn.Utilities
{
    /// <summary>
    /// Central game manager handling game state, pausing, and checkpoints.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private GameState m_currentState = GameState.Playing;
        
        [Header("UI")]
        [SerializeField] private GameObject m_pauseMenu;
        [SerializeField] private GameObject m_deathScreen;
        
        [Header("Checkpoints")]
        [SerializeField] private Transform m_lastCheckpoint;
        [SerializeField] private Vector3 m_spawnPosition;
        
        [Header("Settings")]
        [SerializeField] private bool m_canPause = true;

        public GameState currentState => m_currentState;
        public Transform lastCheckpoint => m_lastCheckpoint;

        public event System.Action<GameState> OnStateChanged;

        public enum GameState
        {
            Playing,
            Paused,
            Dead,
            Cutscene,
            Menu
        }

        private void Awake()
        {
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

        private void Update()
        {
            if (m_canPause && Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            if (m_currentState == GameState.Playing)
                Pause();
            else if (m_currentState == GameState.Paused)
                Resume();
        }

        public void Pause()
        {
            SetState(GameState.Paused);
            Time.timeScale = 0f;
            ShowCursor(true);
            
            if (m_pauseMenu != null)
                m_pauseMenu.SetActive(true);
        }

        public void Resume()
        {
            SetState(GameState.Playing);
            Time.timeScale = 1f;
            ShowCursor(false);
            
            if (m_pauseMenu != null)
                m_pauseMenu.SetActive(false);
        }

        private void SetState(GameState state)
        {
            m_currentState = state;
            OnStateChanged?.Invoke(state);
        }

        public void PlayerDied()
        {
            SetState(GameState.Dead);
            
            if (m_deathScreen != null)
                m_deathScreen.SetActive(true);
        }

        public void RespawnAtCheckpoint()
        {
            if (m_deathScreen != null)
                m_deathScreen.SetActive(false);

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 pos = m_lastCheckpoint != null ? m_lastCheckpoint.position : m_spawnPosition;
                player.transform.position = pos;
                player.GetComponent<PlayerHealth>()?.Respawn();
            }

            SetState(GameState.Playing);
        }

        public void SetCheckpoint(Transform checkpoint)
        {
            m_lastCheckpoint = checkpoint;
            m_spawnPosition = checkpoint.position;
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadScene(string sceneName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowCursor(bool show)
        {
            Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = show;
        }
    }
}
