/// <summary>
/// Controls HUD display for Health, Stamina, and Metal reserves.
/// Usage: HUDController hud = FindObjectOfType<HUDController>();
/// </summary>
public class HUDController : MonoBehaviour
{
    // UI REFERENCES - Assign in Inspector
    public UnityEngine.UI.Image healthBarFill;       // Health bar fill image
    public UnityEngine.UI.Text healthText;           // Health text display
    public UnityEngine.UI.Image staminaBarFill;       // Stamina bar fill image
    public UnityEngine.UI.Text staminaText;           // Stamina text display
    public UnityEngine.UI.Image[] metalBars;          // Array of metal bar fills
    public UnityEngine.UI.Text[] metalTexts;          // Array of metal text displays
    
    // INTERNAL REFERENCES
    private Health health;                            // Reference to Health system
    private Stamina stamina;                          // Reference to Stamina system
    private MetalReserveManager metals;               // Reference to Metal system
    
    // PUBLIC API
    public void UpdateHealthUI(float current, float max)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = current / max;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
        }
    }
    
    public void UpdateStaminaUI(float current, float max)
    {
        if (staminaBarFill != null)
        {
            staminaBarFill.fillAmount = current / max;
        }
        
        if (staminaText != null)
        {
            staminaText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
        }
    }
    
    void Start()
    {
        health = FindObjectOfType<Health>();
        stamina = FindObjectOfType<Stamina>();
        metals = FindObjectOfType<MetalReserveManager>();
        
        if (health != null)
        {
            health.OnHealthChanged += UpdateHealthUI;
        }
        
        if (stamina != null)
        {
            stamina.OnStaminaChanged += UpdateStaminaUI;
        }
    }
    
    void Update()
    {
        if (health != null && healthBarFill != null)
        {
            healthBarFill.fillAmount = health.NormalizedHealth;
        }
        
        if (stamina != null && staminaBarFill != null)
        {
            staminaBarFill.fillAmount = stamina.NormalizedStamina;
        }
    }
    
    void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged -= UpdateHealthUI;
        }
        
        if (stamina != null)
        {
            stamina.OnStaminaChanged -= UpdateStaminaUI;
        }
    }
}
