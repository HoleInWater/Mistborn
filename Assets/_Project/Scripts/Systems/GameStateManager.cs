using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Systems
{
    public class GameStateManager : MonoBehaviour
    {
        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private GameSubState currentSubState = GameSubState.None;
        
        [Header("Difficulty")]
        [SerializeField] private Difficulty difficulty = Difficulty.Normal;
        [SerializeField] private float difficultyModifier = 1f;
        
        [Header("Time")]
        [SerializeField] private float gameTime = 0f;
        [SerializeField] private float playTime = 0f;
        
        [Header("State Management")]
        [SerializeField] private bool isPaused = false;
        [SerializeField] private bool isLoading = false;
        
        private Stack<GameState> stateStack = new Stack<GameState>();
        
        public static GameStateManager instance;
        
        public event System.Action<GameState> OnStateChanged;
        public event System.Action OnGamePaused;
        public event System.Action OnGameResumed;
        
        public GameState CurrentState => currentState;
        public bool IsPaused => isPaused;
        public float GameTime => gameTime;
        public float PlayTime => playTime;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }
        
        private void Start()
        {
            Application.targetFrameRate = 60;
            
            QualitySettings.vSyncCount = 1;
        }
        
        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                playTime += Time.deltaTime;
                gameTime += Time.deltaTime;
            }
            
            HandlePauseInput();
        }
        
        private void HandlePauseInput()
        {
            if (Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else if (currentState == GameState.Playing)
                {
                    PauseGame();
                }
            }
        }
        
        public void SetState(GameState newState)
        {
            if (currentState == newState)
                return;
            
            GameState previousState = currentState;
            currentState = newState;
            
            OnStateChanged?.Invoke(newState);
            
            switch (newState)
            {
                case GameState.MainMenu:
                    EnterMainMenu();
                    break;
                case GameState.Playing:
                    EnterPlaying();
                    break;
                case GameState.Paused:
                    EnterPaused();
                    break;
                case GameState.Inventory:
                    EnterInventory();
                    break;
                case GameState.Dialogue:
                    EnterDialogue();
                    break;
                case GameState.Loading:
                    EnterLoading();
                    break;
                case GameState.Settings:
                    EnterSettings();
                    break;
                case GameState.GameOver:
                    EnterGameOver();
                    break;
            }
            
            Debug.Log($"Game State: {previousState} -> {newState}");
        }
        
        public void PushState(GameState state)
        {
            stateStack.Push(currentState);
            SetState(state);
        }
        
        public void PopState()
        {
            if (stateStack.Count > 0)
            {
                GameState previousState = stateStack.Pop();
                SetState(previousState);
            }
        }
        
        public void PauseGame()
        {
            if (isPaused)
                return;
            
            isPaused = true;
            Time.timeScale = 0f;
            
            PushState(GameState.Paused);
            
            OnGamePaused?.Invoke();
        }
        
        public void ResumeGame()
        {
            if (!isPaused)
                return;
            
            isPaused = false;
            Time.timeScale = 1f;
            
            PopState();
            
            OnGameResumed?.Invoke();
        }
        
        public void StartNewGame()
        {
            ResetGameState();
            SetState(GameState.Playing);
        }
        
        public void LoadGame(int slot)
        {
            SaveSystem saveSystem = FindObjectOfType<SaveSystem>();
            if (saveSystem != null)
            {
                saveSystem.LoadGame(slot);
            }
            
            SetState(GameState.Playing);
        }
        
        public void SaveGame(int slot)
        {
            SaveSystem saveSystem = FindObjectOfType<SaveSystem>();
            if (saveSystem != null)
            {
                saveSystem.SaveGame(slot);
            }
        }
        
        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            isPaused = false;
            stateStack.Clear();
            SetState(GameState.MainMenu);
        }
        
        public void QuitGame()
        {
            Application.Quit();
        }
        
        private void EnterMainMenu()
        {
            Time.timeScale = 0f;
            isPaused = false;
        }
        
        private void EnterPlaying()
        {
            Time.timeScale = 1f;
            isPaused = false;
        }
        
        private void EnterPaused()
        {
        }
        
        private void EnterInventory()
        {
            Time.timeScale = 0f;
        }
        
        private void EnterDialogue()
        {
        }
        
        private void EnterLoading()
        {
            isLoading = true;
        }
        
        private void EnterSettings()
        {
        }
        
        private void EnterGameOver()
        {
            Time.timeScale = 0f;
        }
        
        private void ResetGameState()
        {
            playTime = 0f;
            gameTime = 0f;
            stateStack.Clear();
            
            QuestManager questManager = FindObjectOfType<QuestManager>();
            if (questManager != null)
            {
                questManager.NewGamePlus();
            }
        }
        
        public void SetDifficulty(Difficulty newDifficulty)
        {
            difficulty = newDifficulty;
            
            switch (newDifficulty)
            {
                case Difficulty.Trivial:
                    difficultyModifier = 0.5f;
                    break;
                case Difficulty.Easy:
                    difficultyModifier = 0.75f;
                    break;
                case Difficulty.Normal:
                    difficultyModifier = 1f;
                    break;
                case Difficulty.Hard:
                    difficultyModifier = 1.25f;
                    break;
                case Difficulty.Epic:
                    difficultyModifier = 1.5f;
                    break;
            }
        }
        
        public float GetDifficultyModifier()
        {
            return difficultyModifier;
        }
        
        public void SetSubState(GameSubState state)
        {
            currentSubState = state;
        }
        
        public string GetGameTimeFormatted()
        {
            int hours = Mathf.FloorToInt(gameTime / 3600);
            int minutes = Mathf.FloorToInt((gameTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
        
        public string GetPlayTimeFormatted()
        {
            int hours = Mathf.FloorToInt(playTime / 3600);
            int minutes = Mathf.FloorToInt((playTime % 3600) / 60);
            
            return $"{hours}h {minutes}m";
        }
    }
    
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Inventory,
        Dialogue,
        Loading,
        Settings,
        GameOver
    }
    
    public enum GameSubState
    {
        None,
        Exploring,
        Combat,
        Stealth,
        Cutscene
    }
    
    public enum Difficulty
    {
        Trivial,
        Easy,
        Normal,
        Hard,
        Epic
    }
}
