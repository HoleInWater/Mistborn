using UnityEngine;

public class DashAbility : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashForce = 20f; // Increase this for more "oomph"
    public float dashCooldown = 1f;
    public float metalCost = 10f;
    
    [Header("References")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public Allomancer allomancer;
    public MetalReserveManager metalManager;
    
    private float lastDashTime = -999f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Ensure drag isn't 0 so you don't slide forever
        if (rb.linearDamping == 0) rb.linearDamping = 2f; 
    }

    void Update()
    {
        if (Input.GetKeyDown(dashKey) && Time.time - lastDashTime >= dashCooldown)
        {
            if (CanAffordDash()) PerformInstantDash();
        }
    }

    void PerformInstantDash()
    {
        lastDashTime = Time.time;
        DrainMetal();

        // Use Raw input for more reliability
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 dashDir;
        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
        {
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;
            forward.y = 0;
            right.y = 0;
            dashDir = (forward * v + right * h).normalized;
        }
        else
        {
            dashDir = transform.forward;
        }

        // Apply an Impulse force - this is much more reliable
        rb.AddForce(dashDir * dashForce, ForceMode.Impulse);
        
        Debug.Log("Dash Triggered!");
    }

    // Existing Metal Logic
    bool CanAffordDash() {
        if (allomancer != null) return allomancer.GetMetalReserve(AllomancySkill.MetalType.Pewter) >= metalCost;
        if (metalManager != null) return metalManager.GetReserve(AllomancySkill.MetalType.Pewter) >= metalCost;
        return true;
    }

    void DrainMetal() {
        if (allomancer != null) allomancer.DrainMetal(AllomancySkill.MetalType.Pewter, metalCost);
        else if (metalManager != null) metalManager.Drain(AllomancySkill.MetalType.Pewter, metalCost);
    }
}
