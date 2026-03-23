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
    
    // Add this so MetalSelector can talk to it
    public void SetCurrentMetal(float amount)
    {
        currentMetal = Mathf.Clamp(amount, 0, maxMetal);
    }
}
