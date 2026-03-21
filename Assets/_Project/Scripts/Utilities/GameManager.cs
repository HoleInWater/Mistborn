using UnityEngine;

namespace Mistborn.Utilities
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private bool m_isPaused;
        [SerializeField] private float m_gameTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (!m_isPaused)
            {
                m_gameTime += Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            m_isPaused = !m_isPaused;
            Time.timeScale = m_isPaused ? 0f : 1f;
        }

        public bool IsPaused => m_isPaused;
        public float GameTime => m_gameTime;
    }
}
