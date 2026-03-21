using UnityEngine;

namespace Mistborn.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float cameraDistance = 5f;
        [SerializeField] private float cameraHeight = 2f;
        [SerializeField] private float followSmoothness = 10f;

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
            UpdatePosition();
        }

        private void HandleMouseLook()
        {
            cameraYaw += Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
            cameraPitch -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;
            cameraPitch = Mathf.Clamp(cameraPitch, -45f, 60f);
            transform.eulerAngles = new Vector3(cameraPitch, cameraYaw, 0f);
        }

        private void UpdatePosition()
        {
            if (target == null) return;
            
            Vector3 targetPos = target.position - transform.forward * cameraDistance;
            targetPos.y += cameraHeight;
            
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                targetPos, 
                ref currentVelocity, 
                1f / followSmoothness
            );
        }

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
    }
}
