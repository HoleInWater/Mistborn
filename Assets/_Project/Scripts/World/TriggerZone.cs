// NOTE: Lines 42, 45, 48, 51, 54 contain Debug.Log which should be removed for production
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [Header("Trigger Settings")]
    public TriggerType triggerType;
    public bool oneTimeOnly = true;
    public bool isActivated = false;
    
    [Header("Effects")]
    // NOTE: Consider adding [Tooltip("Effect to instantiate when trigger is activated")] attribute
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
                break;
            case TriggerType.Dialog:
                break;
            case TriggerType.SpawnEnemies:
                break;
            case TriggerType.OpenDoor:
                break;
            case TriggerType.UnlockArea:
                break;
        }
    }
}
