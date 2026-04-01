using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>
/// Playables-based animation controller for the player.
/// Inspired by uPlayableAnimation (EricHu33, MIT licence) — rebuilt for Mistborn.
///
/// Architecture:
///   PlayableGraph
///     └─ AnimationLayerMixerPlayable (2 layers)
///           ├─ [0] AnimatorControllerPlayable  ← existing locomotion controller (full body)
///           └─ [1] AnimationMixerPlayable       ← combat one-shots (upper body if mask assigned)
///                   ├─ [0] Attack
///                   ├─ [1] HeavyAttack
///                   ├─ [2] Parry
///                   └─ [3] Block (loop)
///
/// animator.SetBool / SetFloat calls from animationStateController still work —
/// Unity forwards them to the AnimatorControllerPlayable inside the graph.
/// </summary>
[PlayerComponent("Animation", order: 5)]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Clips — assign from Mixamo or any humanoid pack")]
    [Tooltip("Light attack one-shot")]
    public AnimationClip attackClip;
    [Tooltip("Heavy attack one-shot")]
    public AnimationClip heavyAttackClip;
    [Tooltip("Parry one-shot")]
    public AnimationClip parryClip;
    [Tooltip("Block idle loop (plays while holding right-click)")]
    public AnimationClip blockLoopClip;

    [Header("Blend")]
    [Tooltip("Seconds to fade combat layer in")]
    public float fadeIn  = 0.08f;
    [Tooltip("Seconds to fade combat layer out")]
    public float fadeOut = 0.15f;

    [Header("Upper Body Mask")]
    [Tooltip("Assign an AvatarMask that covers Spine and above so legs keep running during combat.\n" +
             "Leave empty to blend full-body (locomotion will be overridden during attacks).")]
    public AvatarMask upperBodyMask;

    // ── Internals ─────────────────────────────────────────────────────────────

    private PlayableGraph              _graph;
    private AnimationLayerMixerPlayable _layerMixer;
    private AnimatorControllerPlayable  _locoPlayable;
    private AnimationMixerPlayable      _combatMixer;

    private AnimationClipPlayable _attackP;
    private AnimationClipPlayable _heavyP;
    private AnimationClipPlayable _parryP;
    private AnimationClipPlayable _blockP;

    private bool _hasAttack, _hasHeavy, _hasParry, _hasBlock;

    private enum CombatState { None, Attack, Heavy, Parry, Block }
    private CombatState _state = CombatState.None;

    // Layer 1 weight (0 = locomotion only, 1 = combat layer fully visible)
    private float _combatWeight;
    private float _targetCombatWeight;

    // Per-slot target weights inside the combat mixer
    private readonly float[] _slotTarget  = new float[4];
    private readonly float[] _slotCurrent = new float[4];

    private const int S_ATTACK = 0, S_HEAVY = 1, S_PARRY = 2, S_BLOCK = 3;

    private Animator _animator;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogWarning("[PlayerAnimationController] No Animator found — disabling.");
            enabled = false;
            return;
        }

        var locoCtrl = _animator.runtimeAnimatorController;
        if (locoCtrl == null)
        {
            Debug.LogWarning("[PlayerAnimationController] Animator has no RuntimeAnimatorController — locomotion layer skipped.");
        }

        // Disable root motion — let the CharacterController/Rigidbody handle all movement.
        // Without this, baked vertical motion in animation clips causes the player to float.
        _animator.applyRootMotion = false;

        // Build graph
        _graph = PlayableGraph.Create(gameObject.name + "_Anim");
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        // Root: 2-layer mixer
        _layerMixer = AnimationLayerMixerPlayable.Create(_graph, 2);

        // Layer 0 — locomotion
        if (locoCtrl != null)
        {
            _locoPlayable = AnimatorControllerPlayable.Create(_graph, locoCtrl);
            _graph.Connect(_locoPlayable, 0, _layerMixer, 0);
            _layerMixer.SetInputWeight(0, 1f);
        }

        // Layer 1 — combat mixer (4 fixed slots)
        _combatMixer = AnimationMixerPlayable.Create(_graph, 4);
        _graph.Connect(_combatMixer, 0, _layerMixer, 1);
        _layerMixer.SetInputWeight(1, 0f);

        if (upperBodyMask != null)
        {
            _layerMixer.SetLayerMaskFromAvatarMask(1, upperBodyMask);
            _layerMixer.SetLayerAdditive(1, false);
        }

        // Create clip playables (only for non-null clips)
        _hasAttack = TryCreateClip(attackClip,      S_ATTACK, out _attackP);
        _hasHeavy  = TryCreateClip(heavyAttackClip, S_HEAVY,  out _heavyP);
        _hasParry  = TryCreateClip(parryClip,       S_PARRY,  out _parryP);
        _hasBlock  = TryCreateClip(blockLoopClip,   S_BLOCK,  out _blockP);

        // Output to Animator
        var output = AnimationPlayableOutput.Create(_graph, "PlayerAnim", _animator);
        output.SetSourcePlayable(_layerMixer);

        _graph.Play();

        Debug.Log($"[PlayerAnimationController] Ready — " +
                  $"Attack={_hasAttack} Heavy={_hasHeavy} Parry={_hasParry} Block={_hasBlock}");
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
        for (int i = 0; i < 4; i++)
        {
            _slotCurrent[i] = Mathf.MoveTowards(_slotCurrent[i], _slotTarget[i], slotSpeed);
            _combatMixer.SetInputWeight(i, _slotCurrent[i]);
        }

        // Auto-exit one-shot clips when they finish
        CheckOneShotExit(_state == CombatState.Attack, _hasAttack, _attackP, attackClip);
        CheckOneShotExit(_state == CombatState.Heavy,  _hasHeavy,  _heavyP,  heavyAttackClip);
        CheckOneShotExit(_state == CombatState.Parry,  _hasParry,  _parryP,  parryClip);
    }

    void OnDestroy()
    {
        if (_graph.IsValid()) _graph.Destroy();
    }

    // ── Public API — called by PlayerCombat and ParrySystem ───────────────────

    public void PlayAttack()
    {
        if (!_hasAttack) { Debug.Log("[PlayerAnimationController] No attackClip assigned"); return; }
        _attackP.SetTime(0);
        ActivateSlot(S_ATTACK);
        _state = CombatState.Attack;
        Debug.Log("[PlayerAnimationController] Playing Attack");
    }

    public void PlayHeavyAttack()
    {
        if (!_hasHeavy) { Debug.Log("[PlayerAnimationController] No heavyAttackClip assigned"); return; }
        _heavyP.SetTime(0);
        ActivateSlot(S_HEAVY);
        _state = CombatState.Heavy;
        Debug.Log("[PlayerAnimationController] Playing HeavyAttack");
    }

    public void PlayParry()
    {
        if (!_hasParry) { Debug.Log("[PlayerAnimationController] No parryClip assigned"); return; }
        _parryP.SetTime(0);
        ActivateSlot(S_PARRY);
        _state = CombatState.Parry;
        Debug.Log("[PlayerAnimationController] Playing Parry");
    }

    public void SetBlocking(bool blocking)
    {
        if (blocking)
        {
            if (!_hasBlock) return;
            ActivateSlot(S_BLOCK);
            _state = CombatState.Block;
        }
        else if (_state == CombatState.Block)
        {
            ReturnToLoco();
        }
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
        for (int i = 0; i < 4; i++)
            _slotTarget[i] = (i == slot) ? 1f : 0f;
    }

    void ReturnToLoco()
    {
        _state = CombatState.None;
        _targetCombatWeight = 0f;
        for (int i = 0; i < 4; i++)
            _slotTarget[i] = 0f;
    }

    void CheckOneShotExit(bool isActive, bool hasClip, AnimationClipPlayable p, AnimationClip clip)
    {
        if (!isActive || !hasClip || clip == null) return;
        if (p.GetTime() >= clip.length - fadeOut)
            ReturnToLoco();
    }
}
