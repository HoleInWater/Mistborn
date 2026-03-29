using System.Collections.Generic;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Extension methods for Dictionary class
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets a value from a dictionary, returning a default if key doesn't exist
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
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
        public static bool TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out TValue value)
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
        public static void AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
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
        public static bool Remove<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary == null) return false;
            return dictionary.Remove(key);
        }

        /// <summary>
        /// Checks if a dictionary contains a specific key
        /// </summary>
        public static bool ContainsKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary == null) return false;
            return dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Checks if a dictionary contains a specific value
        /// </summary>
        public static bool ContainsValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
        {
            if (dictionary == null) return false;
            return dictionary.ContainsValue(value);
        }

        /// <summary>
        /// Gets all keys from a dictionary
        /// </summary>
        public static List<TKey> GetKeys<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) return new List<TKey>();
            return new List<TKey>(dictionary.Keys);
        }

        /// <summary>
        /// Gets all values from a dictionary
        /// </summary>
        public static List<TValue> GetValues<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) return new List<TValue>();
            return new List<TValue>(dictionary.Values);
        }

        /// <summary>
        /// Clears a dictionary
        /// </summary>
        public static void Clear<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary != null)
            {
                dictionary.Clear();
            }
        }

        /// <summary>
        /// Gets the count of items in a dictionary
        /// </summary>
        public static int GetCount<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) return 0;
            return dictionary.Count;
        }

        /// <summary>
        /// Merges another dictionary into this one, with the other dictionary overriding duplicates
        /// </summary>
        public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> other)
        {
            if (dictionary == null || other == null) return;
            
            foreach (var kvp in other)
            {
                dictionary[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// Converts a dictionary to a list of key-value pairs
        /// </summary>
        public static List<KeyValuePair<TKey, TValue>> ToList<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) return new List<KeyValuePair<TKey, TValue>>();
            return new List<KeyValuePair<TKey, TValue>>(dictionary);
        }

        /// <summary>
        /// Creates a new dictionary with the same keys but transformed values
        /// </summary>
        public static Dictionary<TKey, TResult> Select<TKey, TValue, TResult>(this Dictionary<TKey, TValue> dictionary, System.Func<TValue, TResult> selector)
        {
            if (dictionary == null || selector == null) return new Dictionary<TKey, TResult>();
            
            Dictionary<TKey, TResult> result = new Dictionary<TKey, TResult>();
            foreach (var kvp in dictionary)
            {
                result[kvp.Key] = selector(kvp.Value);
            }
            return result;
        }

        /// <summary>
        /// Filters a dictionary based on a predicate
        /// </summary>
        public static Dictionary<TKey, TValue> Where<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, System.Func<TKey, TValue, bool> predicate)
        {
            if (dictionary == null || predicate == null) return new Dictionary<TKey, TValue>();
            
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            foreach (var kvp in dictionary)
            {
                if (predicate(kvp.Key, kvp.Value))
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the first key that matches a value
        /// </summary>
        public static TKey FindKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
        {
            if (dictionary == null) return default(TKey);
            
            foreach (var kvp in dictionary)
            {
                if (EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                {
                    return kvp.Key;
                }
            }
            return default(TKey);
        }

        /// <summary>
        /// Checks if a dictionary is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return dictionary == null || dictionary.Count == 0;
        }
    }
}
