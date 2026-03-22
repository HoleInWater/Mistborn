// NOTE: Lines 41, 48, 67 contain Debug.Log which should be removed for production
using UnityEngine;
using System.Collections.Generic;

public class BronzeDetect : MonoBehaviour
{
    [Header("Settings")]
    public float detectionRange = 50f;
    public float metalCostPerSecond = 2f;
    public float pulseInterval = 0.5f;
    
    [Header("References")]
    public Camera playerCamera;
    
    private float metalReserve = 100f;
    private bool isBurning = false;
    private float lastPulseTime = 0f;
    private List<GameObject> detectedAllomancers = new List<GameObject>();
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            StartBurning();
        }
        
        if (Input.GetKey(KeyCode.V) && isBurning)
        {
            DetectPulses();
            DrainMetal();
        }
        
        if (Input.GetKeyUp(KeyCode.V))
        {
            StopBurning();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Bronze - Detecting Allomancers!");
    }
    
    void StopBurning()
    {
        isBurning = false;
        detectedAllomancers.Clear();
        Debug.Log("Stopped burning Bronze");
    }
    
    void DetectPulses()
    {
        if (Time.time - lastPulseTime < pulseInterval)
            return;
            
        lastPulseTime = Time.time;
        detectedAllomancers.Clear();
        
        Collider[] nearby = Physics.OverlapSphere(transform.position, detectionRange);
        
        foreach (Collider col in nearby)
        {
            Allomancer allomancer = col.GetComponent<Allomancer>();
            if (allomancer != null && allomancer.IsBurning())
            {
                detectedAllomancers.Add(col.gameObject);
                Debug.Log($"Detected Allomancer: {col.gameObject.name} burning {allomancer.GetCurrentMetal()}");
            }
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
    
    public List<GameObject> GetDetectedAllomancers() => detectedAllomancers;
    public float GetMetalReserve() => metalReserve;
}
