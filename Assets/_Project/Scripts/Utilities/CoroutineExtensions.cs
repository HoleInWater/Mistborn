using UnityEngine;
using System.Collections;

namespace MistbornGame.Utilities
{
    public class CoroutineManager : MonoBehaviour
    {
        private static CoroutineManager instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }
        
        public static Coroutine StartStatic(IEnumerator routine)
        {
            if (instance != null)
            {
                return instance.StartCoroutine(routine);
            }
            return null;
        }
        
        public static void StopStatic(IEnumerator routine)
        {
            if (instance != null)
            {
                instance.StopCoroutine(routine);
            }
        }
        
        public static void StopStaticAll()
        {
            if (instance != null)
            {
                instance.StopAllCoroutines();
            }
        }
    }
    
    public class WaitForSecondsUnscaled : CustomYieldInstruction
    {
        private readonly float duration;
        private float startTime;
        
        public WaitForSecondsUnscaled(float seconds)
        {
            duration = seconds;
            startTime = Time.realtimeSinceStartup;
        }
        
        public override bool keepWaiting
        {
            get { return Time.realtimeSinceStartup - startTime < duration; }
        }
    }
    
    public class WaitUntilNotNull<T> where T : class
    {
        private readonly System.Func<T> getValue;
        
        public WaitUntilNotNull(System.Func<T> getValue)
        {
            this.getValue = getValue;
        }
        
        public bool IsReady
        {
            get { return getValue() != null; }
        }
    }
    
    public class WaitForCondition : CustomYieldInstruction
    {
        private readonly System.Func<bool> condition;
        
        public WaitForCondition(System.Func<bool> condition)
        {
            this.condition = condition;
        }
        
        public override bool keepWaiting
        {
            get { return !condition(); }
        }
    }
    
    public static class CoroutineExtensions
    {
        public static Coroutine StartCoroutine(this MonoBehaviour mb, IEnumerator routine, bool useUnscaledTime = false)
        {
            if (useUnscaledTime)
            {
                return mb.StartCoroutine(UnscaledTimeCoroutine(routine));
            }
            return mb.StartCoroutine(routine);
        }
        
        private static IEnumerator UnscaledTimeCoroutine(IEnumerator routine)
        {
            while (routine.MoveNext())
            {
                yield return routine.Current;
            }
        }
        
        public static void Delay(this MonoBehaviour mb, float delay, System.Action action)
        {
            mb.StartCoroutine(DelayCoroutine(delay, action));
        }
        
        private static IEnumerator DelayCoroutine(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
        
        public static void DelayUnscaled(this MonoBehaviour mb, float delay, System.Action action)
        {
            mb.StartCoroutine(DelayUnscaledCoroutine(delay, action));
        }
        
        private static IEnumerator DelayUnscaledCoroutine(float delay, System.Action action)
        {
            yield return new WaitForSecondsUnscaled(delay);
            action?.Invoke();
        }
        
        public static void DelayFrames(this MonoBehaviour mb, int frames, System.Action action)
        {
            mb.StartCoroutine(DelayFramesCoroutine(frames, action));
        }
        
        private static IEnumerator DelayFramesCoroutine(int frames, System.Action action)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }
            action?.Invoke();
        }
        
        public static Coroutine Lerp(this MonoBehaviour mb, float duration, System.Action<float> onUpdate, System.Action onComplete = null)
        {
            return mb.StartCoroutine(LerpCoroutine(duration, onUpdate, onComplete));
        }
        
        private static IEnumerator LerpCoroutine(float duration, System.Action<float> onUpdate, System.Action onComplete)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                onUpdate?.Invoke(t);
                yield return null;
            }
            onComplete?.Invoke();
        }
        
        public static Coroutine LerpUnscaled(this MonoBehaviour mb, float duration, System.Action<float> onUpdate, System.Action onComplete = null)
        {
            return mb.StartCoroutine(LerpUnscaledCoroutine(duration, onUpdate, onComplete));
        }
        
        private static IEnumerator LerpUnscaledCoroutine(float duration, System.Action<float> onUpdate, System.Action onComplete)
        {
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < duration)
            {
                float t = (Time.realtimeSinceStartup - startTime) / duration;
                onUpdate?.Invoke(t);
                yield return null;
            }
            onComplete?.Invoke();
        }
        
        public static Coroutine EveryFrame(this MonoBehaviour mb, int frames, System.Action action)
        {
            return mb.StartCoroutine(EveryFrameCoroutine(frames, action));
        }
        
        private static IEnumerator EveryFrameCoroutine(int frames, System.Action action)
        {
            int count = 0;
            while (true)
            {
                if (count % frames == 0)
                {
                    action?.Invoke();
                }
                count++;
                yield return null;
            }
        }
        
        public static Coroutine While(this MonoBehaviour mb, System.Func<bool> condition, System.Action action)
        {
            return mb.StartCoroutine(WhileCoroutine(condition, action));
        }
        
        private static IEnumerator WhileCoroutine(System.Func<bool> condition, System.Action action)
        {
            while (condition())
            {
                action?.Invoke();
                yield return null;
            }
        }
    }
}
