using UnityEngine;
using UnityEngine.UIElements;

public class PlayerStamina : MonoBehaviour
{
    public float maxStamina = 100f;
    public float currentStamina;
    public float regenRate = 15f;

    public UIDocument uiDocument;
    private ProgressBar staminaBar;

    void Start()
    {
        currentStamina = maxStamina;
        var root = uiDocument.rootVisualElement;
        staminaBar = root.Q<ProgressBar>("stanima");

        if (staminaBar != null)
        {
            staminaBar.highValue = maxStamina;
        }
    }

    void Update()
    {
        // Smoothly update the UI bar every frame
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina;
        }

        // Basic regeneration when not being drained
        if (currentStamina < maxStamina)
        {
            currentStamina += regenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }
    }

    // Function for the Sprint script to call
    public void DrainStamina(float amount)
    {
        currentStamina -= amount * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }
}
