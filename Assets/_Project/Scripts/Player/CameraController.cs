// ============================================================
// FILE: CameraController.cs
// SYSTEM: Player
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Advanced camera with collision, shake, and cutscene support.
//
// TODO:
//   - Add camera collision
//   - Add head bob
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Player
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;
        public Vector3 offset = new Vector3(0, 2, -5);
        
        [Header("Rotation")]
        public float mouseSensitivity = 100f;
        public float minPitch = -45f;
        public float maxPitch = 60f;
        
        [Header("Follow")]
        public float followSmoothness = 10f;
        public float rotationSmoothness = 10f;
        
        [Header("Collision")]
        public bool enableCollision = true;
        public float collisionRadius = 0.3f;
        public LayerMask collisionLayers;
        
        [Header("Effects")]
        public bool enableFOVKick = true;
        public float sprintFOV = 75f;
        public float normalFOV = 60f;
        public float fovLerpSpeed = 5f;
        
        private float cameraYaw;
        private float cameraPitch;
        private Vector3 currentVelocity;
        private Camera cam;
        
        private void Start()
        {
            cam = GetComponent<Camera>();
            
            if (target != null)
            {
                cameraYaw = target.eulerAngles.y;
                cameraPitch = transform.eulerAngles.x;
            }
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void LateUpdate()
        {
            HandleMouseLook();
            UpdateCameraPosition();
            UpdateFOV();
        }
        
        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;
            
            cameraYaw += mouseX;
            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);
        }
        
        private void UpdateCameraPosition()
        {
            if (target == null) return;
            
            // Calculate rotation
            Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
            
            // Calculate desired position
            Vector3 desiredPosition = target.position + rotation * offset;
            
            // Handle collision
            if (enableCollision)
            {
                desiredPosition = HandleCollision(target.position, desiredPosition);
            }
            
            // Smooth follow
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref currentVelocity,
                1f / followSmoothness
            );
            
            // Look at target
            Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                Time.deltaTime * rotationSmoothness
            );
        }
        
        private Vector3 HandleCollision(Vector3 targetPos, Vector3 desiredPos)
        {
            Vector3 direction = desiredPos - targetPos;
            float distance = direction.magnitude;
            direction = direction.normalized;
            
            if (Physics.SphereCast(targetPos, collisionRadius, direction, out RaycastHit hit, distance, collisionLayers))
            {
                return targetPos + direction * (hit.distance - collisionRadius * 0.5f);
            }
            
            return desiredPos;
        }
        
        private void UpdateFOV()
        {
            if (!enableFOVKick || cam == null) return;
            
            float targetFOV = normalFOV;
            
            if (Input.GetKey(KeyCode.LeftShift))
            {
                targetFOV = sprintFOV;
            }
            
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);
        }
        
        public void Shake(float intensity, float duration)
        {
            StartCoroutine(ShakeRoutine(intensity, duration));
        }
        
        private System.Collections.IEnumerator ShakeRoutine(float intensity, float duration)
        {
            float elapsed = 0;
            Vector3 originalPos = transform.localPosition;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;
                
                transform.localPosition = originalPos + new Vector3(x, y, 0);
                
                yield return null;
            }
            
            transform.localPosition = originalPos;
        }
        
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                cameraYaw = target.eulerAngles.y;
            }
        }
    }
}
