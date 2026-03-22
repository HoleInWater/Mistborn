using System;
using System.Collections.Generic;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Extension methods for List<T> class
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Returns a random element from a list
        /// </summary>
        public static T RandomElement<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                Debug.LogError("ListExtensions: List is null or empty");
                return default(T);
            }
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Shuffles a list in place using Fisher-Yates algorithm
        /// </summary>
        public static void Shuffle<T>(this List<T> list)
        {
            if (list == null) return;
            
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        /// <summary>
        /// Returns a new list with elements reversed
        /// </summary>
        public static List<T> Reversed<T>(this List<T> list)
        {
            if (list == null) return new List<T>();
            
            List<T> reversed = new List<T>(list);
            reversed.Reverse();
            return reversed;
        }

        /// <summary>
        /// Checks if a list is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// Adds an item to a list if it's not already present
        /// </summary>
        public static bool AddIfNotContains<T>(this List<T> list, T item)
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
        public static void RemoveNulls<T>(this List<T> list) where T : class
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
        /// Removes all duplicate elements from a list (preserves order)
        /// </summary>
        public static void RemoveDuplicates<T>(this List<T> list)
        {
            if (list == null) return;
            
            HashSet<T> seen = new HashSet<T>();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (!seen.Add(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Performs an action on each element of the list
        /// </summary>
        public static void ForEach<T>(this List<T> list, Action<T> action)
        {
            if (list == null || action == null) return;
            
            foreach (T item in list)
            {
                action(item);
            }
        }

        /// <summary>
        /// Converts a list to an array
        /// </summary>
        public static T[] ToArray<T>(this List<T> list)
        {
            if (list == null) return new T[0];
            return list.ToArray();
        }

        /// <summary>
        /// Returns the first N elements of a list
        /// </summary>
        public static List<T> Take<T>(this List<T> list, int count)
        {
            if (list == null) return new List<T>();
            if (count >= list.Count) return new List<T>(list);
            return list.GetRange(0, count);
        }

        /// <summary>
        /// Returns the last N elements of a list
        /// </summary>
        public static List<T> TakeLast<T>(this List<T> list, int count)
        {
            if (list == null) return new List<T>();
            if (count >= list.Count) return new List<T>(list);
            return list.GetRange(list.Count - count, count);
        }

        /// <summary>
        /// Returns elements from a list starting at index
        /// </summary>
        public static List<T> Skip<T>(this List<T> list, int count)
        {
            if (list == null) return new List<T>();
            if (count >= list.Count) return new List<T>();
            return list.GetRange(count, list.Count - count);
        }

        /// <summary>
        /// Returns the index of the maximum element using a selector
        /// </summary>
        public static int IndexOfMax<T, U>(this List<T> list, Func<T, U> selector) where U : IComparable<U>
        {
            if (list == null || list.Count == 0) return -1;
            
            int maxIndex = 0;
            U maxValue = selector(list[0]);
            
            for (int i = 1; i < list.Count; i++)
            {
                U value = selector(list[i]);
                if (value.CompareTo(maxValue) > 0)
                {
                    maxValue = value;
                    maxIndex = i;
                }
            }
            return maxIndex;
        }

        /// <summary>
        /// Returns the index of the minimum element using a selector
        /// </summary>
        public static int IndexOfMin<T, U>(this List<T> list, Func<T, U> selector) where U : IComparable<U>
        {
            if (list == null || list.Count == 0) return -1;
            
            int minIndex = 0;
            U minValue = selector(list[0]);
            
            for (int i = 1; i < list.Count; i++)
            {
                U value = selector(list[i]);
                if (value.CompareTo(minValue) < 0)
                {
                    minValue = value;
                    minIndex = i;
                }
            }
            return minIndex;
        }

        /// <summary>
        /// Swaps two elements in a list
        /// </summary>
        public static void Swap<T>(this List<T> list, int index1, int index2)
        {
            if (list == null || index1 < 0 || index1 >= list.Count || index2 < 0 || index2 >= list.Count) return;
            
            T temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        /// <summary>
        /// Rotates the list left by count positions
        /// </summary>
        public static void RotateLeft<T>(this List<T> list, int count = 1)
        {
            if (list == null || list.Count == 0) return;
            count %= list.Count;
            if (count == 0) return;
            
            List<T> range = list.GetRange(0, count);
            list.RemoveRange(0, count);
            list.AddRange(range);
        }

        /// <summary>
        /// Rotates the list right by count positions
        /// </summary>
        public static void RotateRight<T>(this List<T> list, int count = 1)
        {
            if (list == null || list.Count == 0) return;
            count %= list.Count;
            if (count == 0) return;
            
            List<T> range = list.GetRange(list.Count - count, count);
            list.RemoveRange(list.Count - count, count);
            list.InsertRange(0, range);
        }

        /// <summary>
        /// Converts a list to a read-only list
        /// </summary>
        public static IReadOnlyList<T> AsReadOnly<T>(this List<T> list)
        {
            return list?.AsReadOnly();
        }
    }
}
