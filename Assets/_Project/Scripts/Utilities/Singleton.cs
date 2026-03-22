using UnityEngine;

namespace MistbornGame.Singleton
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static readonly object lockObject = new object();
        private static bool applicationIsQuitting = false;
        
        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} is already destroyed. Returning null.");
                    return null;
                }
                
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<T>();
                        
                        if (instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"{typeof(T).Name} (Singleton)";
                            
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
                    
                    return instance;
                }
            }
        }
        
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }
        
        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
    
    public class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static bool isInitialized = false;
        
        public static T Instance
        {
            get
            {
                if (!isInitialized)
                {
                    instance = FindObjectOfType<T>();
                    
                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        instance = singletonObject.AddComponent<T>();
                        singletonObject.name = $"{typeof(T).Name} (Persistent Singleton)";
                    }
                    
                    isInitialized = true;
                }
                
                return instance;
            }
        }
        
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(transform.root.gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        public static bool HasInstance()
        {
            return instance != null;
        }
    }
    
    public class RuntimeSingleton<T> where T : MonoBehaviour, new()
    {
        private static T instance;
        
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject(typeof(T).Name).AddComponent<T>();
                }
                return instance;
            }
        }
        
        public static void CreateIfNotExists()
        {
            if (instance == null)
            {
                instance = new GameObject(typeof(T).Name).AddComponent<T>();
            }
        }
        
        public static void DestroyIfExists()
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
                instance = null;
            }
        }
    }
}
