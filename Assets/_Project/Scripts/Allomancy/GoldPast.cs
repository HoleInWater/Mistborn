/// <summary>
/// Gold - See your past self.
/// Usage: GoldPast gold = GetComponent<GoldPast>();
/// </summary>
public class GoldPast : MonoBehaviour
{
    // SETTINGS
    public float metalCostPerSecond = 8f;    // Cost
    public float ghostDuration = 3f;       // How long ghost shows
    
    // STATE
    private bool isBurning = false;
    private GameObject pastSelf;
    
    // EVENTS
    public System.Action OnBurnStart;
    public System.Action OnBurnEnd;
    
    // PUBLIC API
    public bool IsBurning => isBurning;
    
    void Update()
    {
        // Press G to burn gold
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (isBurning)
                StopBurning();
            else
                StartBurning();
        }
        
        if (isBurning)
        {
            DrainMetal();
            ShowPast();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Gold - Seeing the past!");
        OnBurnStart?.Invoke();
    }
    
    void StopBurning()
    {
        isBurning = false;
        ClearPast();
        Debug.Log("Stopped Gold");
        OnBurnEnd?.Invoke();
    }
    
    void ShowPast()
    {
        if (pastSelf != null) return;
        
        // Create ghost of past self
        pastSelf = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        pastSelf.transform.position = transform.position;
        pastSelf.transform.rotation = transform.rotation;
        pastSelf.name = "GoldGhost";
        
        // Make golden and transparent
        Renderer r = pastSelf.GetComponent<Renderer>();
        r.material.color = new Color(1f, 0.8f, 0.4f, 0.5f);
        
        Destroy(pastSelf.GetComponent<Collider>());
    }
    
    void ClearPast()
    {
        if (pastSelf != null)
        {
            Destroy(pastSelf);
            pastSelf = null;
        }
    }
    
    void DrainMetal()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals != null)
        {
            if (!metals.UseMetal(MetalType.Gold, metalCostPerSecond * Time.deltaTime))
            {
                StopBurning();
            }
        }
    }
    
    void OnDestroy()
    {
        ClearPast();
    }
}
