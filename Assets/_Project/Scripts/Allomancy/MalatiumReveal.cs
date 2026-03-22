/// <summary>
/// Malatium - Reveal true nature of things.
/// Usage: MalatiumReveal malatium = GetComponent<MalatiumReveal>();
/// </summary>
public class MalatiumReveal : MonoBehaviour
{
    // SETTINGS
    public float revealRange = 20f;         // Range to affect
    public float metalCostPerSecond = 5f;    // Cost
    
    // VISUALS
    public Color malatiumColor = new Color(0.8f, 0.3f, 0.1f, 0.5f);
    
    // STATE
    private bool isBurning = false;
    private System.Collections.Generic.Dictionary<UnityEngine.Renderer, UnityEngine.Material> originalMaterials;
    
    // EVENTS
    public System.Action OnBurnStart;
    public System.Action OnBurnEnd;
    
    // PUBLIC API
    public bool IsBurning => isBurning;
    
    void Start()
    {
        originalMaterials = new System.Collections.Generic.Dictionary<UnityEngine.Renderer, UnityEngine.Material>();
    }
    
    void Update()
    {
        // Press Y to reveal
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (isBurning)
                StopBurning();
            else
                StartBurning();
        }
        
        if (isBurning)
        {
            DrainMetal();
            RevealTrueNature();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Malatium - Revealing true nature!");
        OnBurnStart?.Invoke();
    }
    
    void StopBurning()
    {
        isBurning = false;
        RestoreAllMaterials();
        Debug.Log("Stopped Malatium");
        OnBurnEnd?.Invoke();
    }
    
    void RevealTrueNature()
    {
        // Find all renderers in range
        Collider[] hits = Physics.OverlapSphere(transform.position, revealRange);
        
        foreach (Collider hit in hits)
        {
            UnityEngine.Renderer renderer = hit.GetComponent<UnityEngine.Renderer>();
            if (renderer != null && !originalMaterials.ContainsKey(renderer))
            {
                // Save original material
                originalMaterials[renderer] = renderer.material;
                
                // Apply malatium color
                Material malatiumMat = new Material(renderer.material);
                malatiumMat.color = malatiumColor;
                renderer.material = malatiumMat;
            }
        }
    }
    
    void RestoreAllMaterials()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null)
            {
                kvp.Key.material = kvp.Value;
            }
        }
        originalMaterials.Clear();
    }
    
    void DrainMetal()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals != null)
        {
            if (!metals.UseMetal(MetalType.Malatium, metalCostPerSecond * Time.deltaTime))
            {
                StopBurning();
            }
        }
    }
    
    void OnDestroy()
    {
        RestoreAllMaterials();
    }
}
