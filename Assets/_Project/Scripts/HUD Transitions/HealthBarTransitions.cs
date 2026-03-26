<<<<<<< HEAD
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


    private ProgressBar _progressBar;

    void OnEnable()
    {
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
            DecreaseHealth(damagePerSecondWhileTouching * Time.deltaTime);
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
            IncreaseHealth(regenPerSecond * Time.deltaTime);
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
        DecreaseHealth(damage);
    }
=======
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Displays player health via a UI Toolkit ProgressBar and handles
/// passive regen after combat. All health values are owned by PlayerHealth;
/// this component is pure display + regen logic.
///
/// Attach to the player GameObject alongside PlayerHealth.
/// Assign the UIDocument that contains a ProgressBar named "Health".
/// </summary>
public class HealthBarTransitions : MonoBehaviour
{
    [Header("UI Toolkit")]
    public UIDocument uiDocument;
    public string progressBarName = "Health";

    [Header("Regen")]
    public float regenDelayAfterDamage = 10f;   // seconds before regen kicks in
    public float regenPerSecond = 5f;

    private ProgressBar _progressBar;
    private PlayerHealth _playerHealth;
    private float _regenTimer;

    void OnEnable()
    {
        _playerHealth = GetComponent<PlayerHealth>();

        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            _progressBar = root?.Q<ProgressBar>(progressBarName);
            if (_progressBar != null)
            {
                _progressBar.lowValue  = 0;
                _progressBar.highValue = 100;
            }
        }
    }

    void Update()
    {
        if (_playerHealth == null) return;

        // Sync bar to PlayerHealth's authoritative value
        if (_progressBar != null)
        {
            float ratio = _playerHealth.maxHealth > 0f
                ? _playerHealth.currentHealth / _playerHealth.maxHealth
                : 0f;
            _progressBar.value = ratio * 100f;
        }

        // Passive regen: count up, then heal via PlayerHealth
        if (_playerHealth.isDead) return;

        if (_regenTimer > 0f)
        {
            _regenTimer -= Time.deltaTime;
        }
        else if (_playerHealth.currentHealth < _playerHealth.maxHealth)
        {
            _playerHealth.Heal(regenPerSecond * Time.deltaTime);
        }
    }

    // ── Collision damage ─────────────────────────────────────────────────

    void OnCollisionStay(Collision collision)
    {
        // No-op — damage should come through the combat system via PlayerHealth.TakeDamage()
        // Kept as a hook in case designers need contact damage; wire up below if needed.
    }

    // ── Public API (mirrors PlayerHealth for legacy callers) ──────────────

    public float health
    {
        get => _playerHealth != null ? _playerHealth.currentHealth : 0f;
        set { _playerHealth?.TakeDamage(_playerHealth.currentHealth - value); }
    }

    public float currentHealth    => _playerHealth?.currentHealth  ?? 0f;
    public float GetCurrentHealth() => _playerHealth?.GetCurrentHealth() ?? 0f;
    public float GetMaxHealth()     => _playerHealth?.GetMaxHealth()     ?? 100f;

    public void TakeDamage(float damage)
    {
        _playerHealth?.TakeDamage(damage);
        _regenTimer = regenDelayAfterDamage;    // reset regen clock on every hit
    }

    public void Heal(float amount) => _playerHealth?.Heal(amount);
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
}
