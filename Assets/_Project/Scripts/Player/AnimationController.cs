using UnityEngine;

/// <summary>
/// Animation parameter definitions and blend tree configurations
/// Used for setting up Animator Controllers via code
/// </summary>
public class AnimationConfig : MonoBehaviour
{
    [Header("Animation Parameters - Strings")]
    public static class Params
    {
        // Movement
        public const string Speed = "Speed";
        public const string VerticalVelocity = "VerticalVelocity";
        public const string IsGrounded = "IsGrounded";
        
        // Combat
        public const string IsAttacking = "IsAttacking";
        public const string AttackTrigger = "Attack";
        public const string HeavyAttackTrigger = "HeavyAttack";
        public const string ComboCount = "ComboCount";
        
        // Allomancy
        public const string IsBurning = "IsBurning";
        public const string CurrentMetal = "CurrentMetal";
        public const string IsFlaring = "IsFlaring";
        public const string MetalIntensity = "MetalIntensity";
        
        // Parkour / Assassin
        public const string IsWallRunning = "IsWallRunning";
        public const string IsVaulting = "IsVaulting";
        public const string IsRolling = "IsRolling";
        public const string IsSliding = "IsSliding";
        public const string IsClimbing = "IsClimbing";
        public const string IsInCover = "IsInCover";
        
        // State
        public const string IsDead = "IsDead";
        public const string IsHit = "IsHit";
        public const string HitTrigger = "Hit";
        
        // Stealth
        public const string IsCrouching = "IsCrouching";
        public const string IsSneaking = "IsSneaking";
        
        // Environment
        public const string IsSwimming = "IsSwimming";
        public const string WaterDepth = "WaterDepth";
    }

    [Header("Blend Tree Configurations")]
    public BlendTree horizontalMovementTree;
    public BlendTree verticalMovementTree;
    public BlendTree combatTree;
    public BlendTree allomancyTree;

    void Start()
    {
        CreateMovementBlendTrees();
    }

    void CreateMovementBlendTrees()
    {
        // Horizontal Movement Blend Tree
        // Idle, Walk, Run, Sprint
        AnimationBlendTree walkTree = new AnimationBlendTree();
        
        // Vertical Movement Blend Tree
        // Fall, Jump, Mid-air
        AnimationBlendTree jumpTree = new AnimationBlendTree();
    }

    /// <summary>
    /// Creates a locomotion blend tree with speed-based blending
    /// </summary>
    public static void SetupLocomotionBlendTree(Animator animator)
    {
        // This would typically be done in the Animator window
        // But we can configure parameters here
        animator.SetFloat(Params.Speed, 0, 0.1f, Time.deltaTime);
        animator.SetFloat(Params.VerticalVelocity, 0, 0.1f, Time.deltaTime);
        animator.SetBool(Params.IsGrounded, true);
    }

    /// <summary>
    /// Updates animation parameters based on player state
    /// </summary>
    public static void UpdatePlayerAnimations(Animator animator, BasicPlayerMove movement, Allomancer allomancer)
    {
        if (animator == null || movement == null) return;

        // Speed
        float currentSpeed = movement.GetComponent<Rigidbody>()?.linearVelocity.magnitude ?? 0;
        animator.SetFloat(Params.Speed, currentSpeed, 0.1f, Time.deltaTime);

        // Grounded
        animator.SetBool(Params.IsGrounded, movement.IsGrounded());

        // Allomancy
        if (allomancer != null)
        {
            animator.SetBool(Params.IsBurning, allomancer.IsBurning());
            animator.SetInteger(Params.CurrentMetal, (int)allomancer.GetCurrentMetal());
            
            bool isFlaring = FlareManager.Instance?.IsFlaring ?? false;
            animator.SetBool(Params.IsFlaring, isFlaring);
        }

        // Crouching
        bool isCrouching = Input.GetKey(KeyCode.LeftControl);
        animator.SetBool(Params.IsCrouching, isCrouching);
    }

    /// <summary>
    /// Triggers attack animation with combo support
    /// </summary>
    public static void TriggerAttack(Animator animator, int comboCount = 0)
    {
        if (animator == null) return;
        
        animator.SetInteger(Params.ComboCount, comboCount);
        animator.SetTrigger(Params.AttackTrigger);
    }

    /// <summary>
    /// Triggers hit reaction animation
    /// </summary>
    public static void TriggerHit(Animator animator, Vector3 fromDirection)
    {
        if (animator == null) return;

        animator.SetTrigger(Params.HitTrigger);
        animator.SetBool(Params.IsHit, true);

        // IsHit is reset by the animation state machine exit, not immediately here.
        // Animators should have a behaviour or transition condition to clear IsHit.
    }

    /// <summary>
    /// Triggers death animation
    /// </summary>
    public static void TriggerDeath(Animator animator)
    {
        if (animator == null) return;
        
        animator.SetBool(Params.IsDead, true);
        animator.SetTrigger(Params.HitTrigger); // Reuse hit trigger for death
    }
}

public class AnimationEventHandler : MonoBehaviour
{
    [Header("Animation Events")]
    public AudioSource audioSource;
    public AudioClip footstepSound;
    public AudioClip landSound;

    [Header("Footstep Timing")]
    public float walkFootstepInterval = 0.5f;
    public float runFootstepInterval = 0.3f;
    private float footstepTimer;

    void Update()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        float speed = rb.linearVelocity.magnitude;
        bool isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f);

        if (isGrounded && speed > 1f)
        {
            footstepTimer -= Time.deltaTime;
            
            if (footstepTimer <= 0)
            {
                footstepTimer = speed > 5f ? runFootstepInterval : walkFootstepInterval;
                PlayFootstep();
            }
        }
    }

    public void PlayFootstep()
    {
        if (audioSource != null && footstepSound != null)
        {
            audioSource.PlayOneShot(footstepSound);
        }
    }

    public void PlayLand()
    {
        if (audioSource != null && landSound != null)
        {
            audioSource.PlayOneShot(landSound);
        }
    }

    public void AnimationEvent_AttackStart()
    {
        // Enable hitbox during attack animation
        Debug.Log("[ANIM] Attack started");
    }

    public void AnimationEvent_AttackEnd()
    {
        // Disable hitbox after attack animation
        Debug.Log("[ANIM] Attack ended");
    }

    public void AnimationEvent_FootstepLeft()
    {
        PlayFootstep();
    }

    public void AnimationEvent_FootstepRight()
    {
        PlayFootstep();
    }

    public void AnimationEvent_Land()
    {
        PlayLand();
    }

    public void AnimationEvent_Jump()
    {
        Debug.Log("[ANIM] Jump");
    }

    public void AnimationEvent_AllomancyStart()
    {
        // Start particle effects
    }

    public void AnimationEvent_AllomancyEnd()
    {
        // Stop particle effects
    }
}

public class AnimationLayerManager : MonoBehaviour
{
    [Header("Layer Settings")]
    public int baseLayerIndex = 0;
    public int combatLayerIndex = 1;
    public int allomancyLayerIndex = 2;
    public int parkourLayerIndex = 3;
    public int upperBodyLayerIndex = 4;

    [Header("Layer Weights")]
    [Range(0, 1)] public float combatWeight = 1f;
    [Range(0, 1)] public float allomancyWeight = 1f;
    [Range(0, 1)] public float parkourWeight = 1f;
    [Range(0, 1)] public float upperBodyWeight = 1f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        InitializeLayers();
    }

    void InitializeLayers()
    {
        if (animator == null) return;

        animator.SetLayerWeight(combatLayerIndex, combatWeight);
        animator.SetLayerWeight(allomancyLayerIndex, allomancyWeight);
        animator.SetLayerWeight(parkourLayerIndex, parkourWeight);
        animator.SetLayerWeight(upperBodyLayerIndex, upperBodyWeight);
    }

    public void SetCombatWeight(float weight)
    {
        combatWeight = Mathf.Clamp01(weight);
        if (animator != null) animator.SetLayerWeight(combatLayerIndex, combatWeight);
    }

    public void SetAllomancyWeight(float weight)
    {
        allomancyWeight = Mathf.Clamp01(weight);
        if (animator != null) animator.SetLayerWeight(allomancyLayerIndex, allomancyWeight);
    }

    public void SetParkourWeight(float weight)
    {
        parkourWeight = Mathf.Clamp01(weight);
        if (animator != null) animator.SetLayerWeight(parkourLayerIndex, parkourWeight);
    }

    public void EnableCombatLayer()
    {
        SetCombatWeight(1f);
    }

    public void DisableCombatLayer()
    {
        SetCombatWeight(0f);
    }

    public void EnableAllomancyLayer()
    {
        SetAllomancyWeight(1f);
    }

    public void DisableAllomancyLayer()
    {
        SetAllomancyWeight(0f);
    }
}

public class IKHandler : MonoBehaviour
{
    [Header("IK Settings")]
    public bool enableIK = true;
    public float blendSpeed = 5f;

    [Header("Left Hand")]
    public Transform leftHandTarget;
    public float leftHandWeight = 1f;

    [Header("Right Hand")]
    public Transform rightHandTarget;
    public float rightHandWeight = 1f;

    [Header("Look At")]
    public Transform lookAtTarget;
    public float lookAtWeight = 1f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!enableIK || animator == null) return;

        // Look at target
        if (lookAtTarget != null)
        {
            animator.SetLookAtWeight(lookAtWeight);
            animator.SetLookAtPosition(lookAtTarget.position);
        }
        else
        {
            animator.SetLookAtWeight(0);
        }

        // Left hand
        if (leftHandTarget != null)
        {
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
        }

        // Right hand
        if (rightHandTarget != null)
        {
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandWeight);
        }
    }

    public void SetHandTarget(AvatarIKGoal hand, Transform target, float weight = 1f)
    {
        if (hand == AvatarIKGoal.LeftHand)
        {
            leftHandTarget = target;
            leftHandWeight = weight;
        }
        else if (hand == AvatarIKGoal.RightHand)
        {
            rightHandTarget = target;
            rightHandWeight = weight;
        }
    }

    public void ClearHandTarget(AvatarIKGoal hand)
    {
        if (hand == AvatarIKGoal.LeftHand)
        {
            leftHandTarget = null;
            leftHandWeight = 0;
        }
        else if (hand == AvatarIKGoal.RightHand)
        {
            rightHandTarget = null;
            rightHandWeight = 0;
        }
    }
}