// ============================================================
// FILE: GameManager.cs
// SYSTEM: Utilities
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Central game manager. Handles game state, pausing, checkpoints.
//
// TODO:
//   - Add checkpoint system
//   - Add game state machine
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mistborn.Utilities
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Game State")]
        public GameState currentState = GameState.Playing;
        
        [Header("UI")]
        public GameObject pauseMenu;
        public GameObject deathScreen;
        
        [Header("Checkpoint")]
        public Transform lastCheckpoint;
        public Vector3 spawnPosition;
        
        [Header("Settings")]
        public bool canPause = true;
        
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
            HandlePauseInput();
        }
        
        private void HandlePauseInput()
        {
            if (!canPause) return;
            
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }
        }
        
        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                Pause();
            }
            else if (currentState == GameState.Paused)
            {
                Resume();
            }
        }
        
        public void Pause()
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(true);
            }
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            Debug.Log("Game Paused");
        }
        
        public void Resume()
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            Debug.Log("Game Resumed");
        }
        
        public void PlayerDied()
        {
            currentState = GameState.Dead;
            
            if (deathScreen != null)
            {
                deathScreen.SetActive(true);
            }
            
            Debug.Log("Player Died - Game Over");
        }
        
        public void RespawnAtCheckpoint()
        {
            if (deathScreen != null)
            {
                deathScreen.SetActive(false);
            }
            
            // Move player to checkpoint
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 respawnPos = spawnPosition;
                if (lastCheckpoint != null)
                {
                    respawnPos = lastCheckpoint.position;
                }
                
                player.transform.position = respawnPos;
                
                // Reset health
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.Respawn();
                }
            }
            
            currentState = GameState.Playing;
            
            Debug.Log($"Respawned at checkpoint");
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
            Debug.Log("Quitting game");
            Application.Quit();
        }
        
        public void SetCheckpoint(Transform checkpoint)
        {
            lastCheckpoint = checkpoint;
            spawnPosition = checkpoint.position;
            Debug.Log($"Checkpoint set at {checkpoint.position}");
        }
    }
}
