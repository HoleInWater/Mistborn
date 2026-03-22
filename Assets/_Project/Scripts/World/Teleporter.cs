using UnityEngine;

/// <summary>
/// Teleporter to another location or scene.
/// Usage: Attach to teleporter pad.
/// </summary>
public class Teleporter : MonoBehaviour
{
    // SETTINGS
    public string targetScene;                // Leave empty for same scene
    public Transform targetPosition;            // Where to teleport
    public float cooldown = 2f;               // Time between teleports
    
    // EFFECTS
    public GameObject teleportEffect;
    public AudioClip teleportSound;
    
    // STATE
    private bool isOnCooldown = false;
    
    // EVENTS
    public System.Action OnTeleport;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOnCooldown)
        {
            Teleport(other.gameObject);
        }
    }
    
    void Teleport(GameObject player)
    {
        StartCoroutine(TeleportRoutine(player));
    }
    
    System.Collections.IEnumerator TeleportRoutine(GameObject player)
    {
        isOnCooldown = true;
        
        // Effects at origin
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, transform.position, Quaternion.identity);
        }
        
        if (teleportSound != null)
        {
            AudioSource.PlayClipAtPoint(teleportSound, transform.position);
        }
        
        // Disable player briefly
        player.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        
        if (!string.IsNullOrEmpty(targetScene))
        {
            // Scene teleport
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
            yield return new WaitForSeconds(0.5f);
            
            // Find spawn point in new scene
            player.transform.position = targetPosition != null 
                ? targetPosition.position 
                : Vector3.zero;
        }
        else if (targetPosition != null)
        {
            // Same scene teleport
            player.transform.position = targetPosition.position;
        }
        
        player.SetActive(true);
        
        // Effects at destination
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, player.transform.position, Quaternion.identity);
        }
        
        OnTeleport?.Invoke();
        
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }
}
