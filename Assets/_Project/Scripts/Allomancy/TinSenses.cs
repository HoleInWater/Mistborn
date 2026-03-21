using UnityEngine;
using Mistborn.Combat;

namespace Mistborn.Allomancy
{
    public class TinSenses : MonoBehaviour
    {
        [Header("Tin Enhancement")]
        [SerializeField] private float m_sightMultiplier = 2f;
        [SerializeField] private float m_hearingMultiplier = 3f;
        [SerializeField] private float m_dangerSenseRadius = 15f;

        [Header("Visual")]
        [SerializeField] private Color m_tinEyesColor = Color.white;
        [SerializeField] private float m_eyeGlowIntensity = 2f;
        [SerializeField] private GameObject m_tinEyesEffect;

        [Header("Effects")]
        [SerializeField] private bool m_seeThroughWalls = false;
        [SerializeField] private float m_wallSeeAlpha = 0.3f;
        [SerializeField] private bool m_detectHidden = true;

        private AllomancerController m_allomancer;
        private Camera m_camera;
        private float m_originalFOV;
        private float m_originalAmbient;
        private GameObject m_activeEffect;
        private AudioSource m_hearingSource;

        public event System.Action<Transform> OnDangerDetected;

        private void Awake()
        {
            m_allomancer = GetComponent<AllomancerController>();
            m_camera = GetComponentInChildren<Camera>();

            if (m_camera == null)
                m_camera = Camera.main;

            m_hearingSource = gameObject.AddComponent<AudioSource>();
            m_hearingSource.loop = true;
            m_hearingSource.playOnAwake = false;
            m_hearingSource.spatialBlend = 1f;
        }

        private void Start()
        {
            if (m_camera != null)
            {
                m_originalFOV = m_camera.fieldOfView;
            }
        }

        private void Update()
        {
            bool isBurning = m_allomancer != null && m_allomancer.IsBurning(AllomanticMetal.Tin);

            if (isBurning)
            {
                ActivateTinSenses();
            }
            else
            {
                DeactivateTinSenses();
            }
        }

        private void FixedUpdate()
        {
            bool isBurning = m_allomancer != null && m_allomancer.IsBurning(AllomanticMetal.Tin);
            if (!isBurning) return;

            DetectDangers();
            EnhanceHearing();
        }

        private void ActivateTinSenses()
        {
            if (m_camera != null)
            {
                m_camera.fieldOfView = m_originalFOV * m_sightMultiplier;
            }

            if (m_tinEyesEffect != null && m_activeEffect == null)
            {
                m_activeEffect = Instantiate(m_tinEyesEffect, transform);
            }

            RenderSettings.ambientIntensity = m_originalAmbient * 1.5f;
        }

        private void DeactivateTinSenses()
        {
            if (m_camera != null)
            {
                m_camera.fieldOfView = Mathf.Lerp(m_camera.fieldOfView, m_originalFOV, Time.deltaTime * 5f);
            }

            if (m_activeEffect != null)
            {
                Destroy(m_activeEffect);
                m_activeEffect = null;
            }
        }

        private void DetectDangers()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, m_dangerSenseRadius);

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    EnemyBase enemy = hit.GetComponent<EnemyBase>();
                    if (enemy != null && !enemy.isDead)
                    {
                        OnDangerDetected?.Invoke(hit.transform);
                        HighlightTarget(hit.transform);
                    }
                }
            }
        }

        private void HighlightTarget(Transform target)
        {
            Renderer renderer = target.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material.HasProperty("_EmissionColor"))
            {
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", Color.red * 0.5f);
            }
        }

        private void EnhanceHearing()
        {
            AudioListener listener = GetComponent<AudioListener>();
            if (listener != null)
            {
            }
        }

        public void ToggleSeeThroughWalls()
        {
            m_seeThroughWalls = !m_seeThroughWalls;

            if (m_seeThroughWalls)
            {
                EnableSeeThrough();
            }
            else
            {
                DisableSeeThrough();
            }
        }

        private void EnableSeeThrough()
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && !renderer.CompareTag("Player") && !renderer.CompareTag("Enemy"))
                {
                    if (renderer.material.HasProperty("_Alpha"))
                    {
                        StartCoroutine(FadeMaterial(renderer.material, m_wallSeeAlpha));
                    }
                }
            }
        }

        private void DisableSeeThrough()
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && renderer.material.HasProperty("_Alpha"))
                {
                    StartCoroutine(FadeMaterial(renderer.material, 1f));
                }
            }
        }

        private System.Collections.IEnumerator FadeMaterial(Material mat, float targetAlpha)
        {
            float startAlpha = mat.GetFloat("_Alpha");
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                mat.SetFloat("_Alpha", alpha);
                yield return null;
            }

            mat.SetFloat("_Alpha", targetAlpha);
        }

        public float GetDangerSenseRadius()
        {
            bool isBurning = m_allomancer != null && m_allomancer.IsBurning(AllomanticMetal.Tin);
            return isBurning ? m_dangerSenseRadius : 0f;
        }
    }

    public class ElectrumShots : MonoBehaviour
    {
        [Header("Electrum Settings")]
        [SerializeField] private int m_maxShots = 10;
        [SerializeField] private float m_shotInterval = 0.5f;
        [SerializeField] private float m_shotRange = 100f;
        [SerializeField] private LayerMask m_targetLayers = ~0;

        [Header("Shots")]
        [SerializeField] private GameObject m_shotPrefab;
        [SerializeField] private Transform m_shotOrigin;

        [Header("Tracking")]
        [SerializeField] private bool m_autoTarget = true;
        [SerializeField] private float m_targetSwitchDelay = 0.2f;

        private AllomancerController m_allomancer;
        private int m_shotsRemaining;
        private float m_shotTimer;
        private Transform m_currentTarget;
        private float m_targetSwitchTimer;

        private void Awake()
        {
            m_allomancer = GetComponent<AllomancerController>();
            m_shotsRemaining = m_maxShots;

            if (m_shotOrigin == null)
            {
                m_shotOrigin = transform;
            }
        }

        private void Update()
        {
            bool isBurning = m_allomancer != null && m_allomancer.IsBurning(AllomanticMetal.Electrum);

            if (isBurning)
            {
                if (m_shotsRemaining < m_maxShots)
                {
                    m_shotTimer -= Time.deltaTime;
                    if (m_shotTimer <= 0)
                    {
                        m_shotsRemaining++;
                        m_shotTimer = m_shotInterval;
                    }
                }

                if (m_autoTarget)
                {
                    UpdateTargeting();
                }
            }
            else
            {
                m_shotsRemaining = 0;
            }
        }

        private void UpdateTargeting()
        {
            m_targetSwitchTimer -= Time.deltaTime;

            if (m_targetSwitchTimer <= 0)
            {
                FindNewTarget();
                m_targetSwitchTimer = m_targetSwitchDelay;
            }
        }

        private void FindNewTarget()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, m_shotRange, m_targetLayers);
            Transform closest = null;
            float closestDist = m_shotRange;

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    float dist = Vector3.Distance(transform.position, hit.transform.position);
                    if (dist < closestDist)
                    {
                        closest = hit.transform;
                        closestDist = dist;
                    }
                }
            }

            m_currentTarget = closest;
        }

        public void Fire()
        {
            if (m_shotsRemaining <= 0) return;
            if (m_currentTarget == null) return;

            m_shotsRemaining--;

            if (m_shotPrefab != null && m_shotOrigin != null)
            {
                Vector3 direction = (m_currentTarget.position - m_shotOrigin.position).normalized;
                GameObject shot = Instantiate(m_shotPrefab, m_shotOrigin.position, Quaternion.LookRotation(direction));

                Rigidbody rb = shot.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = direction * 50f;
                }
            }
        }

        public int GetShotsRemaining()
        {
            return m_shotsRemaining;
        }

        public float GetShotsPercent()
        {
            return (float)m_shotsRemaining / m_maxShots;
        }
    }

    public class GoldPits : MonoBehaviour
    {
        [Header("Gold Settings")]
        [SerializeField] private float m_visionRange = 30f;
        [SerializeField] private float m_pitDuration = 5f;
        [SerializeField] private float m_cooldown = 15f;

        [Header("Pits")]
        [SerializeField] private GameObject m_pitVFX;
        [SerializeField] private LayerMask m_pitLayers = ~0;

        private AllomancerController m_allomancer;
        private float m_cooldownTimer;
        private bool m_isActive;
        private float m_activeTimer;

        public event System.Action<Transform> OnPastSelfDetected;

        private void Update()
        {
            if (m_cooldownTimer > 0)
            {
                m_cooldownTimer -= Time.deltaTime;
            }

            if (m_isActive)
            {
                m_activeTimer -= Time.deltaTime;
                if (m_activeTimer <= 0)
                {
                    Deactivate();
                }
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                TryActivate();
            }
        }

        private void TryActivate()
        {
            bool isBurning = m_allomancer != null && m_allomancer.IsBurning(AllomanticMetal.Gold);

            if (!isBurning)
            {
                Debug.Log("Must be burning Gold to use this ability");
                return;
            }

            if (m_cooldownTimer > 0)
            {
                Debug.Log($"Gold Pits on cooldown: {m_cooldownTimer:F1}s");
                return;
            }

            Activate();
        }

        private void Activate()
        {
            m_isActive = true;
            m_activeTimer = m_pitDuration;
            m_cooldownTimer = m_cooldown;

            if (m_pitVFX != null)
            {
                Instantiate(m_pitVFX, transform.position, Quaternion.identity);
            }

            LookForPastSelf();
        }

        private void Deactivate()
        {
            m_isActive = false;
        }

        private void LookForPastSelf()
        {
        }

        public float GetCooldownPercent()
        {
            if (m_cooldown <= 0) return 1f;
            return 1f - (m_cooldownTimer / m_cooldown);
        }
    }
}
