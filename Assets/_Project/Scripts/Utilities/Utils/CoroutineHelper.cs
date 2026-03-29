using UnityEngine;
using System.Collections;

namespace MistbornGame.Utilities
{
    public class CoroutineHelper : MonoBehaviour
    {
        /// <summary>
        /// Runs an action after a delay without needing to create a coroutine method
        /// </summary>
        public static Coroutine DelayedAction(MonoBehaviour monoBehaviour, float delay, System.Action action)
        {
            return monoBehaviour.StartCoroutine(DelayedActionCoroutine(delay, action));
        }

        /// <summary>
        /// Runs an action repeatedly at intervals
        /// </summary>
        public static Coroutine RepeatedAction(MonoBehaviour monoBehaviour, float interval, System.Action action, int repetitions = -1)
        {
            return monoBehaviour.StartCoroutine(RepeatedActionCoroutine(interval, action, repetitions));
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

        /// <summary>
        /// Waits for a condition to become true before executing an action
        /// </summary>
        public static Coroutine WaitForCondition(MonoBehaviour monoBehaviour, System.Func<bool> condition, System.Action action, float checkInterval = 0.1f)
        {
            return monoBehaviour.StartCoroutine(WaitForConditionCoroutine(condition, action, checkInterval));
        }

        private static IEnumerator WaitForConditionCoroutine(System.Func<bool> condition, System.Action action, float checkInterval)
        {
            while (!condition())
            {
                yield return new WaitForSeconds(checkInterval);
            }
            action?.Invoke();
        }
    }
}