using UnityEngine;

namespace MistbornGame.Utilities
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
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
                
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));
                        
                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"(Singleton) {typeof(T)}";
                            
                            DontDestroyOnLoad(singletonObject);
                            Debug.Log($"[Singleton] An instance of {typeof(T)} was created.");
                        }
                        else
                        {
                            Debug.Log($"[Singleton] Using existing instance of {typeof(T)}: {_instance.name}");
                        }
                    }
                    
                    return _instance;
                }
            }
        }
        
        private void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }
    }
}
