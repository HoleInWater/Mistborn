using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCollisionHandler3D : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument;
    public string progressBarName = "MyHealthBar";

    [Header("Sprites")]
    public Sprite healthySprite;
    public Sprite damagedSprite;

    private VisualElement _progressFill;
    private ProgressBar _progressBar;

    void OnEnable()
    {
        // Setup UI references
        var root = uiDocument.rootVisualElement;
        _progressBar = root.Q<ProgressBar>(progressBarName);
        
        // Target the internal USS class to change the bar's appearance
        _progressFill = _progressBar.Q(className: "unity-progress-bar__progress");
    }

    // This handles physical collisions in 3D
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TriggerDamageTransition();
        }
    }

    // This handles triggers (ghost-like collisions) in 3D
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            TriggerDamageTransition();
        }
    }

    void TriggerDamageTransition()
    {
        // Reduce the health bar value
        _progressBar.value -= 10; 
        
        // Swap the sprite in UI Toolkit using StyleBackground
        if (damagedSprite != null)
        {
            _progressFill.style.backgroundImage = new StyleBackground(damagedSprite);
        }
        
        Debug.Log("3D Hit Detected! Sprite switched.");
    }
}
