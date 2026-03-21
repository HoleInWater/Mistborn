using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.Effects
{
    public class SmokeCloud : MonoBehaviour
    {
        [Header("Smoke Settings")]
        [SerializeField] private float m_radius = 5f;
        [SerializeField] private float m_duration = 10f;
        [SerializeField] private float m_fadeSpeed = 1f;
        [SerializeField] private int m_particleCount = 200;

        [Header("Effects")]
        [SerializeField] private bool m_hidePlayer = true;
        [SerializeField] private bool m_slowEnemies = true;
        [SerializeField] private float m_slowAmount = 0.5f;
        [SerializeField] private bool m_blockVision = true;

        [Header("Visual")]
        [SerializeField] private Color m_smokeColor = new Color(0.3f, 0.3f, 0.35f, 0.5f);
        [SerializeField] private float m_opacity = 0.6f;
        [SerializeField] private float m_puffSpeed = 2f;

        private ParticleSystem m_particleSystem;
        private float m_lifetime;
        private bool m_isActive;
        private List<GameObject> m_affectedEnemies = new List<GameObject>();

        public static void Create(Vector3 position, float radius = 5f, float duration = 10f)
        {
            GameObject smokeObj = new GameObject("SmokeCloud");
            smokeObj.transform.position = position;

            SmokeCloud smoke = smokeObj.AddComponent<SmokeCloud>();
            smoke.Initialize(radius, duration);
        }

        private void Awake()
        {
            SetupParticleSystem();
        }

        private void SetupParticleSystem()
        {
            m_particleSystem = GetComponent<ParticleSystem>();
            if (m_particleSystem == null)
            {
                m_particleSystem = gameObject.AddComponent<ParticleSystem>();
            }

            var main = m_particleSystem.main;
            main.duration = m_duration;
            main.loop = false;
            main.maxParticles = m_particleCount;
            main.startLifetime = 3f;
            main.startSpeed = m_puffSpeed;
            main.startSize = 2f;
            main.startColor = new Color(m_smokeColor.r, m_smokeColor.g, m_smokeColor.b, m_opacity);

            var emission = m_particleSystem.emission;
            emission.rateOverTime = m_particleCount / main.startLifetime.constant;

            var shape = m_particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var colorOverLifetime = m_particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(m_smokeColor, 0f), new GradientColorKey(m_smokeColor, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(m_opacity, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = m_particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1f, 1, 3f));

            Renderer renderer = m_particleSystem.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                renderer.material.color = m_smokeColor;
            }
        }

        public void Initialize(float radius, float duration)
        {
            m_radius = radius;
            m_duration = duration;
            m_lifetime = duration;
            m_isActive = true;

            transform.localScale = Vector3.one * radius;
        }

        private void Update()
        {
            if (!m_isActive) return;

            m_lifetime -= Time.deltaTime;

            if (m_lifetime <= 0)
            {
                FadeOut();
            }

            UpdateEffects();

            float fadeProgress = 1f - (m_lifetime / m_duration);
            float currentOpacity = Mathf.Lerp(m_opacity, 0f, fadeProgress);
            UpdateOpacity(currentOpacity);
        }

        private void UpdateEffects()
        {
            if (!m_isActive) return;

            Collider[] hits = Physics.OverlapSphere(transform.position, m_radius);

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player") && m_hidePlayer)
                {
                    ApplyStealth(hit.gameObject);
                }

                if (hit.CompareTag("Enemy") && m_slowEnemies)
                {
                    ApplySlow(hit.gameObject);
                }
            }

            ClearOldAffected();
        }

        private void ApplyStealth(GameObject player)
        {
            Renderer renderer = player.GetComponent<Renderer>();
            if (renderer != null && renderer.material.HasProperty("_Alpha"))
            {
                float alpha = Mathf.Lerp(renderer.material.GetFloat("_Alpha"), 0.3f, Time.deltaTime);
                renderer.material.SetFloat("_Alpha", alpha);
            }
        }

        private void ApplySlow(GameObject enemy)
        {
            if (!m_affectedEnemies.Contains(enemy))
            {
                EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
                if (enemyBase != null)
                {
                    m_affectedEnemies.Add(enemy);
                }
            }
        }

        private void ClearOldAffected()
        {
            for (int i = m_affectedEnemies.Count - 1; i >= 0; i--)
            {
                GameObject enemy = m_affectedEnemies[i];
                if (enemy == null)
                {
                    m_affectedEnemies.RemoveAt(i);
                    continue;
                }

                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist > m_radius)
                {
                    m_affectedEnemies.RemoveAt(i);
                }
            }
        }

        private void UpdateOpacity(float opacity)
        {
            if (m_particleSystem == null) return;

            var main = m_particleSystem.main;
            main.startColor = new Color(m_smokeColor.r, m_smokeColor.g, m_smokeColor.b, opacity);
        }

        private void FadeOut()
        {
            m_isActive = false;

            ParticleSystem.MainModule main = m_particleSystem.main;
            main.startLifetime = 1f;

            var emission = m_particleSystem.emission;
            emission.rateOverTime = 0;

            Destroy(gameObject, 2f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, m_radius);
        }
    }

    public class SmokeCloudAbility : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float m_cooldown = 5f;
        [SerializeField] private float m_radius = 8f;
        [SerializeField] private float m_duration = 8f;
        [SerializeField] private float m_staminaCost = 20f;

        [Header("Combat")]
        [SerializeField] private bool m_revealHiddenEnemies = true;
        [SerializeField] private float m_visionBlockStrength = 0.8f;

        private AllomancerController m_allomancer;
        private PlayerStamina m_stamina;
        private float m_cooldownTimer;
        private AudioSource m_audioSource;

        private void Awake()
        {
            m_allomancer = GetComponent<AllomancerController>();
            m_stamina = GetComponent<PlayerStamina>();
            m_audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (m_cooldownTimer > 0)
            {
                m_cooldownTimer -= Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                TryUseSmokeCloud();
            }
        }

        private void TryUseSmokeCloud()
        {
            if (m_cooldownTimer > 0) return;

            if (!CanUse())
            {
                Debug.Log("Cannot use Smoke Cloud - not burning Copper or not enough stamina");
                return;
            }

            UseSmokeCloud();
        }

        private bool CanUse()
        {
            if (m_allomancer != null && !m_allomancer.IsBurning(AllomanticMetal.Copper))
                return false;

            if (m_stamina != null && m_stamina.CurrentStamina < m_staminaCost)
                return false;

            return true;
        }

        private void UseSmokeCloud()
        {
            SmokeCloud.Create(transform.position, m_radius, m_duration);

            if (m_stamina != null)
            {
                m_stamina.UseStamina(m_staminaCost);
            }

            m_cooldownTimer = m_cooldown;

            if (m_audioSource != null)
            {
                m_audioSource.Play();
            }

            Debug.Log("Smoke Cloud deployed!");
        }

        public float GetCooldownPercent()
        {
            if (m_cooldown <= 0) return 1f;
            return 1f - (m_cooldownTimer / m_cooldown);
        }

        public bool IsReady()
        {
            return m_cooldownTimer <= 0 && CanUse();
        }
    }

    public class MistCloud : MonoBehaviour
    {
        [Header("Mist Settings")]
        [SerializeField] private float m_radius = 20f;
        [SerializeField] private float m_density = 0.3f;
        [SerializeField] private bool m_hidesAllomancy = true;

        private ParticleSystem m_mistParticles;

        private void Awake()
        {
            SetupMist();
        }

        private void SetupMist()
        {
            GameObject mistObj = new GameObject("Mist");
            mistObj.transform.SetParent(transform);
            mistObj.transform.localPosition = Vector3.zero;

            m_mistParticles = mistObj.AddComponent<ParticleSystem>();

            var main = m_mistParticles.main;
            main.maxParticles = 1000;
            main.startLifetime = 10f;
            main.startSpeed = 2f;
            main.startSize = 5f;
            main.startColor = new Color(0.8f, 0.8f, 0.9f, m_density);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = m_mistParticles.emission;
            emission.rateOverTime = 100;

            var shape = m_mistParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = m_radius;

            var renderer = m_mistParticles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.color = new Color(0.8f, 0.8f, 0.9f, m_density * 0.5f);

            m_mistParticles.Play();
        }

        private void Update()
        {
            transform.position = Camera.main?.transform.position ?? transform.position;
        }
    }
}
