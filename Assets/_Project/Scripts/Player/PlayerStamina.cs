// NOTE: Line 43 contains Debug.Log which should be removed for production
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
        staminaBar = root.Q<ProgressBar>("stamina"); // Fixed typo: was "stanima"

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            DrainStamina(20f);
            Debug.Log("Manual Drain: " + currentStamina);
        }
    }

    // Function for the Sprint script to call
    public void DrainStamina(float amount)
    {
        currentStamina -= amount * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }
}