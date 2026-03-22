using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Pooling
{
    public class GenericObjectPool<T> where T : Component
    {
        private Queue<T> availableObjects = new Queue<T>();
        private List<T> activeObjects = new List<T>();
        
        private T prefab;
        private Transform parent;
        private int maxSize;
        
        public int ActiveCount => activeObjects.Count;
        public int AvailableCount => availableObjects.Count;
        public int TotalCount => activeObjects.Count + availableObjects.Count;
        
        public GenericObjectPool(T prefab, int initialSize, int maxSize = 100, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.maxSize = maxSize;
            
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }
        
        private T CreateNewObject()
        {
            if (TotalCount >= maxSize)
                return null;
            
            T obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            availableObjects.Enqueue(obj);
            
            return obj;
        }
        
        public T Get(Vector3 position, Quaternion rotation)
        {
            T obj;
            
            if (availableObjects.Count > 0)
            {
                obj = availableObjects.Dequeue();
            }
            else if (TotalCount < maxSize)
            {
                obj = CreateNewObject();
            }
            else
            {
                return null;
            }
            
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.gameObject.SetActive(true);
            activeObjects.Add(obj);
            
            return obj;
        }
        
        public T Get()
        {
            return Get(Vector3.zero, Quaternion.identity);
        }
        
        public T Get(Vector3 position)
        {
            return Get(position, Quaternion.identity);
        }
        
        public void Return(T obj)
        {
            if (obj == null)
                return;
            
            if (!activeObjects.Contains(obj))
                return;
            
            obj.gameObject.SetActive(false);
            activeObjects.Remove(obj);
            availableObjects.Enqueue(obj);
        }
        
        public void ReturnAll()
        {
            for (int i = activeObjects.Count - 1; i >= 0; i--)
            {
                Return(activeObjects[i]);
            }
        }
        
        public void Expand(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                CreateNewObject();
            }
        }
        
        public void Shrink(int amount)
        {
            for (int i = 0; i < amount && availableObjects.Count > 0; i++)
            {
                T obj = availableObjects.Dequeue();
                Object.Destroy(obj.gameObject);
            }
        }
    }
    
    public class GameObjectPool
    {
        private Queue<GameObject> availableObjects = new Queue<GameObject>();
        private List<GameObject> activeObjects = new List<GameObject>();
        
        private GameObject prefab;
        private Transform parent;
        private int maxSize;
        
        public int ActiveCount => activeObjects.Count;
        public int AvailableCount => availableObjects.Count;
        public int TotalCount => activeObjects.Count + availableObjects.Count;
        
        public GameObjectPool(GameObject prefab, int initialSize, int maxSize = 100, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.maxSize = maxSize;
            
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }
        
        private GameObject CreateNewObject()
        {
            if (TotalCount >= maxSize)
                return null;
            
            GameObject obj = Object.Instantiate(prefab, parent);
            obj.SetActive(false);
            availableObjects.Enqueue(obj);
            
            return obj;
        }
        
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject obj;
            
            if (availableObjects.Count > 0)
            {
                obj = availableObjects.Dequeue();
            }
            else if (TotalCount < maxSize)
            {
                obj = CreateNewObject();
            }
            else
            {
                return null;
            }
            
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            activeObjects.Add(obj);
            
            return obj;
        }
        
        public GameObject Get()
        {
            return Get(Vector3.zero, Quaternion.identity);
        }
        
        public GameObject Get(Vector3 position)
        {
            return Get(position, Quaternion.identity);
        }
        
        public void Return(GameObject obj)
        {
            if (obj == null)
                return;
            
            if (!activeObjects.Contains(obj))
                return;
            
            obj.SetActive(false);
            activeObjects.Remove(obj);
            availableObjects.Enqueue(obj);
        }
        
        public void ReturnAll()
        {
            for (int i = activeObjects.Count - 1; i >= 0; i--)
            {
                Return(activeObjects[i]);
            }
        }
    }
    
    public class PoolManager : MonoBehaviour
    {
        [System.Serializable]
        public class PoolConfig
        {
            public string poolName;
            public GameObject prefab;
            public int initialSize = 10;
            public int maxSize = 100;
        }
        
        [SerializeField] private PoolConfig[] poolConfigs;
        
        private Dictionary<string, GameObjectPool> pools = new Dictionary<string, GameObjectPool>();
        
        private void Awake()
        {
            InitializePools();
        }
        
        private void InitializePools()
        {
            foreach (var config in poolConfigs)
            {
                if (config.prefab != null && !string.IsNullOrEmpty(config.poolName))
                {
                    pools[config.poolName] = new GameObjectPool(
                        config.prefab,
                        config.initialSize,
                        config.maxSize,
                        transform
                    );
                }
            }
        }
        
        public GameObject Get(string poolName, Vector3 position, Quaternion rotation)
        {
            if (pools.ContainsKey(poolName))
            {
                return pools[poolName].Get(position, rotation);
            }
            
            Debug.LogWarning($"Pool not found: {poolName}");
            return null;
        }
        
        public GameObject Get(string poolName)
        {
            return Get(poolName, Vector3.zero, Quaternion.identity);
        }
        
        public GameObject Get(string poolName, Vector3 position)
        {
            return Get(poolName, position, Quaternion.identity);
        }
        
        public void Return(string poolName, GameObject obj)
        {
            if (pools.ContainsKey(poolName))
            {
                pools[poolName].Return(obj);
            }
        }
        
        public void ReturnAll(string poolName)
        {
            if (pools.ContainsKey(poolName))
            {
                pools[poolName].ReturnAll();
            }
        }
        
        public void ReturnAll()
        {
            foreach (var pool in pools.Values)
            {
                pool.ReturnAll();
            }
        }
        
        public void CreatePool(string poolName, GameObject prefab, int initialSize = 10, int maxSize = 100)
        {
            if (!pools.ContainsKey(poolName))
            {
                pools[poolName] = new GameObjectPool(prefab, initialSize, maxSize, transform);
            }
        }
        
        public void DestroyPool(string poolName)
        {
            if (pools.ContainsKey(poolName))
            {
                pools[poolName].ReturnAll();
                pools.Remove(poolName);
            }
        }
    }
}
