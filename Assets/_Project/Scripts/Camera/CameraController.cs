using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Camera
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 5, -10);
        
        [Header("Rotation")]
        [SerializeField] private bool enableRotation = true;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float minVerticalAngle = -30f;
        [SerializeField] private float maxVerticalAngle = 60f;
        
        [Header("Zoom")]
        [SerializeField] private bool enableZoom = true;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 20f;
        
        [Header("Follow")]
        [SerializeField] private bool followTarget = true;
        [SerializeField] private float followSpeed = 10f;
        [SerializeField] private float followThreshold = 0.1f;
        
        [Header("Collision")]
        [SerializeField] private bool enableCollision = true;
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private LayerMask collisionLayers;
        
        [Header("FOV")]
        [SerializeField] private bool enableDynamicFOV = true;
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float sprintFOV = 70f;
        [SerializeField] private float zoomFOV = 50f;
        
        [Header("Shake")]
        [SerializeField] private float shakeDuration = 0.2f;
        [SerializeField] private float shakeIntensity = 0.5f;
        
        private float currentRotationX = 0f;
        private float currentRotationY = 0f;
        private float currentZoom = 10f;
        private Vector3 currentOffset;
        
        private bool isShaking = false;
        private float shakeStartTime = 0f;
        private Vector3 shakeOffset;
        
        private Camera cam;
        
        public static CameraController instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = gameObject.AddComponent<Camera>();
            }
            
            currentOffset = offset;
            currentZoom = offset.magnitude;
        }
        
        private void Start()
        {
            if (target == null)
            {
                PlayerStats player = FindObjectOfType<PlayerStats>();
                if (player != null)
                {
                    target = player.transform;
                }
            }
            
            if (collisionLayers == 0)
            {
                collisionLayers = ~0;
            }
        }
        
        private void LateUpdate()
        {
            if (target == null)
                return;
            
            HandleRotationInput();
            HandleZoomInput();
            
            UpdateCameraPosition();
            
            if (enableDynamicFOV)
            {
                UpdateFOV();
            }
            
            if (isShaking)
            {
                UpdateShake();
            }
        }
        
        private void HandleRotationInput()
        {
            if (!enableRotation)
                return;
            
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
            
            currentRotationY += mouseX;
            currentRotationX -= mouseY;
            currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);
        }
        
        private void HandleZoomInput()
        {
            if (!enableZoom)
                return;
            
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            
            if (Mathf.Abs(scrollWheel) > 0.01f)
            {
                currentZoom -= scrollWheel * zoomSpeed;
                currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            }
        }
        
        private void UpdateCameraPosition()
        {
            Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
            
            Vector3 direction = rotation * Vector3.back;
            Vector3 desiredPosition = target.position + direction * currentZoom;
            desiredPosition += offset * (currentZoom / 10f);
            
            if (enableCollision)
            {
                desiredPosition = HandleCollision(target.position, desiredPosition);
            }
            
            if (followTarget)
            {
                transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = desiredPosition;
            }
            
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }
        
        private Vector3 HandleCollision(Vector3 targetPos, Vector3 desiredPos)
        {
            Vector3 direction = desiredPos - targetPos;
            float distance = direction.magnitude;
            direction.Normalize();
            
            RaycastHit hit;
            if (Physics.SphereCast(targetPos, collisionRadius, direction, out hit, distance, collisionLayers))
            {
                Vector3 newPos = targetPos + direction * (hit.distance - collisionRadius);
                return newPos;
            }
            
            return desiredPos;
        }
        
        private void UpdateFOV()
        {
            if (cam == null)
                return;
            
            float targetFOV = normalFOV;
            
            PlayerStats player = target?.GetComponent<PlayerStats>();
            if (player != null && player.IsSprinting)
            {
                targetFOV = sprintFOV;
            }
            
            if (currentZoom < minZoom * 1.5f)
            {
                targetFOV = zoomFOV;
            }
            
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 5f);
        }
        
        public void Shake(float duration, float intensity)
        {
            shakeDuration = duration;
            shakeIntensity = intensity;
            isShaking = true;
            shakeStartTime = Time.time;
        }
        
        private void UpdateShake()
        {
            float elapsed = Time.time - shakeStartTime;
            
            if (elapsed >= shakeDuration)
            {
                isShaking = false;
                shakeOffset = Vector3.zero;
                return;
            }
            
            float remaining = 1f - (elapsed / shakeDuration);
            float intensity = shakeIntensity * remaining;
            
            shakeOffset = new Vector3(
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity)
            );
            
            transform.position += shakeOffset * Time.deltaTime;
        }
        
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        public void SetRotation(float x, float y)
        {
            currentRotationX = x;
            currentRotationY = y;
        }
        
        public void SetZoom(float zoom)
        {
            currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        }
        
        public float GetCurrentZoom()
        {
            return currentZoom;
        }
        
        public void ResetToDefault()
        {
            currentRotationX = 20f;
            currentRotationY = 0f;
            currentZoom = 10f;
        }
        
        public void LookAtPoint(Vector3 point)
        {
            Vector3 direction = point - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            currentRotationY = targetRotation.eulerAngles.y;
            currentRotationX = targetRotation.eulerAngles.x;
        }
    }
}
