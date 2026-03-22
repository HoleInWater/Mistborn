/// <summary>
/// Controls player dash ability.
/// Usage: DashAbility dash = GetComponent<DashAbility>();
/// </summary>

using UnityEngine;

public class DashAbility : MonoBehaviour
{
    // SETTINGS - Adjust in Inspector
    public float dashSpeed = 20f;           // How fast player dashes
    public float dashDuration = 0.2f;       // How long dash lasts
    public float dashCooldown = 1f;         // Time between dashes
    
    // INTERNAL STATE
    private Stamina stamina;               // Reference to stamina system
    private bool isDashing = false;         // Is dash currently active
    private float lastDashTime = -999f;     // When last dash occurred
    private Vector3 dashDirection;          // Direction of current dash
    
    // EVENTS - Subscribe for callbacks
    public System.Action OnDashStart;       // Fired when dash begins
    public System.Action OnDashEnd;         // Fired when dash ends
    
    // PUBLIC API - Call from other scripts
    public bool IsDashing => isDashing;
    public bool CanDash() => !isDashing && Time.time - lastDashTime >= dashCooldown;
    
    void Start()
    {
        stamina = GetComponent<Stamina>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && CanDash())
        {
            StartDash();
        }
    }
    
    void StartDash()
    {
        if (stamina != null)
        {
            stamina.UseStamina(stamina.dashStaminaCost);
        }
        
        isDashing = true;
        lastDashTime = Time.time;
        
        Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        dashDirection = inputDir.magnitude > 0 ? inputDir.normalized : transform.forward;
        
        OnDashStart?.Invoke();
        Invoke(nameof(EndDash), dashDuration);
    }
    
    void EndDash()
    {
        isDashing = false;
        OnDashEnd?.Invoke();
    }
    
    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            rb.linearVelocity = Vector3.zero;
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
        GetComponent<Rigidbody>().velocity = dashDirection * dashSpeed;
    }
}
