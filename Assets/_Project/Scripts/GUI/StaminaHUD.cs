using UnityEngine;
using UnityEngine.UI;

public class StaminaHUD : MonoBehaviour
{
    [Header("UI References")]
    public Image staminaBar;
    public Text staminaText;
    public GameObject staminaIndicator;
    
    [Header("Settings")]
    public float warningThreshold = 0.3f;
    public Color normalColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color emptyColor = Color.red;
    
    private PlayerStamina stamina;
    
    void Start()
    {
        stamina = FindObjectOfType<PlayerStamina>();
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (stamina == null) return;
        
        float staminaPercent = stamina.GetStaminaPercentage();
        
        if (staminaBar != null)
        {
            staminaBar.fillAmount = staminaPercent;
            
            if (staminaPercent < warningThreshold)
            {
                staminaBar.color = emptyColor;
            }
            else if (staminaPercent < warningThreshold * 2)
            {
                staminaBar.color = warningColor;
            }
            else
            {
                staminaBar.color = normalColor;
            }
        }
        
        if (staminaText != null)
        {
            staminaText.text = $"{(int)(staminaPercent * 100)}%";
        }
        
        if (staminaIndicator != null)
        {
            staminaIndicator.SetActive(!stamina.CanSprint());
        }
    }
}
