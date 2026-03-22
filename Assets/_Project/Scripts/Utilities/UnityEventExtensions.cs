using UnityEngine.Events;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Extension methods for UnityEvent
    /// </summary>
    public static class UnityEventExtensions
    {
        /// <summary>
        /// Adds a listener to a UnityEvent, removing any duplicate listeners first
        /// </summary>
        public static void AddUniqueListener(this UnityEvent evt, UnityAction call)
        {
            if (evt != null && call != null)
            {
                evt.RemoveListener(call);
                evt.AddListener(call);
            }
        }

        /// <summary>
        /// Adds a listener to a UnityEvent with one parameter, removing any duplicate listeners first
        /// </summary>
        public static void AddUniqueListener<T>(this UnityEvent<T> evt, UnityAction<T> call)
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
        public static void RemoveAllListeners(this UnityEvent evt)
        {
            if (evt != null)
            {
                evt.RemoveAllListeners();
            }
        }

        /// <summary>
        /// Checks if a UnityEvent has any listeners
        /// </summary>
        public static bool HasListeners(this UnityEvent evt)
        {
            return evt != null && evt.GetPersistentEventCount() > 0;
        }

        /// <summary>
        /// Gets the count of listeners on a UnityEvent
        /// </summary>
        public static int GetListenerCount(this UnityEvent evt)
        {
            if (evt == null) return 0;
            return evt.GetPersistentEventCount();
        }

        /// <summary>
        /// Invokes a UnityEvent safely, catching any exceptions
        /// </summary>
        public static void SafeInvoke(this UnityEvent evt)
        {
            if (evt != null)
            {
                try
                {
                    evt.Invoke();
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"UnityEventExtensions: Exception during event invocation: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Invokes a UnityEvent with one parameter safely, catching any exceptions
        /// </summary>
        public static void SafeInvoke<T>(this UnityEvent<T> evt, T parameter)
        {
            if (evt != null)
            {
                try
                {
                    evt.Invoke(parameter);
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"UnityEventExtensions: Exception during event invocation: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Invokes a UnityEvent with two parameters safely, catching any exceptions
        /// </summary>
        public static void SafeInvoke<T1, T2>(this UnityEvent<T1, T2> evt, T1 param1, T2 param2)
        {
            if (evt != null)
            {
                try
                {
                    evt.Invoke(param1, param2);
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"UnityEventExtensions: Exception during event invocation: {e.Message}");
                }
            }
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

        /// <summary>
        /// Creates a UnityEvent with two parameters
        /// </summary>
        public static UnityEvent<T1, T2> CreateEvent<T1, T2>()
        {
            return new UnityEvent<T1, T2>();
        }
    }
}
