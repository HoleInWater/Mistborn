using UnityEngine;
using System.Linq;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for common validation checks
    /// </summary>
    public static class ValidationUtils
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
        /// Checks if a string is a valid email address (basic validation)
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a string contains only letters and numbers
        /// </summary>
        public static bool IsAlphanumeric(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
                
            foreach (char c in input)
            {
                if (!char.IsLetterOrDigit(c))
                    return false;
            }
            return true;
        }

        /// </// <summary>
        /// Checks if a value is within a specified range (inclusive)
        /// </summary>
        public static bool IsInRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Checks if a value is within a specified range (inclusive)
        /// </summary>
        public static bool IsInRange(float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Checks if a value is within a specified range (exclusive)
        /// </summary>
        public static bool IsInRangeExclusive(int value, int min, int max)
        {
            return value > min && value < max;
        }

        /// <summary>
        /// Checks if a value is within a specified range (exclusive)
        /// </summary>
        public static bool IsInRangeExclusive(float value, float min, float max)
        {
            return value > min && value < max;
        }

        /// <summary>
        /// Validates that a password meets basic requirements
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <param name="minLength">Minimum password length</param>
        /// <param name="requireUpperCase">Whether uppercase is required</param>
        /// <param name="requireLowerCase">Whether lowercase is required</param>
        /// <param name="requireNumbers">Whether numbers are required</param>
        /// <param name="requireSpecialChars">Whether special characters are required</param>
        /// <returns>True if password meets requirements, false otherwise</returns>
        public static bool ValidatePassword(string password, int minLength = 8, 
                                          bool requireUpperCase = true, bool requireLowerCase = true,
                                          bool requireNumbers = true, bool requireSpecialChars = false)
        {
            if (string.IsNullOrEmpty(password) || password.Length < minLength)
                return false;
            
            if (requireUpperCase && !password.Any(char.IsUpper))
                return false;
                
            if (requireLowerCase && !password.Any(char.IsLower))
                return false;
                
            if (requireNumbers && !password.Any(char.IsDigit))
                return false;
                
            if (requireSpecialChars && !password.Any(c => !char.IsLetterOrDigit(c)))
                return false;
                
            return true;
        }

        /// <summary>
        /// Checks if a transform is not null and has a valid parent
        /// </summary>
        public static bool IsValidTransform(Transform transform)
        {
            return transform != null && transform.parent != null;
        }

        /// <summary>
        /// Checks if a GameObject is active in the hierarchy
        /// </summary>
        public static bool IsActiveInHierarchy(GameObject go)
        {
            return go != null && go.activeInHierarchy;
        }

        /// <summary>
        /// Checks if a component is attached to a GameObject
        /// </summary>
        public static bool HasComponent<T>(GameObject go) where T : Component
        {
            if (go == null)
                return false;
                
            return go.GetComponent<T>() != null;
        }

        /// <summary>
        /// Checks if two floats are approximately equal
        /// </summary>
        public static bool Approximately(float a, float b, float tolerance = 0.001f)
        {
            return Mathf.Abs(a - b) < tolerance;
        }

        /// <summary>
        /// Checks if two vectors are approximately equal
        /// </summary>
        public static bool Approximately(Vector3 a, Vector3 b, float tolerance = 0.001f)
        {
            return Vector3.Distance(a, b) < tolerance;
        }

        /// <summary>
        /// Checks if two quaternions are approximately equal
        /// </summary>
        public static bool Approximately(Quaternion a, Quaternion b, float tolerance = 0.001f)
        {
            return Quaternion.Angle(a, b) < tolerance;
        }
    }
}
