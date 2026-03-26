using UnityEngine;
using System.Collections;

/// <summary>
/// Dodge roll with invincibility frames. Costs stamina.
/// Can be enhanced by burning Pewter (faster recovery, longer i-frames).
/// </summary>
public class DodgeRoll : MonoBehaviour
{
    [Header("Dodge Settings")]
    public float dodgeDistance = 4f;
    public float dodgeDuration = 0.4f;
    public float dodgeCooldown = 0.8f;
    public float invincibilityDuration = 0.3f;
    public float staminaCost = 20f;

    [Header("Pewter Enhancement")]
    public float pewterCooldownReduction = 0.4f;
    public float pewterExtraIFrames = 0.15f;

    [Header("Input")]
    public KeyCode dodgeKey = KeyCode.LeftAlt;

    [Header("References")]
    public Rigidbody playerRb;
    public PlayerStamina stamina;
    public Allomancer allomancer;
    public Animator animator;

    private bool isDodging = false;
    private bool isInvincible = false;
    private float cooldownTimer = 0f;
    private Collider playerCollider;

    void Start()
    {
        dodgeKey = Keybinds.DodgeRoll;
        if (playerRb == null) playerRb = GetComponent<Rigidbody>();
        if (stamina == null) stamina = GetComponent<PlayerStamina>();
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        if (animator == null) animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(dodgeKey) && !isDodging && cooldownTimer <= 0f)
        {
            if (stamina != null && stamina.currentStamina < staminaCost) return;
            StartCoroutine(PerformDodge());
        }
    }

    IEnumerator PerformDodge()
    {
        isDodging = true;
        isInvincible = true;

        // Drain stamina
        stamina?.UseStamina(staminaCost);

        // Pewter enhancement check
        bool pewterActive = allomancer != null && allomancer.IsMetalBurning(AllomancySkill.MetalType.Pewter);
        float actualDuration = dodgeDuration;
        float actualIFrames = invincibilityDuration + (pewterActive ? pewterExtraIFrames : 0f);
        float actualCooldown = dodgeCooldown - (pewterActive ? pewterCooldownReduction : 0f);

        // Get dodge direction (input direction or backward)
        Vector3 dodgeDir = GetDodgeDirection();

        animator?.SetTrigger("Dodge");

        // Disable collision during i-frames
        if (playerCollider != null) playerCollider.enabled = false;

        // Apply dodge force
        float elapsed = 0f;
        while (elapsed < actualDuration)
        {
            float t = elapsed / actualDuration;
            float speed = dodgeDistance / actualDuration * (1f - t * 0.5f); // Decelerate
            playerRb.MovePosition(playerRb.position + dodgeDir * speed * Time.deltaTime);

            // End i-frames partway through
            if (elapsed >= actualIFrames && isInvincible)
            {
                isInvincible = false;
                if (playerCollider != null) playerCollider.enabled = true;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure collider is re-enabled
        if (playerCollider != null) playerCollider.enabled = true;
        isInvincible = false;
        isDodging = false;
        cooldownTimer = Mathf.Max(0.1f, actualCooldown);
    }

    Vector3 GetDodgeDirection()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
        {
            Vector3 dir = (transform.right * h + transform.forward * v).normalized;
            return dir;
        }

        return -transform.forward; // Default: dodge backward
    }

    public bool IsDodging() => isDodging;
    public bool IsInvincible() => isInvincible;
}
