using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central time controller. Prevents Oraculum, time bubbles, pause, and
/// Duralumin effects from clashing over Time.timeScale.
/// Priority: Pause > Oraculum > Bubbles > Normal
/// </summary>
public class AshwalkerTimeManager : MonoBehaviour
{
    public static AshwalkerTimeManager Instance { get; private set; }

    [Header("Debug")]
    public float currentEffectiveTimeScale = 1f;

    private float oraculumModifier        = 1f;
    private float pauseModifier        = 1f;
    private float metalWheelModifier   = 1f;
    private float tinPerceptionModifier = 1f;   // Tin: slows world, player speed compensated
    private List<float> activeBubbleModifiers = new List<float>();
    private bool isPaused = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        float target;

        if (isPaused)
        {
            target = 0f;
        }
        else
        {
            target = oraculumModifier * metalWheelModifier * tinPerceptionModifier;

            foreach (var mod in activeBubbleModifiers)
                target *= mod;
        }

        // Smooth transition — allow true 0 when paused, otherwise floor at 0.01
        float next = Mathf.Lerp(Time.timeScale, target, Time.unscaledDeltaTime * 8f);
        float minScale = isPaused ? 0f : 0.01f;
        Time.timeScale = Mathf.Clamp(next, minScale, 12f);

        // fixedDeltaTime controls the physics step rate.
        // Tin perception deliberately excludes physics — it slows animations and
        // Time.deltaTime (visual perception) but must NOT change the physics step
        // frequency, otherwise AddForce calls accumulate faster and the player
        // gains an unintended speed boost.
        // Bubbles and Oraculum DO need to slow physics (objects inside a bubble
        // should genuinely move slower), so they are included here.
        float physicsTarget = oraculumModifier * metalWheelModifier; // intentionally no tinPerceptionModifier
        foreach (var mod in activeBubbleModifiers)
            physicsTarget *= mod;
        physicsTarget = isPaused ? 0f : Mathf.Clamp(physicsTarget, 0.0002f, 12f);
        Time.fixedDeltaTime = Mathf.Max(0.0002f, 0.02f * physicsTarget);

        currentEffectiveTimeScale = Time.timeScale;
    }

    // ── Modifiers ────────────────────────────────────────────────────────

    public void SetOraculumModifier(float val) => oraculumModifier = Mathf.Clamp(val, 0.1f, 1f);
    public void ClearOraculumModifier() => oraculumModifier = 1f;

    /// <summary>
    /// Tin perception dilation — slows the world slightly so the player's enhanced
    /// senses give them a reaction-time advantage without physically speeding them up.
    /// </summary>
    public void SetTinModifier(float val) => tinPerceptionModifier = Mathf.Clamp(val, 0.5f, 1f);
    public void ClearTinModifier()        => tinPerceptionModifier = 1f;

    public void SetMetalWheelModifier(float val) => metalWheelModifier = Mathf.Clamp(val, 0.1f, 1f);
    public void ClearMetalWheelModifier() => metalWheelModifier = 1f;

    public void RegisterBubbleModifier(float val) => activeBubbleModifiers.Add(val);
    public void UnregisterBubbleModifier(float val) => activeBubbleModifiers.Remove(val);

    public void SetPaused(bool paused) => isPaused = paused;

    /// <summary>
    /// Force time to a specific value (used by cutscenes, Ashen King Oraculum phase).
    /// Call ClearForce() to return to normal.
    /// </summary>
    public void ForceTimeScale(float scale)
    {
        Time.timeScale = Mathf.Clamp(scale, 0.01f, 12f);
    }

    public void ClearForce()
    {
        // Next Update() will recalculate from modifiers
    }

    public float GetEffectiveTimeScale() => currentEffectiveTimeScale;
    public bool IsPaused() => isPaused;
}
