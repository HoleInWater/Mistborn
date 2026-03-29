using UnityEngine;

namespace MistbornGame.Utilities
{
    public static class RandomUtils
    {
        /// <summary>
        /// Returns a random element from an array
        /// </summary>
        public static T RandomElement<T>(T[] array)
        {
            if (array == null || array.Length == 0)
                return default(T);
                
            return array[Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Returns a random element from a list
        /// </summary>
        public static T RandomElement<T>(System.Collections.Generic.List<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);
                
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Returns a random boolean with specified probability (0-1)
        /// </summary>
        public static bool RandomBool(float probability)
        {
            return Random.value < Mathf.Clamp01(probability);
        }

        /// <summary>
        /// Returns a random integer in the specified range [min, max]
        /// </summary>
        public static int RandomRange(int min, int max)
        {
            return Random.Range(min, max + 1);
        }

        /// <summary>
        /// Returns a random float in the specified range [min, max]
        /// </summary>
        public static float RandomRange(float min, float max)
        {
            return Random.Range(min, max);
        }

        /// <summary>
        /// Shuffles an array in place using Fisher-Yates algorithm
        /// </summary>
        public static void Shuffle<T>(T[] array)
        {
            if (array == null || array.Length <= 1)
                return;
                
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }

        /// <summary>
        /// Shuffles a list in place using Fisher-Yates algorithm
        /// </summary>
        public static void Shuffle<T>(System.Collections.Generic.List<T> list)
        {
            if (list == null || list.Count <= 1)
                return;
                
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        /// <summary>
        /// Returns a random point inside a circle (2D)
        /// </summary>
        public static Vector2 RandomPointInCircle(float radius)
        {
            return Random.insideUnitCircle * radius;
        }

        /// <summary>
        /// Returns a random point on the circumference of a circle (2D)
        /// </summary>
        public static Vector2 RandomPointOnCircleCircumference(float radius)
        {
            return Random.insideUnitCircle.normalized * radius;
        }
    }
}