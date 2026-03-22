using UnityEngine;
using UnityEngine.UIElements;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float drainRate = 20f;   // Amount lost per second while sprinting
    public float regenRate = 15f;   // Amount gained per second while resting

    [Header("UI References")]
    public UIDocument uiDocument;
    private ProgressBar staminaBar;

    private bool isSprinting;

    void Start()
    {
        currentStamina = maxStamina;

        // Find the progress bar named "stanima" from your UI Toolkit document
        var root = uiDocument.rootVisualElement;
        staminaBar = root.Q<ProgressBar>("stanima");

        if (staminaBar != null)
        {
            staminaBar.highValue = maxStamina;
            staminaBar.value = currentStamina;
        }
    }

    void Update()
    {
        // Detect sprinting (using Shift key as an example)
        isSprinting = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0;

        if (isSprinting)
        {
            // Drain stamina over time
            currentStamina -= drainRate * Time.deltaTime;
        }
        else if (currentStamina < maxStamina)
        {
            // Regenerate stamina when not sprinting
            currentStamina += regenRate * Time.deltaTime;
        }

        // Clamp value so it stays between 0 and Max
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        // Update the UI Toolkit ProgressBar
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina;
            // Optional: Update title text to show percentage
            staminaBar.title = $"Stamina: {(int)currentStamina}%";
        }
    }
}
