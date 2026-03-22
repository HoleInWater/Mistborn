using System;
using System.Collections.Generic;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for array operations
    /// </summary>
    public static class ArrayUtils
    {
        /// <summary>
        /// Gets a random element from an array
        /// </summary>
        public static T GetRandomElement<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                Debug.LogError("ArrayUtils: Array is null or empty");
                return default(T);
            }
            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Shuffles an array using Fisher-Yates algorithm
        /// </summary>
        public static void Shuffle<T>(T[] array)
        {
            if (array == null) return;
            
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }

        /// <summary>
        /// Returns a new array with elements reversed
        /// </summary>
        public static T[] Reverse<T>(T[] array)
        {
            if (array == null) return new T[0];
            
            T[] reversed = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                reversed[i] = array[array.Length - 1 - i];
            }
            return reversed;
        }

        /// <summary>
        /// Checks if an array is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(T[] array)
        {
            return array == null || array.Length == 0;
        }

        /// <summary>
        /// Finds the index of an element in an array
        /// </summary>
        public static int IndexOf<T>(T[] array, T element) where T : IEquatable<T>
        {
            if (array == null) return -1;
            
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(element))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Checks if an array contains an element
        /// </summary>
        public static bool Contains<T>(T[] array, T element) where T : IEquatable<T>
        {
            return IndexOf(array, element) >= 0;
        }

        /// <summary>
        /// Concatenates two arrays
        /// </summary>
        public static T[] Concatenate<T>(T[] first, T[] second)
        {
            if (first == null) first = new T[0];
            if (second == null) second = new T[0];
            
            T[] result = new T[first.Length + second.Length];
            Array.Copy(first, result, first.Length);
            Array.Copy(second, 0, result, first.Length, second.Length);
            return result;
        }

        /// <summary>
        /// Slices an array from start index to end index
        /// </summary>
        public static T[] Slice<T>(T[] array, int start, int end)
        {
            if (array == null) return new T[0];
            
            start = Mathf.Max(0, start);
            end = Mathf.Min(array.Length, end);
            
            if (start >= end) return new T[0];
            
            int length = end - start;
            T[] slice = new T[length];
            Array.Copy(array, start, slice, 0, length);
            return slice;
        }

        /// <summary>
        /// Removes an element at a specific index
        /// </summary>
        public static T[] RemoveAt<T>(T[] array, int index)
        {
            if (array == null || index < 0 || index >= array.Length) return array;
            
            T[] result = new T[array.Length - 1];
            for (int i = 0, j = 0; i < array.Length; i++)
            {
                if (i == index) continue;
                result[j++] = array[i];
            }
            return result;
        }

        /// <summary>
        /// Inserts an element at a specific index
        /// </summary>
        public static T[] InsertAt<T>(T[] array, int index, T element)
        {
            if (array == null) return new T[] { element };
            if (index < 0) index = 0;
            if (index > array.Length) index = array.Length;
            
            T[] result = new T[array.Length + 1];
            for (int i = 0, j = 0; i < result.Length; i++)
            {
                if (i == index)
                {
                    result[i] = element;
                }
                else
                {
                    result[i] = array[j++];
                }
            }
            return result;
        }
    }
}
