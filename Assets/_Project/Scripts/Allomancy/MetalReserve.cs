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
    public float metalFlareRecovery = 25f;

    private ProgressBar _metalBar;

    void OnEnable()
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
        // Passive recovery over time
        currentMetal = Mathf.Min(maxMetal, currentMetal + passiveRecoveryRate * Time.deltaTime);
        
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if (_metalBar != null)
        {
            _metalBar.value = currentMetal;
            // Display as "Metal: 75 / 100"
            _metalBar.title = $"Metal: {Mathf.FloorToInt(currentMetal)} / {maxMetal}";
        }
    }

    // Public methods for other scripts to call
    public void Drain(float amount)
    {
        currentMetal = Mathf.Max(0, currentMetal - amount);
    }

    public void Refill(float amount)
    {
        currentMetal = Mathf.Min(maxMetal, currentMetal + amount);
    }

    public void MetalFlare()
    {
        Refill(metalFlareRecovery);
    }

    public void PurgeAll()
    {
        currentMetal = 0f;
    }
}
