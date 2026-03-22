using UnityEngine;
using System.Collections.Generic;

public class BrassSoothe : MonoBehaviour
{
    [Header("Settings")]
    public float emotionRange = 30f;
    public float sootheStrength = 2f;
    public float metalCostPerSecond = 3f;
    public LayerMask enemyLayer;
    
    [Header("References")]
    public Camera playerCamera;
    
    private float metalReserve = 100f;
    private bool isBurning = false;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            StartBurning();
        }
        
        if (Input.GetKey(KeyCode.X) && isBurning)
        {
            SootheEmotions();
            DrainMetal();
        }
        
        if (Input.GetKeyUp(KeyCode.X))
        {
            StopBurning();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Brass - Soothing emotions!");
    }
    
    void StopBurning()
    {
        isBurning = false;
        Debug.Log("Stopped burning Brass");
    }
    
    void SootheEmotions()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, emotionRange, enemyLayer);
        
        foreach (Collider enemy in enemies)
        {
            AIController ai = enemy.GetComponent<AIController>();
            if (ai != null)
            {
                ai.SetEmotionState(AIController.EmotionState.Calm);
                ai.SetAggressionMultiplier(1f / sootheStrength);
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
    
    public float GetMetalReserve() => metalReserve;
    public void RefillMetal(float amount) => metalReserve = Mathf.Min(metalReserve + amount, 100f);
}
