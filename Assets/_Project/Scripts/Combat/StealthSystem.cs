using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.Combat
{
    public class StealthSystem : MonoBehaviour
    {
        public enum StealthState { Visible, Suspicious, Hidden }

        [Header("Stealth Settings")]
        [SerializeField] private StealthState m_currentState = StealthState.Visible;
        [SerializeField] private float m_detectionThreshold = 1f;
        [SerializeField] private float m_detectionDecayRate = 0.5f;

        [Header("Visibility Factors")]
        [SerializeField] private float m_baseVisibility = 0.5f;
        [SerializeField] private float m_movementVisibilityMultiplier = 2f;
        [SerializeField] private float m_combatVisibilityMultiplier = 5f;
        [SerializeField] private float m_noiseRadius = 10f;

        [Header("Abilities")]
        [SerializeField] private bool m_canHideInCopperCloud = true;
        [SerializeField] private bool m_canStealthWithShadow = true;

        private float m_currentVisibility;
        private float m_detectionLevel;
        private bool m_isInCopperCloud;
        private bool m_isMoving;
        private bool m_isInCombat;
        private List<StealthDetector> m_activeDetectors = new List<StealthDetector>();

        public StealthState CurrentState => m_currentState;
        public float Visibility => m_currentVisibility;
        public float DetectionLevel => m_detectionLevel;

        public event System.Action<StealthState> OnStealthStateChanged;

        private void Awake()
        {
            m_currentVisibility = m_baseVisibility;
        }

        private void Update()
        {
            UpdateVisibility();
            UpdateDetection();
            UpdateState();

            m_isMoving = GetComponent<CharacterController>()?.velocity.magnitude > 0.1f;
        }

        private void UpdateVisibility()
        {
            float targetVisibility = m_baseVisibility;

            if (m_isMoving)
            {
                targetVisibility *= m_movementVisibilityMultiplier;
            }

            if (m_isInCombat)
            {
                targetVisibility *= m_combatVisibilityMultiplier;
            }

            if (m_isInCopperCloud)
            {
                targetVisibility *= 0.1f;
            }

            m_currentVisibility = Mathf.Lerp(m_currentVisibility, targetVisibility, Time.deltaTime * 3f);
            m_currentVisibility = Mathf.Clamp01(m_currentVisibility);
        }

        private void UpdateDetection()
        {
            if (m_detectionLevel > 0)
            {
                m_detectionLevel -= m_detectionDecayRate * Time.deltaTime;
                m_detectionLevel = Mathf.Max(0, m_detectionLevel);
            }
        }

        private void UpdateState()
        {
            StealthState newState;

            if (m_detectionLevel >= m_detectionThreshold)
            {
                newState = StealthState.Visible;
            }
            else if (m_detectionLevel >= m_detectionThreshold * 0.5f)
            {
                newState = StealthState.Suspicious;
            }
            else
            {
                newState = StealthState.Hidden;
            }

            if (newState != m_currentState)
            {
                m_currentState = newState;
                OnStealthStateChanged?.Invoke(newState);
            }
        }

        public void EnterCopperCloud()
        {
            m_isInCopperCloud = true;
        }

        public void ExitCopperCloud()
        {
            m_isInCopperCloud = false;
        }

        public void SetInCombat(bool inCombat)
        {
            m_isInCombat = inCombat;
        }

        public void MakeNoise(float intensity)
        {
            Collider[] detected = Physics.OverlapSphere(transform.position, m_noiseRadius);

            foreach (Collider col in detected)
            {
                StealthDetector detector = col.GetComponent<StealthDetector>();
                if (detector != null)
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    float falloff = 1f - (distance / m_noiseRadius);
                    detector.OnNoiseHeard(transform.position, intensity * falloff * m_currentVisibility);
                }
            }
        }

        public void DetectedBy(StealthDetector detector, float detectionAmount)
        {
            if (!m_activeDetectors.Contains(detector))
            {
                m_activeDetectors.Add(detector);
            }

            m_detectionLevel += detectionAmount * Time.deltaTime;
            m_detectionLevel = Mathf.Clamp01(m_detectionLevel);
        }

        private void OnDestroy()
        {
            foreach (StealthDetector detector in m_activeDetectors)
            {
                if (detector != null)
                    detector.StopDetecting(this);
            }
        }
    }

    public class StealthDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float m_detectionRange = 20f;
        [SerializeField] private float m_peripheralRange = 10f;
        [SerializeField] private float m_maxDetectionRate = 1f;
        [SerializeField] private float m_peripheralMultiplier = 0.5f;
        [SerializeField] private LayerMask m_detectionLayers = ~0;

        [Header("Vision")]
        [SerializeField] private float m_viewAngle = 60f;
        [SerializeField] private float m_viewDistance = 15f;
        [SerializeField] private float m_eyeHeight = 1.5f;

        [Header("Alert")]
        [SerializeField] private float m_suspiciousThreshold = 0.3f;
        [SerializeField] private float m_alertThreshold = 0.7f;
        [SerializeField] private float m_alertDuration = 5f;

        private List<StealthSystem> m_targets = new List<StealthSystem>();
        private Dictionary<StealthSystem, float> m_targetDetection = new Dictionary<StealthSystem, float>();
        private AlertState m_currentAlert = AlertState.Idle;
        private float m_alertTimer;
        private Transform m_eyePosition;

        public AlertState CurrentAlert => m_currentAlert;

        public event System.Action OnSuspicious;
        public event System.Action OnAlerted;
        public event System.Action OnSearchEnded;

        public enum AlertState { Idle, Suspicious, Alert, Searching }

        private void Awake()
        {
            m_eyePosition = new GameObject("EyePosition").transform;
            m_eyePosition.SetParent(transform);
            m_eyePosition.localPosition = Vector3.up * m_eyeHeight;
        }

        private void Update()
        {
            UpdateDetection();
            UpdateAlertState();
        }

        private void UpdateDetection()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, m_detectionRange, m_detectionLayers);

            foreach (Collider hit in hits)
            {
                StealthSystem stealth = hit.GetComponent<StealthSystem>();
                if (stealth != null && !m_targets.Contains(stealth))
                {
                    m_targets.Add(stealth);
                    m_targetDetection[stealth] = 0f;
                }
            }

            for (int i = m_targets.Count - 1; i >= 0; i--)
            {
                StealthSystem target = m_targets[i];
                if (target == null)
                {
                    m_targets.RemoveAt(i);
                    continue;
                }

                float detectionRate = CalculateDetectionRate(target);

                if (m_currentAlert != AlertState.Alert)
                {
                    m_targetDetection[target] += detectionRate * Time.deltaTime;
                    target.DetectedBy(this, detectionRate);
                }
            }
        }

        private float CalculateDetectionRate(StealthSystem target)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            float distanceFactor = 1f - (distance / m_detectionRange);

            if (distance > m_detectionRange) return 0f;

            bool inView = IsInView(target.transform);
            float viewFactor = inView ? 1f : m_peripheralMultiplier;

            bool isMoving = target.IsMoving();
            float movementFactor = isMoving ? 2f : 1f;

            float visibility = target.Visibility;
            float visibilityFactor = visibility;

            float rate = distanceFactor * viewFactor * movementFactor * visibilityFactor;
            return rate * m_maxDetectionRate;
        }

        private bool IsInView(Transform target)
        {
            Vector3 toTarget = target.position - m_eyePosition.position;
            float angle = Vector3.Angle(transform.forward, toTarget);

            if (angle > m_viewAngle * 0.5f)
                return false;

            float distance = toTarget.magnitude;
            if (distance > m_viewDistance)
                return false;

            RaycastHit hit;
            if (Physics.Raycast(m_eyePosition.position, toTarget.normalized, out hit, distance))
            {
                if (hit.transform != target)
                    return false;
            }

            return true;
        }

        private void UpdateAlertState()
        {
            float maxDetection = 0f;
            foreach (var detection in m_targetDetection.Values)
            {
                if (detection > maxDetection)
                    maxDetection = detection;
            }

            AlertState newAlert;

            if (maxDetection >= m_alertThreshold)
            {
                newAlert = AlertState.Alert;
                m_alertTimer = m_alertDuration;
            }
            else if (maxDetection >= m_suspiciousThreshold)
            {
                newAlert = AlertState.Suspicious;
            }
            else
            {
                newAlert = AlertState.Idle;
            }

            if (newAlert != m_currentAlert)
            {
                m_currentAlert = newAlert;

                switch (m_currentAlert)
                {
                    case AlertState.Suspicious:
                        OnSuspicious?.Invoke();
                        break;
                    case AlertState.Alert:
                        OnAlerted?.Invoke();
                        break;
                    case AlertState.Idle:
                        OnSearchEnded?.Invoke();
                        m_targetDetection.Clear();
                        break;
                }
            }

            if (m_currentAlert == AlertState.Alert && m_alertTimer > 0)
            {
                m_alertTimer -= Time.deltaTime;
                if (m_alertTimer <= 0)
                {
                    m_currentAlert = AlertState.Searching;
                }
            }
        }

        public void OnNoiseHeard(Vector3 position, float intensity)
        {
            m_targetDetection.Clear();
            m_currentAlert = AlertState.Suspicious;
            OnSuspicious?.Invoke();
        }

        public void StopDetecting(StealthSystem target)
        {
            if (m_targets.Contains(target))
            {
                m_targets.Remove(target);
                m_targetDetection.Remove(target);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_detectionRange);

            Gizmos.color = Color.cyan;
            Vector3 leftDir = Quaternion.Euler(0, -m_viewAngle * 0.5f, 0) * transform.forward * m_viewDistance;
            Vector3 rightDir = Quaternion.Euler(0, m_viewAngle * 0.5f, 0) * transform.forward * m_viewDistance;
            Gizmos.DrawRay(m_eyePosition.position, leftDir);
            Gizmos.DrawRay(m_eyePosition.position, rightDir);
        }
    }

    public static class StealthExtensions
    {
        public static bool IsMoving(this StealthSystem stealth)
        {
            CharacterController cc = stealth.GetComponent<CharacterController>();
            return cc != null && cc.velocity.magnitude > 0.1f;
        }
    }
}
