using System.Collections.Generic;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for common collection operations
    /// </summary>
    public static class CollectionUtils
    {
        /// <summary>
        /// Gets a random element from a list
        /// </summary>
        public static T GetRandomElement<T>(IList<T> list)
        {
            if (list == null || list.Count == 0)
            {
                Debug.LogError("CollectionUtils: List is null or empty");
                return default(T);
            }
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Shuffles a list using Fisher-Yates algorithm
        /// </summary>
        public static void Shuffle<T>(IList<T> list)
        {
            if (list == null) return;
            
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        /// <summary>
        /// Returns a new list with elements reversed
        /// </summary>
        public static List<T> Reverse<T>(IList<T> list)
        {
            if (list == null) return new List<T>();
            
            List<T> reversed = new List<T>(list);
            reversed.Reverse();
            return reversed;
        }

        /// <summary>
        /// Checks if a collection is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        /// <summary>
        /// Adds an item to a list if it's not already present
        /// </summary>
        public static bool AddIfNotContains<T>(IList<T> list, T item)
        {
            if (list == null) return false;
            
            if (!list.Contains(item))
            {
                list.Add(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes all null elements from a list
        /// </summary>
        public static void RemoveNulls<T>(IList<T> list) where T : class
        {
            if (list == null) return;
            
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null)
                {
                    list.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Converts an array to a list
        /// </summary>
        public static List<T> ToList<T>(T[] array)
        {
            if (array == null) return new List<T>();
            return new List<T>(array);
        }

        /// <summary>
        /// Converts a list to an array
        /// </summary>
        public static T[] ToArray<T>(IList<T> list)
        {
            if (list == null) return new T[0];
            
            T[] array = new T[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                array[i] = list[i];
            }
            return array;
        }
    }
}
