using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float regenerationRate = 2f;
    public float regenDelay = 5f;
    
    [Header("UI")]
    public Image healthBar;
    public Text healthText;
    
    [Header("Death")]
    public GameObject deathEffect;
    public bool destroyOnDeath = true;
    
    private float lastDamageTime;
    private bool isDead = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }
    
    void Update()
    {
        if (isDead) return;
        
        if (Time.time - lastDamageTime > regenDelay && currentHealth < maxHealth)
        {
            Regenerate();
        }
    }
    
    public void TakeDamage(float amount)
    {
        if (isDead) return;
        
        currentHealth -= amount;
        lastDamageTime = Time.time;
        
        Debug.Log($"{gameObject.name} took {amount} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
        
        UpdateUI();
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }
    
    void Regenerate()
    {
        currentHealth = Mathf.Min(currentHealth + regenerationRate * Time.deltaTime, maxHealth);
        UpdateUI();
    }
    
    void Die()
    {
        isDead = true;
        currentHealth = 0;
        
        Debug.Log($"{gameObject.name} died!");
        
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        if (destroyOnDeath)
        {
            Destroy(gameObject, 0.1f);
        }
    }
    
    void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)}/{maxHealth}";
        }
    }
    
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}
