using UnityEngine;
using UnityEngine.UI;

public class TinEnhance : MonoBehaviour
{
    [Header("Settings")]
    public float senseMultiplier = 3f;
    public float metalCostPerSecond = 2f;
    public float hearingRange = 50f;
    public float sightRange = 100f;
    
    [Header("References")]
    public Camera playerCamera;
    
    private float metalReserve = 100f;
    private bool isBurning = false;
    private float originalFOV;
    
    void Start()
    {
        if (playerCamera != null)
            originalFOV = playerCamera.fieldOfView;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartBurning();
        }
        
        if (Input.GetKey(KeyCode.E) && isBurning)
        {
            EnhanceSenses();
            DrainMetal();
        }
        
        if (Input.GetKeyUp(KeyCode.E))
        {
            StopBurning();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Tin - Enhanced senses!");
    }
    
    void StopBurning()
    {
        isBurning = false;
        RestoreSenses();
        Debug.Log("Stopped burning Tin");
    }
    
    void EnhanceSenses()
    {
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = originalFOV * senseMultiplier;
        }
        
        Debug.Log($"Hearing enemies within {hearingRange * senseMultiplier}m");
    }
    
    void RestoreSenses()
    {
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = originalFOV;
        }
    }
    
    void DrainMetal()
    {
        metalReserve -= metalCostPerSecond * Time.deltaTime;
        if (metalReserve <= 0)
        {
            metalReserve = 0;
            StopBurning();
        }
    }
    
    public float GetMetalReserve() => metalReserve;
    public void RefillMetal(float amount) => metalReserve = Mathf.Min(metalReserve + amount, 100f);
}
