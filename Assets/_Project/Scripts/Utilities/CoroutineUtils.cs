using UnityEngine;
using System.Collections;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for managing coroutines
    /// </summary>
    public static class CoroutineUtils
    {
        /// <summary>
        /// Starts a coroutine on a MonoBehaviour
        /// </summary>
        public static Coroutine StartRoutine(MonoBehaviour behaviour, IEnumerator routine)
        {
            if (behaviour == null || routine == null)
            {
                Debug.LogError("CoroutineUtils: Behaviour or routine is null");
                return null;
            }
            return behaviour.StartCoroutine(routine);
        }

        /// <summary>
        /// Stops a coroutine safely
        /// </summary>
        public static void StopRoutine(MonoBehaviour behaviour, Coroutine routine)
        {
            if (behaviour != null && routine != null)
            {
                behaviour.StopCoroutine(routine);
            }
        }

        /// <summary>
        /// Stops all coroutines on a MonoBehaviour
        /// </summary>
        public static void StopAllRoutines(MonoBehaviour behaviour)
        {
            if (behaviour != null)
            {
                behaviour.StopAllCoroutines();
            }
        }

        /// <summary>
        /// Creates a coroutine that waits for a condition
        /// </summary>
        public static IEnumerator WaitForCondition(System.Func<bool> condition, float timeout = float.MaxValue)
        {
            float elapsed = 0f;
            while (!condition() && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Starts a coroutine that waits for a condition and then invokes an action
        /// </summary>
        public static Coroutine WaitForConditionThenInvoke(MonoBehaviour behaviour, System.Func<bool> condition, System.Action onComplete, float timeout = float.MaxValue)
        {
            if (behaviour == null || condition == null) return null;
            return behaviour.StartCoroutine(WaitForConditionCoroutine(condition, onComplete, timeout));
        }

        private static IEnumerator WaitForConditionCoroutine(System.Func<bool> condition, System.Action onComplete, float timeout)
        {
            yield return WaitForCondition(condition, timeout);
            onComplete?.Invoke();
        }

        /// <summary>
        /// Creates a coroutine that invokes an action after a delay
        /// </summary>
        public static IEnumerator InvokeAfterDelay(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            if (action != null)
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// Creates a coroutine that fades a CanvasGroup over time
        /// </summary>
        public static IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                yield return null;
            }
            canvasGroup.alpha = endAlpha;
        }

        /// <summary>
        /// Creates a coroutine that moves a Transform to a target position
        /// </summary>
        public static IEnumerator MoveToPosition(Transform transform, Vector3 targetPosition, float duration)
        {
            Vector3 startPosition = transform.position;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
            transform.position = targetPosition;
        }

        /// <summary>
        /// Creates a coroutine that rotates a Transform to a target rotation
        /// </summary>
        public static IEnumerator RotateToRotation(Transform transform, Quaternion targetRotation, float duration)
        {
            Quaternion startRotation = transform.rotation;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }
            transform.rotation = targetRotation;
        }

        /// <summary>
        /// Creates a coroutine that fades an AudioSource's volume over time
        /// </summary>
        public static IEnumerator FadeAudioSource(AudioSource source, float targetVolume, float duration)
        {
            if (source == null) yield break;
            float startVolume = source.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                source.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }
            source.volume = targetVolume;
        }
    }
}
