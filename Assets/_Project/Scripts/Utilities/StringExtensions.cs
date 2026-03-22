using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Extension methods for string type
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Checks if a string is null, empty, or whitespace only
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Trims the string and returns null if empty
        /// </summary>
        public static string TrimToNull(this string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            string trimmed = str.Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }

        /// <summary>
        /// Trims the string and returns empty if null
        /// </summary>
        public static string TrimToEmpty(this string str)
        {
            return str?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Reverses a string
        /// </summary>
        public static string Reverse(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            char[] array = str.ToCharArray();
            Array.Reverse(array);
            return new string(array);
        }

        /// <summary>
        /// Returns the first N characters of a string
        /// </summary>
        public static string Left(this string str, int count)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (count >= str.Length) return str;
            return str.Substring(0, count);
        }

        /// <summary>
        /// Returns the last N characters of a string
        /// </summary>
        public static string Right(this string str, int count)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (count >= str.Length) return str;
            return str.Substring(str.Length - count);
        }

        /// <summary>
        /// Returns a substring between two delimiters
        /// </summary>
        public static string Between(this string str, string startDelimiter, string endDelimiter)
        {
            if (string.IsNullOrEmpty(str)) return str;
            
            int startIndex = str.IndexOf(startDelimiter);
            if (startIndex == -1) return string.Empty;
            startIndex += startDelimiter.Length;
            
            int endIndex = str.IndexOf(endDelimiter, startIndex);
            if (endIndex == -1) return string.Empty;
            
            return str.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// Counts the occurrences of a substring
        /// </summary>
        public static int CountOccurrences(this string str, string substring)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(substring)) return 0;
            return Regex.Matches(str, Regex.Escape(substring)).Count;
        }

        /// <summary>
        /// Removes all occurrences of a substring
        /// </summary>
        public static string RemoveAll(this string str, string substring)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(substring)) return str;
            return str.Replace(substring, string.Empty);
        }

        /// <summary>
        /// Truncates a string to a maximum length, adding ellipsis if needed
        /// </summary>
        public static string Truncate(this string str, int maxLength, string ellipsis = "...")
        {
            if (string.IsNullOrEmpty(str) || str.Length <= maxLength) return str;
            if (maxLength <= ellipsis.Length) return ellipsis.Substring(0, maxLength);
            return str.Substring(0, maxLength - ellipsis.Length) + ellipsis;
        }

        /// <summary>
        /// Converts string to title case
        /// </summary>
        public static string ToTitleCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        /// Checks if string contains any of the given substrings
        /// </summary>
        public static bool ContainsAny(this string str, params string[] substrings)
        {
            if (string.IsNullOrEmpty(str)) return false;
            foreach (string substring in substrings)
            {
                if (str.Contains(substring)) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if string contains all of the given substrings
        /// </summary>
        public static bool ContainsAll(this string str, params string[] substrings)
        {
            if (string.IsNullOrEmpty(str)) return false;
            foreach (string substring in substrings)
            {
                if (!str.Contains(substring)) return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if string is a valid integer
        /// </summary>
        public static bool IsInteger(this string str)
        {
            return int.TryParse(str, out _);
        }

        /// <summary>
        /// Checks if string is a valid float
        /// </summary>
        public static bool IsFloat(this string str)
        {
            return float.TryParse(str, out _);
        }

        /// <summary>
        /// Checks if string is a valid boolean
        /// </summary>
        public static bool IsBoolean(this string str)
        {
            return bool.TryParse(str, out _);
        }

        /// <summary>
        /// Joins a string array with a separator
        /// </summary>
        public static string Join(this string[] array, string separator = ", ")
        {
            return string.Join(separator, array);
        }

        /// <summary>
        /// Splits a string by given separators
        /// </summary>
        public static string[] Split(this string str, params string[] separators)
        {
            return str.Split(separators, StringSplitOptions.None);
        }

        /// <summary>
        /// Splits a string by given separators, removing empty entries
        /// </summary>
        public static string[] SplitRemoveEmpty(this string str, params string[] separators)
        {
            return str.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Converts a string to snake_case
        /// </summary>
        public static string ToSnakeCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return System.Text.RegularExpressions.Regex.Replace(str, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }

        /// <summary>
        /// Converts a string to camelCase
        /// </summary>
        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (char.IsUpper(str[0])) str = char.ToLower(str[0]) + str.Substring(1);
            return str;
        }

        /// <summary>
        /// Converts a string to PascalCase
        /// </summary>
        public static string ToPascalCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (char.IsLower(str[0])) str = char.ToUpper(str[0]) + str.Substring(1);
            return str;
        }

        /// <summary>
        /// Converts a string to kebab-case
        /// </summary>
        public static string ToKebabCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return System.Text.RegularExpressions.Regex.Replace(str, @"([a-z0-9])([A-Z])", "$1-$2").ToLower();
        }

        /// <summary>
        /// Removes all whitespace from a string
        /// </summary>
        public static string RemoveWhitespace(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return System.Text.RegularExpressions.Regex.Replace(str, @"\s+", "");
        }

        /// <summary>
        /// Returns a string with only alphanumeric characters
        /// </summary>
        public static string OnlyAlphanumeric(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return System.Text.RegularExpressions.Regex.Replace(str, @"[^a-zA-Z0-9]", "");
        }

        /// <summary>
        /// Capitalizes the first letter of a string
        /// </summary>
        public static string CapitalizeFirst(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// Lowercases the first letter of a string
        /// </summary>
        public static string LowercaseFirst(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return char.ToLower(str[0]) + str.Substring(1);
        }
    }
}
