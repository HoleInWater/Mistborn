using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for ScriptableObject operations
    /// </summary>
    public static class ScriptableObjectUtils
    {
        /// <summary>
        /// Creates a new instance of a ScriptableObject
        /// </summary>
        public static T CreateInstance<T>() where T : ScriptableObject
        {
            return ScriptableObject.CreateInstance<T>();
        }

        /// <summary>
        /// Creates a new instance of a ScriptableObject with a specific name
        /// </summary>
        public static T CreateInstance<T>(string name) where T : ScriptableObject
        {
            T instance = CreateInstance<T>();
            instance.name = name;
            return instance;
        }

        /// <summary>
        /// Duplicates a ScriptableObject instance
        /// </summary>
        public static T Duplicate<T>(T original) where T : ScriptableObject
        {
            if (original == null) return null;
            T duplicate = CreateInstance<T>();
            duplicate.name = original.name + " (Copy)";
            // Note: Unity's ScriptableObject duplication via JSON is a simple approach
            // For complex objects, consider implementing custom duplication logic
            string json = JsonUtility.ToJson(original);
            JsonUtility.FromJsonOverwrite(json, duplicate);
            return duplicate;
        }

        /// <summary>
        /// Saves a ScriptableObject to a JSON string
        /// </summary>
        public static string ToJson<T>(T obj, bool prettyPrint = false) where T : ScriptableObject
        {
            if (obj == null) return null;
            return JsonUtility.ToJson(obj, prettyPrint);
        }

        /// <summary>
        /// Loads a ScriptableObject from a JSON string
        /// </summary>
        public static T FromJson<T>(string json) where T : ScriptableObject
        {
            if (string.IsNullOrEmpty(json)) return null;
            T instance = CreateInstance<T>();
            JsonUtility.FromJsonOverwrite(json, instance);
            return instance;
        }

        /// <summary>
        /// Saves a ScriptableObject to a file
        /// </summary>
        public static bool SaveToFile<T>(T obj, string filePath) where T : ScriptableObject
        {
            if (obj == null || string.IsNullOrEmpty(filePath)) return false;
            
            string json = JsonUtility.ToJson(obj, true);
            try
            {
                System.IO.File.WriteAllText(filePath, json);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ScriptableObjectUtils: Failed to save to file {filePath}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads a ScriptableObject from a file
        /// </summary>
        public static T LoadFromFile<T>(string filePath) where T : ScriptableObject
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return null;
            
            try
            {
                string json = System.IO.File.ReadAllText(filePath);
                return FromJson<T>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ScriptableObjectUtils: Failed to load from file {filePath}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Resets a ScriptableObject to its default values
        /// </summary>
        public static void Reset<T>(T obj) where T : ScriptableObject
        {
            if (obj == null) return;
            string defaultJson = JsonUtility.ToJson(CreateInstance<T>());
            JsonUtility.FromJsonOverwrite(defaultJson, obj);
        }

        /// <summary>
        /// Checks if two ScriptableObjects are equal by content
        /// </summary>
        public static bool AreEqual<T>(T a, T b) where T : ScriptableObject
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return JsonUtility.ToJson(a) == JsonUtility.ToJson(b);
        }

        /// <summary>
        /// Gets the hash of a ScriptableObject's content
        /// </summary>
        public static int GetContentHash<T>(T obj) where T : ScriptableObject
        {
            if (obj == null) return 0;
            return JsonUtility.ToJson(obj).GetHashCode();
        }

        /// <summary>
        /// Loads all ScriptableObjects of a type from a Resources folder
        /// </summary>
        public static T[] LoadAllFromResources<T>(string path = "") where T : ScriptableObject
        {
            if (string.IsNullOrEmpty(path))
            {
                return Resources.LoadAll<T>("");
            }
            return Resources.LoadAll<T>(path);
        }

        /// <summary>
        /// Loads a ScriptableObject from Resources
        /// </summary>
        public static T LoadFromResources<T>(string path) where T : ScriptableObject
        {
            if (string.IsNullOrEmpty(path)) return null;
            return Resources.Load<T>(path);
        }
    }
}
