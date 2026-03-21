using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.Effects
{
    public class ParticleEffectManager : MonoBehaviour
    {
        public static ParticleEffectManager Instance { get; private set; }

        [Header("Pools")]
        [SerializeField] private int m_initialPoolSize = 20;
        [SerializeField] private Transform m_poolParent;

        [Header("Prefabs")]
        [SerializeField] private GameObject m_steelPushEffect;
        [SerializeField] private GameObject m_ironPullEffect;
        [SerializeField] private GameObject m_pewterGlow;
        [SerializeField] private GameObject m_tinSparks;
        [SerializeField] private GameObject m_copperCloud;
        [SerializeField] private GameObject m_metalPickupEffect;
        [SerializeField] private GameObject m_impactEffect;
        [SerializeField] private GameObject m_healingEffect;

        private Dictionary<string, Queue<GameObject>> m_effectPools = new Dictionary<string, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (m_poolParent == null)
            {
                m_poolParent = new GameObject("EffectPool").transform;
                m_poolParent.SetParent(transform);
            }

            InitializePools();
        }

        private void InitializePools()
        {
            CreatePool("SteelPush", m_steelPushEffect);
            CreatePool("IronPull", m_ironPullEffect);
            CreatePool("PewterGlow", m_pewterGlow);
            CreatePool("TinSparks", m_tinSparks);
            CreatePool("CopperCloud", m_copperCloud);
            CreatePool("MetalPickup", m_metalPickupEffect);
            CreatePool("Impact", m_impactEffect);
            CreatePool("Healing", m_healingEffect);
        }

        private void CreatePool(string poolName, GameObject prefab)
        {
            if (prefab == null) return;

            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < m_initialPoolSize; i++)
            {
                GameObject obj = Instantiate(prefab, m_poolParent);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
            m_effectPools[poolName] = pool;
        }

        public GameObject Play(string effectName, Vector3 position, Quaternion rotation = default, float duration = 2f)
        {
            if (!m_effectPools.ContainsKey(effectName))
            {
                Debug.LogWarning($"Effect pool '{effectName}' not found");
                return null;
            }

            GameObject effect = GetFromPool(effectName);
            if (effect == null) return null;

            effect.transform.position = position;
            effect.transform.rotation = rotation == default ? Quaternion.identity : rotation;
            effect.SetActive(true);

            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                Destroy(effect, Mathf.Max(ps.main.duration, duration));
            }
            else
            {
                Destroy(effect, duration);
            }

            return effect;
        }

        public GameObject Play(string effectName, Vector3 position, Vector3 direction, float duration = 2f)
        {
            GameObject effect = Play(effectName, position, Quaternion.LookRotation(direction), duration);

            if (effect != null)
            {
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null && ps.shape.shapeType == ParticleSystemShapeType.Cone)
                {
                    effect.transform.rotation = Quaternion.LookRotation(direction);
                }
            }

            return effect;
        }

        public GameObject PlayOnSurface(string effectName, Vector3 position, Vector3 normal, float duration = 2f)
        {
            if (!m_effectPools.ContainsKey(effectName))
                return null;

            GameObject effect = GetFromPool(effectName);
            if (effect == null) return null;

            effect.transform.position = position;
            effect.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            effect.SetActive(true);

            Destroy(effect, duration);
            return effect;
        }

        public GameObject PlayOnObject(string effectName, Transform parent, Vector3 localPosition = default, float duration = 2f)
        {
            if (!m_effectPools.ContainsKey(effectName))
                return null;

            GameObject effect = GetFromPool(effectName);
            if (effect == null) return null;

            effect.transform.SetParent(parent);
            effect.transform.localPosition = localPosition == default ? Vector3.zero : localPosition;
            effect.transform.localRotation = Quaternion.identity;
            effect.SetActive(true);

            Destroy(effect, duration);
            return effect;
        }

        private GameObject GetFromPool(string poolName)
        {
            if (!m_effectPools.ContainsKey(poolName))
                return null;

            Queue<GameObject> pool = m_effectPools[poolName];

            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }

            GameObject prefab = GetPrefab(poolName);
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab, m_poolParent);
                obj.SetActive(false);
                return obj;
            }

            return null;
        }

        private GameObject GetPrefab(string poolName)
        {
            return poolName switch
            {
                "SteelPush" => m_steelPushEffect,
                "IronPull" => m_ironPullEffect,
                "PewterGlow" => m_pewterGlow,
                "TinSparks" => m_tinSparks,
                "CopperCloud" => m_copperCloud,
                "MetalPickup" => m_metalPickupEffect,
                "Impact" => m_impactEffect,
                "Healing" => m_healingEffect,
                _ => null
            };
        }

        public void ReturnToPool(string poolName, GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);
            obj.transform.SetParent(m_poolParent);

            if (!m_effectPools.ContainsKey(poolName))
            {
                m_effectPools[poolName] = new Queue<GameObject>();
            }

            m_effectPools[poolName].Enqueue(obj);
        }
    }

    public static class EffectExtensions
    {
        public static void PlaySteelPush(this Component component, Vector3 position, Vector3 direction)
        {
            ParticleEffectManager.Instance?.Play("SteelPush", position, direction);
        }

        public static void PlayIronPull(this Component component, Vector3 position, Vector3 direction)
        {
            ParticleEffectManager.Instance?.Play("IronPull", position, direction);
        }

        public static void PlayPewterGlow(this Component component, Transform target)
        {
            ParticleEffectManager.Instance?.PlayOnObject("PewterGlow", target);
        }

        public static void PlayMetalPickup(this Component component, Vector3 position)
        {
            ParticleEffectManager.Instance?.Play("MetalPickup", position);
        }

        public static void PlayImpact(this Component component, Vector3 position, Vector3 normal)
        {
            ParticleEffectManager.Instance?.PlayOnSurface("Impact", position, normal);
        }
    }
}
