using UnityEngine;


using UnityEngine;
/// <summary>
/// Controls player dodge roll with invincibility frames.
/// Usage: DodgeRoll dodge = GetComponent<DodgeRoll>();
/// </summary>
public class DodgeRoll : MonoBehaviour
{
    // SETTINGS - Adjust in Inspector
    public float rollSpeed = 8f;            // Speed while rolling
    public float rollDuration = 0.4f;       // How long roll lasts
    public float rollCooldown = 0.8f;       // Time between rolls
    public float invincibleDuration = 0.3f; // Time with invincibility
    
    // INTERNAL STATE
    private Stamina stamina;                // Reference to stamina system
    private Rigidbody rb;                   // Rigidbody reference
    private bool isRolling = false;         // Is roll currently active
    private bool isInvincible = false;      // Is player invincible
    private float lastRollTime = -999f;     // When last roll occurred
    private Vector3 rollDirection;          // Direction of current roll
    
    // EVENTS - Subscribe for callbacks
    public System.Action OnRollStart;       // Fired when roll begins
    public System.Action OnRollEnd;         // Fired when roll ends
    public System.Action OnRollDodge;       // Fired when invincibility ends (dodge window)
    
    // PUBLIC API - Call from other scripts
    public bool IsRolling => isRolling;
    public bool IsInvincible => isInvincible;
    public bool CanRoll() => !isRolling && Time.time - lastRollTime >= rollCooldown;
    
    void Start()
    {
        stamina = GetComponent<Stamina>();
        rb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && CanRoll())
        {
            StartRoll();
        }
    }
    
    void StartRoll()
    {
        if (isDodging)
        {
            rb.linearVelocity = dodgeDirection * dodgeSpeed;
        }
    }
    
    void TryDodge()
    {
        if (Time.time - lastDodgeTime < dodgeCooldown)
        {
            return;
        }
        
        PerformDodge();
    }
    
    void PerformDodge()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        dodgeDirection = transform.forward;
        if (horizontal != 0 || vertical != 0)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            dodgeDirection = (camForward * vertical + camRight * horizontal).normalized;
        }
        
        isDodging = true;
        lastDodgeTime = Time.time;
        lastRollTime = Time.time;
        isRolling = true;
        isInvincible = true;
        
        Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        rollDirection = inputDir.magnitude > 0 ? inputDir.normalized : transform.forward;
        
        if (allomancer != null)
        {
            allomancer.DrainMetal(AllomancySkill.MetalType.Pewter, metalCost);
        }
        
        Debug.Log("Dodge roll!");
    }
    
    void EndDodge()
    {
        isDodging = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        OnRollStart?.Invoke();
        Invoke(nameof(EndInvincibility), invincibleDuration);
        Invoke(nameof(EndRoll), rollDuration);
    }
    
    void EndInvincibility()
    {
        isInvincible = false;
        OnRollDodge?.Invoke();
    }
    
    void EndRoll()
    {
        isRolling = false;
        OnRollEnd?.Invoke();
    }
    
    void FixedUpdate()
    {
        if (isRolling)
        {
            rb.velocity = rollDirection * rollSpeed;
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
        }
    }
}
