using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for boolean operations
    /// </summary>
    public static class BoolUtils
    {
        /// <summary>
        /// Converts a boolean to int (0 for false, 1 for true)
        /// </summary>
        public static int ToInt(bool value)
        {
            return value ? 1 : 0;
        }

        /// <summary>
        /// Converts an int to boolean (0 is false, non-zero is true)
        /// </summary>
        public static bool FromInt(int value)
        {
            return value != 0;
        }

        /// <summary>
        /// Returns the logical NOT of a boolean
        /// </summary>
        public static bool NOT(bool value)
        {
            return !value;
        }

        /// <summary>
        /// Returns the logical AND of two booleans
        /// </summary>
        public static bool AND(bool a, bool b)
        {
            return a && b;
        }

        /// <summary>
        /// Returns the logical OR of two booleans
        /// </summary>
        public static bool OR(bool a, bool b)
        {
            return a || b;
        }

        /// <summary>
        /// Returns the logical XOR of two booleans
        /// </summary>
        public static bool XOR(bool a, bool b)
        {
            return a ^ b;
        }

        /// <summary>
        /// Returns true if all booleans in an array are true
        /// </summary>
        public static bool AllTrue(bool[] values)
        {
            if (values == null) return false;
            foreach (bool value in values)
            {
                if (!value) return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if any boolean in an array is true
        /// </summary>
        public static bool AnyTrue(bool[] values)
        {
            if (values == null) return false;
            foreach (bool value in values)
            {
                if (value) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if all booleans in an array are false
        /// </summary>
        public static bool AllFalse(bool[] values)
        {
            if (values == null) return true;
            foreach (bool value in values)
            {
                if (value) return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if any boolean in an array is false
        /// </summary>
        public static bool AnyFalse(bool[] values)
        {
            if (values == null) return false;
            foreach (bool value in values)
            {
                if (!value) return true;
            }
            return false;
        }

        /// <summary>
        /// Toggles a boolean value
        /// </summary>
        public static bool Toggle(bool value)
        {
            return !value;
        }

        /// <summary>
        /// Returns a string representation of a boolean
        /// </summary>
        public static string ToString(bool value)
        {
            return value ? "true" : "false";
        }

        /// <summary>
        /// Parses a string to boolean
        /// </summary>
        public static bool Parse(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            
            value = value.Trim().ToLower();
            return value == "true" || value == "1" || value == "yes" || value == "on";
        }

        /// <summary>
        /// Tries to parse a string to boolean
        /// </summary>
        public static bool TryParse(string value, out bool result)
        {
            result = false;
            if (string.IsNullOrEmpty(value)) return false;
            
            value = value.Trim().ToLower();
            if (value == "true" || value == "1" || value == "yes" || value == "on")
            {
                result = true;
                return true;
            }
            if (value == "false" || value == "0" || value == "no" || value == "off")
            {
                result = false;
                return true;
            }
            return false;
        }
    }
}
