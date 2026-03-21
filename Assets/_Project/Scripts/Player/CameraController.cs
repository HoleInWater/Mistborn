using UnityEngine;

namespace Mistborn.Player
{
    /// <summary>
    /// Third-person camera controller with mouse look, collision, and FOV kick.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform m_target;
        [SerializeField] private Vector3 m_offset = new Vector3(0, 2, -5);
        
        [Header("Rotation")]
        [Range(10f, 500f)]
        [SerializeField] private float m_mouseSensitivity = 100f;
        [Range(-90f, 0f)]
        [SerializeField] private float m_minPitch = -45f;
        [Range(0f, 90f)]
        [SerializeField] private float m_maxPitch = 60f;
        
        [Header("Follow")]
        [Range(1f, 30f)]
        [SerializeField] private float m_followSmoothness = 10f;
        [Range(1f, 30f)]
        [SerializeField] private float m_rotationSmoothness = 10f;
        
        [Header("Collision")]
        [SerializeField] private bool m_enableCollision = true;
        [SerializeField] private float m_collisionRadius = 0.3f;
        [SerializeField] private LayerMask m_collisionLayers;
        
        [Header("FOV")]
        [SerializeField] private bool m_enableFOVKick = true;
        [Range(30f, 120f)]
        [SerializeField] private float m_sprintFOV = 75f;
        [Range(30f, 120f)]
        [SerializeField] private float m_normalFOV = 60f;
        [Range(1f, 20f)]
        [SerializeField] private float m_fovLerpSpeed = 5f;

        private float m_yaw;
        private float m_pitch;
        private Vector3 m_currentVelocity;
        private Camera m_cam;

        public Camera cam => m_cam;

        private void Awake()
        {
            m_cam = GetComponent<Camera>();
        }

        private void Start()
        {
            if (m_target != null)
            {
                m_yaw = m_target.eulerAngles.y;
                m_pitch = transform.eulerAngles.x;
            }
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            HandleInput();
            UpdatePosition();
            UpdateFOV();
        }

        private void HandleInput()
        {
            m_yaw += Input.GetAxisRaw("Mouse X") * m_mouseSensitivity * Time.deltaTime;
            m_pitch -= Input.GetAxisRaw("Mouse Y") * m_mouseSensitivity * Time.deltaTime;
            m_pitch = Mathf.Clamp(m_pitch, m_minPitch, m_maxPitch);
        }

        private void UpdatePosition()
        {
            if (m_target == null) return;
            
            Quaternion rotation = Quaternion.Euler(m_pitch, m_yaw, 0);
            Vector3 desiredPos = m_target.position + rotation * m_offset;
            
            if (m_enableCollision)
            {
                desiredPos = GetCollisionPosition(desiredPos);
            }
            
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                desiredPos, 
                ref m_currentVelocity, 
                1f / m_followSmoothness
            );
            
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(m_target.position - transform.position),
                Time.deltaTime * m_rotationSmoothness
            );
        }

        private Vector3 GetCollisionPosition(Vector3 desired)
        {
            Vector3 dir = (desired - m_target.position).normalized;
            float dist = Vector3.Distance(m_target.position, desired);
            
            if (Physics.SphereCast(m_target.position, m_collisionRadius, dir, out RaycastHit hit, dist, m_collisionLayers))
            {
                return m_target.position + dir * (hit.distance - m_collisionRadius * 0.5f);
            }
            
            return desired;
        }

        private void UpdateFOV()
        {
            if (!m_enableFOVKick || m_cam == null) return;
            
            float targetFOV = Input.GetKey(KeyCode.LeftShift) ? m_sprintFOV : m_normalFOV;
            m_cam.fieldOfView = Mathf.Lerp(m_cam.fieldOfView, targetFOV, Time.deltaTime * m_fovLerpSpeed);
        }

        /// <summary>Shakes the camera for impact effects.</summary>
        public void Shake(float intensity, float duration)
        {
            StartCoroutine(ShakeCoroutine(intensity, duration));
        }

        private System.Collections.IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            Vector3 original = transform.localPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;
                transform.localPosition = original + new Vector3(x, y, 0);
                yield return null;
            }
            
            transform.localPosition = original;
        }

        public void SetTarget(Transform target)
        {
            m_target = target;
            if (target != null)
            {
                m_yaw = target.eulerAngles.y;
            }
        }

        public void SetFOV(float fov)
        {
            if (m_cam != null)
            {
                m_normalFOV = fov;
                m_cam.fieldOfView = fov;
            }
        }
    }
}
