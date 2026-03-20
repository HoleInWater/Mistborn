// ============================================================
// FILE: PlayerCamera.cs
// SYSTEM: Player
// STATUS: STUB — Not yet implemented
// AUTHOR: 
//
// PURPOSE:
//   Third-person camera controller following Assassin's Creed style.
//   Handles mouse look and smooth camera following.
//
// DEPENDENCIES:
//   - Target (Player) transform
//
// TODO:
//   - Add camera collision with environment geometry
//   - Implement camera shake for Allomantic effects
//   - Add smooth follow damping
//
// TODO (Team):
//   - Define default camera distance and height
//   - Set mouse sensitivity preference
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Camera Settings")]
        public Transform target;
        public float mouseSensitivity = 100f;
        public float cameraDistance = 5f;
        public float cameraHeight = 2f;
        public float followSmoothness = 10f;

        private float cameraYaw;
        private float cameraPitch;
        private Vector3 currentVelocity;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            cameraYaw = transform.eulerAngles.y;
            cameraPitch = transform.eulerAngles.x;
        }

        private void LateUpdate()
        {
            HandleMouseLook();
            UpdateCameraPosition();
        }

        private void HandleMouseLook()
        {
            cameraYaw += Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
            cameraPitch -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;
            cameraPitch = Mathf.Clamp(cameraPitch, -45f, 60f);
            
            transform.eulerAngles = new Vector3(cameraPitch, cameraYaw, 0f);
        }

        private void UpdateCameraPosition()
        {
            if (target == null) return;
            
            Vector3 targetPosition = target.position - transform.forward * cameraDistance;
            targetPosition.y += cameraHeight;
            
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                targetPosition, 
                ref currentVelocity, 
                1f / followSmoothness
            );
        }

        public void Shake(float intensity, float duration)
        {
            // TODO: Implement camera shake
        }
    }
}
