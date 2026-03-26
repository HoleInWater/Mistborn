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
}
