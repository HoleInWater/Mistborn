using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for probability and random chance operations
    /// </summary>
    public static class ProbabilityUtils
    {
        /// <summary>
        /// Returns true with the given probability (0-1)
        /// </summary>
        public static bool Chance(float probability)
        {
            return Random.value <= Mathf.Clamp01(probability);
        }

        /// <summary>
        /// Returns true with the given percentage chance (0-100)
        /// </summary>
        public static bool ChancePercent(float percent)
        {
            return Chance(percent / 100f);
        }

        /// <summary>
        /// Rolls a dice with given sides (returns 1 to sides)
        /// </summary>
        public static int RollDice(int sides = 6)
        {
            if (sides <= 0) return 0;
            return Random.Range(1, sides + 1);
        }

        /// <summary>
        /// Returns a random boolean
        /// </summary>
        public static bool RandomBool()
        {
            return Random.value > 0.5f;
        }

        /// <summary>
        /// Returns a random sign (-1 or 1)
        /// </summary>
        public static int RandomSign()
        {
            return Random.value > 0.5f ? 1 : -1;
        }

        /// <summary>
        /// Returns a random float between min and max
        /// </summary>
        public static float RandomRange(float min, float max)
        {
            return Random.Range(min, max);
        }

        /// <summary>
        /// Returns a random integer between min (inclusive) and max (exclusive)
        /// </summary>
        public static int RandomRange(int min, int max)
        {
            return Random.Range(min, max);
        }

        /// <summary>
        /// Returns a random element from an array with weighted probabilities
        /// </summary>
        public static T WeightedRandom<T>(T[] items, float[] weights)
        {
            if (items == null || weights == null || items.Length != weights.Length || items.Length == 0)
            {
                Debug.LogError("ProbabilityUtils: Arrays are null or mismatched");
                return default(T);
            }

            float totalWeight = 0f;
            foreach (float weight in weights)
            {
                if (weight < 0)
                {
                    Debug.LogError("ProbabilityUtils: Negative weight not allowed");
                    return default(T);
                }
                totalWeight += weight;
            }

            if (totalWeight <= 0f)
            {
                Debug.LogError("ProbabilityUtils: Total weight must be positive");
                return default(T);
            }

            float randomValue = Random.Range(0f, totalWeight);
            float accumulated = 0f;
            for (int i = 0; i < items.Length; i++)
            {
                accumulated += weights[i];
                if (randomValue <= accumulated)
                {
                    return items[i];
                }
            }

            return items[items.Length - 1];
        }

        /// <summary>
        /// Returns a random element from an array with exponential weighting
        /// </summary>
        public static T ExponentialWeightedRandom<T>(T[] items, float exponent = 2f)
        {
            if (items == null || items.Length == 0)
            {
                Debug.LogError("ProbabilityUtils: Array is null or empty");
                return default(T);
            }

            float[] weights = new float[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                weights[i] = Mathf.Pow(i + 1, exponent);
            }

            return WeightedRandom(items, weights);
        }

        /// <summary>
        /// Shuffles an array using Fisher-Yates algorithm
        /// </summary>
        public static void Shuffle<T>(T[] array)
        {
            if (array == null) return;
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }

        /// <summary>
        /// Returns a random color with full alpha
        /// </summary>
        public static Color RandomColor()
        {
            return new Color(Random.value, Random.value, Random.value, 1f);
        }

        /// <summary>
        /// Returns a random color with given alpha
        /// </summary>
        public static Color RandomColor(float alpha)
        {
            return new Color(Random.value, Random.value, Random.value, alpha);
        }

        /// <summary>
        /// Returns a random point on a circle
        /// </summary>
        public static Vector2 RandomPointOnCircle(float radius)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        }

        /// <summary>
        /// Returns a random point inside a circle
        /// </summary>
        public static Vector2 RandomPointInsideCircle(float maxRadius)
        {
            float radius = Mathf.Sqrt(Random.Range(0f, 1f)) * maxRadius;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        }

        /// <summary>
        /// Returns a random point in a rectangle
        /// </summary>
        public static Vector2 RandomPointInRectangle(float width, float height)
        {
            return new Vector2(Random.Range(0f, width), Random.Range(0f, height));
        }

        /// <summary>
        /// Returns a random point in a box
        /// </summary>
        public static Vector3 RandomPointInBox(Vector3 size)
        {
            return new Vector3(
                Random.Range(-size.x / 2f, size.x / 2f),
                Random.Range(-size.y / 2f, size.y / 2f),
                Random.Range(-size.z / 2f, size.z / 2f)
            );
        }
    }
}
