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
        staminaBar = root.Q<ProgressBar>("Stamina");

        if (staminaBar != null)
        {
            staminaBar.highValue = maxStamina;
        }
    }

    void Update()
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina;
        }

        // Regen stamina when not at max
        if (currentStamina < maxStamina)
        {
            currentStamina += regenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }
    }

    // Use this for PER-SECOND costs (Sprinting)
    public void DrainStamina(float amount)
    {
        currentStamina -= amount * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    // Use this for INSTANT costs (Jumping)
    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }
}
