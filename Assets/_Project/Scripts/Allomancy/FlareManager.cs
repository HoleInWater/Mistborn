/* FlareManager.cs
 * 
 * PURPOSE:
 * Centralized manager for Allomantic flaring. Tracks flare state for each metal independently.
 * This allows the player to flare Iron without flaring Steel, and vice versa.
 * 
 * USAGE:
 * =====
 * From any script, check or set flare state for a specific metal:
 *   - FlareManager.Instance.IsIronFlaring
 *   - FlareManager.Instance.IsSteelFlaring
 *   - FlareManager.Instance.ToggleIronFlare()
 *   - FlareManager.Instance.ToggleSteelFlare()
 * 
 * CONTROL SCHEME:
 * ===============
 * Each metal (Iron/Steel) has its own independent flare toggle:
 * 
 * IRON (Q key):
 *   - Q press while NOT burning → Start burning Iron
 *   - Q press while burning, NOT flared → Toggle flare ON
 *   - Q press while burning, flared → Execute pull
 *   - Q release → Stop burning (preserve flare state)
 * 
 * STEEL (E key):
 *   - E press while NOT burning → Start burning Steel  
 *   - E press while burning, NOT flared → Toggle flare ON
 *   - E press while burning, flared → Execute push
 *   - E release → Stop burning (preserve flare state)
 * 
 * CTRL KEY:
 *   - Ctrl press → Toggles BOTH Iron and Steel flare at the same time
 *   - This is for when you want to flare everything (emergency situation)
 * 
 * EVENTS:
 * =======
 * Subscribe to flare changes:
 *   FlareManager.Instance.OnIronFlareChanged += (isFlaring) => { ... };
 *   FlareManager.Instance.OnSteelFlareChanged += (isFlaring) => { ... };
 * 
 *   Or all flares at once:
 *   FlareManager.Instance.OnAnyFlareChanged += (metal, isFlaring) => { ... };
 */

using UnityEngine;
using System;

public class FlareManager : MonoBehaviour
{
    public static FlareManager Instance { get; private set; }

    // Events for when flare state changes
    public event Action<bool> OnIronFlareChanged;
    public event Action<bool> OnSteelFlareChanged;
    public event Action<MetalType, bool> OnAnyFlareChanged;
    
    public enum MetalType { Iron, Steel }

    [Header("Settings")]
    [Tooltip("Key to toggle flaring for both metals at once")]
    public KeyCode globalFlareKey = KeyCode.LeftControl;
    [Tooltip("Enable debug logging")]
    public bool debugMode = true;

    // Per-metal flare states
    private bool _isIronFlaring = false;
    public bool IsIronFlaring
    {
        get => _isIronFlaring;
        private set
        {
            if (_isIronFlaring != value)
            {
                _isIronFlaring = value;
                OnIronFlareChanged?.Invoke(_isIronFlaring);
                OnAnyFlareChanged?.Invoke(MetalType.Iron, _isIronFlaring);
                if (debugMode) Debug.Log($"[FLARE] Iron: {(_isIronFlaring ? "FLARED" : "OFF")}");
            }
        }
    }

    private bool _isSteelFlaring = false;
    public bool IsSteelFlaring
    {
        get => _isSteelFlaring;
        private set
        {
            if (_isSteelFlaring != value)
            {
                _isSteelFlaring = value;
                OnSteelFlareChanged?.Invoke(_isSteelFlaring);
                OnAnyFlareChanged?.Invoke(MetalType.Steel, _isSteelFlaring);
                if (debugMode) Debug.Log($"[FLARE] Steel: {(_isSteelFlaring ? "FLARED" : "OFF")}");
            }
        }
    }

    void Awake()
    {
        // Singleton pattern - only one FlareManager allowed
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        // GLOBAL FLARE: Ctrl toggles BOTH Iron and Steel flares
        // This is for "flare everything" moments in combat
        if (Input.GetKeyDown(globalFlareKey))
        {
            bool newState = !IsIronFlaring; // Toggle based on Iron state
            IsIronFlaring = newState;
            IsSteelFlaring = newState;
            Debug.Log($"[FLARE] GLOBAL FLARE: {(newState ? "ON" : "OFF")}");
        }
    }

    // Public methods for scripts to toggle individual metal flares
    public void ToggleIronFlare()
    {
        IsIronFlaring = !IsIronFlaring;
    }

    public void ToggleSteelFlare()
    {
        IsSteelFlaring = !IsSteelFlaring;
    }

    public void SetIronFlare(bool value)
    {
        IsIronFlaring = value;
    }

    public void SetSteelFlare(bool value)
    {
        IsSteelFlaring = value;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
