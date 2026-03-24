using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central hub for controlling the game's time flow. 
/// Prevents multiple Allomantic effects from clashing over Time.timeScale.
/// </summary>
public class MistbornTimeManager : MonoBehaviour
{
    public static MistbornTimeManager Instance { get; private set; }

    private float atiumModifier = 1f;
    private List<float> activeBubbleModifiers = new List<float>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void SetAtiumModifier(float val) => atiumModifier = val;
    
    public void RegisterBubbleModifier(float val) => activeBubbleModifiers.Add(val);
    public void UnregisterBubbleModifier(float val) => activeBubbleModifiers.Remove(val);

    void Update()
    {
        float targetTimeScale = atiumModifier;
        
        // Multiplicative blending for bubbles (simplified for this demo)
        foreach (var mod in activeBubbleModifiers)
        {
            targetTimeScale *= mod;
        }

        // Apply with lerp for smoothness
        Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, Time.unscaledDeltaTime * 5f);
        
        // Ensure fixedDeltaTime stays in sync with physics
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}
