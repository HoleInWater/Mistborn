using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCollisionHandler3D : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument; // We will fix the drag-and-drop for this below
    public string progressBarName = "Health"; // Matches your UXML name

    [Header("Sprites")]
    public Sprite healthySprite;
    public Sprite damagedSprite;

    private VisualElement _progressFill;
    private ProgressBar _progressBar;

    void OnEnable()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument is missing! Drag the object with the UIDocument component into the slot on the Player.");
            return;
        }

        var root = uiDocument.rootVisualElement;
        _progressBar = root.Q<ProgressBar>(progressBarName);
        
        if (_progressBar != null)
        {
            // This finds the inner "bar" using the USS class we discussed
            _progressFill = _progressBar.Q(className: "unity-progress-bar__progress");
            Debug.Log("Successfully found the Health bar!");
        }
        else
        {
            Debug.LogError($"Could not find a ProgressBar named '{progressBarName}' in the UXML.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Hit Enemy!");
            TriggerDamageTransition();
        }
    }

    void TriggerDamageTransition()
    {
        _progressBar.value -= 20; // Subtract more to make it obvious
        
        if (damagedSprite != null)
        {
            // 1. Clear any default Unity colors
            _progressFill.style.backgroundColor = Color.clear; 
            
            // 2. Set the sprite
            _progressFill.style.backgroundImage = new StyleBackground(damagedSprite);
            
            // 3. Ensure it stretches to the fill area
            _progressFill.style.unityBackgroundScaleMode = ScaleMode.StretchToFill;
            
            Debug.Log($"Bar Value is now: {_progressBar.value}");
        }
    }
}
