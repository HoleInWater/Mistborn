using UnityEngine;

/// <summary>
/// Electrum - See your own future.
/// Usage: ElectrumFuture electrum = GetComponent<ElectrumFuture>();
/// </summary>
public class ElectrumFuture : MonoBehaviour
{
    // SETTINGS
    public float metalCostPerSecond = 8f;    // Cost
    public float futureDistance = 2f;       // How far ahead
    
    // STATE
    private bool isBurning = false;
    private GameObject futureSelf;
    private Material originalMaterial;
    
    // EVENTS
    public System.Action OnBurnStart;
    public System.Action OnBurnEnd;
    
    // PUBLIC API
    public bool IsBurning => isBurning;
    
    void Update()
    {
        // Press 5 to burn electrum
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (isBurning)
                StopBurning();
            else
                StartBurning();
        }
        
        if (isBurning)
        {
            DrainMetal();
            ShowFuture();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Electrum - Seeing your future!");
        OnBurnStart?.Invoke();
    }
    
    void StopBurning()
    {
        isBurning = false;
        ClearFuture();
        Debug.Log("Stopped Electrum");
        OnBurnEnd?.Invoke();
    }
    
    void ShowFuture()
    {
        if (futureSelf != null)
        {
            // Update position to follow
            futureSelf.transform.position = transform.position + transform.forward * futureDistance;
            futureSelf.transform.rotation = transform.rotation;
            return;
        }
        
        // Create future self ghost
        futureSelf = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        futureSelf.transform.position = transform.position + transform.forward * futureDistance;
        futureSelf.transform.rotation = transform.rotation;
        futureSelf.name = "ElectrumGhost";
        
        // Make bright and transparent
        Renderer r = futureSelf.GetComponent<Renderer>();
        originalMaterial = r.material;
        r.material.color = new Color(1f, 1f, 0f, 0.5f);
        
        Destroy(futureSelf.GetComponent<Collider>());
    }
    
    void ClearFuture()
    {
        if (futureSelf != null)
        {
            Renderer r = futureSelf.GetComponent<Renderer>();
            if (r != null && originalMaterial != null)
            {
                Destroy(r.material);
            }
            Destroy(futureSelf);
            futureSelf = null;
        }
    }
    
    void DrainMetal()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals != null)
        {
            if (!metals.UseMetal(MetalType.Electrum, metalCostPerSecond * Time.deltaTime))
            {
                StopBurning();
            }
        }
    }
    
    void OnDestroy()
    {
        ClearFuture();
    }
}
