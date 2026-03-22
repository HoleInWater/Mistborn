using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for handling common serialization operations
    /// </summary>
    public class SerializationUtils : MonoBehaviour
    {
        /// <summary>
        /// Serializes an object to JSON string
        /// </summary>
        public static string ToJson(object obj)
        {
            if (obj == null) return "null";
            return JsonUtility.ToJson(obj);
        }

        /// <summary>
        /// Deserializes a JSON string to an object
        /// </summary>
        public static T FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(T);
                
            return JsonUtility.FromJson<T>(json);
        }

        /// <summary>
        /// Serializes an object to JSON and writes it to a file
        /// </summary>
        public static void SaveJson<T>(string filename, T obj)
        {
            if (string.IsNullOrEmpty(filename))
            {
                Debug.LogError("SerializationUtils: Filename cannot be null or empty");
                return;
            }
            
            string json = ToJson(obj);
            System.IO.File.WriteAllText(filename, json);
        }

        /// <summary>
        /// Deserializes an object from a JSON file
        /// </summary>
        public static T LoadJson<T>(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                Debug.LogError("SerializationUtils: Filename cannot be null or empty");
                return default(T);
            }
            
            if (!System.IO.File.Exists(filename))
            {
                Debug.LogError($"SerializationUtils: File not found: {filename}");
                return default(T);
            }
            
            string json = System.IO.File.ReadAllText(filename);
            return FromJson<T>(json);
        }

        /// <summary>
        /// Serializes an object to JSON and saves it to PlayerPrefs
        /// </summary>
        public static void SaveJsonToPlayerPrefs(string key, object obj)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("SerializationUtils: Key cannot be null or empty");
                return;
            }
            
            string json = ToJson(obj);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Deserializes an object from PlayerPrefs JSON
        /// </summary>
        public static T LoadJsonFromPlayerPrefs<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("SerializationUtils: Key cannot be null or empty");
                return default(T);
            }
            
            string json = PlayerPrefs.GetString(key);
            return FromJson<T>(json);
        }

        /// <summary>
        /// Serializes a dictionary to JSON string
        /// </summary>
        public static string DictionaryToJson<TKey, TValue>(System.Collections.Generic.IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                return "null";
                
            return JsonUtility.ToJson(dictionary);
        }

        /// </// <summary>
        /// Deserializes a JSON string to a dictionary
        /// </summary>
        public static System.Collections.Generic.Dictionary<TKey, TValue> JsonToDictionary<TKey, TValue>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;
                
            return JsonUtility.FromJson<System.Collections.Generic.Dictionary<TKey, TValue>>(json);
        }

        /// <summary>
        /// Serializes a list to JSON string
        /// </summary>
        public static string ListToJson<T>(System.Collections.Generic.IList<T> list)
        {
            if (list == null)
                return "null";
                
            return JsonUtility.ToJson(list);
        }

        /// <summary>
        /// Deserializes a JSON string to a list
        /// </summary>
        public static System.Collections.Generic.List<T> JsonToList<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;
                
            return JsonUtility.FromJson<System.Collections.Generic.List<T>>(json);
        }

        /// <summary>
        /// Creates a serializable wrapper for dictionaries (since Unity's JsonUtility doesn't support dictionaries directly)
        /// </summary>
        [System.Serializable]
        public class SerializableDictionary<TKey, TValue>
        {
            public System.Collections.Generic.List<TKey> keys = new System.Collections.Generic.List<TKey>();
            public System.Collections.Generic.List<TValue> values = new System.Collections.Generic.List<TValue>();
            
            public System.Collections.Generic.Dictionary<TKey, TValue> ToDictionary()
            {
                var dict = new System.Collections.Generic.Dictionary<TKey, TValue>();
                for (int i = 0; i < keys.Count && i < values.Count; i++)
                {
                    dict[keys[i]] = values[i];
                }
                return dict;
            }
            
            public void FromDictionary(System.Collections.Generic.Dictionary<TKey, TValue> dictionary)
            {
                keys.Clear();
                values.Clear();
                
                if (dictionary != null)
                {
                    foreach (var kvp in dictionary)
            {
                        keys.Add(kvp.Key);
                        values.Add(kvp.Value);
                    }
                }
            }
        }
    }
}