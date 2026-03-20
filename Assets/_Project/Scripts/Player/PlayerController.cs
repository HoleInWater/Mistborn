// ============================================================
// FILE: PlayerController.cs
// SYSTEM: Player
// STATUS: STUB — Not yet implemented
// AUTHOR: 
//
// PURPOSE:
//   Third-person character controller for the player.
//   Handles basic movement, jumping, sprinting with CharacterController.
//
// DEPENDENCIES:
//   - CharacterController component required
//   - AllomancerController for Steelpush-assisted jumps
//
// TODO (AI Agent):
//   - Integrate with Allomancer for Steelpush-assisted jumps
//   - Add crouch functionality
//   - Implement ledge detection
//
// TODO (Team):
//   - Define movement speed values
//   - Choose input scheme preferences
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float sprintSpeed = 8f;
        public float jumpForce = 5f;
        public float gravity = -20f;

        private CharacterController characterController;
        private Vector3 velocity;
        private bool isSprinting = false;
        private bool isGrounded = true;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            isGrounded = characterController.isGrounded;
            
            HandleMovement();
            HandleJump();
            HandleSprint();
            
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        public void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            
            Vector3 direction = transform.right * horizontal + transform.forward * vertical;
            direction = direction.normalized;
            
            float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
            characterController.Move(direction * currentSpeed * Time.deltaTime);
        }

        public void HandleJump()
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = jumpForce;
            }
        }

        public void HandleSprint()
        {
            isSprinting = Input.GetKey(KeyCode.LeftShift);
        }

        public void ApplySteelpushBoost(float force)
        {
            velocity.y = force;
        }
    }
}
