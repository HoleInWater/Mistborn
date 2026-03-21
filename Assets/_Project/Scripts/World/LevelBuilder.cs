using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.World
{
    public class LevelBuilder : MonoBehaviour
    {
        [Header("Building Blocks")]
        [SerializeField] private GameObject[] m_wallPrefabs;
        [SerializeField] private GameObject[] m_floorPrefabs;
        [SerializeField] private GameObject[] m_roofPrefabs;
        [SerializeField] private GameObject[] m_doorPrefabs;
        [SerializeField] private GameObject[] m_windowPrefabs;

        [Header("Environment")]
        [SerializeField] private GameObject[] m_streetPrefabs;
        [SerializeField] private GameObject[] m_propPrefabs;
        [SerializeField] private GameObject[] m_debrisPrefabs;

        [Header("Materials")]
        [SerializeField] private Material[] m_wallMaterials;
        [SerializeField] private Material[] m_floorMaterials;
        [SerializeField] private Material[] m_roofMaterials;

        [Header("Settings")]
        [SerializeField] private float m_gridSize = 2f;
        [SerializeField] private bool m_snapToGrid = true;

        private Dictionary<Vector3, GameObject> m_placedObjects = new Dictionary<Vector3, GameObject>();
        private Transform m_buildParent;

        private void Awake()
        {
            m_buildParent = new GameObject("BuiltLevel").transform;
            m_buildParent.SetParent(transform);
        }

        public void BuildWall(Vector3 position, int prefabIndex = 0)
        {
            if (prefabIndex < 0 || prefabIndex >= m_wallPrefabs.Length) return;

            Vector3 snappedPos = m_snapToGrid ? SnapToGrid(position) : position;

            if (m_placedObjects.ContainsKey(snappedPos)) return;

            GameObject wall = Instantiate(m_wallPrefabs[prefabIndex], snappedPos, Quaternion.identity, m_buildParent);
            m_placedObjects[snappedPos] = wall;
        }

        public void BuildFloor(Vector3 position, int prefabIndex = 0)
        {
            if (prefabIndex < 0 || prefabIndex >= m_floorPrefabs.Length) return;

            Vector3 snappedPos = m_snapToGrid ? SnapToGrid(position) : position;
            snappedPos.y = 0;

            if (m_placedObjects.ContainsKey(snappedPos)) return;

            GameObject floor = Instantiate(m_floorPrefabs[prefabIndex], snappedPos, Quaternion.identity, m_buildParent);
            m_placedObjects[snappedPos] = floor;
        }

        public void BuildRoom(Vector3 corner, Vector2 size)
        {
            float width = size.x * m_gridSize;
            float height = size.y * m_gridSize;

            for (float x = 0; x < width; x += m_gridSize)
            {
                BuildWall(corner + Vector3.right * x);
                BuildWall(corner + Vector3.right * x + Vector3.forward * height);
            }

            for (float z = 0; z < height; z += m_gridSize)
            {
                BuildWall(corner + Vector3.forward * z);
                BuildWall(corner + Vector3.forward * z + Vector3.right * width);
            }

            for (float x = m_gridSize; x < width - m_gridSize; x += m_gridSize)
            {
                for (float z = m_gridSize; z < height - m_gridSize; z += m_gridSize)
                {
                    BuildFloor(corner + Vector3.right * x + Vector3.forward * z);
                }
            }
        }

        public void AddDoor(Vector3 wallPosition)
        {
            Vector3 snappedPos = m_snapToGrid ? SnapToGrid(wallPosition) : wallPosition;

            if (m_doorPrefabs.Length > 0)
            {
                GameObject door = Instantiate(m_doorPrefabs[0], snappedPos, Quaternion.identity, m_buildParent);
            }
        }

        public void AddWindow(Vector3 wallPosition)
        {
            Vector3 snappedPos = m_snapToGrid ? SnapToGrid(wallPosition) : wallPosition;

            if (m_windowPrefabs.Length > 0)
            {
                GameObject window = Instantiate(m_windowPrefabs[0], snappedPos, Quaternion.identity, m_buildParent);
            }
        }

        public void ScatterDebris(Vector3 center, float radius, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 randomPos = center + Random.insideUnitSphere * radius;
                randomPos.y = 0;

                if (m_debrisPrefabs.Length > 0)
                {
                    int index = Random.Range(0, m_debrisPrefabs.Length);
                    GameObject debris = Instantiate(m_debrisPrefabs[index], randomPos, Random.rotation, m_buildParent);
                }
            }
        }

        public void SetMaterial(Material material)
        {
            foreach (var obj in m_placedObjects.Values)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = material;
                }
            }
        }

        public void ClearLevel()
        {
            foreach (var obj in m_placedObjects.Values)
            {
                if (obj != null)
                    Destroy(obj);
            }
            m_placedObjects.Clear();
        }

        private Vector3 SnapToGrid(Vector3 position)
        {
            return new Vector3(
                Mathf.Round(position.x / m_gridSize) * m_gridSize,
                Mathf.Round(position.y / m_gridSize) * m_gridSize,
                Mathf.Round(position.z / m_gridSize) * m_gridSize
            );
        }

        public void BuildStreet(Vector3 start, Vector3 end)
        {
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);

            for (float d = 0; d < distance; d += m_gridSize)
            {
                Vector3 pos = start + direction * d;
                pos.y = 0;

                if (m_streetPrefabs.Length > 0)
                {
                    GameObject street = Instantiate(m_streetPrefabs[0], pos, Quaternion.LookRotation(direction), m_buildParent);
                    m_placedObjects[pos] = street;
                }
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(LevelBuilder))]
    public class LevelBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            LevelBuilder builder = (LevelBuilder)target;

            if (UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorGUILayout.HelpBox("Level Builder is only available in Edit mode.", UnityEditor.MessageType.Info);
                return;
            }

            DrawDefaultInspector();

            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.LabelField("Build Actions", UnityEditor.EditorStyles.boldLabel);

            if (GUILayout.Button("Build Test Room (10x10)"))
            {
                builder.BuildRoom(Vector3.zero, new Vector2(10, 10));
            }

            if (GUILayout.Button("Scatter Debris"))
            {
                builder.ScatterDebris(Vector3.zero, 20f, 50);
            }

            if (GUILayout.Button("Clear Level"))
            {
                builder.ClearLevel();
            }
        }
    }
#endif
}
