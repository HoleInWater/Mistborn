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

public class FlareManager : MonoBehaviour
{
    [Header("Dependencies")]
    public MetalReserve metalReserve; // Drag your MetalReserve object here!

    [Header("Flare Settings")]
    public float flareBurnRate = 10f; // Amount of metal drained per second while flaring
    public bool IsFlaring { get; private set; }
    public static FlareManager Instance { get; private set; }
    public bool IsIronFlaring => IsFlaring;
    public bool IsSteelFlaring => IsFlaring;

    void Update()
    {
        // Example logic: Toggle flare with the 'F' key
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlare();
        }

        // DRAIN LOGIC
        if (IsFlaring && metalReserve != null)
        {
            // Drain the reserve over time
            metalReserve.Drain(flareBurnRate * Time.deltaTime);

            // Auto-stop flaring if we run out of metal
            if (metalReserve.currentMetal <= 0)
            {
                StopFlaring();
            }
        }
    }

    public void ToggleFlare()
    {
        if (IsFlaring) StopFlaring();
        else StartFlaring();
    }

    private void StartFlaring()
    {
        if (metalReserve != null && metalReserve.currentMetal > 0)
        {
            IsFlaring = true;
            Debug.Log("Metal Flare Started!");
        }
    }

    private void StopFlaring()
    {
        IsFlaring = false;
        Debug.Log("Metal Flare Stopped.");
    }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        }
    }
}
