/// <summary>
/// Controls wall running and wall jumping mechanics.
/// Usage: WallRun wallRun = GetComponent<WallRun>();
/// </summary>
public class WallRun : MonoBehaviour
{
    // SETTINGS - Adjust in Inspector
    public float wallRunSpeed = 8f;         // Speed while wall running
    public float wallJumpForce = 10f;       // Force of wall jump
    public float wallDetectionRange = 2f;    // How far to detect walls
    public float maxWallRunTime = 3f;       // Max time on one wall
    
    // INTERNAL STATE
    private Rigidbody rb;                   // Rigidbody reference
    private Stamina stamina;                // Reference to stamina system
    private bool isWallRunning = false;      // Is wall running active
    private bool isWallLeft = false;         // Wall detected on left
    private bool isWallRight = false;        // Wall detected on right
    private float wallRunTime = 0f;          // Current wall run duration
    private Vector3 wallNormal;              // Direction away from wall
    
    // EVENTS - Subscribe for callbacks
    public System.Action OnWallRunStart;     // Fired when wall run begins
    public System.Action OnWallRunEnd;       // Fired when wall run ends
    
    // PUBLIC API - Call from other scripts
    public bool IsWallRunning => isWallRunning;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        stamina = GetComponent<Stamina>();
    }
    
    void Update()
    {
        CheckForWalls();
        
        if (CanWallRun() && IsWallContact())
        {
            StartWallRun();
        }
        else if (isWallRunning)
        {
            StopWallRun();
        }
        
        if (Input.GetKeyDown(KeyCode.Space) && isWallRunning)
        {
            WallJump();
        }
    }
    
    void CheckForWalls()
    {
        Vector3 origin = transform.position;
        
        isWallLeft = Physics.Raycast(origin, -transform.right, out RaycastHit leftHit, wallDetectionRange);
        isWallRight = Physics.Raycast(origin, transform.right, out RaycastHit rightHit, wallDetectionRange);
        
        wallNormal = isWallLeft ? leftHit.normal : (isWallRight ? rightHit.normal : Vector3.zero);
    }
    
    bool CanWallRun()
    {
        if (stamina != null && stamina.currentStamina <= 0) return false;
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.UpArrow)) return false;
        return true;
    }
    
    bool IsWallContact() => isWallLeft || isWallRight;
    
    void StartWallRun()
    {
        if (!isWallRunning)
        {
            isWallRunning = true;
            wallRunTime = 0f;
            OnWallRunStart?.Invoke();
        }
        
        wallRunTime += Time.deltaTime;
        
        if (stamina != null && wallRunTime > 0)
        {
            stamina.UseStamina(stamina.wallRunStaminaCost * Time.deltaTime);
        }
        
        if (wallRunTime >= maxWallRunTime)
        {
            StopWallRun();
            return;
        }
        
        rb.velocity = new Vector3(rb.velocity.x, -2f, wallRunSpeed);
    }
    
    void StopWallRun()
    {
        isWallRunning = false;
        OnWallRunEnd?.Invoke();
    }
    
    void WallJump()
    {
<<<<<<< HEAD
        isWallRunning = false;
        
        Vector3 jumpDir = wallNormal + Vector3.up;
        rb.linearVelocity = jumpDir * jumpForce;
        
        Debug.Log("Wall jump!");
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = isNearWall ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, transform.right * wallStickDistance);
        Gizmos.DrawRay(transform.position, -transform.right * wallStickDistance);
=======
        Vector3 jumpDir = (transform.forward + Vector3.up).normalized + wallNormal;
        rb.velocity = new Vector3(0, wallJumpForce, 0);
        rb.AddForce(jumpDir * wallJumpForce, ForceMode.Impulse);
        StopWallRun();
>>>>>>> 7675fe0d8d5d9a15a05f10d5ccb6d54374440501
    }
}
