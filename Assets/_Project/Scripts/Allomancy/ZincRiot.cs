// NOTE: Lines 41 and 47 contain Debug.Log which should be removed for production
using UnityEngine;
using System.Collections.Generic;

public class ZincRiot : MonoBehaviour
{
    [Header("Settings")]
    public float emotionRange = 30f;
    public float riotStrength = 2f;
    public float metalCostPerSecond = 3f;
    public LayerMask enemyLayer;
    
    [Header("References")]
    public Camera playerCamera;
    
    private float metalReserve = 100f;
    private bool isBurning = false;
    private List<GameObject> affectedEnemies = new List<GameObject>();
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            StartBurning();
        }
        
        if (Input.GetKey(KeyCode.Z) && isBurning)
        {
            RiotEmotions();
            DrainMetal();
        }
        
        if (Input.GetKeyUp(KeyCode.Z))
        {
            StopBurning();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Zinc - Rioting emotions!");
    }
    
    void StopBurning()
    {
        isBurning = false;
        Debug.Log("Stopped burning Zinc");
    }
    
    void RiotEmotions()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, emotionRange, enemyLayer);
        
        foreach (Collider enemy in enemies)
        {
            AIController ai = enemy.GetComponent<AIController>();
            if (ai != null)
            {
                ai.SetEmotionState(AIController.EmotionState.Enraged);
                ai.SetAggressionMultiplier(riotStrength);
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
