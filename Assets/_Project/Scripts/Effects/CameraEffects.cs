using UnityEngine;
using System;

namespace Mistborn.Effects
{
    public class CameraControllerEffects : MonoBehaviour
    {
        public static CameraControllerEffects Instance { get; private set; }

        [Header("Shake Settings")]
        [SerializeField] private float m_baseShakeDuration = 0.2f;
        [SerializeField] private float m_baseShakeIntensity = 0.5f;
        [SerializeField] private AnimationCurve m_shakeFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Recoil")]
        [SerializeField] private float m_recoilRecoverySpeed = 5f;
        [SerializeField] private float m_maxRecoilPitch = 15f;
        [SerializeField] private float m_maxRecoilYaw = 10f;

        [Header("Kickback")]
        [SerializeField] private float m_kickbackRecovery = 8f;
        [SerializeField] private float m_maxKickback = 2f;

        private Camera m_camera;
        private Vector3 m_originalPosition;
        private float m_shakeTimer;
        private float m_shakeIntensity;
        private Vector3 m_recoilRotation;
        private Vector3 m_currentRecoil;
        private Vector3 m_kickbackOffset;
        private Vector3 m_currentKickback;
        private bool m_isShaking;

        public event Action OnShakeComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            m_camera = GetComponent<Camera>();
            if (m_camera == null)
                m_camera = Camera.main;

            m_originalPosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            UpdateShake();
            UpdateRecoil();
            UpdateKickback();
            ApplyEffects();
        }

        public void ScreenShake(float intensity = 1f, float duration = -1f)
        {
            if (duration < 0) duration = m_baseShakeDuration;
            duration *= intensity;

            m_isShaking = true;
            m_shakeTimer = duration;
            m_shakeIntensity = m_baseShakeIntensity * intensity;
        }

        public void ScreenShake(float intensity, float duration, Vector3 direction)
        {
            m_isShaking = true;
            m_shakeTimer = duration;
            m_shakeIntensity = m_baseShakeIntensity * intensity;

            direction.Normalize();
            m_shakeDirection = direction;
        }

        private Vector3 m_shakeDirection = Vector3.zero;

        private void UpdateShake()
        {
            if (!m_isShaking) return;

            m_shakeTimer -= Time.deltaTime;

            if (m_shakeTimer <= 0)
            {
                m_isShaking = false;
                m_shakeDirection = Vector3.zero;
                OnShakeComplete?.Invoke();
                return;
            }

            float progress = 1f - (m_shakeTimer / m_baseShakeDuration);
            float falloff = m_shakeFalloff.Evaluate(progress);
            float currentIntensity = m_shakeIntensity * falloff;

            float x = UnityEngine.Random.Range(-1f, 1f);
            float y = UnityEngine.Random.Range(-1f, 1f);

            if (m_shakeDirection != Vector3.zero)
            {
                Vector3 perp = Vector3.Cross(m_shakeDirection, Vector3.up).normalized;
                float deviation = UnityEngine.Random.Range(-0.5f, 0.5f);
                m_shakeOffset = m_shakeDirection * currentIntensity + perp * deviation * currentIntensity;
            }
            else
            {
                m_shakeOffset = new Vector3(x, y, 0) * currentIntensity;
            }
        }

        private Vector3 m_shakeOffset = Vector3.zero;

        public void Recoil(float intensity = 1f)
        {
            float pitch = UnityEngine.Random.Range(-m_maxRecoilPitch, -m_maxRecoilPitch * 0.5f) * intensity;
            float yaw = UnityEngine.Random.Range(-m_maxRecoilYaw, m_maxRecoilYaw) * intensity;

            m_recoilRotation += new Vector3(pitch, yaw, 0);
            m_recoilRotation = Vector3.ClampMagnitude(m_recoilRotation, m_maxRecoilPitch + m_maxRecoilYaw);
        }

        private void UpdateRecoil()
        {
            if (m_recoilRotation.magnitude < 0.1f) return;

            m_recoilRotation = Vector3.Lerp(m_recoilRotation, Vector3.zero, m_recoilRecoverySpeed * Time.deltaTime);

            if (m_recoilRotation.magnitude < 0.1f)
                m_recoilRotation = Vector3.zero;
        }

        public void Kickback(float intensity = 1f)
        {
            m_kickbackOffset.z = -m_maxKickback * intensity;
        }

        private void UpdateKickback()
        {
            if (m_kickbackOffset.z < -0.01f)
            {
                m_kickbackOffset.z = Mathf.Lerp(m_kickbackOffset.z, 0, m_kickbackRecovery * Time.deltaTime);
            }
        }

        private void ApplyEffects()
        {
            Vector3 totalOffset = m_shakeOffset + m_kickbackOffset;
            transform.localPosition = m_originalPosition + totalOffset;
            transform.localRotation = Quaternion.Euler(m_recoilRotation);
        }

        public void StopAllEffects()
        {
            m_isShaking = false;
            m_shakeTimer = 0f;
            m_shakeOffset = Vector3.zero;
            m_recoilRotation = Vector3.zero;
            m_kickbackOffset = Vector3.zero;
        }

        public void SetShakeIntensity(float intensity)
        {
            m_baseShakeIntensity = intensity;
        }

        public void SetRecoilRecovery(float speed)
        {
            m_recoilRecoverySpeed = speed;
        }
    }

    public class DamageNumber : MonoBehaviour
    {
        public enum DamageType { Normal, Critical, Heal, Block }

        [Header("Settings")]
        [SerializeField] private float m_lifetime = 1f;
        [SerializeField] private float m_floatSpeed = 2f;
        [SerializeField] private float m_spread = 0.5f;
        [SerializeField] private AnimationCurve m_scaleCurve = AnimationCurve.EaseInOut(0, 0, 0.3f, 1, 1, 1);

        [Header("Colors")]
        [SerializeField] private Color m_normalColor = Color.red;
        [SerializeField] private Color m_criticalColor = Color.yellow;
        [SerializeField] private Color m_healColor = Color.green;
        [SerializeField] private Color m_blockColor = Color.gray;

        private TextMesh m_textMesh;
        private Vector3 m_velocity;
        private float m_lifetimeTimer;
        private float m_startScale;

        public static void Create(Vector3 position, float amount, DamageType type = DamageType.Normal)
        {
            GameObject obj = new GameObject("DamageNumber");
            obj.transform.position = position + Vector3.up;

            DamageNumber number = obj.AddComponent<DamageNumber>();
            number.Setup(amount, type);
        }

        private void Awake()
        {
            m_textMesh = gameObject.AddComponent<TextMesh>();
            m_textMesh.fontSize = 24;
            m_textMesh.anchor = TextAnchor.MiddleCenter;
            m_textMesh.alignment = TextAlignment.Center;

            m_startScale = transform.localScale.x;
        }

        public void Setup(float amount, DamageType type)
        {
            m_textMesh.text = Mathf.RoundToInt(amount).ToString();
            m_textMesh.color = GetColor(type);

            if (type == DamageType.Critical)
            {
                m_textMesh.fontSize = 32;
                m_textMesh.fontStyle = FontStyle.Bold;
            }

            float randomX = UnityEngine.Random.Range(-m_spread, m_spread);
            m_velocity = new Vector3(randomX, m_floatSpeed, 0);

            m_lifetimeTimer = m_lifetime;

            float scaleMultiplier = type == DamageType.Critical ? 1.5f : 1f;
            transform.localScale = Vector3.one * m_startScale * scaleMultiplier;
        }

        private Color GetColor(DamageType type)
        {
            return type switch
            {
                DamageType.Normal => m_normalColor,
                DamageType.Critical => m_criticalColor,
                DamageType.Heal => m_healColor,
                DamageType.Block => m_blockColor,
                _ => m_normalColor
            };
        }

        private void Update()
        {
            m_lifetimeTimer -= Time.deltaTime;

            float progress = 1f - (m_lifetimeTimer / m_lifetime);
            float scale = m_scaleCurve.Evaluate(progress);
            transform.localScale = Vector3.one * m_startScale * scale;

            transform.position += m_velocity * Time.deltaTime;

            float fadeStart = 0.7f;
            if (progress > fadeStart)
            {
                float fade = 1f - ((progress - fadeStart) / (1f - fadeStart));
                Color color = m_textMesh.color;
                color.a = fade;
                m_textMesh.color = color;
            }

            if (m_lifetimeTimer <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
