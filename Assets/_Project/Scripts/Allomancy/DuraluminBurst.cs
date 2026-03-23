// NOTE: Lines 48 and 67 contain Debug.Log which should be removed for production
using UnityEngine;

public class DuraluminBurst : MonoBehaviour
{
    [Header("Settings")]
    public float burstCost = 50f;
    public float burstMultiplier = 5f;
    public float burstDuration = 2f;
    
    private float metalReserve = 100f;
    private bool isBursting = false;
    
    public void TryBurst()
    {
        if (metalReserve >= burstCost && !isBursting)
        {
            PerformBurst();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryBurst();
        }
    }
    
    void PerformBurst()
    {
        metalReserve -= burstCost;
        isBursting = true;
        
        SteelPush steel = GetComponent<SteelPush>();
        if (steel != null)
        {
            steel.allomanticStrength *= burstMultiplier;
            Invoke("EndBurst", burstDuration);
        }
        
        IronPull iron = GetComponent<IronPull>();
        if (iron != null)
        {
            iron.allomanticStrength *= burstMultiplier;
            Invoke("EndBurst", burstDuration);
        }
    }
    
    void EndBurst()
    {
        isBursting = false;
        
        SteelPush steel = GetComponent<SteelPush>();
        if (steel != null)
        {
            steel.allomanticStrength /= burstMultiplier;
        }
        
        IronPull iron = GetComponent<IronPull>();
        if (iron != null)
        {
            iron.allomanticStrength /= burstMultiplier;
        }
    }
    
    public float GetMetalReserve() => metalReserve;
}
