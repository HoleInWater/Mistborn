using System.Collections.Generic;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for dictionary operations
    /// </summary>
    public static class DictionaryUtils
    {
        /// <summary>
        /// Gets a value from a dictionary, returning a default if key doesn't exist
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            if (dictionary == null || !dictionary.ContainsKey(key))
            {
                return defaultValue;
            }
            return dictionary[key];
        }

        /// <summary>
        /// Tries to get a value from a dictionary, returning false if key doesn't exist
        /// </summary>
        public static bool TryGetValue<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, out TValue value)
        {
            if (dictionary == null)
            {
                value = default(TValue);
                return false;
            }
            return dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Adds a key-value pair to a dictionary, overwriting if key already exists
        /// </summary>
        public static void AddOrReplace<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null) return;
            
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        /// <summary>
        /// Removes a key from a dictionary, returning true if key existed
        /// </summary>
        public static bool Remove<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary == null) return false;
            return dictionary.Remove(key);
        }

        /// <summary>
        /// Checks if a dictionary contains a specific key
        /// </summary>
        public static bool ContainsKey<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary == null) return false;
            return dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Checks if a dictionary contains a specific value
        /// </summary>
        public static bool ContainsValue<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TValue value)
        {
            if (dictionary == null) return false;
            return dictionary.ContainsValue(value);
        }

        /// <summary>
        /// Gets all keys from a dictionary
        /// </summary>
        public static List<TKey> GetKeys<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) return new List<TKey>();
            return new List<TKey>(dictionary.Keys);
        }

        /// <summary>
        /// Gets all values from a dictionary
        /// </summary>
        public static List<TValue> GetValues<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) return new List<TValue>();
            return new List<TValue>(dictionary.Values);
        }

        /// <summary>
        /// Clears a dictionary
        /// </summary>
        public static void Clear<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary != null)
            {
                dictionary.Clear();
            }
        }

        /// <summary>
        /// Gets the count of items in a dictionary
        /// </summary>
        public static int GetCount<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) return 0;
            return dictionary.Count;
        }

        /// <summary>
        /// Merges two dictionaries, with the second dictionary overriding duplicates
        /// </summary>
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(Dictionary<TKey, TValue> first, Dictionary<TKey, TValue> second)
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            
            if (first != null)
            {
                foreach (var kvp in first)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            
            if (second != null)
            {
                foreach (var kvp in second)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Converts a dictionary to a list of key-value pairs
        /// </summary>
        public static List<KeyValuePair<TKey, TValue>> ToList<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) return new List<KeyValuePair<TKey, TValue>>();
            return new List<KeyValuePair<TKey, TValue>>(dictionary);
        }

        /// <summary>
        /// Creates a dictionary from a list of key-value pairs
        /// </summary>
        public static Dictionary<TKey, TValue> FromList<TKey, TValue>(List<KeyValuePair<TKey, TValue>> list)
        {
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
            if (list == null) return dictionary;
            
            foreach (var kvp in list)
            {
                dictionary[kvp.Key] = kvp.Value;
            }
            return dictionary;
        }
    }
}
