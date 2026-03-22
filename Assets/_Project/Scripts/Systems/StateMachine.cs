/// <summary>
/// Simple state machine for AI or game logic.
/// Usage: StateMachine machine = new StateMachine();
/// </summary>
public class StateMachine
{
    private object currentState;
    
    public object CurrentState => currentState;
    
    public void SetState(object newState)
    {
        if (currentState == newState) return;
        
        // Exit old state
        if (currentState is IState oldState)
        {
            oldState.OnExit();
        }
        
        currentState = newState;
        
        // Enter new state
        if (currentState is IState newStateInterface)
        {
            newStateInterface.OnEnter();
        }
    }
    
    public void Update()
    {
        (currentState as IState)?.OnUpdate();
    }
}

/// <summary>
/// Interface for states.
/// Usage: public class MyState : IState { }
/// </summary>
public interface IState
{
    void OnEnter();
    void OnExit();
    void OnUpdate();
}
