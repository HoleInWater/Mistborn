using UnityEngine;

namespace Mistborn.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform m_player;
        [SerializeField] private Vector3 m_offset = new Vector3(0, 2, -5);

        [Header("Rotation")]
        [SerializeField] private float m_sensitivity = 100f;
        [SerializeField] private float m_minPitch = -30f;
        [SerializeField] private float m_maxPitch = 60f;

        private float m_yaw;
        private float m_pitch;

        private void Start()
        {
            if (m_player == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    m_player = player.transform;
            }

            Cursor.lockState = CursorLockMode.Locked;
        }

        private void LateUpdate()
        {
            if (m_player == null) return;

            CameraRotation();
            PositionCamera();
        }

        private void CameraRotation()
        {
            m_yaw += Input.GetAxis("Mouse X") * m_sensitivity * Time.deltaTime;
            m_pitch -= Input.GetAxis("Mouse Y") * m_sensitivity * Time.deltaTime;
            m_pitch = Mathf.Clamp(m_pitch, m_minPitch, m_maxPitch);
        }

        private void PositionCamera()
        {
            transform.eulerAngles = new Vector3(m_pitch, m_yaw, 0);

            Vector3 desiredPosition = m_player.position + transform.rotation * m_offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10f);
        }

        public void SetSensitivity(float sensitivity)
        {
            m_sensitivity = sensitivity;
        }
    }
}
