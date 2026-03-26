using UnityEngine;
using System.Collections;

namespace MistbornGame.Utilities
{
    public class CoroutineHelper : MonoBehaviour
    {
        /// <summary>
        /// Runs an action after a delay.
        /// Pass realtime:true for UI/menu callbacks that must run even when timeScale = 0.
        /// </summary>
        public static Coroutine DelayedAction(MonoBehaviour monoBehaviour, float delay, System.Action action, bool realtime = false)
        {
            return monoBehaviour.StartCoroutine(DelayedActionCoroutine(delay, action, realtime));
        }

        /// <summary>
        /// Runs an action repeatedly at intervals.
        /// Pass realtime:true for UI callbacks that must run even when timeScale = 0.
        /// </summary>
        public static Coroutine RepeatedAction(MonoBehaviour monoBehaviour, float interval, System.Action action, int repetitions = -1, bool realtime = false)
        {
            return monoBehaviour.StartCoroutine(RepeatedActionCoroutine(interval, action, repetitions, realtime));
        }

        private static IEnumerator DelayedActionCoroutine(float delay, System.Action action, bool realtime)
        {
            if (realtime) yield return new WaitForSecondsRealtime(delay);
            else          yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        private static IEnumerator RepeatedActionCoroutine(float interval, System.Action action, int repetitions, bool realtime)
        {
            int count = 0;
            while (repetitions == -1 || count < repetitions)
            {
                if (realtime) yield return new WaitForSecondsRealtime(interval);
                else          yield return new WaitForSeconds(interval);
                action?.Invoke();
                count++;
            }
        }

        /// <summary>
        /// Waits for a condition to become true before executing an action.
        /// Pass realtime:true to poll even when timeScale = 0.
        /// </summary>
        public static Coroutine WaitForCondition(MonoBehaviour monoBehaviour, System.Func<bool> condition, System.Action action, float checkInterval = 0.1f, bool realtime = false)
        {
            return monoBehaviour.StartCoroutine(WaitForConditionCoroutine(condition, action, checkInterval, realtime));
        }

        private static IEnumerator WaitForConditionCoroutine(System.Func<bool> condition, System.Action action, float checkInterval, bool realtime)
        {
            while (!condition())
            {
                if (realtime) yield return new WaitForSecondsRealtime(checkInterval);
                else          yield return new WaitForSeconds(checkInterval);
            }
            action?.Invoke();
        }
    }
}
