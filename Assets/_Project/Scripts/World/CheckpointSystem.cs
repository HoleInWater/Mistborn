using UnityEngine;
using System.Collections;

/// <summary>
/// Checkpoint and respawn system. Tracks last checkpoint, handles death/respawn.
/// </summary>
public class CheckpointSystem : MonoBehaviour
{
    public static CheckpointSystem Instance { get; private set; }

    [Header("Respawn")]
    public Vector3 defaultSpawnPoint = Vector3.zero;
    public float respawnDelay = 2f;
    public bool restoreHealthOnRespawn = true;
    public bool restoreMetalsOnRespawn = true;

    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;
    private bool hasCheckpoint = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        lastCheckpointPosition = defaultSpawnPoint;
        lastCheckpointRotation = Quaternion.identity;
    }

    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        lastCheckpointPosition = position;
        lastCheckpointRotation = rotation;
        hasCheckpoint = true;
    }

    public void RespawnPlayer()
    {
        StartCoroutine(RespawnSequence());
    }

    IEnumerator RespawnSequence()
    {
        // Screen fade could go here
        yield return new WaitForSeconds(respawnDelay);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        player.transform.position = lastCheckpointPosition;
        player.transform.rotation = lastCheckpointRotation;

        // Restore health
        if (restoreHealthOnRespawn)
        {
            IDamageable health = player.GetComponent<IDamageable>();
            // Reset via PlayerHealth if available
            var ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.Heal(ph.GetMaxHealth());
        }

        // Restore metals
        if (restoreMetalsOnRespawn)
        {
            Allomancer allo = player.GetComponent<Allomancer>();
            allo?.RefillAllMetals();
        }

        // Re-enable player
        var move = player.GetComponent<BasicPlayerMove>();
        if (move != null) move.enabled = true;
        var combat = player.GetComponent<PlayerCombat>();
        if (combat != null) combat.enabled = true;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;
    }

    public Vector3 GetLastCheckpoint() => lastCheckpointPosition;
    public bool HasCheckpoint() => hasCheckpoint;
}

/// <summary>
/// PlayerHealth referenced by PlayerHealth in AudioAndHealthSystem wasn't accessible.
/// This is a lightweight reference so CheckpointSystem can heal on respawn.
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    public static PlayerHealth Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (currentHealth <= 0) { currentHealth = 0; Die(); }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        isDead = false;
    }

    void Die()
    {
        isDead = true;
        EventManager.TriggerEvent("PlayerDied");
        CheckpointSystem.Instance?.RespawnPlayer();
    }

    public float GetMaxHealth() => maxHealth;
    public float GetCurrentHealth() => currentHealth;
    public bool IsDead() => isDead;
}
