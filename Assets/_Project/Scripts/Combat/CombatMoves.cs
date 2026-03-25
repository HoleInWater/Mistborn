using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatComboSystem : MonoBehaviour
{
    [Header("Combo Settings")]
    public float comboWindow = 0.5f;
    public float comboResetTime = 1f;
    public int maxCombo = 5;
    public float comboDamageMultiplier = 1.2f;
    public float comboSpeedMultiplier = 1.1f;

    [Header("Damage")]
    public float baseDamage = 25f;
    public float heavyDamageMultiplier = 2f;
    public float comboFinishDamage = 3f;

    [Header("References")]
    public Animator animator;
    public Rigidbody rb;
    public Transform attackPoint;
    public float attackRange = 2f;
    public LayerMask enemyLayer;

    private int currentCombo = 0;
    private float comboTimer = 0f;
    private bool canCombo = false;
    private bool isAttacking = false;
    private bool isHeavyAttacking = false;
    private float lastAttackTime;

    public System.Action<int> OnComboChanged;
    public System.Action OnComboFinish;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandleComboInput();
        UpdateComboTimer();
    }

    void HandleComboInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (canCombo)
            {
                ContinueCombo();
            }
            else if (!isAttacking)
            {
                StartCombo();
            }
        }

        if (Input.GetMouseButtonDown(1) && !isAttacking)
        {
            StartHeavyAttack();
        }
    }

    void StartCombo()
    {
        currentCombo = 1;
        isAttacking = true;
        canCombo = false;

        animator?.SetTrigger("Attack1");
        animator?.SetInteger("ComboCount", currentCombo);

        ApplyDamage();

        OnComboChanged?.Invoke(currentCombo);

        Debug.Log($"[COMBO] Started combo: {currentCombo}");
    }

    void ContinueCombo()
    {
        currentCombo++;
        canCombo = false;

        if (currentCombo > maxCombo)
        {
            FinishCombo();
            return;
        }

        animator?.SetInteger("ComboCount", currentCombo);
        animator?.SetTrigger($"Attack{currentCombo}");

        ApplyDamage();

        OnComboChanged?.Invoke(currentCombo);

        Debug.Log($"[COMBO] Continued combo: {currentCombo}");
    }

    void FinishCombo()
    {
        float damage = baseDamage * comboFinishDamage * currentCombo;

        animator?.SetTrigger("ComboFinish");

        ApplyDamage(damage);

        OnComboFinish?.Invoke();
        OnComboChanged?.Invoke(0);

        ResetCombo();

        Debug.Log($"[COMBO] Finished combo with {damage} damage");
    }

    void StartHeavyAttack()
    {
        isHeavyAttacking = true;
        isAttacking = true;

        animator?.SetTrigger("HeavyAttack");

        float damage = baseDamage * heavyDamageMultiplier;
        ApplyDamage(damage);

        StartCoroutine(HeavyAttackCooldown());

        Debug.Log($"[COMBO] Heavy attack: {damage} damage");
    }

    IEnumerator HeavyAttackCooldown()
    {
        yield return new WaitForSeconds(0.8f);
        isHeavyAttacking = false;
        isAttacking = false;
    }

    void ApplyDamage(float customDamage = -1)
    {
        Collider[] enemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayer);

        float damage = customDamage > 0 ? customDamage : baseDamage * Mathf.Pow(comboDamageMultiplier, currentCombo - 1);

        foreach (Collider enemy in enemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 knockDir = (enemy.transform.position - transform.position).normalized;
                enemyRb.AddForce(knockDir * 5f * currentCombo, ForceMode.Impulse);
            }
        }
    }

    void UpdateComboTimer()
    {
        if (!isAttacking) return;

        comboTimer += Time.deltaTime;

        if (comboTimer >= comboWindow && !canCombo)
        {
            canCombo = true;
        }

        if (comboTimer >= comboResetTime)
        {
            ResetCombo();
        }
    }

    public void AnimationEvent_EnableComboWindow()
    {
        canCombo = true;
        comboTimer = 0f;
    }

    public void AnimationEvent_DisableComboWindow()
    {
        canCombo = false;
    }

    public void AnimationEvent_AttackEnd()
    {
        isAttacking = false;
        isHeavyAttacking = false;
    }

    void ResetCombo()
    {
        currentCombo = 0;
        comboTimer = 0f;
        canCombo = false;
        isAttacking = false;
        isHeavyAttacking = false;
    }

    public int GetCurrentCombo() => currentCombo;
    public bool IsAttacking() => isAttacking;
    public bool IsHeavyAttacking() => isHeavyAttacking;
}

public class ParrySystem : MonoBehaviour
{
    [Header("Parry Settings")]
    public float parryWindow = 0.2f;
    public float parryCooldown = 0.5f;
    public float parryDamage = 50f;
    public float parryStunDuration = 1.5f;
    public float parryKnockback = 10f;

    [Header("Block Settings")]
    public float blockDamageReduction = 0.7f;
    public float blockStaminaCost = 10f;
    public float maxBlockAngle = 60f;

    [Header("References")]
    public Animator animator;
    public Rigidbody rb;
    public Transform shieldTransform;

    private bool isParrying = false;
    private bool isBlocking = false;
    private float parryTimer = 0f;
    private float lastParryTime;

    public System.Action OnParrySuccess;
    public System.Action OnBlockSuccess;
    public System.Action OnBlockFail;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandleParryInput();
        HandleBlockInput();
        UpdateParryWindow();
    }

    void HandleParryInput()
    {
        if (Input.GetKeyDown(KeyCode.F) && Time.time - lastParryTime >= parryCooldown)
        {
            StartParry();
        }
    }

    void HandleBlockInput()
    {
        if (Input.GetKey(KeyCode.F) && !isParrying)
        {
            if (!isBlocking) StartBlock();
        }
        else if (Input.GetKeyUp(KeyCode.F) && isBlocking)
        {
            EndBlock();
        }
    }

    void StartParry()
    {
        isParrying = true;
        parryTimer = parryWindow;
        lastParryTime = Time.time;

        animator?.SetTrigger("Parry");

        Debug.Log("[PARRY] Started parry window");
    }

    void StartBlock()
    {
        isBlocking = true;

        animator?.SetBool("IsBlocking", true);

        if (shieldTransform != null)
        {
            shieldTransform.gameObject.SetActive(true);
        }

        Debug.Log("[BLOCK] Started blocking");
    }

    void EndBlock()
    {
        isBlocking = false;

        animator?.SetBool("IsBlocking", false);

        if (shieldTransform != null)
        {
            shieldTransform.gameObject.SetActive(false);
        }

        Debug.Log("[BLOCK] Stopped blocking");
    }

    void UpdateParryWindow()
    {
        if (!isParrying) return;

        parryTimer -= Time.deltaTime;

        if (parryTimer <= 0)
        {
            isParrying = false;
        }
    }

    public bool AttemptParry(Vector3 attackDirection, float incomingDamage)
    {
        if (!isParrying) return false;

        float angle = Vector3.Angle(transform.forward, attackDirection);

        if (angle <= maxBlockAngle)
        {
            SuccessfulParry(attackDirection, incomingDamage);
            return true;
        }

        return false;
    }

    public bool AttemptBlock(Vector3 attackDirection, float incomingDamage)
    {
        if (!isBlocking) return false;

        float angle = Vector3.Angle(transform.forward, attackDirection);

        if (angle <= maxBlockAngle)
        {
            float reducedDamage = incomingDamage * (1 - blockDamageReduction);
            IDamageable damageable = GetComponent<IDamageable>();
            damageable?.TakeDamage(reducedDamage);

            OnBlockSuccess?.Invoke();

            Debug.Log($"[BLOCK] Blocked {incomingDamage} damage, took {reducedDamage}");

            return true;
        }

        OnBlockFail?.Invoke();
        return false;
    }

    void SuccessfulParry(Vector3 attackDirection, float incomingDamage)
    {
        isParrying = false;

        animator?.SetTrigger("ParrySuccess");

        Vector3 knockbackDir = -attackDirection.normalized;
        rb.AddForce(knockbackDir * parryKnockback, ForceMode.Impulse);

        OnParrySuccess?.Invoke();

        Debug.Log($"[PARRY] Parried successfully! {parryDamage} counter damage");
    }

    public void OnIncomingAttack(Vector3 attackDirection, float damage)
    {
        if (isParrying && AttemptParry(attackDirection, damage))
        {
            return;
        }

        if (isBlocking && AttemptBlock(attackDirection, damage))
        {
            return;
        }

        IDamageable damageable = GetComponent<IDamageable>();
        damageable?.TakeDamage(damage);
    }

    public bool IsParrying() => isParrying;
    public bool IsBlocking() => isBlocking;
}

public class DodgeRollSystem : MonoBehaviour
{
    [Header("Dodge Settings")]
    public float dodgeSpeed = 15f;
    public float dodgeDuration = 0.4f;
    public float dodgeCooldown = 0.5f;
    public float invincibilityDuration = 0.3f;
    public float staminaCost = 15f;

    [Header("References")]
    public Rigidbody rb;
    public Animator animator;
    public PlayerStamina stamina;

    private bool isDodging = false;
    private bool isInvincible = false;
    private float lastDodgeTime;
    private Vector3 dodgeDirection;

    public System.Action OnDodgeStart;
    public System.Action OnDodgeEnd;
    public System.Action OnDodgeSuccess;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();
        stamina = GetComponent<PlayerStamina>();
    }

    void Update()
    {
        if (isDodging) return;

        if (Time.time - lastDodgeTime < dodgeCooldown) return;

        if (Input.GetKeyDown(KeyCode.Space) && IsMoving())
        {
            if (stamina == null || stamina.currentStamina >= staminaCost)
            {
                StartDodge();
            }
        }
    }

    bool IsMoving()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        return Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
    }

    void StartDodge()
    {
        isDodging = true;
        lastDodgeTime = Time.time;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        dodgeDirection = new Vector3(h, 0, v).normalized;

        if (rb != null)
        {
            rb.linearVelocity = dodgeDirection * dodgeSpeed;
        }

        if (animator != null)
        {
            animator.SetTrigger("Dodge");
            animator.SetBool("IsDodging", true);
        }

        stamina?.UseStamina(staminaCost);

        StartCoroutine(DodgeRoutine());
        StartCoroutine(InvincibilityRoutine());

        OnDodgeStart?.Invoke();

        Debug.Log("[DODGE] Started dodge roll");
    }

    IEnumerator DodgeRoutine()
    {
        float elapsed = 0;

        while (elapsed < dodgeDuration)
        {
            elapsed += Time.deltaTime;

            if (rb != null)
            {
                rb.linearVelocity = dodgeDirection * dodgeSpeed;
            }

            yield return null;
        }

        EndDodge();
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
    }

    void EndDodge()
    {
        isDodging = false;

        if (animator != null)
        {
            animator.SetBool("IsDodging", false);
        }

        OnDodgeEnd?.Invoke();

        Debug.Log("[DODGE] Ended dodge roll");
    }

    public bool AttemptDodge(Vector3 attackDirection)
    {
        if (!isDodging) return false;

        OnDodgeSuccess?.Invoke();

        Debug.Log("[DODGE] Successfully dodged attack");

        return true;
    }

    public bool IsDodging() => isDodging;
    public bool IsInvincible() => isInvincible;
}