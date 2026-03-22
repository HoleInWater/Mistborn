// NOTE: Lines 42, 45, 48, 51, 54 contain Debug.Log which should be removed for production
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [Header("Trigger Settings")]
    public TriggerType triggerType;
    public bool oneTimeOnly = true;
    public bool isActivated = false;
    
    [Header("Effects")]
    public GameObject triggerEffect;
    
    public enum TriggerType
    {
        Cutscene,
        Dialog,
        SpawnEnemies,
        OpenDoor,
        UnlockArea
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!oneTimeOnly || !isActivated))
        {
            Activate();
        }
    }
    
    void Activate()
    {
        isActivated = true;
        
        if (triggerEffect != null)
        {
            Instantiate(triggerEffect, transform.position, Quaternion.identity);
        }
        
        switch (triggerType)
        {
            case TriggerType.Cutscene:
                Debug.Log("Triggering cutscene!");
                break;
            case TriggerType.Dialog:
                Debug.Log("Starting dialog!");
                break;
            case TriggerType.SpawnEnemies:
                Debug.Log("Spawning enemies!");
                break;
            case TriggerType.OpenDoor:
                Debug.Log("Opening door!");
                break;
            case TriggerType.UnlockArea:
                Debug.Log("Area unlocked!");
                break;
        }
    }
}
