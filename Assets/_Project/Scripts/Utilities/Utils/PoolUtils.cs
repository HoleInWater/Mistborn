using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for handling common object pooling operations
    /// </summary>
    public static class PoolUtils
    {
        /// <summary>
        /// Creates a simple pool for prefabs
        /// </summary>
        public static GameObject GetFromPool(GameObject prefab, Transform parent = null, bool worldPositionStays = true)
        {
            if (prefab == null) return null;
            
            // This is a simplified pool - in production you'd want a proper pooling system
            GameObject instance = Object.Instantiate(prefab, parent, worldPositionStays);
            return instance;
        }

        /// <summary>
        /// Returns an object to the pool (destroys it in this simple implementation)
        /// </summary>
        public static void ReturnToPool(GameObject obj)
        {
            if (obj != null)
            {
                Object.Destroy(obj);
            }
        }

        /// <summary>
        /// Gets a pooled object or creates a new one if none available
        /// </summary>
        public static T GetOrCreate<T>(T prefab, Transform parent = null, bool worldPositionStays = true) where T : Component
        {
            if (prefab == null) return null;
            
            T instance = Object.Instantiate(prefab, parent, worldPositionStays);
            return instance;
        }

        /// <summary>
        /// Creates a pool of objects and returns them
        /// </summary>
        public static List<T> CreatePool<T>(T prefab, int count, Transform parent = null, bool worldPositionStays = true) where T : Component
        {
            List<T> pool = new List<T>();
            if (prefab == null) return pool;
            
            for (int i = 0; i < count; i++)
            {
                T instance = Object.Instantiate(prefab, parent, worldPositionStays);
                pool.Add(instance);
            }
            return pool;
        }

        /// <summary>
        /// Gets an object from a pool and marks it as used
        /// </summary>
        public static T GetFromPool<T>(List<T> pool) where T : Component
        {
            if (pool == null || pool.Count == 0) return null;
            
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null && !pool[i].gameObject.activeInHierarchy)
                {
                    pool[i].gameObject.SetActive(true);
                    return pool[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Returns an object to the pool and marks it as unused
        /// </summary>
        public static void ReturnToPool<T>(List<T> pool, T obj) where T : Component
        {
            if (pool == null || obj == null) return;
            
            if (pool.Contains(obj))
            {
                obj.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Pre-warms a pool by activating and then deactivating all objects
        /// </summary>
        public static void WarmUpPool<T>(List<T> pool) where T : Component
        {
            if (pool == null) return;
            
            foreach (T obj in pool)
            {
                if (obj != null)
                {
                    obj.gameObject.SetActive(true);
                    obj.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Clears a pool by destroying all objects
        /// </summary>
        public static void ClearPool<T>(List<T> pool) where T : Component
        {
            if (pool == null) return;
            
            foreach (T obj in pool)
            {
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }
            pool.Clear();
        }
    }
}