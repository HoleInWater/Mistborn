using UnityEngine;
using System.Collections;

namespace MistbornGame.Utilities
{
    public class CoroutineHelper : MonoBehaviour
    {
        /// <summary>
<<<<<<< HEAD
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
=======
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
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        {
            int count = 0;
            while (repetitions == -1 || count < repetitions)
            {
<<<<<<< HEAD
                yield return new WaitForSeconds(interval);
=======
                if (realtime) yield return new WaitForSecondsRealtime(interval);
                else          yield return new WaitForSeconds(interval);
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
                action?.Invoke();
                count++;
            }
        }

        /// <summary>
<<<<<<< HEAD
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
=======
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
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
            }
            action?.Invoke();
        }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
