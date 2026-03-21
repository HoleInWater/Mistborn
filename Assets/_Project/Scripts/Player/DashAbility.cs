using UnityEngine;

public class DashAbility : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float metalCost = 10f;
    
    [Header("References")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public Allomancer allomancer;
    public MetalReserveManager metalManager;
    
    private float lastDashTime = -999f;
    private bool isDashing = false;
    private Vector3 dashDirection;
    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(dashKey))
        {
            TryDash();
        }
    }
    
    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.velocity = dashDirection * dashSpeed;
        }
    }
    
    void TryDash()
    {
        if (Time.time - lastDashTime < dashCooldown)
        {
            Debug.Log("Dash on cooldown!");
            return;
        }
        
        if (!CanAffordDash())
        {
            Debug.Log("Not enough metal for dash!");
            return;
        }
        
        PerformDash();
    }
    
    bool CanAffordDash()
    {
        if (allomancer != null)
        {
            return allomancer.GetMetalReserve(AllomancySkill.MetalType.Pewter) >= metalCost;
        }
        else if (metalManager != null)
        {
            return metalManager.GetReserve(AllomancySkill.MetalType.Pewter) >= metalCost;
        }
        return true;
    }
    
    void PerformDash()
    {
        dashDirection = transform.forward;
        if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
        {
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;
            forward.y = 0;
            right.y = 0;
            dashDirection = (forward * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal")).normalized;
        }
        
        isDashing = true;
        lastDashTime = Time.time;
        
        if (allomancer != null)
        {
            allomancer.DrainMetal(AllomancySkill.MetalType.Pewter, metalCost);
        }
        else if (metalManager != null)
        {
            metalManager.Drain(AllomancySkill.MetalType.Pewter, metalCost);
        }
        
        Debug.Log("Dashed!");
        
        Invoke("EndDash", dashDuration);
    }
    
    void EndDash()
    {
        isDashing = false;
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }
    }
}
