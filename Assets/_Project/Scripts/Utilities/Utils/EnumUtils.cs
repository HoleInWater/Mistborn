using System;
using System.Collections.Generic;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for enum operations
    /// </summary>
    public static class EnumUtils
    {
        /// <summary>
        /// Gets all values of an enum
        /// </summary>
        public static T[] GetValues<T>() where T : Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        /// <summary>
        /// Gets all names of an enum
        /// </summary>
        public static string[] GetNames<T>() where T : Enum
        {
            return Enum.GetNames(typeof(T));
        }

        /// <summary>
        /// Parses an enum value from a string
        /// </summary>
        public static T Parse<T>(string value, bool ignoreCase = false) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        /// <summary>
        /// Tries to parse an enum value from a string
        /// </summary>
        public static bool TryParse<T>(string value, out T result, bool ignoreCase = false) where T : struct, Enum
        {
            return Enum.TryParse<T>(value, ignoreCase, out result);
        }

        /// <summary>
        /// Checks if an enum value is defined
        /// </summary>
        public static bool IsDefined<T>(T value) where T : Enum
        {
            return Enum.IsDefined(typeof(T), value);
        }

        /// <summary>
        /// Gets the next value in an enum (wraps around)
        /// </summary>
        public static T Next<T>(T current) where T : Enum
        {
            T[] values = GetValues<T>();
            int index = Array.IndexOf(values, current);
            index = (index + 1) % values.Length;
            return values[index];
        }

        /// <summary>
        /// Gets the previous value in an enum (wraps around)
        /// </summary>
        public static T Previous<T>(T current) where T : Enum
        {
            T[] values = GetValues<T>();
            int index = Array.IndexOf(values, current);
            index = (index - 1 + values.Length) % values.Length;
            return values[index];
        }

        /// <summary>
        /// Gets a random value from an enum
        /// </summary>
        public static T Random<T>() where T : Enum
        {
            T[] values = GetValues<T>();
            return values[UnityEngine.Random.Range(0, values.Length)];
        }

        /// <summary>
        /// Converts an enum to a list
        /// </summary>
        public static List<T> ToList<T>() where T : Enum
        {
            return new List<T>(GetValues<T>());
        }

        /// <summary>
        /// Gets the description attribute of an enum value (if any)
        /// </summary>
        public static string GetDescription<T>(T value) where T : Enum
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null) return string.Empty;
            
            var attributes = (System.ComponentModel.DescriptionAttribute[])field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}
