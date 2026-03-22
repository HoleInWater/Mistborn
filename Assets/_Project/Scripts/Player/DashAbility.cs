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
        if (Input.GetKeyDown(dashKey) && Time.time - lastDashTime >= dashCooldown)
        {
            if (CanAffordDash()) StartCoroutine(SmoothDash());
            else Debug.Log("Not enough metal!");
        }
    }

    IEnumerator SmoothDash()
    {
        isDashing = true;
        lastDashTime = Time.time;
        DrainMetal();

        // Determine Direction
        Vector3 dashDir = GetDashDirection();
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            float t = (Time.time - startTime) / dashDuration;
            // Use Lerp to ease out the speed toward the end of the duration
            float currentSpeed = Mathf.Lerp(dashSpeed, dashSpeed * 0.5f, t);
            
            rb.linearVelocity = dashDir * currentSpeed;
            yield return new WaitForFixedUpdate();
        }

        // Soft stop
        rb.linearVelocity = Vector3.zero;
        isDashing = false;
    }

    Vector3 GetDashDirection()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        
        Vector3 dir = (Camera.main.transform.forward * v + Camera.main.transform.right * h);
        dir.y = 0; // Keep dash horizontal

        if (dir.sqrMagnitude < 0.1f) dir = transform.forward;
        return dir.normalized;
    }

    bool CanAffordDash() { /* Keep existing logic */ return true; }
    void DrainMetal() { /* Keep existing logic */ }
}
