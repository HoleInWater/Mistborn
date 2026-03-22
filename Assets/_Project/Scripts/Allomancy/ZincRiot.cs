using UnityEngine;

/// <summary>
/// Zinc Riot - Enrage nearby enemies.
/// Usage: ZincRiot zinc = GetComponent<ZincRiot>();
/// </summary>
public class ZincRiot : MonoBehaviour
{
    // SETTINGS
    public float emotionRange = 30f;          // Range to affect
    public float riotStrength = 2f;          // How much to enrage
    public float metalCostPerSecond = 3f;    // Drain rate
    public LayerMask enemyLayer;             // What to affect
    
    // STATE
    private bool isBurning = false;
    
    // EVENTS
    public System.Action OnRiotStart;
    public System.Action OnRiotEnd;
    
    // PUBLIC API
    public bool IsBurning => isBurning;
    
    void Update()
    {
        // Press Z to riot
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (isBurning)
                StopRiot();
            else
                StartRiot();
        }
        
        if (isBurning)
        {
            RiotAllEnemies();
            DrainMetal();
        }
    }
    
    void StartRiot()
    {
        isBurning = true;
        Debug.Log("Burning Zinc - Rioting!");
        OnRiotStart?.Invoke();
    }
    
    void StopRiot()
    {
        isBurning = false;
        CalmAllEnemies();
        Debug.Log("Stopped Zinc");
        OnRiotEnd?.Invoke();
    }
    
    void RiotAllEnemies()
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
    
    void CalmAllEnemies()
    {
        AIController[] allAI = FindObjectsOfType<AIController>();
        
        foreach (AIController ai in allAI)
        {
            ai.SetEmotionState(AIController.EmotionState.Neutral);
            ai.SetAggressionMultiplier(1f);
        }
    }
    
    void DrainMetal()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals != null)
        {
            if (!metals.UseMetal(MetalType.Zinc, metalCostPerSecond * Time.deltaTime))
            {
                StopRiot();
            }
        }
    }
}
