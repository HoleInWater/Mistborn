using UnityEngine;
/// <summary>
/// Simple object pool for reusing GameObjects.
/// Usage: ObjectPool.Instance.Get("coin", position);
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }
    
    // POOLS - Assign prefabs in Inspector
    public GameObject[] pooledPrefabs;
    public int[] poolSizes;
    
    // INTERNAL
    private System.Collections.Generic.Dictionary<string, System.Collections.Generic.Queue<GameObject>> pools;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePools();
        }
    }
    
    void InitializePools()
    {
        pools = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Queue<GameObject>>();
        
        for (int i = 0; i < pooledPrefabs.Length; i++)
        {
            GameObject prefab = pooledPrefabs[i];
            int size = poolSizes.Length > i ? poolSizes[i] : 10;
            string name = prefab.name;
            
            Queue<GameObject> queue = new System.Collections.Generic.Queue<GameObject>();
            
            for (int j = 0; j < size; j++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
            
            pools.Add(name, queue);
        }
    }
    
    // Get object from pool
    public GameObject Get(string poolName, Vector3 position, Quaternion rotation = default)
    {
        if (!pools.ContainsKey(poolName))
        {
            Debug.LogWarning($"Pool '{poolName}' not found!");
            return null;
        }
        
        Queue<GameObject> pool = pools[poolName];
        
        // Try to find inactive object
        while (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            
            if (obj != null && !obj.activeSelf)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                return obj;
            }
        }
        
        // No inactive objects - instantiate new
        GameObject prefab = System.Array.Find(pooledPrefabs, p => p.name == poolName);
        if (prefab != null)
        {
            GameObject newObj = Instantiate(prefab, position, rotation, transform);
            return newObj;
        }
        
        return null;
    }
    
    // Return object to pool
    public void Return(string poolName, GameObject obj)
    {
        if (pools.ContainsKey(poolName))
        {
            obj.SetActive(false);
            pools[poolName].Enqueue(obj);
        }
    }
}
