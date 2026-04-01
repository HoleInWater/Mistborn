// NOTE: Lines 29 and 61 contain Debug.Log which should be removed for production
using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarTransitions : MonoBehaviour
{
    public UIDocument uiDocument;
    public string progressBarName = "Health";
    public float damagePerSecondWhileTouching = 10f; // How much health to lose per second while touching the enemy
    // Regen settings
    public float timeNotTouchedEnemy = 0f; // Timer to track how long since last touched enemy
    public float regenDelayAfterTouch = 10f; // seconds to wait before starting regen
    public float regenPerSecond = 5f;
    private bool isTouchingEnemy = false;

    /// <summary>
    /// Multiplier applied to all incoming damage (0–1). Set by Pewter.cs while burning.
    /// 1 = no reduction. 0.5 = half damage. Reset to 1 when Pewter stops.
    /// </summary>
    public float incomingDamageMultiplier = 1f;


    private ProgressBar _progressBar;

    void OnEnable()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;
        _progressBar = root.Q<ProgressBar>(progressBarName);

        if (_progressBar != null)
        {
            // Set the bar to be 100% full at the start
            _progressBar.lowValue = 0;
            _progressBar.highValue = 100;
            _progressBar.value = 100; 
            Debug.Log("Health Bar initialized to 100%");
        }
    }


    void DecreaseHealth(float amount)
    {
        if (_progressBar != null)
        {
            // Subtract the amount and clamp it so it doesn't go below 0
            _progressBar.value = Mathf.Max(_progressBar.value - amount, 0);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Mark that we're touching and decrease health continuously while touching the enemy
            isTouchingEnemy = true;
            timeNotTouchedEnemy = 0f;
            DecreaseHealth(damagePerSecondWhileTouching * incomingDamageMultiplier * Time.deltaTime);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Start counting time since last touch
            isTouchingEnemy = false;
            timeNotTouchedEnemy = 0f;
            Debug.Log("Stopped touching enemy, starting health regeneration timer.");
        }
    }

    void IncreaseHealth(float amount)
    {
        if (_progressBar != null)
        {
            // Add the amount and clamp it so it doesn't go above 100
            _progressBar.value = Mathf.Min(_progressBar.value + amount, 100);
        }

    }

    private void Update()
    {
        // If currently touching, keep timer at zero
        if (isTouchingEnemy)
        {
            // ensure timer stays reset while touching
            timeNotTouchedEnemy = 0f;
            return;
        }

        // Not touching: count up
        timeNotTouchedEnemy += Time.deltaTime;

        // Start regenerating after delay
        if (timeNotTouchedEnemy >= regenDelayAfterTouch)
        {
            // Surv_HealthRegen + Surv_HealthRegen2 skills stack additively
            float regenMult = 1f;
            if (AllomanticSkillTree.Instance != null)
                regenMult += AllomanticSkillTree.Instance.GetSkillValue("Surv_HealthRegen")
                           + AllomanticSkillTree.Instance.GetSkillValue("Surv_HealthRegen2");
            IncreaseHealth(regenPerSecond * regenMult * Time.deltaTime);
        }
    }
    
    public float health 
    {
        get { return _progressBar != null ? _progressBar.value : 100f; }
        set { if (_progressBar != null) _progressBar.value = value; }
    }

    // Health API aliases for external scripts (SazedAI, Pewter, etc.)
    public float currentHealth => health;
    public float GetCurrentHealth() => health;
    public float GetMaxHealth() => 100f; // UI bar is statically set to 100
    public void Heal(float amount) => IncreaseHealth(amount);
    
    // Add this so the Combat script can damage the health bar
    public void TakeDamage(float damage)
    {
        if (_progressBar != null && _progressBar.value <= 0f) return; // already dead

        // Surv_DamageReduction skill: flat % off all incoming damage
        float skillReduction = AllomanticSkillTree.Instance != null
            ? AllomanticSkillTree.Instance.GetSkillValue("Surv_DamageReduction")
            : 0f;
        float finalDamage = damage * incomingDamageMultiplier * (1f - skillReduction);
        DecreaseHealth(finalDamage);

        Debug.Log($"[PlayerHealth] Took {finalDamage:F1} dmg — HP={health:F1}");

        if (health <= 0f)
        {
            Debug.Log("[PlayerHealth] PLAYER DIED — firing PlayerDied event");
            EventManager.TriggerEvent("PlayerDied");
        }
    }
}
