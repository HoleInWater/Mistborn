using System;
using System.Collections.Generic;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Extension methods for arrays
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Returns a random element from an array
        /// </summary>
        public static T RandomElement<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
            {
                Debug.LogError("ArrayExtensions: Array is null or empty");
                return default(T);
            }
            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Shuffles an array in place using Fisher-Yates algorithm
        /// </summary>
        public static void Shuffle<T>(this T[] array)
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
        public static T[] Reversed<T>(this T[] array)
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
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        /// <summary>
        /// Finds the index of an element in an array
        /// </summary>
        public static int IndexOf<T>(this T[] array, T element) where T : IEquatable<T>
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
        public static bool Contains<T>(this T[] array, T element) where T : IEquatable<T>
        {
            return IndexOf(array, element) >= 0;
        }

        /// <summary>
        /// Concatenates two arrays
        /// </summary>
        public static T[] Concatenate<T>(this T[] first, T[] second)
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
        public static T[] Slice<T>(this T[] array, int start, int end)
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
        public static T[] RemoveAt<T>(this T[] array, int index)
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
        public static T[] InsertAt<T>(this T[] array, int index, T element)
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

        /// <summary>
        /// Converts an array to a list
        /// </summary>
        public static List<T> ToList<T>(this T[] array)
        {
            if (array == null) return new List<T>();
            return new List<T>(array);
        }

        /// <summary>
        /// Performs an action on each element of the array
        /// </summary>
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            if (array == null || action == null) return;
            
            foreach (T item in array)
            {
                action(item);
            }
        }

        /// <summary>
        /// Returns the first N elements of an array
        /// </summary>
        public static T[] Take<T>(this T[] array, int count)
        {
            if (array == null) return new T[0];
            if (count >= array.Length) return (T[])array.Clone();
            return Slice(array, 0, count);
        }

        /// <summary>
        /// Returns the last N elements of an array
        /// </summary>
        public static T[] TakeLast<T>(this T[] array, int count)
        {
            if (array == null) return new T[0];
            if (count >= array.Length) return (T[])array.Clone();
            return Slice(array, array.Length - count, array.Length);
        }

        /// <summary>
        /// Returns elements from an array starting at index
        /// </summary>
        public static T[] Skip<T>(this T[] array, int count)
        {
            if (array == null) return new T[0];
            if (count >= array.Length) return new T[0];
            return Slice(array, count, array.Length);
        }

        /// <summary>
        /// Swaps two elements in an array
        /// </summary>
        public static void Swap<T>(this T[] array, int index1, int index2)
        {
            if (array == null || index1 < 0 || index1 >= array.Length || index2 < 0 || index2 >= array.Length) return;
            
            T temp = array[index1];
            array[index1] = array[index2];
            array[index2] = temp;
        }

        /// <summary>
        /// Rotates the array left by count positions
        /// </summary>
        public static void RotateLeft<T>(this T[] array, int count = 1)
        {
            if (array == null || array.Length == 0) return;
            count %= array.Length;
            if (count == 0) return;
            
            T[] range = Take(array, count);
            for (int i = 0; i < array.Length - count; i++)
            {
                array[i] = array[i + count];
            }
            for (int i = 0; i < count; i++)
            {
                array[array.Length - count + i] = range[i];
            }
        }

        /// <summary>
        /// Rotates the array right by count positions
        /// </summary>
        public static void RotateRight<T>(this T[] array, int count = 1)
        {
            if (array == null || array.Length == 0) return;
            count %= array.Length;
            if (count == 0) return;
            
            T[] range = TakeLast(array, count);
            for (int i = array.Length - 1; i >= count; i--)
            {
                array[i] = array[i - count];
            }
            for (int i = 0; i < count; i++)
            {
                array[i] = range[i];
            }
        }

        /// <summary>
        /// Returns the index of the maximum element using a selector
        /// </summary>
        public static int IndexOfMax<T, U>(this T[] array, Func<T, U> selector) where U : IComparable<U>
        {
            if (array == null || array.Length == 0) return -1;
            
            int maxIndex = 0;
            U maxValue = selector(array[0]);
            
            for (int i = 1; i < array.Length; i++)
            {
                U value = selector(array[i]);
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
        public static int IndexOfMin<T, U>(this T[] array, Func<T, U> selector) where U : IComparable<U>
        {
            if (array == null || array.Length == 0) return -1;
            
            int minIndex = 0;
            U minValue = selector(array[0]);
            
            for (int i = 1; i < array.Length; i++)
            {
                U value = selector(array[i]);
                if (value.CompareTo(minValue) < 0)
                {
                    minValue = value;
                    minIndex = i;
                }
            }
            return minIndex;
        }
    }
}
