/// <summary>
/// Slow motion effect controller.
/// Usage: TimeManager.SlowMotion(0.5f, 3f); // 50% speed for 3 seconds
/// </summary>
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    
    // SETTINGS
    public float defaultTimeScale = 1f;
    
    // STATE
    private float targetTimeScale = 1f;
    private float timeScaleChangeSpeed = 5f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    void Update()
    {
        // Smoothly interpolate to target time scale
        if (Time.timeScale != targetTimeScale)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, timeScaleChangeSpeed * Time.deltaTime);
            
            // Snap when close enough
            if (Mathf.Abs(Time.timeScale - targetTimeScale) < 0.01f)
            {
                Time.timeScale = targetTimeScale;
            }
        }
    }
    
    // Set target time scale
    public static void SetTimeScale(float scale)
    {
        if (Instance != null)
        {
            Instance.targetTimeScale = Mathf.Clamp(scale, 0f, 1f);
        }
    }
    
    // Slow motion for duration
    public static void SlowMotion(float speed, float duration)
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.SlowMotionRoutine(speed, duration));
        }
    }
    
    System.Collections.IEnumerator SlowMotionRoutine(float speed, float duration)
    {
        targetTimeScale = speed;
        yield return new WaitForSeconds(duration);
        targetTimeScale = 1f;
    }
    
    // Freeze time completely
    public static void Freeze()
    {
        SetTimeScale(0f);
    }
    
    // Unfreeze time
    public static void Unfreeze()
    {
        SetTimeScale(1f);
    }
    
    // Pause game (same as Freeze)
    public static void Pause()
    {
        Freeze();
    }
    
    // Resume game
    public static void Resume()
    {
        Unfreeze();
    }
}
