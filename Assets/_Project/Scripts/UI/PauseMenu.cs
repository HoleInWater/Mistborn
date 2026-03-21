using UnityEngine;
using UnityEngine.UI;

namespace Mistborn.UI
{
    /// <summary>
    /// Handles pause menu UI and functionality.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject m_mainPanel;
        [SerializeField] private GameObject m_optionsPanel;
        [SerializeField] private GameObject m_controlsPanel;
        
        [Header("Buttons")]
        [SerializeField] private Button m_resumeButton;
        [SerializeField] private Button m_optionsButton;
        [SerializeField] private Button m_controlsButton;
        [SerializeField] private Button m_mainMenuButton;
        [SerializeField] private Button m_quitButton;
        
        [Header("Input")]
        [SerializeField] private KeyCode m_pauseKey = KeyCode.Escape;

        private bool m_isPaused;

        public bool isPaused => m_isPaused;

        private void Start()
        {
            SetupButtons();
            
            if (m_mainPanel != null)
                m_mainPanel.SetActive(false);
        }

        private void SetupButtons()
        {
            if (m_resumeButton != null) m_resumeButton.onClick.AddListener(Resume);
            if (m_optionsButton != null) m_optionsButton.onClick.AddListener(ShowOptions);
            if (m_controlsButton != null) m_controlsButton.onClick.AddListener(ShowControls);
            if (m_mainMenuButton != null) m_mainMenuButton.onClick.AddListener(LoadMainMenu);
            if (m_quitButton != null) m_quitButton.onClick.AddListener(QuitGame);
        }

        private void Update()
        {
            if (Input.GetKeyDown(m_pauseKey))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            if (m_isPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            m_isPaused = true;
            Time.timeScale = 0f;
            ShowCursor(true);
            
            if (m_mainPanel != null)
                m_mainPanel.SetActive(true);
        }

        public void Resume()
        {
            m_isPaused = false;
            Time.timeScale = 1f;
            ShowCursor(false);
            HideAllPanels();
        }

        private void HideAllPanels()
        {
            if (m_mainPanel != null) m_mainPanel.SetActive(false);
            if (m_optionsPanel != null) m_optionsPanel.SetActive(false);
            if (m_controlsPanel != null) m_controlsPanel.SetActive(false);
        }

        private void ShowOptions()
        {
            if (m_optionsPanel != null) m_optionsPanel.SetActive(true);
        }

        private void ShowControls()
        {
            if (m_controlsPanel != null) m_controlsPanel.SetActive(true);
        }

        private void LoadMainMenu()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        private void QuitGame()
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
