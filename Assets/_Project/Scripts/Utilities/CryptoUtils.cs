using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for handling common encryption and encoding operations
    /// </summary>
    public class CryptoUtils : MonoBehaviour
    {
        /// <summary>
        /// Simple XOR encryption/decryption for strings
        /// </summary>
        public static string XOREncrypt(string input, string key)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            if (string.IsNullOrEmpty(key)) return input;
            
            char[] output = new char[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = (char)(input[i] ^ key[i % key.Length]);
            }
            return new string(output);
        }

        /// <summary>
        /// Simple Base64 encoding
        /// </summary>
        public static string Base64Encode(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
            return System.Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Simple Base64 decoding
        /// </summary>
        public static string Base64Decode(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            byte[] bytes = System.Convert.FromBase64String(input);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Computes a simple hash code for a string
        /// </summary>
        public static int GetHashCode(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;
            
            int hash = 0;
            foreach (char c in input)
            {
                hash = ((hash << 5) - hash) + c;
            }
            return hash;
        }

        /// <summary>
        /// Generates a random password of specified length
        /// </summary>
        public static string GeneratePassword(int length = 12, 
                                            bool useUppercase = true, 
                                            bool useLowercase = true, 
                                            bool useNumbers = true, 
                                            bool useSpecial = true)
        {
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            
            string chars = "";
            if (useUppercase) chars += uppercase;
            if (useLowercase) chars += lowercase;
            if (useNumbers) chars += numbers;
            if (useSpecial) chars += special;
            
            if (string.IsNullOrEmpty(chars))
                chars = "a"; // fallback to avoid empty string
            
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            System.Random rand = new System.Random();
            
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[rand.Next(chars.Length)]);
            }
            
            return result.ToString();
        }

        /// <summary>
        /// Checks if a string is a valid hexadecimal string
        /// </summary>
        public static bool IsHexadecimal(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            
            foreach (char c in input)
            {
                if (!((c >= '0' && c <= '9') || 
                      (c >= 'a' && c <= 'f') || 
                      (c >= 'A' && c <= 'F')))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Converts a hexadecimal string to a byte array
        /// </summary>
        public static byte[] HexStringToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
                return new byte[0];
                
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = System.Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string
        /// </summary>
        public static string BytesToHexString(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;
                
            System.Text.StringBuilder hex = new System.Text.StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
    }
}