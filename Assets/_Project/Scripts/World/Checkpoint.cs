using UnityEngine;

/// <summary>
/// Manages checkpoints throughout the game.
/// Usage: Checkpoint checkpoint = GetComponent<Checkpoint>();
/// 
/// EVENTS:
///   checkpoint.OnActivate += () => { };
/// 
/// STATIC:
///   Checkpoint.LastCheckpoint - Access the most recently activated checkpoint
/// </summary>
public class Checkpoint : MonoBehaviour
{
    // SETTINGS - Adjust in Inspector
    public bool isActivated = false;          // Is this checkpoint active
    public GameObject activationEffect;        // VFX when checkpoint activates
    public AudioClip activationSound;          // Sound when checkpoint activates
    
    // EVENTS
    public System.Action OnActivate;          // Fired when checkpoint is activated
    
    // STATIC - Shared across all checkpoints
    public static Checkpoint LastCheckpoint { get; private set; }
    
    // INTERNAL
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && activationSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (isActivated)
        {
            LastCheckpoint = this;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            Activate();
        }
    }
    
    public void Activate()
    {
        if (isActivated) return;
        
        if (LastCheckpoint != null && LastCheckpoint != this)
        {
            LastCheckpoint.isActivated = false;
        }
        
        isActivated = true;
        LastCheckpoint = this;
        
        if (activationEffect != null)
        {
            Instantiate(activationEffect, transform.position, Quaternion.identity);
        }
        
        if (activationSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        Debug.Log($"Checkpoint activated: {gameObject.name}");
        OnActivate?.Invoke();
    }
    
    public static void RespawnAtLastCheckpoint(Transform player)
    {
        if (LastCheckpoint != null)
        {
            player.position = LastCheckpoint.transform.position;
            Debug.Log("Respawned at last checkpoint");
        }
        else
        {
            Debug.LogWarning("No checkpoint found!");
        }
    }
}
