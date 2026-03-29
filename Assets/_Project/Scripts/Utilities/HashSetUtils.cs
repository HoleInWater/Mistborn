using System.Collections.Generic;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for HashSet operations
    /// </summary>
    public static class HashSetUtils
    {
        /// <summary>
        /// Creates a new HashSet with initial capacity
        /// </summary>
        public static HashSet<T> Create<T>(int capacity = 0)
        {
            return new HashSet<T>(capacity);
        }

        /// <summary>
        /// Creates a new HashSet from a collection
        /// </summary>
        public static HashSet<T> Create<T>(IEnumerable<T> collection)
        {
            return new HashSet<T>(collection);
        }

        /// <summary>
        /// Adds multiple items to a HashSet
        /// </summary>
        public static void AddRange<T>(HashSet<T> hashSet, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                hashSet.Add(item);
            }
        }

        /// <summary>
        /// Removes multiple items from a HashSet
        /// </summary>
        public static void RemoveRange<T>(HashSet<T> hashSet, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                hashSet.Remove(item);
            }
        }

        /// <summary>
        /// Checks if a HashSet contains any of the items
        /// </summary>
        public static bool ContainsAny<T>(HashSet<T> hashSet, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                if (hashSet.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a HashSet contains all of the items
        /// </summary>
        public static bool ContainsAll<T>(HashSet<T> hashSet, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                if (!hashSet.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the union of two HashSets
        /// </summary>
        public static HashSet<T> Union<T>(HashSet<T> set1, HashSet<T> set2)
        {
            HashSet<T> result = Create(set1);
            result.UnionWith(set2);
            return result;
        }

        /// <summary>
        /// Returns the intersection of two HashSets
        /// </summary>
        public static HashSet<T> Intersection<T>(HashSet<T> set1, HashSet<T> set2)
        {
            HashSet<T> result = Create(set1);
            result.IntersectWith(set2);
            return result;
        }

        /// <summary>
        /// Returns the difference of two HashSets (set1 - set2)
        /// </summary>
        public static HashSet<T> Difference<T>(HashSet<T> set1, HashSet<T> set2)
        {
            HashSet<T> result = Create(set1);
            result.ExceptWith(set2);
            return result;
        }

        /// <summary>
        /// Returns the symmetric difference of two HashSets
        /// </summary>
        public static HashSet<T> SymmetricDifference<T>(HashSet<T> set1, HashSet<T> set2)
        {
            HashSet<T> result = Create(set1);
            result.SymmetricExceptWith(set2);
            return result;
        }

        /// <summary>
        /// Checks if two HashSets are equal (contain the same elements)
        /// </summary>
        public static bool AreEqual<T>(HashSet<T> set1, HashSet<T> set2)
        {
            return set1.SetEquals(set2);
        }

        /// <summary>
        /// Checks if a HashSet is a subset of another
        /// </summary>
        public static bool IsSubset<T>(HashSet<T> subset, HashSet<T> superset)
        {
            return subset.IsSubsetOf(superset);
        }

        /// <summary>
        /// Checks if a HashSet is a superset of another
        /// </summary>
        public static bool IsSuperset<T>(HashSet<T> superset, HashSet<T> subset)
        {
            return superset.IsSupersetOf(subset);
        }

        /// <summary>
        /// Checks if a HashSet is a proper subset of another
        /// </summary>
        public static bool IsProperSubset<T>(HashSet<T> subset, HashSet<T> superset)
        {
            return subset.IsProperSubsetOf(superset);
        }

        /// <summary>
        /// Checks if a HashSet is a proper superset of another
        /// </summary>
        public static bool IsProperSuperset<T>(HashSet<T> superset, HashSet<T> subset)
        {
            return superset.IsProperSupersetOf(subset);
        }

        /// <summary>
        /// Overlaps checks if two HashSets have any common elements
        /// </summary>
        public static bool Overlaps<T>(HashSet<T> set1, HashSet<T> set2)
        {
            return set1.Overlaps(set2);
        }

        /// <summary>
        /// Removes all elements that match a condition
        /// </summary>
        public static int RemoveWhere<T>(HashSet<T> hashSet, System.Predicate<T> predicate)
        {
            return hashSet.RemoveWhere(predicate);
        }

        /// <summary>
        /// Clears the HashSet
        /// </summary>
        public static void Clear<T>(HashSet<T> hashSet)
        {
            hashSet.Clear();
        }

        /// <summary>
        /// Gets the count of elements
        /// </summary>
        public static int GetCount<T>(HashSet<T> hashSet)
        {
            return hashSet.Count;
        }

        /// <summary>
        /// Checks if the HashSet is empty
        /// </summary>
        public static bool IsEmpty<T>(HashSet<T> hashSet)
        {
            return hashSet.Count == 0;
        }

        /// <summary>
        /// Converts the HashSet to a list
        /// </summary>
        public static List<T> ToList<T>(HashSet<T> hashSet)
        {
            return new List<T>(hashSet);
        }

        /// <summary>
        /// Converts the HashSet to an array
        /// </summary>
        public static T[] ToArray<T>(HashSet<T> hashSet)
        {
            T[] array = new T[hashSet.Count];
            hashSet.CopyTo(array);
            return array;
        }

        /// <summary>
        /// Gets a random element from the HashSet
        /// </summary>
        public static T GetRandomElement<T>(HashSet<T> hashSet)
        {
            if (hashSet.Count == 0) return default(T);
            
            int index = UnityEngine.Random.Range(0, hashSet.Count);
            using (var enumerator = hashSet.GetEnumerator())
            {
                for (int i = 0; i <= index; i++)
                {
                    enumerator.MoveNext();
                }
                return enumerator.Current;
            }
        }

        /// <summary>
        /// Returns an element or default if empty
        /// </summary>
        public static T FirstOrDefault<T>(HashSet<T> hashSet, T defaultValue = default(T))
        {
            using (var enumerator = hashSet.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }
            return defaultValue;
        }
    }
}
