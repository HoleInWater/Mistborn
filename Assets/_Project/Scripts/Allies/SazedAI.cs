using UnityEngine;

/// <summary>
/// Support AI for Sazed. Simulates Feruchemical healing and strength.
/// </summary>
public class SazedAI : MonoBehaviour
{
    private Transform player;
    private PlayerHealth playerHealth;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        if (CompanionManager.Instance != null) CompanionManager.Instance.RegisterCompanion(gameObject);
    }

    void Update()
    {
        // Sazed watches player health
        if (playerHealth != null && playerHealth.currentHealth < 50f)
        {
            HealPlayer();
        }
    }

    private void HealPlayer()
    {
        Debug.Log("[SAZED] Tapping Gold... healing you, Master Elend.");
        playerHealth.Heal(0.1f); // Passive trickle heal
    }
}
