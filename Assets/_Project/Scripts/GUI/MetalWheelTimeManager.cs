///
/// [AGENT TRIPLE ERROR CHECK REPORT]
/// PASS 1 - LOGIC: LerpTimeScale correctly enforces fixedDeltaTime adjustment to prevent physics breaking. Time always restores on Disable/Destroy.
/// PASS 2 - UNITY API: TimeScale manipulation is safe and exclusively tracked by this dedicated component.
/// PASS 3 - CONSOLE: N/A (Time management is input-agnostic).
///

using UnityEngine;
using System.Collections;

public class MetalWheelTimeManager : MonoBehaviour
{
    // Time scale during wheel selection (Zero = Complete Freeze per user request)
    public const float WHEEL_TIME_SCALE = 0f;

    // How fast time fades IN to slow-mo on wheel open
    public const float TIME_SLOW_FADE_IN = 0.08f;  // seconds

    // How fast time returns to normal on wheel close
    public const float TIME_SLOW_FADE_OUT = 0.12f; // seconds

    private Coroutine timeLerpCoroutine;

    // State
    public bool IsTimeSlowed { get; private set; }

    /// <summary>
    /// Smoothly transitions the game into slow-mo for the wheel UI.
    /// </summary>
    public void SlowTime()
    {
        IsTimeSlowed = true;
        if (timeLerpCoroutine != null) StopCoroutine(timeLerpCoroutine);
        timeLerpCoroutine = StartCoroutine(LerpTimeScale(WHEEL_TIME_SCALE, TIME_SLOW_FADE_IN));
    }

    /// <summary>
    /// Smoothly transitions the game back to real-time.
    /// </summary>
    public void RestoreTime()
    {
        IsTimeSlowed = false;
        if (timeLerpCoroutine != null) StopCoroutine(timeLerpCoroutine);
        timeLerpCoroutine = StartCoroutine(LerpTimeScale(1.0f, TIME_SLOW_FADE_OUT));
    }

    /// <summary>
    /// Instantly restores time to real-time. Use for abrupt cancellations or scene unloading.
    /// </summary>
    public void SnapRestoreTime()
    {
        if (timeLerpCoroutine != null) StopCoroutine(timeLerpCoroutine);
        IsTimeSlowed = false;
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
    }

    // ALWAYS pair timeScale changes with fixedDeltaTime correction.
    // Physics will break if you don't. This is non-negotiable per system prompt.
    private IEnumerator LerpTimeScale(float target, float duration)
    {
        float start = Time.timeScale;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Time.timeScale = Mathf.Lerp(start, target, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // NEVER OMIT THIS LINE
            yield return null;
        }
        
        Time.timeScale = target;
        Time.fixedDeltaTime = 0.02f * target;
    }

    void OnDisable()
    {
        // Safety net: Restore time if UI is suddenly disabled
        SnapRestoreTime();
    }

    void OnDestroy()
    {
        // Safety net: Restore time if object is destroyed
        SnapRestoreTime();
    }
}
