using UnityEngine;
using System.Collections.Generic;

public class StealthSystem : MonoBehaviour
{
    [Header("Stealth Settings")]
    public float detectionRadius = 10f;
    public float detectionAngle = 90f;
    public float suspicionTime = 3f;
    public float alertTime = 5f;

    [Header("Player State")]
    public bool isCrouching = false;
    public bool isInShadow = false;
    public bool isBurningCopper = false;

    [Header("Visual Feedback")]
    public UnityEngine.UI.Image stealthIndicator;
    public Color hiddenColor = Color.green;
    public Color detectedColor = Color.red;
    public Color suspiciousColor = Color.yellow;

    private enum StealthState { Hidden, Suspicious, Detected }
    private StealthState currentState = StealthState.Hidden;
    private float suspicionTimer = 0f;

    void Update()
    {
        UpdateStealthState();
        UpdateVisuals();
    }

    void UpdateStealthState()
    {
        if (isBurningCopper)
        {
            currentState = StealthState.Hidden;
            return;
        }

        bool detected = IsPlayerDetected();

        if (detected)
        {
            currentState = StealthState.Detected;
            suspicionTimer = alertTime;
        }
        else if (suspicionTimer > 0f)
        {
            suspicionTimer -= Time.deltaTime;
            currentState = StealthState.Suspicious;
        }
        else
        {
            currentState = StealthState.Hidden;
        }
    }

    bool IsPlayerDetected()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius, LayerMask.GetMask("Enemy"));

        foreach (Collider enemy in enemies)
        {
            AICombat ai = enemy.GetComponent<AICombat>();
            if (ai != null)
            {
                Vector3 toPlayer = transform.position - enemy.transform.position;
                float angle = Vector3.Angle(enemy.transform.forward, toPlayer);

                bool inAngle = angle <= detectionAngle / 2f;
                bool canSee = !Physics.Linecast(enemy.transform.position, transform.position, LayerMask.GetMask("World"));
                bool crouchBonus = isCrouching && canSee;

                if (inAngle && (canSee || crouchBonus))
                {
                    if (!isInShadow || canSee)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    void UpdateVisuals()
    {
        if (stealthIndicator == null) return;

        switch (currentState)
        {
            case StealthState.Hidden:
                stealthIndicator.color = hiddenColor;
                break;
            case StealthState.Suspicious:
                stealthIndicator.color = suspiciousColor;
                break;
            case StealthState.Detected:
                stealthIndicator.color = detectedColor;
                break;
        }
    }

    public bool IsHidden() => currentState == StealthState.Hidden;
    public bool IsDetected() => currentState == StealthState.Detected;
    public float GetSuspicionLevel() => suspicionTimer / alertTime;

    public void SetCrouching(bool crouch) => isCrouching = crouch;
    public void SetInShadow(bool shadow) => isInShadow = shadow;
    public void SetCopperActive(bool active) => isBurningCopper = active;
}

public class AICombat : MonoBehaviour
{
    [Header("AI Settings")]
    public float detectionRange = 15f;
    public float attackRange = 3f;
    public float moveSpeed = 5f;
    public float attackDamage = 10f;
    public float attackRate = 1f;

    [Header("State")]
    public Transform target;
    public bool isAggressive = false;
    public float aggressionMultiplier = 1f;
    public float externalTimeScaleMultiplier = 1f;

    private float nextAttackTime = 0f;
    private UnityEngine.AI.NavMeshAgent navAgent;

    void Start()
    {
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime * externalTimeScaleMultiplier;

        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance <= attackRange)
            {
                Attack();
                if (navAgent != null) navAgent.isStopped = true;
            }
            else if (isAggressive)
            {
                if (navAgent != null)
                {
                    navAgent.isStopped = false;
                    navAgent.SetDestination(target.position);
                }
            }
        }
    }

    void Attack()
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + 1f / attackRate;

            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage * aggressionMultiplier);
            }
        }
    }

    public void SetAggressionMultiplier(float mult)
    {
        aggressionMultiplier = mult;
        isAggressive = mult > 1f;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}