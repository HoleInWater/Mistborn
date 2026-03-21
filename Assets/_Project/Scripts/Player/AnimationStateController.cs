using UnityEngine;

namespace Mistborn.Player
{
    public class AnimationStateController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator m_animator;
        [SerializeField] private CharacterController m_characterController;

        [Header("Animation Settings")]
        [SerializeField] private float m_directionSmoothTime = 0.1f;
        [SerializeField] private float m_landingBlendDuration = 0.2f;

        [Header("Layer Masks")]
        [SerializeField] private int m_baseLayer = 0;
        [SerializeField] private int m_combatLayer = 1;
        [SerializeField] private int m_allomancyLayer = 2;

        private float m_currentDirection;
        private float m_directionVelocity;
        private bool m_isGrounded;
        private bool m_wasGrounded;
        private float m_groundedTime;

        private static readonly int AnimIDSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimIDGrounded = Animator.StringToHash("Grounded");
        private static readonly int AnimIDJump = Animator.StringToHash("Jump");
        private static readonly int AnimIDFreeFall = Animator.StringToHash("FreeFall");
        private static readonly int AnimIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        private static readonly int AnimIDAttack = Animator.StringToHash("Attack");
        private static readonly int AnimIDDodge = Animator.StringToHash("Dodge");
        private static readonly int AnimIDBlock = Animator.StringToHash("Block");
        private static readonly int AnimIDSteelPush = Animator.StringToHash("SteelPush");
        private static readonly int AnimIDIronPull = Animator.StringToHash("IronPull");
        private static readonly int AnimIDPewter = Animator.StringToHash("Pewter");
        private static readonly int AnimIDTin = Animator.StringToHash("Tin");

        private void Start()
        {
            if (m_animator == null)
                m_animator = GetComponent<Animator>();

            if (m_characterController == null)
                m_characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            UpdateGroundCheck();
            UpdateAnimationParameters();
            UpdateCombatAnimations();
            UpdateAllomancyAnimations();
        }

        private void UpdateGroundCheck()
        {
            m_wasGrounded = m_isGrounded;
            m_isGrounded = m_characterController != null && m_characterController.isGrounded;

            if (m_isGrounded)
            {
                m_groundedTime = 0f;
            }
            else
            {
                m_groundedTime += Time.deltaTime;
            }
        }

        private void UpdateAnimationParameters()
        {
            if (m_animator == null) return;

            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            float inputMagnitude = input.magnitude;

            m_animator.SetFloat(AnimIDSpeed, inputMagnitude, 0.1f, Time.deltaTime);

            if (inputMagnitude > 0.1f)
            {
                float targetDirection = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetDirection, ref m_directionVelocity, m_directionSmoothTime);
                float delta = Mathf.DeltaAngle(transform.eulerAngles.y, angle);
                float normalizedDelta = delta / 180f;

                m_currentDirection = Mathf.Lerp(m_currentDirection, normalizedDelta, Time.deltaTime * 10f);
                m_animator.SetFloat(AnimIDMotionSpeed, Mathf.Abs(m_currentDirection));
            }
            else
            {
                m_currentDirection = Mathf.Lerp(m_currentDirection, 0f, Time.deltaTime * 5f);
                m_animator.SetFloat(AnimIDMotionSpeed, 0f);
            }

            m_animator.SetBool(AnimIDGrounded, m_isGrounded);

            if (!m_wasGrounded && m_isGrounded)
            {
                m_animator.SetFloat(AnimIDFreeFall, 0f);
            }
            else if (!m_isGrounded && m_groundedTime > 0.1f)
            {
                m_animator.SetFloat(AnimIDFreeFall, 1f);
            }
        }

        private void UpdateCombatAnimations()
        {
            if (m_animator == null) return;

            if (Input.GetMouseButtonDown(0))
            {
                m_animator.SetTrigger(AnimIDAttack);
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                m_animator.SetTrigger(AnimIDDodge);
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                m_animator.SetBool(AnimIDBlock, true);
            }
            else
            {
                m_animator.SetBool(AnimIDBlock, false);
            }
        }

        private void UpdateAllomancyAnimations()
        {
            if (m_animator == null) return;

            AllomancerController allomancy = GetComponent<AllomancerController>();
            if (allomancy == null) return;

            bool steelPush = allomancy.IsBurning(Mistborn.Allomancy.AllomanticMetal.Steel);
            bool ironPull = allomancy.IsBurning(Mistborn.Allomancy.AllomanticMetal.Iron);
            bool pewter = allomancy.IsBurning(Mistborn.Allomancy.AllomanticMetal.Pewter);
            bool tin = allomancy.IsBurning(Mistborn.Allomancy.AllomanticMetal.Tin);

            m_animator.SetBool(AnimIDSteelPush, steelPush);
            m_animator.SetBool(AnimIDIronPull, ironPull);
            m_animator.SetBool(AnimIDPewter, pewter);
            m_animator.SetBool(AnimIDTin, tin);
        }

        public void TriggerAttack()
        {
            if (m_animator != null)
                m_animator.SetTrigger(AnimIDAttack);
        }

        public void TriggerJump()
        {
            if (m_animator != null)
                m_animator.SetTrigger(AnimIDJump);
        }

        public void TriggerDodge()
        {
            if (m_animator != null)
                m_animator.SetTrigger(AnimIDDodge);
        }

        public void SetLayerWeight(int layerIndex, float weight)
        {
            if (m_animator != null)
                m_animator.SetLayerWeight(layerIndex, weight);
        }

        public void CrossFadeInFixedTime(string stateName, float fixedTransitionDuration = 0.1f)
        {
            if (m_animator != null)
                m_animator.CrossFadeInFixedTime(stateName, fixedTransitionDuration);
        }
    }
}
