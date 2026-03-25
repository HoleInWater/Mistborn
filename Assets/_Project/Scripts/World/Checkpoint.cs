// NOTE: Line 44 contains Debug.Log which should be removed for production
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static Checkpoint lastCheckpoint { get; private set; }
    
    [Header("Checkpoint Settings")]
    public bool isActivated = false;
    
    [Header("Effects")]
    // NOTE: Consider adding [Tooltip("Effect to instantiate when checkpoint is activated")] attribute
    public GameObject activationEffect;
    
    void Start()
    {
        if (isActivated)
        {
            lastCheckpoint = this;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            Activate();
        }
    }
    
    void Activate()
    {
        if (lastCheckpoint != null)
        {
            lastCheckpoint.isActivated = false;
        }
        
        isActivated = true;
        lastCheckpoint = this;
        
        if (activationEffect != null)
        {
            Instantiate(activationEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log("Checkpoint activated!");
    }
}
