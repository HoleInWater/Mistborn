/// <summary>
/// Controls player dash ability.
/// Usage: DashAbility dash = GetComponent<DashAbility>();
/// </summary>
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
            GetComponent<Rigidbody>().velocity = dashDirection * dashSpeed;
        }
    }
}
