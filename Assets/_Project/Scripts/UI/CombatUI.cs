using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Combat HUD: health bar, stamina bar, metal reserve indicator, combo counter, boss HP.
/// </summary>
public class CombatUI : MonoBehaviour
{
    [Header("Health")]
    public Image healthBarFill;
    public Text healthText;

    [Header("Stamina")]
    public Image staminaBarFill;

    [Header("Metal Reserve")]
    public Image metalBarFill;
    public Text metalNameText;

    [Header("Combo")]
    public Text comboCountText;
    public float comboDisplayDuration = 2f;

    [Header("Boss")]
    public GameObject bossHealthPanel;
    public Image bossHealthBarFill;
    public Text bossNameText;

    [Header("Crosshair")]
    public Image crosshairImage;
    public Color defaultCrosshairColor = Color.white;
    public Color metalTargetColor = Color.cyan;
    public Color enemyTargetColor = Color.red;

    private float comboTimer;
    private PlayerHealth playerHealth;
    private PlayerStamina playerStamina;
    private Allomancer allomancer;
    private ComboSystem comboSystem;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerStamina = player.GetComponent<PlayerStamina>();
            allomancer = player.GetComponent<Allomancer>();
            comboSystem = player.GetComponent<ComboSystem>();
        }

        if (bossHealthPanel != null) bossHealthPanel.SetActive(false);
    }

    void Update()
    {
        UpdateHealthBar();
        UpdateStaminaBar();
        UpdateMetalBar();
        UpdateCombo();
    }

    void UpdateHealthBar()
    {
        if (playerHealth == null || healthBarFill == null) return;
<<<<<<< HEAD
        healthBarFill.fillAmount = playerHealth.GetCurrentHealth() / playerHealth.GetMaxHealth();
=======
        float maxHealth = playerHealth.GetMaxHealth();
        if (maxHealth <= 0f) return;
        healthBarFill.fillAmount = playerHealth.GetCurrentHealth() / maxHealth;
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        if (healthText != null)
            healthText.text = $"{Mathf.RoundToInt(playerHealth.GetCurrentHealth())}";
    }

    void UpdateStaminaBar()
    {
        if (playerStamina == null || staminaBarFill == null) return;
<<<<<<< HEAD
=======
        if (playerStamina.maxStamina <= 0f) return;
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        staminaBarFill.fillAmount = playerStamina.GetCurrentStamina() / playerStamina.maxStamina;
    }

    void UpdateMetalBar()
    {
        if (allomancer == null || metalBarFill == null) return;
        var metal = allomancer.GetCurrentMetal();
        float reserve = allomancer.GetMetalReserve(metal);
        metalBarFill.fillAmount = reserve / 100f;
        if (metalNameText != null) metalNameText.text = metal.ToString();
    }

    void UpdateCombo()
    {
        if (comboSystem == null || comboCountText == null) return;
        int combo = comboSystem.CurrentCombo;
        if (combo > 1)
        {
            comboCountText.text = $"{combo}x";
            comboCountText.gameObject.SetActive(true);
            comboTimer = comboDisplayDuration;
        }
        else
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                comboCountText.gameObject.SetActive(false);
        }
    }

    // Boss health bar
    public void ShowBossHealth(string bossName, float healthPercent)
    {
        if (bossHealthPanel == null) return;
        bossHealthPanel.SetActive(true);
        if (bossNameText != null) bossNameText.text = bossName;
        if (bossHealthBarFill != null) bossHealthBarFill.fillAmount = healthPercent;
    }

    public void HideBossHealth()
    {
        if (bossHealthPanel != null) bossHealthPanel.SetActive(false);
    }

    public void SetCrosshairColor(Color color)
    {
        if (crosshairImage != null) crosshairImage.color = color;
    }
}
