using UnityEngine;
using System.Collections;

public class DashAbility : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 1f;
    public float metalCost = 10f;
    
    [Header("References")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public Allomancer allomancer;
    public MetalReserveManager metalManager;
    
    private float lastDashTime = -999f;
    private bool isDashing = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Check for key press and cooldown
        if (Input.GetKeyDown(dashKey) && Time.time - lastDashTime >= dashCooldown && !isDashing)
        {
            if (CanAffordDash()) 
            {
                StartCoroutine(SmoothDash());
            }
            else 
            {
                Debug.Log("Not enough metal for dash!");
            }
        }
    }

    IEnumerator SmoothDash()
    {
        isDashing = true;
        lastDashTime = Time.time;
        
        DrainMetal();

        // Get direction based on WASD/Joystick input
        Vector3 dashDir = GetDashDirection();
        float startTime = Time.time;

        // Smoothly move over the duration
        while (Time.time < startTime + dashDuration)
        {
            float elapsed = Time.time - startTime;
            float percentComplete = elapsed / dashDuration;
            float currentSpeed = Mathf.Lerp(dashSpeed, dashSpeed * 0.4f, percentComplete);
            
            // Add this line to see the direction in the Scene view
            Debug.DrawRay(transform.position, dashDir * 5f, Color.red, 1f);
        
            rb.linearVelocity = dashDir * currentSpeed;
            yield return new WaitForFixedUpdate();
        }

        // Clean up at the end
        rb.linearVelocity = Vector3.zero;
        isDashing = false;
        Debug.Log("Dash Finished");
    }

    Vector3 GetDashDirection()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        
        // Calculate relative to camera
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0;
        right.y = 0;

        Vector3 dir = (forward * v + right * h).normalized;

        // If not pressing any keys, dash forward
        if (dir.sqrMagnitude < 0.1f) dir = transform.forward;
        
        return dir;
    }

    // Re-added your original logic so it actually functions
    bool CanAffordDash()
    {
        if (allomancer != null)
            return allomancer.GetMetalReserve(AllomancySkill.MetalType.Pewter) >= metalCost;
        if (metalManager != null)
            return metalManager.GetReserve(AllomancySkill.MetalType.Pewter) >= metalCost;
        
        return true; // Dashing is free if no manager is found
    }

    void DrainMetal()
    {
        if (allomancer != null)
            allomancer.DrainMetal(AllomancySkill.MetalType.Pewter, metalCost);
        else if (metalManager != null)
            metalManager.Drain(AllomancySkill.MetalType.Pewter, metalCost);
    }
}
