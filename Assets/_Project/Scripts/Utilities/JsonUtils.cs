using UnityEngine;
using System.IO;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for JSON serialization and deserialization
    /// </summary>
    public static class JsonUtils
    {
        /// <summary>
        /// Serializes an object to JSON string
        /// </summary>
        public static string Serialize<T>(T obj, bool prettyPrint = false)
        {
            try
            {
                return JsonUtility.ToJson(obj, prettyPrint);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JsonUtils: Failed to serialize {typeof(T).Name}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deserializes a JSON string to an object
        /// </summary>
        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("JsonUtils: JSON string is null or empty");
                return default(T);
            }
            
            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JsonUtils: Failed to deserialize to {typeof(T).Name}: {e.Message}");
                return default(T);
            }
        }

        /// <summary>
        /// Saves an object to a JSON file
        /// </summary>
        public static bool SaveToFile<T>(T obj, string filePath, bool prettyPrint = false)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("JsonUtils: File path is null or empty");
                return false;
            }
            
            string json = Serialize(obj, prettyPrint);
            if (json == null) return false;
            
            try
            {
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JsonUtils: Failed to write to file {filePath}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads an object from a JSON file
        /// </summary>
        public static T LoadFromFile<T>(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("JsonUtils: File path is null or empty");
                return default(T);
            }
            
            if (!File.Exists(filePath))
            {
                Debug.LogError($"JsonUtils: File not found: {filePath}");
                return default(T);
            }
            
            try
            {
                string json = File.ReadAllText(filePath);
                return Deserialize<T>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JsonUtils: Failed to read from file {filePath}: {e.Message}");
                return default(T);
            }
        }

        /// <summary>
        /// Saves an object to a JSON file in the persistent data path
        /// </summary>
        public static bool SaveToPersistentDataPath<T>(T obj, string fileName, bool prettyPrint = false)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            return SaveToFile(obj, filePath, prettyPrint);
        }

        /// <summary>
        /// Loads an object from a JSON file in the persistent data path
        /// </summary>
        public static T LoadFromPersistentDataPath<T>(string fileName)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            return LoadFromFile<T>(filePath);
        }
    }
}
