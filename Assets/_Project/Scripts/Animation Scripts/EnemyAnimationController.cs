using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>
/// Playables-based animation controller for enemies.
/// Mirrors PlayerAnimationController (combat layer) + animationStateController (locomotion).
///
/// Architecture:
///   PlayableGraph
///     └─ AnimationLayerMixerPlayable (2 layers)
///           ├─ [0] AnimatorControllerPlayable  ← locomotion Animator Controller (NavAgent velocity)
///           └─ [1] AnimationMixerPlayable       ← combat one-shots
///                   ├─ [0] Attack
///                   ├─ [1] HeavyAttack
///                   └─ [2] Death (loop)
///
/// Locomotion is driven by NavMeshAgent velocity magnitude rather than Input keys,
/// so the Animator Controller transitions (Idle → Walk → Run) work the same as the
/// player's but respond to AI movement instead of keystrokes.
/// </summary>
public class EnemyAnimationController : MonoBehaviour
{
    [Header("Clips — assign from Mixamo or any humanoid pack")]
    [Tooltip("Light attack one-shot")]
    public AnimationClip attackClip;
    [Tooltip("Heavy attack one-shot")]
    public AnimationClip heavyAttackClip;
    [Tooltip("Death loop (played until the GameObject is destroyed)")]
    public AnimationClip deathClip;

    [Header("Blend")]
    [Tooltip("Seconds to fade combat layer in")]
    public float fadeIn  = 0.08f;
    [Tooltip("Seconds to fade combat layer out")]
    public float fadeOut = 0.15f;

    [Header("Upper Body Mask (optional)")]
    [Tooltip("Assign an AvatarMask covering Spine and above to keep legs walking during attacks.\n" +
             "Leave empty for full-body blend.")]
    public AvatarMask upperBodyMask;

    // ── Internals ─────────────────────────────────────────────────────────────

    private PlayableGraph               _graph;
    private AnimationLayerMixerPlayable _layerMixer;
    private AnimatorControllerPlayable  _locoPlayable;
    private AnimationMixerPlayable      _combatMixer;

    private AnimationClipPlayable _attackP;
    private AnimationClipPlayable _heavyP;
    private AnimationClipPlayable _deathP;

    private bool _hasAttack, _hasHeavy, _hasDeath;

    private enum CombatState { None, Attack, Heavy, Death }
    private CombatState _combatState = CombatState.None;

    private float _combatWeight;
    private float _targetCombatWeight;

    private readonly float[] _slotTarget  = new float[3];
    private readonly float[] _slotCurrent = new float[3];

    private const int S_ATTACK = 0, S_HEAVY = 1, S_DEATH = 2;

    private Animator     _animator;
    private NavMeshAgent _navAgent;
    private EnemyAI      _enemyAI;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogWarning("[EnemyAnimationController] No Animator found — disabling.");
            enabled = false;
            return;
        }

        var locoCtrl = _animator.runtimeAnimatorController;
        if (locoCtrl == null)
            Debug.LogWarning("[EnemyAnimationController] Animator has no RuntimeAnimatorController — locomotion layer skipped.");

        _animator.applyRootMotion = false;

        _graph = PlayableGraph.Create(gameObject.name + "_EnemyAnim");
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        _layerMixer = AnimationLayerMixerPlayable.Create(_graph, 2);

        // Layer 0 — locomotion AnimatorController
        if (locoCtrl != null)
        {
            _locoPlayable = AnimatorControllerPlayable.Create(_graph, locoCtrl);
            _graph.Connect(_locoPlayable, 0, _layerMixer, 0);
            _layerMixer.SetInputWeight(0, 1f);
        }

        // Layer 1 — combat (3 fixed slots)
        _combatMixer = AnimationMixerPlayable.Create(_graph, 3);
        _graph.Connect(_combatMixer, 0, _layerMixer, 1);
        _layerMixer.SetInputWeight(1, 0f);

        if (upperBodyMask != null)
        {
            _layerMixer.SetLayerMaskFromAvatarMask(1, upperBodyMask);
            _layerMixer.SetLayerAdditive(1, false);
        }

        _hasAttack = TryCreateClip(attackClip,      S_ATTACK, out _attackP);
        _hasHeavy  = TryCreateClip(heavyAttackClip, S_HEAVY,  out _heavyP);
        _hasDeath  = TryCreateClip(deathClip,       S_DEATH,  out _deathP);

        var output = AnimationPlayableOutput.Create(_graph, "EnemyAnim", _animator);
        output.SetSourcePlayable(_layerMixer);

        _graph.Play();

        Debug.Log($"[EnemyAnimationController] {name} — " +
                  $"Attack={_hasAttack} Heavy={_hasHeavy} Death={_hasDeath}");
    }

    void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _enemyAI  = GetComponent<EnemyAI>();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Fade combat layer weight
        float weightSpeed = (_targetCombatWeight > _combatWeight ? fadeIn : fadeOut);
        weightSpeed = weightSpeed > 0f ? dt / weightSpeed : 1f;
        _combatWeight = Mathf.MoveTowards(_combatWeight, _targetCombatWeight, weightSpeed);
        _layerMixer.SetInputWeight(1, _combatWeight);

        // Fade per-slot weights
        float slotSpeed = fadeIn > 0f ? dt / fadeIn : 1f;
        for (int i = 0; i < 3; i++)
        {
            _slotCurrent[i] = Mathf.MoveTowards(_slotCurrent[i], _slotTarget[i], slotSpeed);
            _combatMixer.SetInputWeight(i, _slotCurrent[i]);
        }

        // Auto-exit one-shot clips when they finish
        CheckOneShotExit(_combatState == CombatState.Attack, _hasAttack, _attackP, attackClip);
        CheckOneShotExit(_combatState == CombatState.Heavy,  _hasHeavy,  _heavyP,  heavyAttackClip);
        // Death clip loops — no auto-exit

        SyncLocoParams();
    }

    /// <summary>
    /// Forwards locomotion state to the AnimatorControllerPlayable inside the graph.
    /// Reads velocity from NavMeshAgent (falls back to Rigidbody) — no Input dependency.
    /// </summary>
    void SyncLocoParams()
    {
        if (!_locoPlayable.IsValid()) return;

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

        // Derive state booleans — mirrors EnemyAI.UpdateAnimations but writes to _locoPlayable
        bool isDead        = _enemyAI != null && _enemyAI.IsDead;
        bool isChasing     = _enemyAI != null && _enemyAI.CurrentState == EnemyAI.State.Chase;
        bool isAttacking   = _combatState == CombatState.Attack || _combatState == CombatState.Heavy;
        bool isPatrolling  = _enemyAI != null && _enemyAI.CurrentState == EnemyAI.State.Patrol;
        bool isFleeing     = _enemyAI != null && _enemyAI.CurrentState == EnemyAI.State.Flee;

        _locoPlayable.SetFloat("Speed",    speed, 0.1f, Time.deltaTime);
        _locoPlayable.SetFloat("Velocity", Mathf.Clamp01(speed / Mathf.Max(runSpeed, 0.1f)), 0.1f, Time.deltaTime);

        _locoPlayable.SetBool("IsWalking",      isMoving && !isRunning);
        _locoPlayable.SetBool("IsRunning",      isRunning);
        _locoPlayable.SetBool("IsChasing",      isChasing);
        _locoPlayable.SetBool("IsAttacking",    isAttacking);
        _locoPlayable.SetBool("IsPatrolling",   isPatrolling);
        _locoPlayable.SetBool("IsFleeing",      isFleeing);
        _locoPlayable.SetBool("IsDead",         isDead);

        // Also mirror to the raw Animator so external systems reading animator params still work
        _animator.SetFloat("Speed",    speed, 0.1f, Time.deltaTime);
        _animator.SetFloat("Velocity", Mathf.Clamp01(speed / Mathf.Max(runSpeed, 0.1f)), 0.1f, Time.deltaTime);
        _animator.SetBool("IsWalking",      isMoving && !isRunning);
        _animator.SetBool("IsRunning",      isRunning);
        _animator.SetBool("IsChasing",      isChasing);
        _animator.SetBool("IsAttacking",    isAttacking);
        _animator.SetBool("IsPatrolling",   isPatrolling);
        _animator.SetBool("IsFleeing",      isFleeing);
        _animator.SetBool("IsDead",         isDead);
    }

    void OnDestroy()
    {
        if (_graph.IsValid()) _graph.Destroy();
    }

    // ── Public API — called by EnemyAI ────────────────────────────────────────

    public void PlayAttack()
    {
        if (!_hasAttack) { Debug.Log($"[EnemyAnimationController] {name}: no attackClip"); return; }
        _attackP.SetTime(0);
        ActivateSlot(S_ATTACK);
        _combatState = CombatState.Attack;
    }

    public void PlayHeavyAttack()
    {
        if (!_hasHeavy) { Debug.Log($"[EnemyAnimationController] {name}: no heavyAttackClip"); return; }
        _heavyP.SetTime(0);
        ActivateSlot(S_HEAVY);
        _combatState = CombatState.Heavy;
    }

    public void PlayDeath()
    {
        if (!_hasDeath) { Debug.Log($"[EnemyAnimationController] {name}: no deathClip"); return; }
        _deathP.SetTime(0);
        ActivateSlot(S_DEATH);
        _combatState = CombatState.Death;
        _targetCombatWeight = 1f; // death never fades out
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    bool TryCreateClip(AnimationClip clip, int slot, out AnimationClipPlayable playable)
    {
        if (clip == null) { playable = default; return false; }
        playable = AnimationClipPlayable.Create(_graph, clip);
        _graph.Connect(playable, 0, _combatMixer, slot);
        _combatMixer.SetInputWeight(slot, 0f);
        return true;
    }

    void ActivateSlot(int slot)
    {
        _targetCombatWeight = 1f;
        for (int i = 0; i < 3; i++)
            _slotTarget[i] = (i == slot) ? 1f : 0f;
    }

    void ReturnToLoco()
    {
        _combatState = CombatState.None;
        _targetCombatWeight = 0f;
        for (int i = 0; i < 3; i++)
            _slotTarget[i] = 0f;
    }

    void CheckOneShotExit(bool isActive, bool hasClip, AnimationClipPlayable p, AnimationClip clip)
    {
        if (!isActive || !hasClip || clip == null) return;
        if (p.GetTime() >= clip.length - fadeOut)
            ReturnToLoco();
    }
}
