/// <summary>
/// Brass Soothe - Calm nearby enemies.
/// Usage: BrassSoothe brass = GetComponent<BrassSoothe>();
/// </summary>
public class BrassSoothe : MonoBehaviour
{
    // SETTINGS
    public float emotionRange = 30f;          // Range to affect
    public float sootheStrength = 0.5f;     // How much to calm (lower = calmer)
    public float metalCostPerSecond = 3f;    // Drain rate
    public LayerMask enemyLayer;             // What to affect
    
    // STATE
    private bool isBurning = false;
    
    // EVENTS
    public System.Action OnSootheStart;
    public System.Action OnSootheEnd;
    
    // PUBLIC API
    public bool IsBurning => isBurning;
    
    void Update()
    {
        // Press X to soothe
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (isBurning)
                StopSoothe();
            else
                StartSoothe();
        }
        
        if (isBurning)
        {
            DrainMetal();
        }
    }
    
    void StartSoothe()
    {
        isBurning = true;
        Debug.Log("Burning Brass - Soothing!");
        OnSootheStart?.Invoke();
    }
    
    void StopSoothe()
    {
        isBurning = false;
        Debug.Log("Stopped Brass");
        OnSootheEnd?.Invoke();
    }
    
    void SootheAllEnemies()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, emotionRange, enemyLayer);
        
        foreach (Collider enemy in enemies)
        {
            AIController ai = enemy.GetComponent<AIController>();
            if (ai != null)
            {
                ai.SetEmotionState(AIController.EmotionState.Calm);
                ai.SetAggressionMultiplier(sootheStrength);
            }
        }
    }
    
    void DrainMetal()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals != null)
        {
            if (!metals.UseMetal(MetalType.Brass, metalCostPerSecond * Time.deltaTime))
            {
                StopSoothe();
            }
        }
    }
}
