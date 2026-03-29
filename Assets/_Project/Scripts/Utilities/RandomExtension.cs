using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for handling common random operations
    /// </summary>
    public static class RandomExtension
    {
        /// <summary>
        /// Returns a random element from an array
        /// </summary>
        public static T RandomElement<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
                return default(T);
                
            return array[Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Returns a random element from a list
        /// </summary>
        public static T RandomElement<T>(this System.Collections.Generic.List<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);
                
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Returns a random boolean with specified probability (0-1)
        /// </summary>
        public static bool RandomBool(this float probability)
        {
            return Random.value < Mathf.Clamp01(probability);
        }

        /// <summary>
        /// Shuffles an array in place using Fisher-Yates algorithm
        /// </summary>
        public static void Shuffle<T>(this T[] array)
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
        public static void Shuffle<T>(this System.Collections.Generic.List<T> list)
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
        /// Returns a random weight based on a weight distribution
        /// </summary>
        public static T WeightedRandom<T>(this System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<T, float>> weightedItems)
        {
            if (weightedItems == null || weightedItems.Count == 0)
                return default(T);
                
            float totalWeight = 0f;
            foreach (var item in weightedItems)
            {
                totalWeight += item.Value;
            }
            
            if (totalWeight <= 0f)
                return weightedItems[0].Key;
                
            float randomPoint = Random.value * totalWeight;
            float current = 0f;
            
            foreach (var item in weightedItems)
            {
                current += item.Value;
                if (randomPoint <= current)
                    return item.Key;
            }
            
            return weightedItems[^1].Key; // fallback to last item
        }

        /// <summary>
        /// Returns a random position inside a circle (2D)
        /// </summary>
        public static Vector2 RandomPointInCircle(this float radius)
        {
            return Random.insideUnitCircle * radius;
        }

        /// <summary>
        /// Returns a random position on the circumference of a circle (2D)
        /// </summary>
        public static Vector2 RandomPointOnCircleCircumference(this float radius)
        {
            return Random.insideUnitCircle.normalized * radius;
        }

        /// <summary>
        /// Returns a random position inside a sphere (3D)
        /// </summary>
        public static Vector3 RandomPointInSphere(this float radius)
        {
            return Random.insideUnitSphere * radius;
        }

        /// <summary>
        /// Returns a random position on the surface of a sphere (3D)
        /// </summary>
        public static Vector3 RandomPointOnSphereSurface(this float radius)
        {
            return Random.onUnitSphere * radius;
        }
    }
}