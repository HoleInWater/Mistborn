using UnityEngine;

namespace AshwalkerGame.Utilities
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
                        }
                        else
                        {
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
