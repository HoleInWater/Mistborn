using UnityEngine;

using UnityEngine;

/// <summary>
/// Handles player death, respawn, and game over logic.
/// Usage: RespawnSystem respawn = GetComponent<RespawnSystem>();
/// 
/// METHODS:
///   respawn.Respawn();           // Manually trigger respawn
///   respawn.SetSpawnPoint(pos);  // Set custom spawn point
/// </summary>

using UnityEngine;

public class RespawnSystem : MonoBehaviour
{
    // SETTINGS - Adjust in Inspector
    public float respawnDelay = 2f;           // Time before respawn after death
    public float deathScreenDelay = 1f;       // Time before death screen shows
    public Vector3 defaultSpawnPoint;          // Initial spawn position
    
    // REFERENCES
    public GameObject deathScreen;             // Game over UI panel
    public GameObject respawnEffect;           // VFX on respawn
    
    // INTERNAL STATE
    private Health health;                     // Reference to Health component
    private Vector3 currentSpawnPoint;
    private bool isRespawning = false;
    
    // EVENTS
    public System.Action OnPlayerDeath;        // Fired when player dies
    public System.Action OnPlayerRespawn;      // Fired after respawn
    
    // PUBLIC API
    public bool IsRespawning => isRespawning;
    public Vector3 CurrentSpawnPoint => currentSpawnPoint;
    
    void Start()
    {
        health = GetComponent<Health>();
        
        if (health != null)
        {
            health.OnDeath += HandleDeath;
        }
        
        currentSpawnPoint = defaultSpawnPoint;
        
        if (defaultSpawnPoint == Vector3.zero)
        {
            defaultSpawnPoint = transform.position;
            currentSpawnPoint = transform.position;
        }
    }
    
    void HandleDeath()
    {
        if (isRespawning) return;
        
        isRespawning = true;
        OnPlayerDeath?.Invoke();
        
        if (deathScreen != null)
        {
            Invoke(nameof(ShowDeathScreen), deathScreenDelay);
        }
        
        Invoke(nameof(Respawn), respawnDelay);
    }
    
    void ShowDeathScreen()
    {
        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
        }
    }
    
    public void Respawn()
    {
        if (Checkpoint.LastCheckpoint != null)
        {
            transform.position = Checkpoint.LastCheckpoint.transform.position;
        }
        else
        {
            transform.position = currentSpawnPoint;
        }
        
        if (health != null)
        {
            health.Revive(health.maxHealth);
        }
        
        if (respawnEffect != null)
        {
            Instantiate(respawnEffect, transform.position, Quaternion.identity);
        }
        
        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
        }
        
        isRespawning = false;
        OnPlayerRespawn?.Invoke();
        
        Debug.Log("Player respawned");
    }
    
    public void SetSpawnPoint(Vector3 position)
    {
        currentSpawnPoint = position;
    }
    
    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
        }
    }
}
