using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Master animation controller for the enemy.
/// Reads from NavMeshAgent and EnemyAI to natively drive the Unity Animator.
/// Completely replaces the old PlayableGraph system.
/// </summary>
[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    private Animator _animator;
    private NavMeshAgent _navAgent;
    private EnemyAI _enemyAI;

    // Animator Hashes for performance
    private int _hashSpeed;
    private int _hashVelocity;
    private int _hashIsWalking;
    private int _hashIsRunning;
    private int _hashIsChasing;
    private int _hashIsAttacking;
    private int _hashIsPatrolling;
    private int _hashIsFleeing;
    private int _hashIsDead;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false; // NavAgent controls movement

        _hashSpeed        = Animator.StringToHash("Speed");
        _hashVelocity     = Animator.StringToHash("Velocity");
        _hashIsWalking    = Animator.StringToHash("IsWalking");
        _hashIsRunning    = Animator.StringToHash("IsRunning");
        _hashIsChasing    = Animator.StringToHash("IsChasing");
        _hashIsAttacking  = Animator.StringToHash("IsAttacking");
        _hashIsPatrolling = Animator.StringToHash("IsPatrolling");
        _hashIsFleeing    = Animator.StringToHash("IsFleeing");
        _hashIsDead       = Animator.StringToHash("IsDead");
    }

    void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _enemyAI  = GetComponent<EnemyAI>();
    }

    void Update()
    {
        SyncLocomotion();
    }

    private void SyncLocomotion()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null) return;

        float speed = 0f;
        if (_navAgent != null && _navAgent.isActiveAndEnabled)
            speed = _navAgent.velocity.magnitude;
        else
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null) speed = rb.linearVelocity.magnitude;
        }

        float runSpeed  = _enemyAI != null ? _enemyAI.runSpeed  : 2.4f;
        float walkSpeed = _enemyAI != null ? _enemyAI.moveSpeed : 1.4f;

        bool isMoving  = speed > 0.1f;
        bool isRunning = speed >= walkSpeed + 0.4f;

        // Drive float blends
        _animator.SetFloat(_hashSpeed, speed, 0.1f, Time.deltaTime);
        _animator.SetFloat(_hashVelocity, Mathf.Clamp01(speed / Mathf.Max(runSpeed, 0.1f)), 0.1f, Time.deltaTime);

        // Drive boolean state switches exactly as expected by the EnemyController architecture
        _animator.SetBool(_hashIsWalking, isMoving && !isRunning);
        _animator.SetBool(_hashIsRunning, isRunning);

        if (_enemyAI != null)
        {
            _animator.SetBool(_hashIsChasing,    _enemyAI.CurrentState == EnemyAI.State.Chase);
            _animator.SetBool(_hashIsPatrolling, _enemyAI.CurrentState == EnemyAI.State.Patrol);
            _animator.SetBool(_hashIsFleeing,    _enemyAI.CurrentState == EnemyAI.State.Flee);
            _animator.SetBool(_hashIsDead,       _enemyAI.IsDead);
            
            // Attacking is naturally driven by EnemyAI's state here
            _animator.SetBool(_hashIsAttacking,  _enemyAI.CurrentState == EnemyAI.State.Attack);
        }
    }

    // ── Public API — called by EnemyAI ────────────────────────────────────────

    public void PlayAttack()
    {
        if (_animator == null) return;
        _animator.SetBool(_hashIsAttacking, true);
    }

    public void PlayHeavyAttack()
    {
        if (_animator == null) return;
        _animator.SetBool(_hashIsAttacking, true); // Maps to Heavy eventually if broken out
    }

    public void PlayDeath()
    {
        if (_animator == null) return;
        _animator.SetBool(_hashIsDead, true);
    }
}
