using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mistborn.Effects
{
    public class VFXLibrary : MonoBehaviour
    {
        public static VFXLibrary Instance { get; private set; }

        [Serializable]
        public class VFXEntry
        {
            public string vfxId;
            public GameObject prefab;
            public bool isLooping;
            public float duration;
        }

        [Header("VFX Library")]
        [SerializeField] private List<VFXEntry> m_vfxEntries = new List<VFXEntry>();

        private Dictionary<string, VFXEntry> m_vfxLookup = new Dictionary<string, VFXEntry>();
        private Dictionary<string, Queue<GameObject>> m_pools = new Dictionary<string, Queue<GameObject>>();
        private Transform m_poolParent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildLookup();
            CreatePoolParent();
            InitializePools();
        }

        private void BuildLookup()
        {
            m_vfxLookup.Clear();
            foreach (VFXEntry entry in m_vfxEntries)
            {
                if (!string.IsNullOrEmpty(entry.vfxId) && entry.prefab != null)
                {
                    m_vfxLookup[entry.vfxId] = entry;
                }
            }
        }

        private void CreatePoolParent()
        {
            m_poolParent = new GameObject("VFX_Pool").transform;
            m_poolParent.SetParent(transform);
        }

        private void InitializePools()
        {
            foreach (var kvp in m_vfxLookup)
            {
                m_pools[kvp.Key] = new Queue<GameObject>();
                for (int i = 0; i < 5; i++)
                {
                    GameObject obj = Instantiate(kvp.Value.prefab, m_poolParent);
                    obj.SetActive(false);
                    m_pools[kvp.Key].Enqueue(obj);
                }
            }
        }

        public GameObject Play(string vfxId, Vector3 position, Quaternion rotation = default)
        {
            if (!m_vfxLookup.TryGetValue(vfxId, out VFXEntry entry))
            {
                Debug.LogWarning($"VFX '{vfxId}' not found");
                return null;
            }

            GameObject vfx = GetFromPool(vfxId);
            if (vfx == null) return null;

            vfx.transform.position = position;
            vfx.transform.rotation = rotation == default ? Quaternion.identity : rotation;
            vfx.SetActive(true);

            if (!entry.isLooping)
            {
                float duration = entry.duration > 0 ? entry.duration : 3f;
                Destroy(vfx, duration);
            }

            return vfx;
        }

        public GameObject Play(string vfxId, Transform parent, Vector3 localPosition = default)
        {
            GameObject vfx = Play(vfxId, parent.position, parent.rotation);
            if (vfx != null)
            {
                vfx.transform.SetParent(parent);
                vfx.transform.localPosition = localPosition;
                vfx.transform.localRotation = Quaternion.identity;
            }
            return vfx;
        }

        public GameObject PlayOnSurface(string vfxId, Vector3 position, Vector3 normal)
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
            return Play(vfxId, position, rotation);
        }

        public void Stop(string vfxId, GameObject vfx)
        {
            if (vfx != null)
            {
                vfx.SetActive(false);
                ReturnToPool(vfxId, vfx);
            }
        }

        private GameObject GetFromPool(string vfxId)
        {
            if (!m_pools.TryGetValue(vfxId, out Queue<GameObject> pool))
                return null;

            if (pool.Count > 0)
                return pool.Dequeue();

            if (m_vfxLookup.TryGetValue(vfxId, out VFXEntry entry))
            {
                GameObject obj = Instantiate(entry.prefab, m_poolParent);
                return obj;
            }

            return null;
        }

        private void ReturnToPool(string vfxId, GameObject obj)
        {
            if (!m_pools.ContainsKey(vfxId))
                m_pools[vfxId] = new Queue<GameObject>();

            obj.SetActive(false);
            obj.transform.SetParent(m_poolParent);
            m_pools[vfxId].Enqueue(obj);
        }
    }

    public class TrailManager : MonoBehaviour
    {
        public enum TrailType { Steel, Iron, Pewter, Tin, Custom }

        [Header("Trail Settings")]
        [SerializeField] private TrailType m_trailType = TrailType.Steel;
        [SerializeField] private GameObject m_trailPrefab;
        [SerializeField] private float m_trailLifetime = 0.5f;
        [SerializeField] private float m_spawnRate = 0.02f;
        [SerializeField] private bool m_useWorldSpace = true;

        [Header("Visual")]
        [SerializeField] private Color m_trailColor = Color.cyan;
        [SerializeField] private float m_startWidth = 0.1f;
        [SerializeField] private float m_endWidth = 0f;

        private TrailRenderer m_trail;
        private float m_spawnTimer;
        private bool m_isActive;
        private Transform m_target;

        private void Awake()
        {
            SetupTrail();
        }

        private void SetupTrail()
        {
            m_trail = GetComponent<TrailRenderer>();
            if (m_trail == null)
                m_trail = gameObject.AddComponent<TrailRenderer>();

            m_trail.time = m_trailLifetime;
            m_trail.startWidth = m_startWidth;
            m_trail.endWidth = m_endWidth;
            m_trail.startColor = m_trailColor;
            m_trail.endColor = new Color(m_trailColor.r, m_trailColor.g, m_trailColor.b, 0f);
            m_trail.material = new Material(Shader.Find("Sprites/Default"));
            m_trail.material.color = m_trailColor;
            m_trail.numCapVertices = 5;
            m_trail.numCornerVertices = 5;

            if (!m_useWorldSpace)
                m_trail.space = TrailRenderer.ComponentSpace.Local;
        }

        public void SetColor(Color color)
        {
            m_trailColor = color;
            m_trail.startColor = color;
            m_trail.endColor = new Color(color.r, color.g, color.b, 0f);
            m_trail.material.color = color;
        }

        public void StartTrail()
        {
            m_isActive = true;
            m_trail.Clear();
        }

        public void StopTrail()
        {
            m_isActive = false;
        }

        private void Update()
        {
            if (!m_isActive) return;

            m_spawnTimer -= Time.deltaTime;
            if (m_spawnTimer <= 0)
            {
                m_spawnTimer = m_spawnRate;
            }
        }

        public static TrailManager CreateTrail(Transform parent, TrailType type)
        {
            GameObject trailObj = new GameObject($"{type}Trail");
            trailObj.transform.SetParent(parent);

            Vector3 offset = type switch
            {
                TrailType.Steel => Vector3.forward * 0.5f,
                TrailType.Iron => Vector3.back * 0.5f,
                TrailType.Pewter => Vector3.up * 0.2f,
                TrailType.Tin => Vector3.forward * 0.3f,
                _ => Vector3.zero
            };

            trailObj.transform.localPosition = offset;

            TrailManager trail = trailObj.AddComponent<TrailManager>();
            trail.SetTrailType(type);

            return trail;
        }

        public void SetTrailType(TrailType type)
        {
            m_trailType = type;

            Color color = type switch
            {
                TrailType.Steel => Color.cyan,
                TrailType.Iron => Color.blue,
                TrailType.Pewter => new Color(1f, 0.5f, 0f),
                TrailType.Tin => Color.white,
                _ => Color.white
            };

            SetColor(color);
        }
    }

    public class SlowMotionEffect : MonoBehaviour
    {
        public static SlowMotionEffect Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float m_defaultTimescale = 1f;
        [SerializeField] private float m_transitionSpeed = 5f;

        private float m_targetTimeScale = 1f;
        private float m_currentTimeScale = 1f;
        private bool m_isActive;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (Mathf.Abs(m_currentTimeScale - m_targetTimeScale) > 0.01f)
            {
                m_currentTimeScale = Mathf.Lerp(m_currentTimeScale, m_targetTimeScale, m_transitionSpeed * Time.deltaTime);
                Time.timeScale = m_currentTimeScale;
                Time.fixedDeltaTime = 0.02f * m_currentTimeScale;
            }
        }

        public void Activate(float timeScale = 0.3f, float duration = 2f)
        {
            m_targetTimeScale = timeScale;
            m_isActive = true;

            if (duration > 0)
            {
                CancelInvoke(nameof(Deactivate));
                Invoke(nameof(Deactivate), duration);
            }
        }

        public void Deactivate()
        {
            m_targetTimeScale = m_defaultTimescale;
            m_isActive = false;
        }

        public void SetTimeScale(float timeScale)
        {
            m_targetTimeScale = timeScale;
        }

        public bool IsActive => m_isActive;
        public float CurrentTimeScale => m_currentTimeScale;
    }
}
