// ============================================================
// FILE: PauseMenu.cs
// SYSTEM: UI
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Handles the pause menu UI and functionality.
//
// TODO:
//   - Add menu options
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;
using UnityEngine.UI;

namespace Mistborn.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("Menu Panels")]
        public GameObject mainPausePanel;
        public GameObject optionsPanel;
        public GameObject controlsPanel;
        
        [Header("Buttons")]
        public Button resumeButton;
        public Button optionsButton;
        public Button controlsButton;
        public Button mainMenuButton;
        public Button quitButton;
        
        [Header("Settings")]
        public KeyCode pauseKey = KeyCode.Escape;
        
        private bool isPaused;
        
        private void Start()
        {
            // Set up button listeners
            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);
            
            if (optionsButton != null)
                optionsButton.onClick.AddListener(ShowOptions);
            
            if (controlsButton != null)
                controlsButton.onClick.AddListener(ShowControls);
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
            
            // Start hidden
            if (mainPausePanel != null)
                mainPausePanel.SetActive(false);
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                TogglePause();
            }
        }
        
        public void TogglePause()
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
        
        public void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;
            
            if (mainPausePanel != null)
                mainPausePanel.SetActive(true);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        public void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;
            
            if (mainPausePanel != null)
                mainPausePanel.SetActive(false);
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Hide sub-panels
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (controlsPanel != null) controlsPanel.SetActive(false);
        }
        
        private void ShowOptions()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(true);
            }
        }
        
        private void ShowControls()
        {
            if (controlsPanel != null)
            {
                controlsPanel.SetActive(true);
            }
        }
        
        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        
        private void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
        }
    }
}
