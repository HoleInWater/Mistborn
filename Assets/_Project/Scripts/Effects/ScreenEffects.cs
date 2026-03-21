using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.Effects
{
    public class ScreenEffects : MonoBehaviour
    {
        public static ScreenEffects Instance { get; private set; }

        [Header("References")]
        [SerializeField] private UnityEngine.UI.Image m_damageOverlay;
        [SerializeField] private UnityEngine.UI.Image m_healOverlay;
        [SerializeField] private UnityEngine.UI.Image m_lowHealthOverlay;
        [SerializeField] private UnityEngine.UI.RawImage m_whiteFlash;

        [Header("Settings")]
        [SerializeField] private float m_damageOverlayDuration = 0.3f;
        [SerializeField] private float m_healOverlayDuration = 0.5f;
        [SerializeField] private float m_lowHealthThreshold = 0.3f;
        [SerializeField] private float m_vignetteIntensity = 0.5f;

        [Header("Colors")]
        [SerializeField] private Color m_damageColor = new Color(1f, 0f, 0f, 0.3f);
        [SerializeField] private Color m_healColor = new Color(0f, 1f, 0f, 0.3f);

        private float m_damageTimer;
        private float m_healTimer;
        private float m_shakeIntensity;
        private float m_shakeDuration;
        private Vector3 m_originalCameraPos;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (m_damageOverlay != null)
                m_damageOverlay.gameObject.SetActive(false);

            if (m_healOverlay != null)
                m_healOverlay.gameObject.SetActive(false);

            if (m_whiteFlash != null)
                m_whiteFlash.gameObject.SetActive(false);
        }

        private void Update()
        {
            UpdateDamageOverlay();
            UpdateHealOverlay();
            UpdateLowHealth();
            UpdateScreenShake();
        }

        public void ShowDamageOverlay()
        {
            if (m_damageOverlay == null) return;

            m_damageOverlay.gameObject.SetActive(true);
            m_damageOverlay.color = m_damageColor;
            m_damageTimer = m_damageOverlayDuration;
        }

        private void UpdateDamageOverlay()
        {
            if (m_damageOverlay == null || !m_damageOverlay.gameObject.activeSelf) return;

            m_damageTimer -= Time.deltaTime;

            float alpha = Mathf.Clamp01(m_damageTimer / m_damageOverlayDuration);
            Color color = m_damageColor;
            color.a = alpha * m_damageColor.a;
            m_damageOverlay.color = color;

            if (m_damageTimer <= 0)
            {
                m_damageOverlay.gameObject.SetActive(false);
            }
        }

        public void ShowHealOverlay()
        {
            if (m_healOverlay == null) return;

            m_healOverlay.gameObject.SetActive(true);
            m_healOverlay.color = m_healColor;
            m_healTimer = m_healOverlayDuration;
        }

        private void UpdateHealOverlay()
        {
            if (m_healOverlay == null || !m_healOverlay.gameObject.activeSelf) return;

            m_healTimer -= Time.deltaTime;

            float alpha = Mathf.Clamp01(m_healTimer / m_healOverlayDuration);
            Color color = m_healColor;
            color.a = alpha * m_healColor.a;
            m_healOverlay.color = color;

            if (m_healTimer <= 0)
            {
                m_healOverlay.gameObject.SetActive(false);
            }
        }

        private void UpdateLowHealth()
        {
            if (m_lowHealthOverlay == null) return;

            PlayerHealth health = FindObjectOfType<PlayerHealth>();
            if (health != null)
            {
                float healthPercent = health.CurrentHealth / health.MaxHealth;
                m_lowHealthOverlay.gameObject.SetActive(healthPercent <= m_lowHealthThreshold);

                Color color = m_lowHealthOverlay.color;
                color.a = (1f - (healthPercent / m_lowHealthThreshold)) * m_vignetteIntensity;
                m_lowHealthOverlay.color = color;
            }
        }

        public void ScreenShake(float intensity, float duration)
        {
            m_shakeIntensity = intensity;
            m_shakeDuration = duration;
        }

        private void UpdateScreenShake()
        {
            if (m_shakeDuration <= 0) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            float x = Random.Range(-1f, 1f) * m_shakeIntensity;
            float y = Random.Range(-1f, 1f) * m_shakeIntensity;

            cam.transform.position += new Vector3(x, y, 0);

            m_shakeDuration -= Time.deltaTime;
            m_shakeIntensity *= 0.95f;
        }

        public void WhiteFlash(float duration = 0.2f)
        {
            if (m_whiteFlash == null) return;

            StartCoroutine(FlashRoutine(duration));
        }

        private System.Collections.IEnumerator FlashRoutine(float duration)
        {
            m_whiteFlash.gameObject.SetActive(true);
            Color color = Color.white;
            color.a = 1f;
            m_whiteFlash.color = color;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = 1f - (elapsed / duration);
                m_whiteFlash.color = color;
                yield return null;
            }

            m_whiteFlash.gameObject.SetActive(false);
        }

        public void FadeToBlack(float duration = 1f)
        {
            if (m_whiteFlash == null) return;

            StartCoroutine(FadeToBlackRoutine(duration));
        }

        private System.Collections.IEnumerator FadeToBlackRoutine(float duration)
        {
            m_whiteFlash.gameObject.SetActive(true);
            Color color = Color.black;
            m_whiteFlash.color = color;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = elapsed / duration;
                m_whiteFlash.color = color;
                yield return null;
            }
        }

        public void FadeFromBlack(float duration = 1f)
        {
            if (m_whiteFlash == null) return;

            StartCoroutine(FadeFromBlackRoutine(duration));
        }

        private System.Collections.IEnumerator FadeFromBlackRoutine(float duration)
        {
            m_whiteFlash.gameObject.SetActive(true);
            Color color = Color.black;
            color.a = 1f;
            m_whiteFlash.color = color;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = 1f - (elapsed / duration);
                m_whiteFlash.color = color;
                yield return null;
            }

            m_whiteFlash.gameObject.SetActive(false);
        }
    }
}
