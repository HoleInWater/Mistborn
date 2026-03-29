using UnityEngine;
using UnityEngine.Events;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for managing Unity events and delegates
    /// </summary>
    public static class EventUtils
    {
        /// <summary>
        /// Safely invokes a UnityAction, catching any exceptions
        /// </summary>
        public static void SafeInvoke(UnityAction action)
        {
            if (action != null)
            {
                try
                {
                    action.Invoke();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"EventUtils: Exception during event invocation: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Safely invokes a UnityAction with one parameter, catching any exceptions
        /// </summary>
        public static void SafeInvoke<T>(UnityAction<T> action, T parameter)
        {
            if (action != null)
            {
                try
                {
                    action.Invoke(parameter);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"EventUtils: Exception during event invocation: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Safely invokes a UnityAction with two parameters, catching any exceptions
        /// </summary>
        public static void SafeInvoke<T1, T2>(UnityAction<T1, T2> action, T1 param1, T2 param2)
        {
            if (action != null)
            {
                try
                {
                    action.Invoke(param1, param2);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"EventUtils: Exception during event invocation: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Adds a listener to a UnityEvent, removing any duplicate listeners first
        /// </summary>
        public static void AddUniqueListener(UnityEvent evt, UnityAction call)
        {
            if (evt != null && call != null)
            {
                evt.RemoveListener(call);
                evt.AddListener(call);
            }
        }

        /// <summary>
        /// Removes all listeners from a UnityEvent
        /// </summary>
        public static void RemoveAllListeners(UnityEvent evt)
        {
            if (evt != null)
            {
                evt.RemoveAllListeners();
            }
        }

        /// <summary>
        /// Checks if a UnityEvent has any listeners
        /// </summary>
        public static bool HasListeners(UnityEvent evt)
        {
            if (evt == null) return false;
            return evt.GetPersistentEventCount() > 0;
        }

        /// <summary>
        /// Creates a UnityEvent that can be used for messaging between objects
        /// </summary>
        public static UnityEvent CreateEvent()
        {
            return new UnityEvent();
        }

        /// <summary>
        /// Creates a UnityEvent with one parameter
        /// </summary>
        public static UnityEvent<T> CreateEvent<T>()
        {
            return new UnityEvent<T>();
        }
    }
}
