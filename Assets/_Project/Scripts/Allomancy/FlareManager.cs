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
using UnityEngine.UIElements;

public class MetalReserve : MonoBehaviour
{
    [Header("UI Settings")]
    public UIDocument uiDocument;
    public string metalProgressBarName = "Metal";

    [Header("Metal Settings")]
    public float currentMetal = 100f;
    public float maxMetal = 100f;
    
    [Header("Recovery Settings")]
    public float passiveRecoveryRate = 0.5f;

    private ProgressBar _metalBar;
    private float _lastDisplayedMetal = -1f;

    void Start()
    {
        SetupUI();
    }

    private void SetupUI()
    {
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            _metalBar = root.Q<ProgressBar>(metalProgressBarName);

            if (_metalBar != null)
            {
                _metalBar.lowValue = 0;
                _metalBar.highValue = maxMetal;
            }
        }
    }

    void Update()
    {
        // Passive recovery
        if (currentMetal < maxMetal)
        {
            currentMetal = Mathf.MoveTowards(currentMetal, maxMetal, passiveRecoveryRate * Time.deltaTime);
        }
        
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if (_metalBar == null) SetupUI();

        if (_metalBar != null)
        {
            if (!Mathf.Approximately(_lastDisplayedMetal, currentMetal))
            {
                _metalBar.value = currentMetal;
                _metalBar.title = $"Metal: {Mathf.FloorToInt(currentMetal)} / {maxMetal}";
                _lastDisplayedMetal = currentMetal;
            }
        }
    }

    // THESE ARE THE METHODS THE ERROR SAYS ARE MISSING
    public void Drain(float amount)
    {
        currentMetal = Mathf.Max(0, currentMetal - amount);
    }

    public void Refill(float amount)
    {
        currentMetal = Mathf.Min(maxMetal, currentMetal + amount);
    }
}
