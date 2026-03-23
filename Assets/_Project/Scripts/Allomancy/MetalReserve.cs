using UnityEngine;
using UnityEngine.UIElements;

public class MetalReserve : MonoBehaviour
{
    public UIDocument uiDocument;
    public string metalProgressBarName = "Metal";

    public float currentMetal = 50f; // Start at 50 to see if it moves to 100
    public float maxMetal = 100f;
    public float passiveRecoveryRate = 5f; // Faster for testing

    private ProgressBar _metalBar;

    void OnEnable()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (uiDocument == null)
        {
            Debug.LogError("MetalReserve: UIDocument is missing from the Inspector!");
            return;
        }

        var root = uiDocument.rootVisualElement;
        _metalBar = root.Q<ProgressBar>(metalProgressBarName);

        if (_metalBar != null)
        {
            Debug.Log($"MetalReserve: Found ProgressBar '{metalProgressBarName}'!");
            _metalBar.lowValue = 0;
            _metalBar.highValue = maxMetal;
        }
        else
        {
            Debug.LogError($"MetalReserve: Could NOT find a ProgressBar named '{metalProgressBarName}'. Check your UXML names.");
        }
    }

    void Update()
    {
        // If it wasn't found at start, keep trying (in case UI loads late)
        if (_metalBar == null) InitializeUI();

        if (currentMetal < maxMetal)
        {
            currentMetal += passiveRecoveryRate * Time.deltaTime;
        }

        if (_metalBar != null)
        {
            _metalBar.value = currentMetal;
            _metalBar.title = $"Metal: {Mathf.FloorToInt(currentMetal)} / {maxMetal}";
        }
    }
}
