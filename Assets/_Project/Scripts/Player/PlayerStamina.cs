// NOTE: Line 43 contains Debug.Log which should be removed for production
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerStamina : MonoBehaviour
{
    // NOTE: Consider adding [Range(1f, 500f)] attribute for maxStamina
    public float maxStamina = 100f;
    // NOTE: Consider adding [SerializeField] for currentStamina (private field)
    public float currentStamina;
    // NOTE: Consider adding [Range(1f, 100f)] attribute for regenRate
    public float regenRate = 15f;

    public UIDocument uiDocument;
    public float jumpCost = 15f; // How much a jump costs
    private ProgressBar staminaBar;

    void Start()
    {
        currentStamina = maxStamina;
        var root = uiDocument.rootVisualElement;
        staminaBar = root.Q<ProgressBar>("Stamina"); // Fixed typo: was "stanima"

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
    
    // Keep your existing DrainStamina for sprinting (per second)
    public void DrainStamina(float amount)
    {
        currentStamina -= amount * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }
    
    // Add this for instant costs (like jumping)
    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }
}
