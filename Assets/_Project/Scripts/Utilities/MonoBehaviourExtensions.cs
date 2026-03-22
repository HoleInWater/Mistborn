using System;
using System.Collections;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Extension methods for MonoBehaviour class
    /// </summary>
    public static class MonoBehaviourExtensions
    {
        /// <summary>
        /// Starts a coroutine on a MonoBehaviour with optional completion callback
        /// </summary>
        public static Coroutine StartCoroutine(this MonoBehaviour behaviour, Action onComplete, float delay = 0f)
        {
            if (behaviour == null) return null;
            return behaviour.StartCoroutine(RunCoroutine(onComplete, delay));
        }

        private static IEnumerator RunCoroutine(Action onComplete, float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null;
            }
            onComplete?.Invoke();
        }

        /// <summary>
        /// Invokes an action after a delay
        /// </summary>
        public static void InvokeAction(this MonoBehaviour behaviour, Action action, float delay)
        {
            if (behaviour == null || action == null) return;
            behaviour.StartCoroutine(InvokeAfterDelay(action, delay));
        }

        private static IEnumerator InvokeAfterDelay(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        /// <summary>
        /// Invokes an action repeatedly at an interval
        /// </summary>
        public static Coroutine InvokeRepeating(this MonoBehaviour behaviour, Action action, float delay, float repeatInterval)
        {
            if (behaviour == null || action == null) return null;
            return behaviour.StartCoroutine(RepeatAction(action, delay, repeatInterval));
        }

        private static IEnumerator RepeatAction(Action action, float delay, float repeatInterval)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
            while (true)
            {
                action?.Invoke();
                yield return new WaitForSeconds(repeatInterval);
            }
        }

        /// <summary>
        /// Stops all coroutines on a MonoBehaviour
        /// </summary>
        public static void StopAllCoroutines(this MonoBehaviour behaviour)
        {
            if (behaviour != null)
            {
                behaviour.StopAllCoroutines();
            }
        }

        /// <summary>
        /// Checks if a MonoBehaviour is enabled
        /// </summary>
        public static bool IsEnabled(this MonoBehaviour behaviour)
        {
            return behaviour != null && behaviour.enabled;
        }

        /// <summary>
        /// Enables a MonoBehaviour
        /// </summary>
        public static void Enable(this MonoBehaviour behaviour)
        {
            if (behaviour != null)
            {
                behaviour.enabled = true;
            }
        }

        /// <summary>
        /// Disables a MonoBehaviour
        /// </summary>
        public static void Disable(this MonoBehaviour behaviour)
        {
            if (behaviour != null)
            {
                behaviour.enabled = false;
            }
        }

        /// <summary>
        /// Toggles the enabled state of a MonoBehaviour
        /// </summary>
        public static void Toggle(this MonoBehaviour behaviour)
        {
            if (behaviour != null)
            {
                behaviour.enabled = !behaviour.enabled;
            }
        }

        /// <summary>
        /// Gets the game object of a MonoBehaviour
        /// </summary>
        public static GameObject GameObject(this MonoBehaviour behaviour)
        {
            return behaviour != null ? behaviour.gameObject : null;
        }

        /// <summary>
        /// Gets the transform of a MonoBehaviour
        /// </summary>
        public static Transform Transform(this MonoBehaviour behaviour)
        {
            return behaviour != null ? behaviour.transform : null;
        }

        /// <summary>
        /// Checks if a component is active and enabled
        /// </summary>
        public static bool IsActiveAndEnabled(this MonoBehaviour behaviour)
        {
            return behaviour != null && behaviour.isActiveAndEnabled;
        }

        /// <summary>
        /// Logs a debug message with the MonoBehaviour's name
        /// </summary>
        public static void Log(this MonoBehaviour behaviour, object message)
        {
            if (behaviour != null)
            {
                Debug.Log($"[{behaviour.name}] {message}", behaviour);
            }
        }

        /// <summary>
        /// Logs a warning message with the MonoBehaviour's name
        /// </summary>
        public static void LogWarning(this MonoBehaviour behaviour, object message)
        {
            if (behaviour != null)
            {
                Debug.LogWarning($"[{behaviour.name}] {message}", behaviour);
            }
        }

        /// <summary>
        /// Logs an error message with the MonoBehaviour's name
        /// </summary>
        public static void LogError(this MonoBehaviour behaviour, object message)
        {
            if (behaviour != null)
            {
                Debug.LogError($"[{behaviour.name}] {message}", behaviour);
            }
        }

        /// <summary>
        /// Starts a coroutine that returns a value
        /// </summary>
        public static Coroutine<T> StartCoroutine<T>(this MonoBehaviour behaviour, System.Collections.Generic.IEnumerator<T> coroutine)
        {
            if (behaviour == null) return null;
            Coroutine<T> wrapper = new Coroutine<T>(behaviour.StartCoroutine(coroutine));
            return wrapper;
        }

        /// <summary>
        /// Starts a coroutine with a timeout
        /// </summary>
        public static Coroutine StartCoroutineWithTimeout(this MonoBehaviour behaviour, System.Collections.IEnumerator coroutine, float timeout, System.Action onTimeout = null)
        {
            if (behaviour == null) return null;
            return behaviour.StartCoroutine(RunCoroutineWithTimeout(coroutine, timeout, onTimeout));
        }

        private static System.Collections.IEnumerator RunCoroutineWithTimeout(System.Collections.IEnumerator coroutine, float timeout, System.Action onTimeout)
        {
            float elapsed = 0f;
            while (coroutine.MoveNext())
            {
                if (elapsed >= timeout)
                {
                    onTimeout?.Invoke();
                    yield break;
                }
                elapsed += Time.deltaTime;
                yield return coroutine.Current;
            }
        }

        /// <summary>
        /// Starts a coroutine that can be cancelled
        /// </summary>
        public static CancellableCoroutine StartCoroutineCancellable(this MonoBehaviour behaviour, System.Collections.IEnumerator coroutine)
        {
            if (behaviour == null) return null;
            CancellableCoroutine cancellable = new CancellableCoroutine(behaviour.StartCoroutine(coroutine));
            return cancellable;
        }
    }

    /// <summary>
    /// Wrapper for coroutines that return a value
    /// </summary>
    public class Coroutine<T>
    {
        private Coroutine coroutine;
        private T value;
        private bool isComplete;
        
        public Coroutine(Coroutine coroutine)
        {
            this.coroutine = coroutine;
        }
        
        public T Value => value;
        public bool IsComplete => isComplete;
        public Coroutine Coroutine => coroutine;
    }

    /// <summary>
    /// Wrapper for cancellable coroutines
    /// </summary>
    public class CancellableCoroutine
    {
        private Coroutine coroutine;
        private bool isCancelled;
        
        public CancellableCoroutine(Coroutine coroutine)
        {
            this.coroutine = coroutine;
        }
        
        public void Cancel()
        {
            isCancelled = true;
        }
        
        public bool IsCancelled => isCancelled;
        public Coroutine Coroutine => coroutine;
    }
}
