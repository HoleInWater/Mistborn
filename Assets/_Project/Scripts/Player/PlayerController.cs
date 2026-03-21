using UnityEngine;

namespace Mistborn.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float m_moveSpeed = 5f;
        [SerializeField] private float m_sprintSpeed = 8f;
        [SerializeField] private float m_jumpForce = 5f;
        [SerializeField] private float m_gravity = -20f;

        [Header("Ground Check")]
        [SerializeField] private Transform m_groundCheck;
        [SerializeField] private float m_groundDistance = 0.2f;
        [SerializeField] private LayerMask m_groundMask;

        private CharacterController m_controller;
        private Vector3 m_velocity;
        private bool m_isGrounded;
        private float m_currentSpeed;

        private void Awake()
        {
            m_controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            GroundCheck();
            Move();
            Jump();
        }

        private void GroundCheck()
        {
            if (m_groundCheck == null) return;
            m_isGrounded = Physics.CheckSphere(m_groundCheck.position, m_groundDistance, m_groundMask);
        }

        private void Move()
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 move = transform.right * x + transform.forward * z;

            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            m_currentSpeed = isSprinting ? m_sprintSpeed : m_moveSpeed;

            m_controller.Move(move * m_currentSpeed * Time.deltaTime);
        }

        private void Jump()
        {
            if (Input.GetButtonDown("Jump") && m_isGrounded)
            {
                m_velocity.y = Mathf.Sqrt(m_jumpForce * -2f * m_gravity);
            }

            m_velocity.y += m_gravity * Time.deltaTime;
            m_controller.Move(m_velocity * Time.deltaTime);
        }

        public bool IsGrounded => m_isGrounded;
        public float CurrentSpeed => m_currentSpeed;
    }
}
