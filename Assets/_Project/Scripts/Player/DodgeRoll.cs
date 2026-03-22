using UnityEngine;

public class DodgeRoll : MonoBehaviour
{
    [Header("Dodge Settings")]
    public float dodgeSpeed = 15f;
    public float dodgeDuration = 0.3f;
    public float dodgeCooldown = 0.8f;
    public float invincibilityFrames = 0.2f;
    public float metalCost = 5f;
    
    [Header("References")]
    public KeyCode dodgeKey = KeyCode.Space;
    public Allomancer allomancer;
    
    private float lastDodgeTime = -999f;
    private bool isDodging = false;
    private Vector3 dodgeDirection;
    private bool isInvincible = false;
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
        if (Input.GetKeyDown(dodgeKey) && !isDodging)
        {
            TryDodge();
        }
    }
    
    void FixedUpdate()
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
        isInvincible = true;
        
        Invoke("EndDodge", dodgeDuration);
        Invoke("EndInvincibility", invincibilityFrames);
        
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
    }
    
    void EndInvincibility()
    {
        isInvincible = false;
    }
    
    public bool IsInvincible()
    {
        return isInvincible;
    }
}
