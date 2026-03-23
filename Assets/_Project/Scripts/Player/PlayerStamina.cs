// NOTE: Line 43 contains Debug.Log which should be removed for production
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float regenRate = 15f;
    public float regenDelay = 2f; // The 2-second wait time

    [Header("UI")]
    public UIDocument uiDocument;
    private ProgressBar staminaBar;

    private float regenTimer; // Internal countdown

    void Start()
    {
        currentStamina = maxStamina;
        var root = uiDocument.rootVisualElement;
        staminaBar = root.Q<ProgressBar>("Stamina");

        if (staminaBar != null)
            staminaBar.highValue = maxStamina;
    }

    void Update()
    {
        // Update UI
        if (staminaBar != null)
            staminaBar.value = currentStamina;

        // Count down the delay timer
        if (regenTimer > 0)
        {
            regenTimer -= Time.deltaTime;
        }
        // Only regenerate if the timer is finished and stamina isn't full
        else if (currentStamina < maxStamina)
        {
            currentStamina += regenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }
    }

    // Call this for continuous costs (Sprinting)
    public void DrainStamina(float amount)
    {
        currentStamina -= amount * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        
        // Reset the timer every frame stamina is being used
        regenTimer = regenDelay; 
    }

    // Call this for instant costs (Jumping)
    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        
        // Reset the timer once
        regenTimer = regenDelay;
    }
}
