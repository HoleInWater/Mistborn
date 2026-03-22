using UnityEngine;
using System.Collections.Generic;

public class AnimationController : MonoBehaviour
{
    [Header("Animation States")]
    public Animator animator;
    public string currentState = "Idle";
    
    [Header("State Transitions")]
    public string idleState = "Idle";
    public string walkState = "Walk";
    public string runState = "Run";
    public string jumpState = "Jump";
    public string attackState = "Attack";
    public string hurtState = "Hurt";
    public string deathState = "Death";
    
    [Header("Blend Trees")]
    public bool useBlendTree = true;
    public string velocityXParameter = "VelocityX";
    public string velocityZParameter = "VelocityZ";
    public string speedParameter = "Speed";
    
    [Header("State Machine")]
    public StateMachine stateMachine;
    
    private Dictionary<string, AnimationState> states = new Dictionary<string, AnimationState>();
    
    void Start()
    {
        InitializeStates();
    }
    
    void InitializeStates()
    {
        states["Idle"] = new AnimationState(idleState, CanIdle);
        states["Walk"] = new AnimationState(walkState, CanWalk);
        states["Run"] = new AnimationState(runState, CanRun);
        states["Jump"] = new AnimationState(jumpState, CanJump);
        states["Attack"] = new AnimationState(attackState, CanAttack);
        states["Hurt"] = new AnimationState(hurtState, CanHurt);
        states["Death"] = new AnimationState(deathState, CanDie);
    }
    
    void Update()
    {
        UpdateAnimationState();
        UpdateBlendTree();
    }
    
    void UpdateAnimationState()
    {
        foreach (var state in states)
        {
            if (state.Value.canTransition())
            {
                ChangeState(state.Key);
                break;
            }
        }
    }
    
    void UpdateBlendTree()
    {
        if (!useBlendTree || animator == null) return;
        
        Vector3 velocity = Vector3.zero;
        
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            velocity = controller.velocity;
        }
        
        if (animator != null)
        {
            animator.SetFloat(velocityXParameter, velocity.x);
            animator.SetFloat(velocityZParameter, velocity.z);
            animator.SetFloat(speedParameter, velocity.magnitude);
        }
    }
    
    public void ChangeState(string newState)
    {
        if (currentState == newState) return;
        
        if (animator != null)
        {
            animator.SetTrigger(newState);
        }
        
        currentState = newState;
    }
    
    bool CanIdle() { return true; }
    bool CanWalk() { return false; }
    bool CanRun() { return false; }
    bool CanJump() { return false; }
    bool CanAttack() { return false; }
    bool CanHurt() { return false; }
    bool CanDie() { return false; }
    
    public void PlayAttack(int attackIndex)
    {
        if (animator != null)
        {
            animator.SetInteger("AttackIndex", attackIndex);
            animator.SetTrigger("Attack");
        }
    }
    
    public void PlayHurt()
    {
        ChangeState("Hurt");
        
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
    }
    
    public void PlayDeath()
    {
        ChangeState("Death");
        
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
    }
    
    public void SetFloat(string parameter, float value)
    {
        if (animator != null)
        {
            animator.SetFloat(parameter, value);
        }
    }
    
    public void SetBool(string parameter, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(parameter, value);
        }
    }
    
    public void SetTrigger(string parameter)
    {
        if (animator != null)
        {
            animator.SetTrigger(parameter);
        }
    }
}

public class AnimationState
{
    public string stateName;
    public System.Func<bool> canTransition;
    
    public AnimationState(string name, System.Func<bool> canTrans)
    {
        stateName = name;
        canTransition = canTrans;
    }
}

public class ComboAnimator : MonoBehaviour
{
    [Header("Combo Settings")]
    public int maxComboCount = 5;
    public float comboWindow = 2f;
    public float[] comboDelays;
    
    [Header("Animations")]
    public string[] attackAnimations;
    public string comboEndAnimation = "ComboEnd";
    
    private int currentCombo = 0;
    private float comboTimer = 0f;
    private bool isInCombo = false;
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (comboDelays == null || comboDelays.Length < maxComboCount)
        {
            comboDelays = new float[maxComboCount];
            for (int i = 0; i < maxComboCount; i++)
            {
                comboDelays[i] = 0.5f;
            }
        }
    }
    
    void Update()
    {
        if (isInCombo)
        {
            comboTimer -= Time.deltaTime;
            
            if (comboTimer <= 0f)
            {
                EndCombo();
            }
        }
    }
    
    public void Attack()
    {
        if (isInCombo && currentCombo >= maxComboCount)
        {
            return;
        }
        
        if (isInCombo)
        {
            currentCombo++;
            comboTimer = comboWindow;
        }
        else
        {
            currentCombo = 1;
            isInCombo = true;
            comboTimer = comboWindow;
        }
        
        PlayComboAnimation(currentCombo - 1);
    }
    
    void PlayComboAnimation(int index)
    {
        if (animator == null) return;
        
        if (index < attackAnimations.Length)
        {
            animator.SetInteger("ComboCount", currentCombo);
            animator.SetTrigger("ComboAttack");
        }
    }
    
    void EndCombo()
    {
        isInCombo = false;
        currentCombo = 0;
        
        if (animator != null)
        {
            animator.SetTrigger("ComboEnd");
        }
    }
    
    public int GetCurrentCombo()
    {
        return currentCombo;
    }
    
    public bool IsInCombo()
    {
        return isInCombo;
    }
}

public class AnimationEvents : MonoBehaviour
{
    public System.Action OnAttackHit;
    public System.Action OnFootstep;
    public System.Action OnAnimationComplete;
    
    public void AttackHit()
    {
        OnAttackHit?.Invoke();
    }
    
    public void Footstep()
    {
        OnFootstep?.Invoke();
    }
    
    public void AnimationComplete()
    {
        OnAnimationComplete?.Invoke();
    }
    
    public void PlaySound(string soundName)
    {
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        audioManager?.PlaySound(soundName);
    }
    
    public void SpawnEffect(string effectName)
    {
        Debug.Log($"Spawning effect: {effectName}");
    }
}
