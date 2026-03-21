using UnityEngine;
using System.Collections.Generic;
using Mistborn.Combat;

namespace Mistborn.Allomancy
{
    public class AtiumSight : MonoBehaviour
    {
        [Header("Atium Settings")]
        [SerializeField] private float m_visionRange = 50f;
        [SerializeField] private float m_slowMotionFactor = 0.3f;
        [SerializeField] private float m_dodgeWindowBonus = 0.5f;
        [SerializeField] private float m_costPerSecond = 15f;

        [Header("Visual")]
        [SerializeField] private Color m_atiumColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private float m_glowIntensity = 2f;
        [SerializeField] private GameObject m_atiumParticles;

        [Header("Predictions")]
        [SerializeField] private bool m_showAttackPredictions = true;
        [SerializeField] private bool m_highlightDodgeable = true;
        [SerializeField] private float m_predictionDuration = 1f;

        private AllomancerController m_allomancer;
        private List<AtiumTarget> m_visibleTargets = new List<AtiumTarget>();
        private GameObject m_activeParticles;
        private bool m_isActive;
        private SlowMotionEffect m_slowMotion;

        private class AtiumTarget
        {
            public Transform transform;
            public GameObject ghost;
            public LineRenderer predictionLine;
            public Vector3 predictedPosition;
            public bool isAttacking;
            public float attackTime;
        }

        private void Awake()
        {
            m_allomancer = GetComponent<AllomancerController>();
            m_slowMotion = FindObjectOfType<SlowMotionEffect>();
        }

        private void Update()
        {
            bool isBurning = m_allomancer != null && m_allomancer.IsBurning(AllomanticMetal.Atium);

            if (isBurning && !m_isActive)
            {
                ActivateAtium();
            }
            else if (!isBurning && m_isActive)
            {
                DeactivateAtium();
            }

            if (m_isActive)
            {
                UpdateTargets();
                UpdateSlowMotion();
                DrainReserve();
            }
        }

        private void ActivateAtium()
        {
            m_isActive = true;

            if (m_atiumParticles != null && m_activeParticles == null)
            {
                m_activeParticles = Instantiate(m_atiumParticles, transform);
            }

            FindTargets();
            CreateGhosts();
        }

        private void DeactivateAtium()
        {
            m_isActive = false;

            if (m_activeParticles != null)
            {
                Destroy(m_activeParticles);
                m_activeParticles = null;
            }

            DestroyGhosts();

            if (m_slowMotion != null && m_slowMotion.IsActive)
            {
                m_slowMotion.Deactivate();
            }
        }

        private void FindTargets()
        {
            m_visibleTargets.Clear();

            Collider[] hits = Physics.OverlapSphere(transform.position, m_visionRange);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    AtiumTarget target = new AtiumTarget
                    {
                        transform = hit.transform
                    };
                    m_visibleTargets.Add(target);
                }
            }
        }

        private void CreateGhosts()
        {
            foreach (AtiumTarget target in m_visibleTargets)
            {
                if (target.transform == null) continue;

                GameObject ghostObj = CreateGhostObject(target.transform);
                target.ghost = ghostObj;

                LineRenderer line = ghostObj.AddComponent<LineRenderer>();
                line.startWidth = 0.05f;
                line.endWidth = 0.05f;
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = m_atiumColor;
                line.endColor = new Color(m_atiumColor.r, m_atiumColor.g, m_atiumColor.b, 0.5f);
                target.predictionLine = line;
            }
        }

        private GameObject CreateGhostObject(Transform source)
        {
            GameObject ghost = new GameObject("AtiumGhost");
            ghost.transform.position = source.position;
            ghost.transform.rotation = source.rotation;
            ghost.transform.localScale = source.localScale;

            Renderer sourceRenderer = source.GetComponent<Renderer>();
            if (sourceRenderer != null)
            {
                Renderer ghostRenderer = ghost.AddComponent<MeshRenderer>();
                ghostRenderer.material = new Material(sourceRenderer.material);
                ghostRenderer.material.color = new Color(m_atiumColor.r, m_atiumColor.g, m_atiumColor.b, 0.3f);

                MeshFilter mf = ghost.AddComponent<MeshFilter>();
                MeshFilter sourceMF = sourceRenderer.GetComponent<MeshFilter>();
                if (sourceMF != null)
                {
                    mf.mesh = sourceMF.mesh;
                }
            }

            return ghost;
        }

        private void UpdateTargets()
        {
            foreach (AtiumTarget target in m_visibleTargets)
            {
                if (target.transform == null)
                {
                    if (target.ghost != null)
                        Destroy(target.ghost);
                    continue;
                }

                PredictPosition(target);
                UpdateGhost(target);
            }
        }

        private void PredictPosition(AtiumTarget target)
        {
            EnemyBase enemy = target.transform.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                Vector3 velocity = target.transform.forward * enemy.CurrentSpeed;
                target.predictedPosition = target.transform.position + velocity * m_predictionDuration;
            }
            else
            {
                target.predictedPosition = target.transform.position;
            }
        }

        private void UpdateGhost(AtiumTarget target)
        {
            if (target.ghost == null) return;

            target.ghost.transform.position = Vector3.Lerp(target.ghost.transform.position, target.predictedPosition, Time.deltaTime * 5f);
            target.ghost.transform.rotation = target.transform.rotation;

            if (target.predictionLine != null)
            {
                target.predictionLine.SetPosition(0, target.transform.position + Vector3.up);
                target.predictionLine.SetPosition(1, target.predictedPosition + Vector3.up);
            }
        }

        private void UpdateSlowMotion()
        {
            if (m_slowMotion != null)
            {
                m_slowMotion.SetTimeScale(m_slowMotionFactor);
            }
        }

        private void DrainReserve()
        {
            if (m_allomancer != null)
            {
                float drain = m_costPerSecond * Time.deltaTime;
                m_allomancer.DrainMetal(AllomanticMetal.Atium, drain);
            }
        }

        private void DestroyGhosts()
        {
            foreach (AtiumTarget target in m_visibleTargets)
            {
                if (target.ghost != null)
                    Destroy(target.ghost);
            }
            m_visibleTargets.Clear();
        }

        private void OnDestroy()
        {
            DestroyGhosts();
        }
    }

    public class MalatiumFuture : MonoBehaviour
    {
        [Header("Malatium Settings")]
        [SerializeField] private float m_visionRange = 30f;
        [SerializeField] private float m_costPerUse = 20f;
        [SerializeField] private float m_duration = 3f;

        [Header("Visual")]
        [SerializeField] private Color m_malatiumColor = new Color(1f, 0.4f, 0.2f);
        [SerializeField] private float m_shadowOpacity = 0.4f;

        private AllomancerController m_allomancer;
        private bool m_isActive;
        private float m_durationTimer;
        private List<FutureShadow> m_shadows = new List<FutureShadow>();

        private class FutureShadow
        {
            public GameObject shadow;
            public Transform source;
            public Vector3 originalPosition;
        }

        private void Update()
        {
            if (m_isActive)
            {
                m_durationTimer -= Time.deltaTime;
                if (m_durationTimer <= 0)
                {
                    Deactivate();
                }
            }
        }

        public void Activate()
        {
            if (m_allomancer == null)
            {
                m_allomancer = GetComponent<AllomancerController>();
            }

            if (m_allomancer != null && !m_allomancer.IsBurning(AllomanticMetal.Malatium))
            {
                Debug.Log("Must be burning Malatium");
                return;
            }

            if (!m_allomancer.ConsumeMetal(AllomanticMetal.Malatium, m_costPerUse))
            {
                Debug.Log("Not enough Malatium");
                return;
            }

            m_isActive = true;
            m_durationTimer = m_duration;

            CreateFutureShadows();
        }

        private void CreateFutureShadows()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, m_visionRange);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    CreateShadow(hit.transform);
                }
            }
        }

        private void CreateShadow(Transform source)
        {
            GameObject shadow = new GameObject("FutureShadow");
            shadow.transform.position = source.position;
            shadow.transform.rotation = source.rotation;

            Renderer sourceRenderer = source.GetComponent<Renderer>();
            if (sourceRenderer != null)
            {
                Renderer shadowRenderer = shadow.AddComponent<MeshRenderer>();
                shadowRenderer.material = new Material(sourceRenderer.material);
                shadowRenderer.material.color = new Color(m_malatiumColor.r, m_malatiumColor.g, m_malatiumColor.b, m_shadowOpacity);

                MeshFilter mf = shadow.AddComponent<MeshFilter>();
                MeshFilter sourceMF = sourceRenderer.GetComponent<MeshFilter>();
                if (sourceMF != null)
                {
                    mf.mesh = sourceMF.mesh;
                }
            }

            FutureShadow futureShadow = new FutureShadow
            {
                shadow = shadow,
                source = source,
                originalPosition = source.position
            };
            m_shadows.Add(futureShadow);
        }

        private void Deactivate()
        {
            m_isActive = false;

            foreach (FutureShadow fs in m_shadows)
            {
                if (fs.shadow != null)
                    Destroy(fs.shadow);
            }
            m_shadows.Clear();
        }

        private void OnDestroy()
        {
            Deactivate();
        }
    }

    public class DuraluminSurge : MonoBehaviour
    {
        [Header("Duralumin Settings")]
        [SerializeField] private float m_surgeForce = 100f;
        [SerializeField] private float m_surgeRadius = 15f;
        [SerializeField] private float m_cooldown = 30f;
        [SerializeField] private float m_cost = 50f;

        [Header("Effects")]
        [SerializeField] private GameObject m_surgeVFX;
        [SerializeField] private float m_knockbackMultiplier = 3f;
        [SerializeField] private bool m_affectsPlayer = false;

        private AllomancerController m_allomancer;
        private float m_cooldownTimer;

        private void Awake()
        {
            m_allomancer = GetComponent<AllomancerController>();
        }

        private void Update()
        {
            if (m_cooldownTimer > 0)
                m_cooldownTimer -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.R) && CanUseSurge())
            {
                ActivateSurge();
            }
        }

        private bool CanUseSurge()
        {
            return m_cooldownTimer <= 0 &&
                   m_allomancer != null &&
                   m_allomancer.IsBurning(AllomanticMetal.Duralumin);
        }

        private void ActivateSurge()
        {
            m_cooldownTimer = m_cooldown;

            if (m_surgeVFX != null)
            {
                Instantiate(m_surgeVFX, transform.position, Quaternion.identity);
            }

            Collider[] hits = Physics.OverlapSphere(transform.position, m_surgeRadius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    ApplySurgeForce(hit.gameObject);
                }
                else if (hit.CompareTag("Player") && m_affectsPlayer)
                {
                    ApplySurgeForce(hit.gameObject);
                }
            }

            Debug.Log("Duralumin Surge activated!");
        }

        private void ApplySurgeForce(GameObject target)
        {
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb == null) return;

            Vector3 direction = (target.transform.position - transform.position).normalized;
            direction.y = 0.5f;
            direction.Normalize();

            rb.AddForce(direction * m_surgeForce * m_knockbackMultiplier, ForceMode.Impulse);
        }

        public float GetCooldownPercent()
        {
            if (m_cooldown <= 0) return 1f;
            return 1f - (m_cooldownTimer / m_cooldown);
        }
    }

    public class ChromiumNegate : MonoBehaviour
    {
        [Header("Chromium Settings")]
        [SerializeField] private float m_negateRadius = 20f;
        [SerializeField] private float m_negateDuration = 5f;
        [SerializeField] private bool m_negateBuffs = true;
        [SerializeField] private bool m_negateDefense = true;

        private AllomancerController m_allomancer;
        private bool m_isActive;
        private List<GameObject> m_affectedEnemies = new List<GameObject>();

        private void Update()
        {
            bool isBurning = m_allomancer != null && m_allomancer.IsBurning(AllomanticMetal.Chromium);

            if (isBurning && !m_isActive)
            {
                Activate();
            }
            else if (!isBurning && m_isActive)
            {
                Deactivate();
            }
        }

        private void Activate()
        {
            m_isActive = true;
            FindEnemies();
            NegateEffects();
        }

        private void Deactivate()
        {
            m_isActive = false;
            m_affectedEnemies.Clear();
        }

        private void FindEnemies()
        {
            m_affectedEnemies.Clear();
            Collider[] hits = Physics.OverlapSphere(transform.position, m_negateRadius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    m_affectedEnemies.Add(hit.gameObject);
                }
            }
        }

        private void NegateEffects()
        {
            foreach (GameObject enemy in m_affectedEnemies)
            {
                if (enemy == null) continue;

                if (m_negateDefense)
                {
                    EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
                    if (enemyBase != null)
                    {
                    }
                }
            }
        }
    }
}
