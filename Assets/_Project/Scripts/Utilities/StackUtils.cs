using System.Collections.Generic;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for Stack operations
    /// </summary>
    public static class StackUtils
    {
        /// <summary>
        /// Creates a new Stack with initial capacity
        /// </summary>
        public static Stack<T> Create<T>(int capacity = 0)
        {
            return new Stack<T>(capacity);
        }

        /// <summary>
        /// Creates a new Stack from a collection
        /// </summary>
        public static Stack<T> Create<T>(IEnumerable<T> collection)
        {
            return new Stack<T>(collection);
        }

        /// <summary>
        /// Pushes multiple items onto a Stack
        /// </summary>
        public static void PushRange<T>(Stack<T> stack, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                stack.Push(item);
            }
        }

        /// <summary>
        /// Pops multiple items from a Stack
        /// </summary>
        public static List<T> PopRange<T>(Stack<T> stack, int count)
        {
            List<T> items = new List<T>();
            for (int i = 0; i < count && stack.Count > 0; i++)
            {
                items.Add(stack.Pop());
            }
            return items;
        }

        /// <summary>
        /// Peeks at the top item without removing it
        /// </summary>
        public static T PeekOrDefault<T>(Stack<T> stack, T defaultValue = default(T))
        {
            if (stack.Count == 0) return defaultValue;
            return stack.Peek();
        }

        /// <summary>
        /// Pops the top item or returns a default value
        /// </summary>
        public static T PopOrDefault<T>(Stack<T> stack, T defaultValue = default(T))
        {
            if (stack.Count == 0) return defaultValue;
            return stack.Pop();
        }

        /// <summary>
        /// Checks if a Stack contains an item
        /// </summary>
        public static bool Contains<T>(Stack<T> stack, T item)
        {
            return stack.Contains(item);
        }

        /// <summary>
        /// Clears the Stack
        /// </summary>
        public static void Clear<T>(Stack<T> stack)
        {
            stack.Clear();
        }

        /// <summary>
        /// Gets the count of items in the Stack
        /// </summary>
        public static int GetCount<T>(Stack<T> stack)
        {
            return stack.Count;
        }

        /// <summary>
        /// Checks if the Stack is empty
        /// </summary>
        public static bool IsEmpty<T>(Stack<T> stack)
        {
            return stack.Count == 0;
        }

        /// <summary>
        /// Converts the Stack to an array
        /// </summary>
        public static T[] ToArray<T>(Stack<T> stack)
        {
            return stack.ToArray();
        }

        /// <summary>
        /// Converts the Stack to a list
        /// </summary>
        public static List<T> ToList<T>(Stack<T> stack)
        {
            return new List<T>(stack);
        }

        /// <summary>
        /// Trims the Stack's internal capacity
        /// </summary>
        public static void TrimExcess<T>(Stack<T> stack)
        {
            stack.TrimExcess();
        }

        /// <summary>
        /// Gets the internal capacity of the Stack
        /// </summary>
        public static int GetCapacity<T>(Stack<T> stack)
        {
            // Stack doesn't expose capacity directly, but we can estimate
            return stack.Count;
        }

        /// <summary>
        /// Pushes an item if it's not already in the Stack
        /// </summary>
        public static bool PushUnique<T>(Stack<T> stack, T item)
        {
            if (!stack.Contains(item))
            {
                stack.Push(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Pops all items from the Stack
        /// </summary>
        public static List<T> PopAll<T>(Stack<T> stack)
        {
            List<T> items = new List<T>();
            while (stack.Count > 0)
            {
                items.Add(stack.Pop());
            }
            return items;
        }

        /// <summary>
        /// Reverses the order of the Stack
        /// </summary>
        public static void Reverse<T>(Stack<T> stack)
        {
            List<T> items = PopAll<T>(stack);
            items.Reverse();
            PushRange(stack, items);
        }

        /// <summary>
        /// Returns a random element from the Stack without removing it
        /// </summary>
        public static T GetRandomElement<T>(Stack<T> stack)
        {
            if (stack.Count == 0) return default(T);
            int index = UnityEngine.Random.Range(0, stack.Count);
            T[] array = stack.ToArray();
            return array[index];
        }

        /// <summary>
        /// Removes all elements that match a condition
        /// </summary>
        public static int RemoveWhere<T>(Stack<T> stack, System.Predicate<T> predicate)
        {
            List<T> remaining = new List<T>();
            int removed = 0;
            
            while (stack.Count > 0)
            {
                T item = stack.Pop();
                if (predicate(item))
                {
                    removed++;
                }
                else
                {
                    remaining.Add(item);
                }
            }
            
            remaining.Reverse();
            PushRange(stack, remaining);
            return removed;
        }

        /// <summary>
        /// Checks if two Stacks are equal (same elements in same order)
        /// </summary>
        public static bool AreEqual<T>(Stack<T> stack1, Stack<T> stack2)
        {
            if (stack1.Count != stack2.Count) return false;
            
            T[] array1 = stack1.ToArray();
            T[] array2 = stack2.ToArray();
            
            for (int i = 0; i < array1.Length; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(array1[i], array2[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the top N elements without popping
        /// </summary>
        public static List<T> PeekRange<T>(Stack<T> stack, int count)
        {
            List<T> items = new List<T>();
            T[] array = stack.ToArray();
            int start = Mathf.Max(0, array.Length - count);
            for (int i = start; i < array.Length; i++)
            {
                items.Add(array[i]);
            }
            return items;
        }
    }
}
