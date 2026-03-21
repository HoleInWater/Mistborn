using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mistborn.Events
{
    public class GameEventListener : MonoBehaviour
    {
        [SerializeField] private GameEvent m_event;
        [SerializeField] private UnityEngine.Events.UnityEvent m_response;

        private void OnEnable()
        {
            if (m_event != null)
                m_event.Register(this);
        }

        private void OnDisable()
        {
            if (m_event != null)
                m_event.Unregister(this);
        }

        public void OnEventRaised()
        {
            m_response?.Invoke();
        }
    }

    [CreateAssetMenu(fileName = "GameEvent", menuName = "Mistborn/Game Event")]
    public class GameEvent : ScriptableObject
    {
        private List<GameEventListener> m_listeners = new List<GameEventListener>();

        public void Register(GameEventListener listener)
        {
            if (!m_listeners.Contains(listener))
                m_listeners.Add(listener);
        }

        public void Unregister(GameEventListener listener)
        {
            m_listeners.Remove(listener);
        }

        public void Raise()
        {
            for (int i = m_listeners.Count - 1; i >= 0; i--)
            {
                m_listeners[i].OnEventRaised();
            }
        }
    }

    public class GameEvent<T> : ScriptableObject
    {
        private List<GameEventListener<T>> m_listeners = new List<GameEventListener<T>>();

        public void Register(GameEventListener<T> listener)
        {
            if (!m_listeners.Contains(listener))
                m_listeners.Add(listener);
        }

        public void Unregister(GameEventListener<T> listener)
        {
            m_listeners.Remove(listener);
        }

        public void Raise(T value)
        {
            for (int i = m_listeners.Count - 1; i >= 0; i--)
            {
                m_listeners[i].OnEventRaised(value);
            }
        }
    }

    public class GameEventListener<T> : MonoBehaviour
    {
        [SerializeField] private GameEvent<T> m_event;
        [SerializeField] private UnityEngine.Events.UnityEvent<T> m_response;

        private void OnEnable()
        {
            if (m_event != null)
                m_event.Register(this);
        }

        private void OnDisable()
        {
            if (m_event != null)
                m_event.Unregister(this);
        }

        public void OnEventRaised(T value)
        {
            m_response?.Invoke(value);
        }
    }
}
