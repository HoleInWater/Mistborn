using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mistborn.Combat
{
    public class LockOnTargeting : MonoBehaviour
    {
        public static LockOnTargeting Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float m_lockOnRange = 50f;
        [SerializeField] private float m_lockOnAngle = 60f;
        [SerializeField] private float m_lockOnRadius = 5f;
        [SerializeField] private LayerMask m_targetLayers = ~0;
        [SerializeField] private KeyCode m_lockOnKey = KeyCode.Tab;
        [SerializeField] private KeyCode m_nextTargetKey = KeyCode.Tab;

        [Header("Behavior")]
        [SerializeField] private bool m_autoLockOnWeakEnemies = false;
        [SerializeField] private float m_autoLockThreshold = 0.3f;
        [SerializeField] private bool m_stayLockedWhenOutOfRange = false;
        [SerializeField] private float m_unlockDistance = 5f;

        [Header("Visual")]
        [SerializeField] private GameObject m_lockOnIndicator;
        [SerializeField] private Color m_enemyColor = Color.red;
        [SerializeField] private Color m_allyColor = Color.green;
        [SerializeField] private float m_indicatorHeight = 1.5f;

        private Transform m_currentTarget;
        private List<Transform> m_availableTargets = new List<Transform>();
        private int m_targetIndex;
        private bool m_isLockedOn;
        private Camera m_camera;
        private GameObject m_activeIndicator;
        private LineRenderer m_aimLine;

        public event Action<Transform> OnTargetLocked;
        public event Action<Transform> OnTargetUnlocked;
        public event Action<Transform> OnTargetChanged;

        public Transform CurrentTarget => m_currentTarget;
        public bool IsLockedOn => m_isLockedOn;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            m_camera = GetComponentInChildren<Camera>();
            if (m_camera == null)
                m_camera = Camera.main;

            SetupAimLine();
        }

        private void SetupAimLine()
        {
            m_aimLine = gameObject.AddComponent<LineRenderer>();
            m_aimLine.startWidth = 0.02f;
            m_aimLine.endWidth = 0.01f;
            m_aimLine.material = new Material(Shader.Find("Sprites/Default"));
            m_aimLine.startColor = new Color(1f, 0f, 0f, 0.5f);
            m_aimLine.endColor = new Color(1f, 0f, 0f, 0f);
            m_aimLine.enabled = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(m_lockOnKey))
            {
                ToggleLockOn();
            }

            if (Input.GetKeyDown(m_nextTargetKey))
            {
                CycleTargets();
            }

            if (m_isLockedOn)
            {
                UpdateLockedTarget();
                UpdateAimLine();
            }

            if (m_autoLockOnWeakEnemies && !m_isLockedOn)
            {
                AutoLockOnCheck();
            }
        }

        public void ToggleLockOn()
        {
            if (m_isLockedOn)
            {
                UnlockTarget();
            }
            else
            {
                LockOnNearest();
            }
        }

        public void LockOnNearest()
        {
            FindTargets();

            if (m_availableTargets.Count == 0)
            {
                Debug.Log("No targets in range");
                return;
            }

            m_currentTarget = FindClosestTarget();
            m_targetIndex = m_availableTargets.IndexOf(m_currentTarget);
            ActivateLockOn();
        }

        public void LockOn(Transform target)
        {
            if (target == null) return;

            m_currentTarget = target;
            m_targetIndex = m_availableTargets.IndexOf(target);
            ActivateLockOn();
        }

        private void ActivateLockOn()
        {
            m_isLockedOn = true;
            OnTargetLocked?.Invoke(m_currentTarget);

            if (m_lockOnIndicator != null)
            {
                m_activeIndicator = Instantiate(m_lockOnIndicator, m_currentTarget);
                m_activeIndicator.transform.localPosition = Vector3.up * m_indicatorHeight;
            }

            if (m_aimLine != null)
                m_aimLine.enabled = true;
        }

        public void UnlockTarget()
        {
            m_isLockedOn = false;
            OnTargetUnlocked?.Invoke(m_currentTarget);

            if (m_activeIndicator != null)
            {
                Destroy(m_activeIndicator);
                m_activeIndicator = null;
            }

            if (m_aimLine != null)
                m_aimLine.enabled = false;

            m_currentTarget = null;
        }

        private void UpdateLockedTarget()
        {
            if (m_currentTarget == null)
            {
                UnlockTarget();
                return;
            }

            if (m_activeIndicator != null)
            {
                m_activeIndicator.transform.position = m_currentTarget.position + Vector3.up * m_indicatorHeight;
            }

            float dist = Vector3.Distance(transform.position, m_currentTarget.position);
            if (!m_stayLockedWhenOutOfRange && dist > m_lockOnRange + m_unlockDistance)
            {
                UnlockTarget();
            }

            if (!IsInView(m_currentTarget))
            {
            }
        }

        private void UpdateAimLine()
        {
            if (m_aimLine == null || m_camera == null) return;

            Vector3 startPos = m_camera.transform.position + m_camera.transform.forward * 0.5f;
            Vector3 endPos = m_currentTarget != null ? m_currentTarget.position : startPos + m_camera.transform.forward * m_lockOnRange;

            m_aimLine.SetPosition(0, startPos);
            m_aimLine.SetPosition(1, endPos);
        }

        private void CycleTargets()
        {
            FindTargets();

            if (m_availableTargets.Count == 0)
            {
                UnlockTarget();
                return;
            }

            m_targetIndex++;
            if (m_targetIndex >= m_availableTargets.Count)
                m_targetIndex = 0;

            Transform previousTarget = m_currentTarget;
            m_currentTarget = m_availableTargets[m_targetIndex];

            if (previousTarget != m_currentTarget)
            {
                OnTargetChanged?.Invoke(m_currentTarget);

                if (m_activeIndicator != null)
                {
                    Destroy(m_activeIndicator);
                }

                m_activeIndicator = Instantiate(m_lockOnIndicator, m_currentTarget);
                m_activeIndicator.transform.localPosition = Vector3.up * m_indicatorHeight;
            }
        }

        private void FindTargets()
        {
            m_availableTargets.Clear();

            Collider[] hits = Physics.OverlapSphere(transform.position, m_lockOnRange, m_targetLayers);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Enemy") || hit.CompareTag("Target"))
                {
                    if (IsInLockOnAngle(hit.transform) && IsInView(hit.transform))
                    {
                        m_availableTargets.Add(hit.transform);
                    }
                }
            }

            m_availableTargets.Sort((a, b) =>
            {
                float distA = Vector3.Distance(transform.position, a.position);
                float distB = Vector3.Distance(transform.position, b.position);
                return distA.CompareTo(distB);
            });
        }

        private Transform FindClosestTarget()
        {
            Transform closest = null;
            float closestDist = m_lockOnRange;

            foreach (Transform target in m_availableTargets)
            {
                float dist = Vector3.Distance(transform.position, target.position);
                if (dist < closestDist)
                {
                    closest = target;
                    closestDist = dist;
                }
            }

            return closest;
        }

        private bool IsInLockOnAngle(Transform target)
        {
            if (m_camera == null) return true;

            Vector3 toTarget = (target.position - m_camera.transform.position).normalized;
            float angle = Vector3.Angle(m_camera.transform.forward, toTarget);
            return angle <= m_lockOnAngle;
        }

        private bool IsInView(Transform target)
        {
            if (m_camera == null) return true;

            Vector3 viewportPoint = m_camera.WorldToViewportPoint(target.position);
            return viewportPoint.z > 0 &&
                   viewportPoint.x > 0 && viewportPoint.x < 1 &&
                   viewportPoint.y > 0 && viewportPoint.y < 1;
        }

        private void AutoLockOnCheck()
        {
        }

        public Vector3 GetTargetPosition()
        {
            if (m_currentTarget == null)
                return transform.position + transform.forward * 10f;

            return m_currentTarget.position;
        }

        public float GetDistanceToTarget()
        {
            if (m_currentTarget == null)
                return float.MaxValue;

            return Vector3.Distance(transform.position, m_currentTarget.position);
        }

        private void OnDestroy()
        {
            if (m_activeIndicator != null)
                Destroy(m_activeIndicator);
        }
    }

    public class TargetIndicator : MonoBehaviour
    {
        [Header("Indicator Settings")]
        [SerializeField] private float m_rotationSpeed = 180f;
        [SerializeField] private float m_pulseSpeed = 2f;
        [SerializeField] private float m_pulseAmount = 0.2f;

        [Header("Parts")]
        [SerializeField] private GameObject m_centerDot;
        [SerializeField] private LineRenderer[] m_lines;
        [SerializeField] private GameObject[] m_corners;

        private RectTransform m_rectTransform;
        private Transform m_target;

        private void Awake()
        {
            m_rectTransform = GetComponent<RectTransform>();
        }

        public void SetTarget(Transform target)
        {
            m_target = target;
        }

        private void Update()
        {
            if (m_target == null) return;

            transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, m_target.position + Vector3.up * 1.5f);

            transform.Rotate(Vector3.forward, m_rotationSpeed * Time.deltaTime);

            float pulse = 1f + Mathf.Sin(Time.time * m_pulseSpeed) * m_pulseAmount;
            transform.localScale = Vector3.one * pulse;
        }

        public void SetColor(Color color)
        {
            if (m_lines != null)
            {
                foreach (LineRenderer line in m_lines)
                {
                    line.startColor = color;
                    line.endColor = color;
                }
            }

            if (m_centerDot != null)
            {
                Renderer renderer = m_centerDot.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.color = color;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
