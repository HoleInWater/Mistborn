using System.Collections.Generic;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for Queue operations
    /// </summary>
    public static class QueueUtils
    {
        /// <summary>
        /// Creates a new Queue with initial capacity
        /// </summary>
        public static Queue<T> Create<T>(int capacity = 0)
        {
            return new Queue<T>(capacity);
        }

        /// <summary>
        /// Creates a new Queue from a collection
        /// </summary>
        public static Queue<T> Create<T>(IEnumerable<T> collection)
        {
            return new Queue<T>(collection);
        }

        /// <summary>
        /// Enqueues multiple items to a Queue
        /// </summary>
        public static void EnqueueRange<T>(Queue<T> queue, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                queue.Enqueue(item);
            }
        }

        /// <summary>
        /// Dequeues multiple items from a Queue
        /// </summary>
        public static List<T> DequeueRange<T>(Queue<T> queue, int count)
        {
            List<T> items = new List<T>();
            for (int i = 0; i < count && queue.Count > 0; i++)
            {
                items.Add(queue.Dequeue());
            }
            return items;
        }

        /// <summary>
        /// Peeks at the first item without removing it
        /// </summary>
        public static T PeekOrDefault<T>(Queue<T> queue, T defaultValue = default(T))
        {
            if (queue.Count == 0) return defaultValue;
            return queue.Peek();
        }

        /// <summary>
        /// Dequeues the first item or returns a default value
        /// </summary>
        public static T DequeueOrDefault<T>(Queue<T> queue, T defaultValue = default(T))
        {
            if (queue.Count == 0) return defaultValue;
            return queue.Dequeue();
        }

        /// <summary>
        /// Checks if a Queue contains an item
        /// </summary>
        public static bool Contains<T>(Queue<T> queue, T item)
        {
            return queue.Contains(item);
        }

        /// <summary>
        /// Clears the Queue
        /// </summary>
        public static void Clear<T>(Queue<T> queue)
        {
            queue.Clear();
        }

        /// <summary>
        /// Gets the count of items in the Queue
        /// </summary>
        public static int GetCount<T>(Queue<T> queue)
        {
            return queue.Count;
        }

        /// <summary>
        /// Checks if the Queue is empty
        /// </summary>
        public static bool IsEmpty<T>(Queue<T> queue)
        {
            return queue.Count == 0;
        }

        /// <summary>
        /// Converts the Queue to an array
        /// </summary>
        public static T[] ToArray<T>(Queue<T> queue)
        {
            return queue.ToArray();
        }

        /// <summary>
        /// Converts the Queue to a list
        /// </summary>
        public static List<T> ToList<T>(Queue<T> queue)
        {
            return new List<T>(queue);
        }

        /// <summary>
        /// Trims the Queue's internal capacity
        /// </summary>
        public static void TrimExcess<T>(Queue<T> queue)
        {
            queue.TrimExcess();
        }

        /// <summary>
        /// Gets the internal capacity of the Queue
        /// </summary>
        public static int GetCapacity<T>(Queue<T> queue)
        {
            // Queue doesn't expose capacity directly, but we can estimate
            return queue.Count;
        }

        /// <summary>
        /// Enqueues an item if it's not already in the Queue
        /// </summary>
        public static bool EnqueueUnique<T>(Queue<T> queue, T item)
        {
            if (!queue.Contains(item))
            {
                queue.Enqueue(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Dequeues all items from the Queue
        /// </summary>
        public static List<T> DequeueAll<T>(Queue<T> queue)
        {
            List<T> items = new List<T>();
            while (queue.Count > 0)
            {
                items.Add(queue.Dequeue());
            }
            return items;
        }

        /// <summary>
        /// Rotates the Queue (dequeue from front, enqueue to back)
        /// </summary>
        public static void Rotate<T>(Queue<T> queue, int count = 1)
        {
            if (queue.Count == 0) return;
            count %= queue.Count;
            if (count == 0) return;
            
            List<T> items = DequeueRange(queue, count);
            EnqueueRange(queue, items);
        }

        /// <summary>
        /// Returns a random element from the Queue without removing it
        /// </summary>
        public static T GetRandomElement<T>(Queue<T> queue)
        {
            if (queue.Count == 0) return default(T);
            int index = UnityEngine.Random.Range(0, queue.Count);
            T[] array = queue.ToArray();
            return array[index];
        }

        /// <summary>
        /// Removes all elements that match a condition
        /// </summary>
        public static int RemoveWhere<T>(Queue<T> queue, System.Predicate<T> predicate)
        {
            List<T> remaining = new List<T>();
            int removed = 0;
            
            while (queue.Count > 0)
            {
                T item = queue.Dequeue();
                if (predicate(item))
                {
                    removed++;
                }
                else
                {
                    remaining.Add(item);
                }
            }
            
            EnqueueRange(queue, remaining);
            return removed;
        }

        /// <summary>
        /// Checks if two Queues are equal (same elements in same order)
        /// </summary>
        public static bool AreEqual<T>(Queue<T> queue1, Queue<T> queue2)
        {
            if (queue1.Count != queue2.Count) return false;
            
            T[] array1 = queue1.ToArray();
            T[] array2 = queue2.ToArray();
            
            for (int i = 0; i < array1.Length; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(array1[i], array2[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
