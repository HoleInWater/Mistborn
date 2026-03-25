using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central time controller. Prevents Atium, time bubbles, pause, and
/// Duralumin effects from clashing over Time.timeScale.
/// Priority: Pause > Atium > Bubbles > Normal
/// </summary>
public class MistbornTimeManager : MonoBehaviour
{
    public static MistbornTimeManager Instance { get; private set; }

    [Header("Debug")]
    public float currentEffectiveTimeScale = 1f;

    private float atiumModifier = 1f;
    private float pauseModifier = 1f;
    private float metalWheelModifier = 1f;
    private List<float> activeBubbleModifiers = new List<float>();
    private bool isPaused = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
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
            target = atiumModifier * metalWheelModifier;

            foreach (var mod in activeBubbleModifiers)
                target *= mod;
        }

        // Smooth transition
        float next = Mathf.Lerp(Time.timeScale, target, Time.unscaledDeltaTime * 8f);
        Time.timeScale = Mathf.Clamp(next, 0.01f, 12f);
        Time.fixedDeltaTime = Mathf.Max(0.0002f, 0.02f * Time.timeScale);

        currentEffectiveTimeScale = Time.timeScale;
    }

    // ── Modifiers ────────────────────────────────────────────────────────

    public void SetAtiumModifier(float val) => atiumModifier = Mathf.Clamp(val, 0.1f, 1f);
    public void ClearAtiumModifier() => atiumModifier = 1f;

    public void SetMetalWheelModifier(float val) => metalWheelModifier = Mathf.Clamp(val, 0.1f, 1f);
    public void ClearMetalWheelModifier() => metalWheelModifier = 1f;

    public void RegisterBubbleModifier(float val) => activeBubbleModifiers.Add(val);
    public void UnregisterBubbleModifier(float val) => activeBubbleModifiers.Remove(val);

    public void SetPaused(bool paused) => isPaused = paused;

    /// <summary>
    /// Force time to a specific value (used by cutscenes, Lord Ruler Atium phase).
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
