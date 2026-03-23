using System.Text;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for StringBuilder operations
    /// </summary>
    public static class StringBuilderUtils
    {
        /// <summary>
        /// Creates a new StringBuilder with initial capacity
        /// </summary>
        public static StringBuilder Create(int capacity = 16)
        {
            return new StringBuilder(capacity);
        }

        /// <summary>
        /// Creates a new StringBuilder with initial content
        /// </summary>
        public static StringBuilder Create(string initialContent)
        {
            return new StringBuilder(initialContent);
        }

        /// <summary>
        /// Appends a string with a newline
        /// </summary>
        public static StringBuilder AppendLine(StringBuilder sb, string value)
        {
            return sb.AppendLine(value);
        }

        /// <summary>
        /// Appends a formatted string with a newline
        /// </summary>
        public static StringBuilder AppendLineFormat(StringBuilder sb, string format, params object[] args)
        {
            return sb.AppendFormat(format, args).AppendLine();
        }

        /// <summary>
        /// Appends multiple strings
        /// </summary>
        public static StringBuilder AppendMultiple(StringBuilder sb, params string[] values)
        {
            foreach (string value in values)
            {
                sb.Append(value);
            }
            return sb;
        }

        /// <summary>
        /// Appends multiple strings with a separator
        /// </summary>
        public static StringBuilder AppendJoin(StringBuilder sb, string separator, params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) sb.Append(separator);
                sb.Append(values[i]);
            }
            return sb;
        }

        /// <summary>
        /// Clears the StringBuilder
        /// </summary>
        public static StringBuilder Clear(StringBuilder sb)
        {
            return sb.Clear();
        }

        /// <summary>
        /// Removes a range of characters
        /// </summary>
        public static StringBuilder Remove(StringBuilder sb, int startIndex, int length)
        {
            return sb.Remove(startIndex, length);
        }

        /// <summary>
        /// Inserts a string at a position
        /// </summary>
        public static StringBuilder Insert(StringBuilder sb, int index, string value)
        {
            return sb.Insert(index, value);
        }

        /// <summary>
        /// Reverses the contents of the StringBuilder
        /// </summary>
        public static StringBuilder Reverse(StringBuilder sb)
        {
            if (sb == null) return null;

            int left = 0;
            int right = sb.Length - 1;
            while (left < right)
            {
                char temp = sb[left];
                sb[left] = sb[right];
                sb[right] = temp;
                left++;
                right--;
            }
            return sb;
        }

        /// <summary>
        /// Replaces all occurrences of a string within a range
        /// </summary>
        public static StringBuilder Replace(StringBuilder sb, string oldValue, string newValue, int startIndex, int count)
        {
            return sb.Replace(oldValue, newValue, startIndex, count);
        }

        /// <summary>
        /// Reverses the contents of the StringBuilder
        /// </summary>
        public static StringBuilder Reverse(StringBuilder sb)
        {
            return sb.Reverse();
        }

        /// <summary>
        /// Gets the string representation
        /// </summary>
        public static string ToString(StringBuilder sb)
        {
            return sb.ToString();
        }

        /// <summary>
        /// Gets a substring from the StringBuilder
        /// </summary>
        public static string ToString(StringBuilder sb, int startIndex, int length)
        {
            return sb.ToString(startIndex, length);
        }

        /// <summary>
        /// Gets the length of the StringBuilder
        /// </summary>
        public static int GetLength(StringBuilder sb)
        {
            return sb.Length;
        }

        /// <summary>
        /// Gets the capacity of the StringBuilder
        /// </summary>
        public static int GetCapacity(StringBuilder sb)
        {
            return sb.Capacity;
        }

        /// <summary>
        /// Sets the capacity of the StringBuilder
        /// </summary>
        public static void SetCapacity(StringBuilder sb, int capacity)
        {
            sb.Capacity = capacity;
        }

        /// <summary>
        /// Ensures the capacity is at least a certain value
        /// </summary>
        public static void EnsureCapacity(StringBuilder sb, int capacity)
        {
            sb.EnsureCapacity(capacity);
        }

        /// <summary>
        /// Checks if the StringBuilder is empty
        /// </summary>
        public static bool IsEmpty(StringBuilder sb)
        {
            return sb.Length == 0;
        }

        /// <summary>
        /// Gets the character at a position
        /// </summary>
        public static char GetChar(StringBuilder sb, int index)
        {
            return sb[index];
        }

        /// <summary>
        /// Sets the character at a position
        /// </summary>
        public static void SetChar(StringBuilder sb, int index, char value)
        {
            sb[index] = value;
        }

        /// <summary>
        /// Appends a character multiple times
        /// </summary>
        public static StringBuilder AppendRepeat(StringBuilder sb, char c, int count)
        {
            for (int i = 0; i < count; i++)
            {
                sb.Append(c);
            }
            return sb;
        }

        /// <summary>
        /// Appends a string with padding
        /// </summary>
        public static StringBuilder AppendPadded(StringBuilder sb, string value, int totalWidth, char paddingChar = ' ', bool alignLeft = false)
        {
            if (value.Length >= totalWidth)
            {
                return sb.Append(value);
            }
            
            int padding = totalWidth - value.Length;
            if (alignLeft)
            {
                return sb.Append(value).AppendRepeat(paddingChar, padding);
            }
            else
            {
                return sb.AppendRepeat(paddingChar, padding).Append(value);
            }
        }

        /// <summary>
        /// Appends a line with indentation
        /// </summary>
        public static StringBuilder AppendIndentedLine(StringBuilder sb, string value, int indentLevel, string indentString = "    ")
        {
            for (int i = 0; i < indentLevel; i++)
            {
                sb.Append(indentString);
            }
            return sb.AppendLine(value);
        }

        /// <summary>
        /// Joins a collection of strings with a separator
        /// </summary>
        public static StringBuilder Join<T>(System.Collections.Generic.IEnumerable<T> items, string separator, System.Func<T, string> selector = null)
        {
            StringBuilder sb = Create();
            bool first = true;
            foreach (T item in items)
            {
                if (!first) sb.Append(separator);
                string value = selector != null ? selector(item) : item.ToString();
                sb.Append(value);
                first = false;
            }
            return sb;
        }
    }
}
