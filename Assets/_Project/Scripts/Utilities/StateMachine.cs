using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Utilities
{
    public class StateMachine<T> where T : System.Enum
    {
        private T currentState;
        private T previousState;
        
        private Dictionary<T, System.Action> onEnterStates = new Dictionary<T, System.Action>();
        private Dictionary<T, System.Action> onExitStates = new Dictionary<T, System.Action>();
        private Dictionary<T, System.Action> onUpdateStates = new Dictionary<T, System.Action>();
        
        public T CurrentState => currentState;
        public T PreviousState => previousState;
        
        public event System.Action<T, T> OnStateChanged;
        
        public StateMachine(T initialState)
        {
            currentState = initialState;
            previousState = initialState;
        }
        
        public void RegisterOnEnter(T state, System.Action callback)
        {
            if (!onEnterStates.ContainsKey(state))
            {
                onEnterStates[state] = callback;
            }
            else
            {
                onEnterStates[state] += callback;
            }
        }
        
        public void RegisterOnExit(T state, System.Action callback)
        {
            if (!onExitStates.ContainsKey(state))
            {
                onExitStates[state] = callback;
            }
            else
            {
                onExitStates[state] += callback;
            }
        }
        
        public void RegisterOnUpdate(T state, System.Action callback)
        {
            if (!onUpdateStates.ContainsKey(state))
            {
                onUpdateStates[state] = callback;
            }
            else
            {
                onUpdateStates[state] += callback;
            }
        }
        
        public void SetState(T newState)
        {
            if (EqualityComparer<T>.Default.Equals(currentState, newState))
                return;
            
            previousState = currentState;
            currentState = newState;
            
            if (onExitStates.ContainsKey(previousState))
            {
                onExitStates[previousState]?.Invoke();
            }
            
            if (onEnterStates.ContainsKey(currentState))
            {
                onEnterStates[currentState]?.Invoke();
            }
            
            OnStateChanged?.Invoke(previousState, currentState);
        }
        
        public void Update()
        {
            if (onUpdateStates.ContainsKey(currentState))
            {
                onUpdateStates[currentState]?.Invoke();
            }
        }
        
        public bool IsInState(T state)
        {
            return EqualityComparer<T>.Default.Equals(currentState, state);
        }
        
        public bool WasInState(T state)
        {
            return EqualityComparer<T>.Default.Equals(previousState, state);
        }
        
        public void RevertToPreviousState()
        {
            SetState(previousState);
        }
    }
    
    public class StateMachine
    {
        public interface IState
        {
            void Enter();
            void Exit();
            void Update();
        }
        
        private IState currentState;
        
        public IState CurrentState => currentState;
        
        public event System.Action<IState, IState> OnStateChanged;
        
        public void SetState(IState newState)
        {
            if (currentState != null)
            {
                currentState.Exit();
            }
            
            IState previousState = currentState;
            currentState = newState;
            
            if (currentState != null)
            {
                currentState.Enter();
            }
            
            OnStateChanged?.Invoke(previousState, currentState);
        }
        
        public void Update()
        {
            currentState?.Update();
        }
    }
    
    public abstract class StateBase : StateMachine.IState
    {
        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
    }
}
