using UnityEngine;

namespace Mistborn.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float m_moveSpeed = 5f;
        [SerializeField] private float m_sprintSpeed = 8f;
        [SerializeField] private float m_jumpForce = 5f;
        [SerializeField] private float m_gravity = -20f;

        private CharacterController m_controller;
        private Vector3 m_velocity;
        private bool m_isSprinting;

        public Vector3 velocity => m_velocity;
        public bool isSprinting => m_isSprinting;

        private void Awake()
        {
            m_controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!m_controller.enabled) return;
            
            HandleInput();
            ApplyGravity();
            Move();
        }

        private void HandleInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            
            Vector3 direction = (transform.right * horizontal + transform.forward * vertical).normalized;
            
            m_isSprinting = Input.GetKey(KeyCode.LeftShift);
            float currentSpeed = m_isSprinting ? m_sprintSpeed : m_moveSpeed;
            
            m_velocity.x = direction.x * currentSpeed;
            m_velocity.z = direction.z * currentSpeed;
            
            if (Input.GetButtonDown("Jump") && IsGrounded)
            {
                m_velocity.y = m_jumpForce;
            }
        }

        private void ApplyGravity()
        {
            if (!IsGrounded)
            {
                m_velocity.y += m_gravity * Time.deltaTime;
            }
        }

        private void Move()
        {
            m_controller.Move(m_velocity * Time.deltaTime);
        }

        public bool IsGrounded => m_controller.isGrounded;

        /// <summary>
        /// Applies a vertical boost from Steelpush-assisted jump.
        /// </summary>
        public void ApplySteelpushBoost(float force)
        {
            m_velocity.y = force;
        }

        /// <summary>
        /// Freezes player movement (for cutscenes, etc.).
        /// </summary>
        public void FreezeMovement()
        {
            m_controller.enabled = false;
            m_velocity = Vector3.zero;
        }

        /// <summary>
        /// Unfreezes player movement.
        /// </summary>
        public void UnfreezeMovement()
        {
            m_controller.enabled = true;
        }
    }
}
