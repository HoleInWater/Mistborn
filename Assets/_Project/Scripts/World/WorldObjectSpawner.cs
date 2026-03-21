using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.World
{
    public class WorldObjectSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private float m_spawnRadius = 50f;
        [SerializeField] private int m_maxObjects = 100;
        [SerializeField] private bool m_spawnOnStart = true;

        [Header("Object Pools")]
        [SerializeField] private GameObject[] m_propPrefabs;
        [SerializeField] private int[] m_poolSizes;

        private Dictionary<string, Queue<GameObject>> m_objectPools = new Dictionary<string, Queue<GameObject>>();
        private Transform m_poolParent;

        private void Awake()
        {
            m_poolParent = new GameObject("ObjectPool").transform;
            m_poolParent.SetParent(transform);
            InitializePools();
        }

        private void Start()
        {
            if (m_spawnOnStart)
            {
                SpawnInitialObjects();
            }
        }

        private void InitializePools()
        {
            for (int i = 0; i < m_propPrefabs.Length; i++)
            {
                GameObject prefab = m_propPrefabs[i];
                int poolSize = i < m_poolSizes.Length ? m_poolSizes[i] : 5;

                Queue<GameObject> pool = new Queue<GameObject>();
                for (int j = 0; j < poolSize; j++)
                {
                    GameObject obj = Instantiate(prefab, m_poolParent);
                    obj.SetActive(false);
                    pool.Enqueue(obj);
                }

                m_objectPools[prefab.name] = pool;
            }
        }

        private void SpawnInitialObjects()
        {
            foreach (GameObject prefab in m_propPrefabs)
            {
                int count = Random.Range(1, 5);
                for (int i = 0; i < count; i++)
                {
                    SpawnObject(prefab.name);
                }
            }
        }

        public GameObject SpawnObject(string objectName)
        {
            if (!m_objectPools.ContainsKey(objectName))
            {
                Debug.LogWarning($"No pool for object: {objectName}");
                return null;
            }

            Queue<GameObject> pool = m_objectPools[objectName];
            GameObject obj;

            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else
            {
                foreach (GameObject prefab in m_propPrefabs)
                {
                    if (prefab.name == objectName)
                    {
                        obj = Instantiate(prefab, m_poolParent);
                        break;
                    }
                }
                return null;
            }

            Vector3 spawnPos = GetRandomSpawnPosition();
            obj.transform.position = spawnPos;
            obj.transform.rotation = Random.rotation;
            obj.SetActive(true);

            return obj;
        }

        public void ReturnObject(GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);
            obj.transform.SetParent(m_poolParent);

            string poolName = obj.name.Replace("(Clone)", "").Trim();
            if (m_objectPools.ContainsKey(poolName))
            {
                m_objectPools[poolName].Enqueue(obj);
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            Vector3 offset = new Vector3(randomCircle.x, 0, randomCircle.y) * Random.Range(5f, m_spawnRadius);
            Vector3 spawnPos = transform.position + offset;

            RaycastHit hit;
            if (Physics.Raycast(spawnPos + Vector3.up * 50f, Vector3.down, out hit, 100f))
            {
                spawnPos.y = hit.point.y;
            }
            else
            {
                spawnPos.y = 0;
            }

            return spawnPos;
        }

        public void DespawnAll()
        {
            foreach (var pool in m_objectPools.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (obj != null)
                        Destroy(obj);
                }
            }
            m_objectPools.Clear();
            InitializePools();
        }
    }

    public class DebrisPlacer : MonoBehaviour
    {
        [Header("Debris Settings")]
        [SerializeField] private GameObject[] m_debrisPrefabs;
        [SerializeField] private int m_debrisCount = 50;
        [SerializeField] private float m_spawnRadius = 30f;
        [SerializeField] private float m_minScale = 0.5f;
        [SerializeField] private float m_maxScale = 2f;

        [Header("Placement")]
        [SerializeField] private bool m_alignToGround = true;
        [SerializeField] private bool m_randomRotation = true;
        [SerializeField] private LayerMask m_groundLayer = ~0;

        private void Start()
        {
            PlaceDebris();
        }

        public void PlaceDebris()
        {
            for (int i = 0; i < m_debrisCount; i++)
            {
                Vector3 position = GetRandomPosition();
                Quaternion rotation = m_randomRotation ? Random.rotation : Quaternion.identity;

                GameObject debris = Instantiate(GetRandomDebris(), position, rotation, transform);

                float scale = Random.Range(m_minScale, m_maxScale);
                debris.transform.localScale = Vector3.one * scale;

                if (m_alignToGround)
                {
                    AlignToGround(debris);
                }
            }
        }

        private Vector3 GetRandomPosition()
        {
            Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(5f, m_spawnRadius);
            Vector3 pos = transform.position + new Vector3(circle.x, 50f, circle.y);

            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 100f, m_groundLayer))
            {
                return hit.point;
            }

            return transform.position + Random.insideUnitSphere * m_spawnRadius;
        }

        private void AlignToGround(GameObject obj)
        {
            Vector3 pos = obj.transform.position + Vector3.up * 10f;

            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 20f, m_groundLayer))
            {
                obj.transform.position = hit.point;
                obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * obj.transform.rotation;
            }
        }

        private GameObject GetRandomDebris()
        {
            if (m_debrisPrefabs.Length == 0) return new GameObject("Debris");

            return m_debrisPrefabs[Random.Range(0, m_debrisPrefabs.Length)];
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, m_spawnRadius);
        }
    }

    public class AshFall : MonoBehaviour
    {
        [Header("Particle Settings")]
        [SerializeField] private int m_particleCount = 500;
        [SerializeField] private float m_fallSpeed = 2f;
        [SerializeField] private float m_windStrength = 1f;
        [SerializeField] private Vector3 m_windDirection = Vector3.right;
        [SerializeField] private Color m_ashColor = new Color(0.4f, 0.4f, 0.4f);

        [Header("Spawn Area")]
        [SerializeField] private float m_spawnRadius = 50f;
        [SerializeField] private float m_spawnHeight = 30f;

        private ParticleSystem m_particleSystem;

        private void Awake()
        {
            SetupParticles();
        }

        private void SetupParticles()
        {
            m_particleSystem = GetComponent<ParticleSystem>();
            if (m_particleSystem == null)
            {
                m_particleSystem = gameObject.AddComponent<ParticleSystem>();
            }

            var main = m_particleSystem.main;
            main.maxParticles = m_particleCount;
            main.startLifetime = m_spawnHeight / m_fallSpeed;
            main.startSpeed = m_fallSpeed;
            main.startSize = 0.1f;
            main.startColor = m_ashColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = m_particleSystem.emission;
            emission.rateOverTime = m_particleCount / main.startLifetime.constant;

            var shape = m_particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 30f;
            shape.radius = m_spawnRadius;

            var velocityOverLifetime = m_particleSystem.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.x = m_windDirection.x * m_windStrength;
            velocityOverLifetime.z = m_windDirection.z * m_windStrength;

            Renderer renderer = m_particleSystem.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                renderer.material.color = m_ashColor;
            }

            m_particleSystem.Play();
        }

        public void SetWind(Vector3 direction, float strength)
        {
            m_windDirection = direction.normalized;
            m_windStrength = strength;

            var velocity = m_particleSystem.velocityOverLifetime;
            velocity.x = m_windDirection.x * m_windStrength;
            velocity.z = m_windDirection.z * m_windStrength;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, m_spawnRadius);
        }
    }
}
