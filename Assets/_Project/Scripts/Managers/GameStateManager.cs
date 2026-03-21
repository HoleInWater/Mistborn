using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Mistborn.Managers
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Loading,
        GameOver,
        Cutscene
    }

    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [Header("States")]
        [SerializeField] private GameState m_initialState = GameState.MainMenu;
        [SerializeField] private GameState m_currentState;

        [Header("Time Scale")]
        [SerializeField] private bool m_pauseOnPauseState = true;

        private Dictionary<GameState, IGameState> m_stateHandlers = new Dictionary<GameState, IGameState>();
        private Stack<GameState> m_stateHistory = new Stack<GameState>();
        private float[] m_previousTimeScales = new float[10];
        private int m_timeScaleIndex;

        public GameState CurrentState => m_currentState;
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            DontDestroyOnLoad(gameObject);
            InitializeStates();
        }

        private void Start()
        {
            SetState(m_initialState);
        }

        private void InitializeStates()
        {
            RegisterState(GameState.MainMenu, new MainMenuState());
            RegisterState(GameState.Playing, new PlayingState());
            RegisterState(GameState.Paused, new PausedState());
            RegisterState(GameState.Loading, new LoadingState());
            RegisterState(GameState.GameOver, new GameOverState());
            RegisterState(GameState.Cutscene, new CutsceneState());
        }

        public void RegisterState(GameState state, IGameState handler)
        {
            m_stateHandlers[state] = handler;
            handler.Initialize();
        }

        public void SetState(GameState newState)
        {
            if (m_currentState == newState) return;

            if (m_stateHandlers.TryGetValue(m_currentState, out IGameState current))
            {
                current.Exit();
            }

            m_stateHistory.Push(m_currentState);
            m_currentState = newState;

            if (m_stateHandlers.TryGetValue(newState, out IGameState next))
            {
                next.Enter();
            }

            UpdateTimeScale(newState);
            OnStateChanged?.Invoke(newState);
        }

        public void ReturnToPreviousState()
        {
            if (m_stateHistory.Count == 0) return;

            GameState previous = m_stateHistory.Pop();
            SetState(previous);
        }

        private void UpdateTimeScale(GameState state)
        {
            if (!m_pauseOnPauseState) return;

            switch (state)
            {
                case GameState.Paused:
                case GameState.MainMenu:
                    SaveTimeScale();
                    Time.timeScale = 0f;
                    break;
                default:
                    RestoreTimeScale();
                    break;
            }
        }

        private void SaveTimeScale()
        {
            m_previousTimeScales[m_timeScaleIndex++] = Time.timeScale;
            if (m_timeScaleIndex >= m_previousTimeScales.Length)
                m_timeScaleIndex = 0;
        }

        private void RestoreTimeScale()
        {
            if (m_timeScaleIndex > 0)
            {
                Time.timeScale = m_previousTimeScales[--m_timeScaleIndex];
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        public void Pause()
        {
            if (m_currentState == GameState.Playing)
            {
                SetState(GameState.Paused);
            }
        }

        public void Resume()
        {
            if (m_currentState == GameState.Paused)
            {
                SetState(GameState.Playing);
            }
        }

        public void TogglePause()
        {
            if (m_currentState == GameState.Playing)
                Pause();
            else if (m_currentState == GameState.Paused)
                Resume();
        }

        public void StartGame()
        {
            SetState(GameState.Loading);
        }

        public void GameOver()
        {
            SetState(GameState.GameOver);
        }

        public void ReturnToMenu()
        {
            SetState(GameState.MainMenu);
        }
    }

    public interface IGameState
    {
        void Initialize();
        void Enter();
        void Exit();
        void Update();
    }

    public class MainMenuState : IGameState
    {
        public virtual void Initialize() { }
        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
    }

    public class PlayingState : IGameState
    {
        public virtual void Initialize() { }
        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
    }

    public class PausedState : IGameState
    {
        public virtual void Initialize() { }
        public virtual void Enter()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        public virtual void Exit()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        public virtual void Update() { }
    }

    public class LoadingState : IGameState
    {
        public virtual void Initialize() { }
        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
    }

    public class GameOverState : IGameState
    {
        public virtual void Initialize() { }
        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
    }

    public class CutsceneState : IGameState
    {
        public virtual void Initialize() { }
        public virtual void Enter()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        public virtual void Exit()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        public virtual void Update() { }
    }

    public class LevelTransition : MonoBehaviour
    {
        [SerializeField] private string m_targetScene;
        [SerializeField] private Vector3 m_spawnPoint;
        [SerializeField] private Transform m_doorTransform;

        [Header("Transition Effects")]
        [SerializeField] private bool m_useDoorTransition = true;
        [SerializeField] private float m_doorCloseDuration = 0.5f;

        private bool m_isTransitioning;

        public void Transition()
        {
            if (m_isTransitioning) return;
            StartCoroutine(TransitionRoutine());
        }

        private IEnumerator TransitionRoutine()
        {
            m_isTransitioning = true;

            if (m_useDoorTransition)
            {
                yield return StartCoroutine(DoorCloseRoutine());
            }

            if (!string.IsNullOrEmpty(m_targetScene))
            {
                SaveLoadSystem.Save("LastScene", m_targetScene);
                LoadingScreenManager.Load(m_targetScene);
            }

            yield return new WaitForSeconds(0.5f);

            if (!string.IsNullOrEmpty(m_targetScene))
            {
                PlayerSpawnManager.SetSpawnPoint(m_spawnPoint);
            }

            m_isTransitioning = false;
        }

        private IEnumerator DoorCloseRoutine()
        {
            float elapsed = 0f;
            while (elapsed < m_doorCloseDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Transition();
        }
    }

    public class PlayerSpawnManager : MonoBehaviour
    {
        private static Vector3 m_spawnPoint = Vector3.zero;
        private static Quaternion m_spawnRotation = Quaternion.identity;

        public static void SetSpawnPoint(Vector3 position, Quaternion rotation = default)
        {
            m_spawnPoint = position;
            m_spawnRotation = rotation == default ? Quaternion.identity : rotation;
        }

        public static Vector3 GetSpawnPoint()
        {
            return m_spawnPoint;
        }

        public static Quaternion GetSpawnRotation()
        {
            return m_spawnRotation;
        }
    }
}
