using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for handling common coroutine operations
    /// </summary>
    public class CoroutineUtils : MonoBehaviour
    {
        /// <summary>
        /// Runs an action after a delay in seconds
        /// </summary>
        public static Coroutine DelayedAction(MonoBehaviour monoBehaviour, float delay, System.Action action)
        {
            if (monoBehaviour == null || action == null) return null;
            return monoBehaviour.StartCoroutine(DelayedActionCoroutine(delay, action));
        }

        /// <summary>
        /// Runs an action repeatedly at intervals
        /// </summary>
        public static Coroutine RepeatedAction(MonoBehaviour monoBehaviour, float interval, System.Action action, int repetitions = -1)
        {
            if (monoBehaviour == null || action == null) return null;
            return monoBehaviour.StartCoroutine(RepeatedActionCoroutine(interval, action, repetitions));
        }

        /// <summary>
        /// Waits until a condition becomes true, then executes an action
        /// </summary>
        public static Coroutine WaitForCondition(MonoBehaviour monoBehaviour, System.Func<bool> condition, System.Action action, float checkInterval = 0.1f)
        {
            if (monoBehaviour == null || condition == null || action == null) return null;
            return monoBehaviour.StartCoroutine(WaitForConditionCoroutine(condition, action, checkInterval));
        }

        /// <summary>
        /// Executes an action for a specified duration
        /// </summary>
        public static Coroutine TimedAction(MonoBehaviour monoBehaviour, float duration, System.Action action)
        {
            if (monoBehaviour == null || action == null) return null;
            return monoBehaviour.StartCoroutine(TimedActionCoroutine(duration, action));
        }

        /// <summary>
        /// Fades a value from start to end over time
        /// </summary>
        public static Coroutine FloatLerp(MonoBehaviour monoBehaviour, float start, float end, float duration, System.Action<float> onUpdate)
        {
            if (monoBehaviour == null || onUpdate == null) return null;
            return monoBehaviour.StartCoroutine(FloatLerpCoroutine(start, end, duration, onUpdate));
        }

        private static IEnumerator DelayedActionCoroutine(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        private static IEnumerator RepeatedActionCoroutine(float interval, System.Action action, int repetitions)
        {
            int count = 0;
            while (repetitions == -1 || count < repetitions)
            {
                yield return new WaitForSeconds(interval);
                action?.Invoke();
                count++;
            }
        }

        private static IEnumerator WaitForConditionCoroutine(System.Func<bool> condition, System.Action action, float checkInterval)
        {
            while (!condition())
            {
                yield return new WaitForSeconds(checkInterval);
            }
            action?.Invoke();
        }

        private static IEnumerator TimedActionCoroutine(float duration, System.Action action)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                action?.Invoke();
                yield return null;
            }
        }

        private static IEnumerator FloatLerpCoroutine(float start, float end, float duration, System.Action<float> onUpdate)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float value = Mathf.Lerp(start, end, t);
                onUpdate?.Invoke(value);
                yield return null;
            }
        }
    }
}