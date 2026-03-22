/// <summary>
/// Atium - See enemy futures as ghosts.
/// Usage: AtiumBurn atium = GetComponent<AtiumBurn>();
/// </summary>
public class AtiumBurn : MonoBehaviour
{
    // SETTINGS
    public float visionRange = 50f;           // How far to see
    public float metalCostPerSecond = 10f;   // High cost - rare metal
    public float ghostAlpha = 0.3f;          // Transparency of ghosts
    
    // STATE
    private bool isBurning = false;
    private System.Collections.Generic.List<GameObject> futureGhosts;
    
    // EVENTS
    public System.Action OnBurnStart;
    public System.Action OnBurnEnd;
    
    // PUBLIC API
    public bool IsBurning => isBurning;
    
    void Start()
    {
        futureGhosts = new System.Collections.Generic.List<GameObject>();
    }
    
    void Update()
    {
        // Press T to burn atium
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (isBurning)
                StopBurning();
            else
                StartBurning();
        }
        
        if (isBurning)
        {
            DrainMetal();
            SeeFutures();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Atium - Seeing the future!");
        OnBurnStart?.Invoke();
    }
    
    void StopBurning()
    {
        isBurning = false;
        ClearFutures();
        Debug.Log("Stopped Atium");
        OnBurnEnd?.Invoke();
    }
    
    void SeeFutures()
    {
        ClearFutures();
        
        // Find enemies and create ghost copies showing their predicted positions
        AIController[] enemies = FindObjectsOfType<AIController>();
        
        foreach (AIController enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance > visionRange) continue;
            
            GameObject ghost = CreateFutureGhost(enemy);
            futureGhosts.Add(ghost);
        }
    }
    
    GameObject CreateFutureGhost(AIController enemy)
    {
        // Clone the enemy as a ghost
        GameObject ghost = Instantiate(enemy.gameObject, enemy.transform.position, enemy.transform.rotation);
        ghost.name = $"FutureGhost_{enemy.name}";
        
        // Make transparent
        Renderer[] renderers = ghost.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            Material ghostMat = new Material(r.material);
            Color c = ghostMat.color;
            ghostMat.color = new Color(c.r, c.g, c.b, ghostAlpha);
            r.material = ghostMat;
        }
        
        // Disable AI and colliders on ghost
        ghost.GetComponent<AIController>().enabled = false;
        
        Collider[] cols = ghost.GetComponentsInChildren<Collider>();
        foreach (Collider c in cols)
            c.enabled = false;
        
        return ghost;
    }
    
    void ClearFutures()
    {
        foreach (GameObject ghost in futureGhosts)
        {
            if (ghost != null)
                Destroy(ghost);
        }
        futureGhosts.Clear();
    }
    
    void DrainMetal()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals != null)
        {
            if (!metals.UseMetal(MetalType.Atium, metalCostPerSecond * Time.deltaTime))
            {
                StopBurning();
            }
        }
    }
    
    void OnDestroy()
    {
        ClearFutures();
    }
}
