using UnityEngine;
using System.Linq;

namespace MistbornGame.Utilities
{
    public class StringUtils : MonoBehaviour
    {
        /// <summary>
        /// Checks if a string is null, empty, or whitespace only
        /// </summary>
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;
            
            foreach (char c in value)
            {
                if (!char.IsWhiteSpace(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Truncates a string to a maximum length and adds ellipsis if needed
        /// </summary>
        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;
            
            return value.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// Formats a number with commas as thousand separators
        /// </summary>
        public static string FormatNumber(int number)
        {
            return number.ToString("N0");
        }

        /// <summary>
        /// Formats a float with specified decimal places
        /// </summary>
        public static string FormatFloat(float value, int decimalPlaces = 2)
        {
            return value.ToString($"F{decimalPlaces}");
        }

        /// <summary>
        /// Converts a string to title case (first letter of each word capitalized)
        /// </summary>
        public static string ToTitleCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        /// <summary>
        /// Splits a string by a separator and trims each part
        /// </summary>
        public static string[] SplitAndTrim(string value, char separator)
        {
            if (string.IsNullOrEmpty(value))
                return new string[0];
            
            return value.Split(separator)
                       .Select(s => s.Trim())
                       .Where(s => !string.IsNullOrEmpty(s))
                       .ToArray();
        }
    }
}
