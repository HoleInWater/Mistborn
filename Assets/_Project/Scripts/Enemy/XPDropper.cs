using UnityEngine;

/// <summary>
/// Grants XP to the player when the owner object is destroyed/defeated.
/// </summary>
public class XPDropper : MonoBehaviour
{
    public float xpValue = 25f;
    private bool dropped = false;

    // This should be called by the enemy's death logic (e.g. EnemyHealth)
    public void DropXP()
    {
        if (dropped) return;
        dropped = true;

        if (PlayerExperience.Instance != null)
        {
            PlayerExperience.Instance.AddXP(xpValue);
            Debug.Log($"[XP] +{xpValue} XP gained from {gameObject.name}");
        }
    }

    // Optional: hook into OnDestroy or a custom Die event
    private void OnDisable()
    {
        // Simple safety check if not explicitly called
        if (!dropped && gameObject.scene.isLoaded)
        {
            // Note: In a real game, you'd only call this if health <= 0
        }
    }
}
