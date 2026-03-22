/// <summary>
/// Simple game state machine.
/// States: Menu, Playing, Paused, GameOver
/// Usage: GameState.Current == GameState.State.Playing
/// </summary>
public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }
    
    public enum State
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }
    
    [Header("Current State")]
    public State currentState = State.Menu;
    
    // EVENTS
    public System.Action<State> OnStateChanged;
    
    void Awake()
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
    
    void Update()
    {
        // Escape to pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == State.Playing)
                Pause();
            else if (currentState == State.Paused)
                Resume();
        }
    }
    
    public void StartGame()
    {
        SetState(State.Playing);
    }
    
    public void Pause()
    {
        SetState(State.Paused);
        Time.timeScale = 0f;
    }
    
    public void Resume()
    {
        SetState(State.Playing);
        Time.timeScale = 1f;
    }
    
    public void GameOver()
    {
        SetState(State.GameOver);
        Time.timeScale = 0f;
    }
    
    public void ReturnToMenu()
    {
        SetState(State.Menu);
        Time.timeScale = 1f;
    }
    
    void SetState(State newState)
    {
        if (currentState == newState) return;
        
        currentState = newState;
        OnStateChanged?.Invoke(newState);
        
        Debug.Log($"Game State: {newState}");
    }
    
    // Helpers
    public bool IsPlaying => currentState == State.Playing;
    public bool IsPaused => currentState == State.Paused;
    public bool IsGameOver => currentState == State.GameOver;
}
