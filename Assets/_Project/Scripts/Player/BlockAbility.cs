// NOTE: Lines 53 and 65 contain Debug.Log which should be removed for production
// NOTE: Consider adding [DisallowMultipleComponent] attribute to prevent duplicate components
using UnityEngine;

public class BlockAbility : MonoBehaviour
{
    [Header("Block Settings")]
    public KeyCode blockKey = KeyCode.Mouse1;
    // NOTE: Consider adding [Range(0.1f, 1f)] attribute for blockSpeedReduction
    public float blockSpeedReduction = 0.5f;
    // NOTE: Consider adding [Range(0.1f, 10f)] attribute for metalCostPerSecond
    public float metalCostPerSecond = 2f;
    // NOTE: Consider adding [Range(0f, 1f)] attribute for damageReduction
    public float damageReduction = 0.5f;
    
    [Header("References")]
    public BasicPlayerMove playerController;
    public Allomancer allomancer;
    
    private bool isBlocking = false;
    private float originalSpeed;
    
    void Start()
    {
        if (playerController != null)
        {
            originalSpeed = playerController.moveSpeed;
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(blockKey))
        {
            StartBlock();
        }
        
        if (Input.GetKey(blockKey) && isBlocking)
        {
            DrainMetal();
        }
        
        if (Input.GetKeyUp(blockKey))
        {
            EndBlock();
        }
    }
    
    void StartBlock()
    {
        isBlocking = true;
        
        if (playerController != null)
        {
            playerController.moveSpeed = originalSpeed * blockSpeedReduction;
        }
        
        Debug.Log("Blocking!");
    }
    
    void EndBlock()
    {
        isBlocking = false;
        
        if (playerController != null)
        {
            playerController.moveSpeed = originalSpeed;
        }
        
        Debug.Log("Stopped blocking");
    }
    
    void DrainMetal()
    {
        if (allomancer != null)
        {
            allomancer.DrainMetal(AllomancySkill.MetalType.Pewter, metalCostPerSecond * Time.deltaTime);
        }
    }
    
    public bool IsBlocking()
    {
        return isBlocking;
    }
    
    public float GetDamageReduction()
    {
        return isBlocking ? damageReduction : 0f;
    }
}
