using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for handling common string operations specific to game development
    /// </summary>
    public class GameStringUtils : MonoBehaviour
    {
        /// <summary>
        /// Formats a number with ordinal suffix (1st, 2nd, 3rd, 4th, etc.)
        /// </summary>
        public static string Ordinal(int number)
        {
            if (number <= 0) return number.ToString();

            int remainder = number % 100;
            if (remainder >= 11 && remainder <= 13)
                return number + "th";

            switch (number % 10)
            {
                case 1: return number + "st";
                case 2: return number + "nd";
                case 3: return number + "rd";
                default: return number + "th";
            }
        }

        /// <summary>
        /// Formats a time span into a readable string (e.g., "2h 30m 15s")
        /// </summary>
        public static string FormatTimeSpan(float seconds)
        {
            if (seconds < 0)
                return "-0s";

            int hours = Mathf.FloorToInt(seconds / 3600f);
            int minutes = Mathf.FloorToInt((seconds % 3600f) / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            int milliseconds = Mathf.FloorToInt((seconds * 1000f) % 1000f);

            if (hours > 0)
                return string.Format("{0}h {1}m {2}s", hours, minutes, secs);
            else if (minutes > 0)
                return string.Format("{0}m {1}s", minutes, secs);
            else if (milliseconds > 0)
                return string.Format("{0}s {1}ms", secs, milliseconds);
            else
                return string.Format("{0}s", secs);
        }

        /// <summary>
        /// Formats a large number with K/M/B suffixes
        /// </summary>
        public static string FormatLargeNumber(long number)
        {
            if (number < 1000)
                return number.ToString();
            else if (number < 1000000)
                return string.Format("{0:0.0}K", number / 1000.0);
            else if (number < 1000000000)
                return string.Format("{0:0.0}M", number / 1000000.0);
            else
                return string.Format("{0:0.0}B", number / 1000000000.0);
        }

        /// <summary>
        /// Formats a bytes value into a readable string (B, KB, MB, GB, TB)
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            if (bytes < 1024)
                return bytes + " B";
            else if (bytes < 1048576)
                return string.Format("{0:0.0} KB", bytes / 1024.0);
            else if (bytes < 1073741824)
                return string.Format("{0:0.0} MB", bytes / 1048576.0);
            else if (bytes < 1099511627776)
                return string.Format("{0:0.0} GB", bytes / 1073741824.0);
            else
                return string.Format("{0:0.0} TB", bytes / 1099511627776.0);
        }

        /// <summary>
        /// Checks if a string contains only numeric characters
        /// </summary>
        public static bool IsNumeric(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
                
            foreach (char c in input)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Removes all non-numeric characters from a string
        /// </summary>
        public static string NumbersOnly(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
                
            string result = "";
            foreach (char c in input)
            {
                if (char.IsDigit(c))
                    result += c;
            }
            return result;
        }

        /// <summary>
        /// Converts a string to a safe filename by removing invalid characters
        /// </summary>
        public static string ToSafeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "unnamed";
                
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}])", invalidChars);
            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }
    }
}