using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CombatUI : MonoBehaviour
{
    [Header("Health")]
    public Slider healthBar;
    public Image healthFill;
    public Image healthDamageFlash;
    public TextMeshProUGUI healthText;
    public Color healthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public float lowHealthThreshold = 0.3f;

    [Header("Stamina")]
    public Slider staminaBar;
    public Image staminaFill;
    public TextMeshProUGUI staminaText;
    public Color staminaColor = new Color(1, 0.8f, 0);
    public Color exhaustedColor = new Color(0.5f, 0.5f, 0);

    [Header("Metal Reserves")]
    public Image[] metalBars;
    public Image activeMetalIndicator;
    public TextMeshProUGUI metalNameText;
    public TextMeshProUGUI metalAmountText;

    [Header("Flare Indicator")]
    public Image flareRing;
    public Image flareIntensityFill;
    public TextMeshProUGUI flareLevelText;
    public Color flareColor = Color.yellow;
    public Color notFlaringColor = Color.gray;

    [Header("Combo")]
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI comboMultiplierText;
    public GameObject comboPanel;
    public float comboTimeout = 2f;
    public Animator comboAnimator;

    private int currentCombo = 0;
    private float comboTimer = 0f;
    private bool isComboActive = false;

    void Update()
    {
        UpdateCombo();
    }

    public void UpdateHealth(float current, float max)
    {
        float percent = Mathf.Clamp01(current / max);

        if (healthBar != null) healthBar.value = percent;
        if (healthFill != null)
        {
            healthFill.color = percent <= lowHealthThreshold ? lowHealthColor : healthColor;
        }
        if (healthText != null) healthText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
    }

    public void UpdateStamina(float current, float max)
    {
        float percent = Mathf.Clamp01(current / max);

        if (staminaBar != null) staminaBar.value = percent;
        if (staminaFill != null)
        {
            staminaFill.color = percent <= 0.1f ? exhaustedColor : staminaColor;
        }
        if (staminaText != null) staminaText.text = $"{Mathf.RoundToInt(current)}";
    }

    public void UpdateMetalReserve(AllomancySkill.MetalType metal, float current, float max)
    {
        int index = (int)metal;
        if (index >= 0 && index < metalBars.Length && metalBars[index] != null)
        {
            metalBars[index].fillAmount = Mathf.Clamp01(current / max);
        }

        if (metalNameText != null) metalNameText.text = metal.ToString();
        if (metalAmountText != null) metalAmountText.text = $"{Mathf.RoundToInt(current)}";
    }

    public void UpdateAllMetalReserves(float[] reserves)
    {
        for (int i = 0; i < reserves.Length && i < metalBars.Length; i++)
        {
            if (metalBars[i] != null)
            {
                metalBars[i].fillAmount = Mathf.Clamp01(reserves[i] / 100f);
            }
        }
    }

    public void SetActiveMetal(AllomancySkill.MetalType metal)
    {
        if (activeMetalIndicator != null)
        {
            activeMetalIndicator.fillAmount = 1f;
        }
    }

    public void UpdateFlare(int intensity, bool isFlaring)
    {
        if (flareIntensityFill != null)
        {
            flareIntensityFill.fillAmount = intensity / 10f;
            flareIntensityFill.color = isFlaring ? flareColor : notFlaringColor;
        }

        if (flareRing != null)
        {
            flareRing.color = isFlaring ? flareColor : notFlaringColor;
        }

        if (flareLevelText != null)
        {
            flareLevelText.text = isFlaring ? $"Flare x{intensity}" : "";
        }
    }

    public void AddCombo(int damage)
    {
        currentCombo++;
        comboTimer = comboTimeout;
        isComboActive = true;

        if (comboPanel != null) comboPanel.SetActive(true);

        if (comboText != null)
        {
            comboText.text = $"{currentCombo} HITS";
        }

        if (comboMultiplierText != null)
        {
            float multiplier = 1f + (currentCombo * 0.1f);
            comboMultiplierText.text = $"x{multiplier:F1}";
        }

        if (comboAnimator != null)
        {
            comboAnimator.SetTrigger("Hit");
        }

        AchievementSystem.Instance?.UpdateStat("combo_hits", 1);
    }

    void UpdateCombo()
    {
        if (!isComboActive) return;

        comboTimer -= Time.deltaTime;

        if (comboTimer <= 0)
        {
            ResetCombo();
        }
    }

    void ResetCombo()
    {
        currentCombo = 0;
        isComboActive = false;

        if (comboPanel != null) comboPanel.SetActive(false);
    }

    public void ShowHealthDamage()
    {
        if (healthDamageFlash != null)
        {
            StartCoroutine(DamageFlash(healthDamageFlash));
        }
    }

    IEnumerator DamageFlash(Image image)
    {
        Color original = image.color;
        image.color = new Color(1, 0, 0, 0.5f);

        float elapsed = 0;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            image.color = Color.Lerp(image.color, original, elapsed / 0.3f);
            yield return null;
        }
    }

    public void ShowCombatIndicator(Vector3 worldPosition)
    {
        // Show direction indicator for incoming damage
    }
}

public class HealthBarTransition : MonoBehaviour
{
    [Header("Settings")]
    public float transitionSpeed = 5f;
    public float flashDuration = 0.2f;
    public Color flashColor = Color.red;
    public Color normalColor = Color.white;

    [Header("References")]
    public Image barFill;
    public Image damageFill;
    public Slider healthSlider;

    private float targetHealth;
    private float displayedHealth;
    private bool isFlashing = false;

    void Start()
    {
        if (healthSlider != null)
        {
            targetHealth = healthSlider.value;
            displayedHealth = targetHealth;
        }
    }

    void Update()
    {
        if (healthSlider != null && damageFill != null)
        {
            displayedHealth = Mathf.Lerp(displayedHealth, targetHealth, Time.deltaTime * transitionSpeed);
            damageFill.fillAmount = displayedHealth;
        }
    }

    public void SetHealth(float health, float maxHealth)
    {
        float newTarget = health / maxHealth;

        if (newTarget < targetHealth)
        {
            StartCoroutine(DamageFlashEffect());
        }

        targetHealth = newTarget;

        if (healthSlider != null) healthSlider.value = targetHealth;
        if (barFill != null) barFill.fillAmount = targetHealth;
    }

    IEnumerator DamageFlashEffect()
    {
        isFlashing = true;

        if (barFill != null)
        {
            Color original = barFill.color;
            barFill.color = flashColor;

            yield return new WaitForSeconds(flashDuration);

            barFill.color = original;
        }

        isFlashing = false;
    }

    public void SetColor(Color color)
    {
        if (barFill != null) barFill.color = color;
    }
}

public class DamageNumbers : MonoBehaviour
{
    public static DamageNumbers Instance { get; private set; }

    [Header("Settings")]
    public GameObject damageNumberPrefab;
    public float floatSpeed = 2f;
    public float fadeTime = 1f;
    public float scaleSpeed = 0.5f;
    public Vector3 spawnOffset = new Vector3(0, 1.5f, 0);

    [Header("Colors")]
    public Color normalDamageColor = Color.white;
    public Color criticalDamageColor = Color.red;
    public Color healingColor = Color.green;
    public Color poisonColor = Color.magenta;

    [Header("Pool")]
    public int poolSize = 20;
    private Queue<GameObject> damagePool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    void Start()
    {
        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = CreateDamageNumber();
            obj.SetActive(false);
            damagePool.Enqueue(obj);
        }
    }

    GameObject CreateDamageNumber()
    {
        if (damageNumberPrefab != null)
        {
            return Instantiate(damageNumberPrefab, transform);
        }

        GameObject obj = new GameObject("DamageNumber");
        obj.transform.SetParent(transform);

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;

        return obj;
    }

    public void ShowDamage(Vector3 position, float damage, bool isCritical = false)
    {
        GameObject obj = GetFromPool();
        if (obj == null) return;

        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = isCritical ? $"{Mathf.RoundToInt(damage)}!" : $"{Mathf.RoundToInt(damage)}";
            text.color = isCritical ? criticalDamageColor : normalDamageColor;
        }

        obj.transform.position = position + spawnOffset;
        obj.SetActive(true);

        StartCoroutine(FloatAndFade(obj, text));
    }

    public void ShowHealing(Vector3 position, float amount)
    {
        GameObject obj = GetFromPool();
        if (obj == null) return;

        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = $"+{Mathf.RoundToInt(amount)}";
            text.color = healingColor;
        }

        obj.transform.position = position + spawnOffset;
        obj.SetActive(true);

        StartCoroutine(FloatAndFade(obj, text));
    }

    public void ShowPoisonDamage(Vector3 position, float damage)
    {
        GameObject obj = GetFromPool();
        if (obj == null) return;

        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = $"-{Mathf.RoundToInt(damage)}";
            text.color = poisonColor;
        }

        obj.transform.position = position + spawnOffset;
        obj.SetActive(true);

        StartCoroutine(FloatAndFade(obj, text));
    }

    IEnumerator FloatAndFade(GameObject obj, TextMeshProUGUI text)
    {
        float elapsed = 0;
        Vector3 startPos = obj.transform.position;
        Color startColor = text.color;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;

            obj.transform.position = startPos + Vector3.up * (floatSpeed * t);
            text.color = Color.Lerp(startColor, Color.clear, t);

            obj.transform.localScale = Vector3.one * (1 + t * scaleSpeed);

            yield return null;
        }

        ReturnToPool(obj);
    }

    GameObject GetFromPool()
    {
        if (damagePool.Count > 0)
        {
            GameObject obj = damagePool.Dequeue();
            obj.transform.localScale = Vector3.one;
            return obj;
        }

        return CreateDamageNumber();
    }

    void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        damagePool.Enqueue(obj);
    }
}

public class BossHealthBar : MonoBehaviour
{
    [Header("UI")]
    public Slider healthBar;
    public Image healthFill;
    public Image healthDamageFlash;
    public TextMeshProUGUI bossNameText;
    public GameObject bossBarPanel;

    [Header("Settings")]
    public float transitionSpeed = 3f;
    public Color flashColor = Color.red;
    public float flashDuration = 0.2f;

    private float targetHealth = 1f;
    private float displayedHealth = 1f;
    private bool isActive = false;

    void Update()
    {
        if (!isActive) return;

        displayedHealth = Mathf.Lerp(displayedHealth, targetHealth, Time.deltaTime * transitionSpeed);
        if (healthDamageFlash != null) healthDamageFlash.fillAmount = displayedHealth;
    }

    public void Show(string bossName, float health, float maxHealth)
    {
        isActive = true;
        if (bossBarPanel != null) bossBarPanel.SetActive(true);
        if (bossNameText != null) bossNameText.text = bossName;

        targetHealth = health / maxHealth;
        displayedHealth = targetHealth;

        if (healthBar != null) healthBar.value = targetHealth;
        if (healthFill != null) healthFill.fillAmount = targetHealth;
    }

    public void UpdateHealth(float health, float maxHealth)
    {
        float newTarget = health / maxHealth;

        if (newTarget < targetHealth)
        {
            StartCoroutine(DamageFlash());
        }

        targetHealth = newTarget;
        if (healthBar != null) healthBar.value = targetHealth;
        if (healthFill != null) healthFill.fillAmount = targetHealth;
    }

    public void Hide()
    {
        isActive = false;
        if (bossBarPanel != null) bossBarPanel.SetActive(false);
    }

    IEnumerator DamageFlash()
    {
        if (healthFill != null)
        {
            Color original = healthFill.color;
            healthFill.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            healthFill.color = original;
        }
    }
}