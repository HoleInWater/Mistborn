using UnityEngine;

/// <summary>
/// Support AI for Sazed. Simulates Feruchemical healing and strength.
/// </summary>
public class SazedAI : MonoBehaviour
{
    private Transform player;
    private HealthBarTransitions playerHealth;

    void Start()
    {
        var _po = GameObject.FindGameObjectWithTag("Player");
        if (_po != null)
        {
            player = _po.transform;
            playerHealth = _po.GetComponent<PlayerHealth>();
        }
        if (CompanionManager.Instance != null) CompanionManager.Instance.RegisterCompanion(gameObject);
    }

    private float healCooldown = 0f;

    void Update()
    {
        healCooldown -= Time.deltaTime;
        // Sazed watches player health
        if (healCooldown <= 0f && playerHealth != null && playerHealth.currentHealth < 50f)
        {
            HealPlayer();
            healCooldown = 1f; // Heal once per second, not 60x/sec
        }
    }

    private void HealPlayer()
    {
        playerHealth.Heal(2f); // 2 HP/sec while Sazed is active
    }
}
