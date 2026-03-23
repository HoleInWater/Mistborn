
using UnityEngine;

public class PewterBurn : MonoBehaviour
{
    [Header("Settings")]
    public float strengthMultiplier = 2f;
    public float speedMultiplier = 1.5f;
    public float healingRate = 5f;
    public float metalCostPerSecond = 3f;
    public float jumpBoost = 2f;
    
    [Header("References")]
    public BasicPlayerMove playerController;
    
    private float metalReserve = 100f;
    private bool isBurning = false;
    private float originalSpeed;
    private float originalJump;
    
    void Start()
    {
        if (playerController == null)
            playerController = GetComponent<BasicPlayerMove>();

        // Capture the baseline speed so EnhancePhysical and RestoreStats
        // multiply/restore the real value rather than the default 0.
        if (playerController != null)
        {
            originalSpeed = playerController.moveSpeed;
            originalJump  = playerController.jumpVelocity;
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartBurning();
        }
        
        if (Input.GetKey(KeyCode.Q) && isBurning)
        {
            EnhancePhysical();
            DrainMetal();
        }
        
        if (Input.GetKeyUp(KeyCode.Q))
        {
            StopBurning();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
#if UNITY_EDITOR
        Debug.Log("[PewterBurn] Burning Pewter - Enhanced!");
#endif
    }
    
    void StopBurning()
    {
        isBurning = false;
        RestoreStats();
#if UNITY_EDITOR
        Debug.Log("[PewterBurn] Stopped burning Pewter");
#endif
    }
    
    void EnhancePhysical()
    {
        if (playerController != null)
        {
            playerController.moveSpeed = originalSpeed * speedMultiplier;
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * jumpBoost, ForceMode.Impulse);
        }
    }
    
    void RestoreStats()
    {
        if (playerController != null)
        {
            playerController.moveSpeed = originalSpeed;
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
