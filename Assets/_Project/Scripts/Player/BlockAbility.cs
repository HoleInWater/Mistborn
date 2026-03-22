using UnityEngine;

public class BlockAbility : MonoBehaviour
{
    [Header("Block Settings")]
    public KeyCode blockKey = KeyCode.Mouse1;
    public float blockSpeedReduction = 0.5f;
    public float metalCostPerSecond = 2f;
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
