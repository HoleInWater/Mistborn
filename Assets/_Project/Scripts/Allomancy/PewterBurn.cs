// NOTE: Lines 48 and 55 contain Debug.Log which should be removed for production
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
        Debug.Log("Burning Pewter - Enhanced!");
    }
    
    void StopBurning()
    {
        isBurning = false;
        RestoreStats();
        Debug.Log("Stopped burning Pewter");
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
