using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Extension methods for numeric types
    /// </summary>
    public static class MathExtensions
    {
        // int extensions
        
        /// <summary>
        /// Checks if an integer is between two values (inclusive)
        /// </summary>
        public static bool IsBetween(this int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Clamps an integer between min and max
        /// </summary>
        public static int Clamp(this int value, int min, int max)
        {
            return Mathf.Clamp(value, min, max);
        }

        /// <summary>
        /// Returns the sign of an integer (-1, 0, or 1)
        /// </summary>
        public static int Sign(this int value)
        {
            return value > 0 ? 1 : (value < 0 ? -1 : 0);
        }

        /// <summary>
        /// Checks if an integer is even
        /// </summary>
        public static bool IsEven(this int value)
        {
            return value % 2 == 0;
        }

        /// <summary>
        /// Checks if an integer is odd
        /// </summary>
        public static bool IsOdd(this int value)
        {
            return value % 2 != 0;
        }

        // float extensions
        
        /// <summary>
        /// Checks if a float is between two values (inclusive)
        /// </summary>
        public static bool IsBetween(this float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Clamps a float between min and max
        /// </summary>
        public static float Clamp(this float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }

        /// <summary>
        /// Clamps a float between 0 and 1
        /// </summary>
        public static float Clamp01(this float value)
        {
            return Mathf.Clamp01(value);
        }

        /// <summary>
        /// Returns the sign of a float (-1, 0, or 1)
        /// </summary>
        public static float Sign(this float value)
        {
            return Mathf.Sign(value);
        }

        /// <summary>
        /// Linearly interpolates between this value and a target
        /// </summary>
        public static float LerpTo(this float current, float target, float t)
        {
            return Mathf.Lerp(current, target, t);
        }

        /// <summary>
        /// Linearly interpolates between this value and a target (unclamped)
        /// </summary>
        public static float LerpToUnclamped(this float current, float target, float t)
        {
            return Mathf.LerpUnclamped(current, target, t);
        }

        /// <summary>
        /// Moves a current value towards a target at a given speed
        /// </summary>
        public static float MoveTowards(this float current, float target, float maxDelta)
        {
            return Mathf.MoveTowards(current, target, maxDelta);
        }

        /// <summary>
        /// Checks if a float is approximately equal to another
        /// </summary>
        public static bool Approximately(this float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        /// <summary>
        /// Returns the reciprocal of a float
        /// </summary>
        public static float Reciprocal(this float value)
        {
            return 1f / value;
        }

        /// <summary>
        /// Returns the square of a float
        /// </summary>
        public static float Square(this float value)
        {
            return value * value;
        }

        /// <summary>
        /// Returns the cube of a float
        /// </summary>
        public static float Cube(this float value)
        {
            return value * value * value;
        }

        /// <summary>
        /// Returns the square root of a float
        /// </summary>
        public static float Sqrt(this float value)
        {
            return Mathf.Sqrt(value);
        }

        /// <summary>
        /// Returns the absolute value of a float
        /// </summary>
        public static float Abs(this float value)
        {
            return Mathf.Abs(value);
        }

        /// <summary>
        /// Returns the floor of a float
        /// </summary>
        public static int FloorToInt(this float value)
        {
            return Mathf.FloorToInt(value);
        }

        /// <summary>
        /// Returns the ceiling of a float
        /// </summary>
        public static int CeilToInt(this float value)
        {
            return Mathf.CeilToInt(value);
        }

        /// <summary>
        /// Returns the rounded value of a float
        /// </summary>
        public static int RoundToInt(this float value)
        {
            return Mathf.RoundToInt(value);
        }

        /// <summary>
        /// Returns the nearest power of two
        /// </summary>
        public static int NextPowerOfTwo(this int value)
        {
            return Mathf.NextPowerOfTwo(value);
        }

        /// <summary>
        /// Returns the logarithm of a float with a given base
        /// </summary>
        public static float Log(this float value, float newBase)
        {
            return Mathf.Log(value, newBase);
        }

        /// <summary>
        /// Returns the natural logarithm of a float
        /// </summary>
        public static float Ln(this float value)
        {
            return Mathf.Log(value);
        }

        /// <summary>
        /// Returns the base-10 logarithm of a float
        /// </summary>
        public static float Log10(this float value)
        {
            return Mathf.Log10(value);
        }

        /// <summary>
        /// Returns the exponential of a float (e^x)
        /// </summary>
        public static float Exp(this float value)
        {
            return Mathf.Exp(value);
        }

        /// <summary>
        /// Returns the power of a float
        /// </summary>
        public static float Pow(this float value, float power)
        {
            return Mathf.Pow(value, power);
        }

        /// <summary>
        /// Returns the sine of a float (in radians)
        /// </summary>
        public static float Sin(this float value)
        {
            return Mathf.Sin(value);
        }

        /// <summary>
        /// Returns the cosine of a float (in radians)
        /// </summary>
        public static float Cos(this float value)
        {
            return Mathf.Cos(value);
        }

        /// <summary>
        /// Returns the tangent of a float (in radians)
        /// </summary>
        public static float Tan(this float value)
        {
            return Mathf.Tan(value);
        }

        /// <summary>
        /// Returns the arcsine of a float (in radians)
        /// </summary>
        public static float Asin(this float value)
        {
            return Mathf.Asin(value);
        }

        /// <summary>
        /// Returns the arccosine of a float (in radians)
        /// </summary>
        public static float Acos(this float value)
        {
            return Mathf.Acos(value);
        }

        /// <summary>
        /// Returns the arctangent of a float (in radians)
        /// </summary>
        public static float Atan(this float value)
        {
            return Mathf.Atan(value);
        }

        /// <summary>
        /// Returns the arctangent of y/x (in radians)
        /// </summary>
        public static float Atan2(this float y, float x)
        {
            return Mathf.Atan2(y, x);
        }

        /// <summary>
        /// Converts degrees to radians
        /// </summary>
        public static float Deg2Rad(this float degrees)
        {
            return degrees * Mathf.Deg2Rad;
        }

        /// <summary>
        /// Converts radians to degrees
        /// </summary>
        public static float Rad2Deg(this float radians)
        {
            return radians * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Returns the inverse of a value (1/x)
        /// </summary>
        public static float Inverse(this float value)
        {
            return 1f / value;
        }

        /// <summary>
        /// Returns the modulo of a float
        /// </summary>
        public static float Mod(this float value, float divisor)
        {
            return value % divisor;
        }

        /// <summary>
        /// Returns the average of two floats
        /// </summary>
        public static float Average(this float a, float b)
        {
            return (a + b) * 0.5f;
        }

        /// <summary>
        /// Returns the weighted average of two floats
        /// </summary>
        public static float WeightedAverage(this float a, float b, float weight)
        {
            return a * (1f - weight) + b * weight;
        }

        /// <summary>
        /// Returns true if the float is a power of two
        /// </summary>
        public static bool IsPowerOfTwo(this int value)
        {
            return value > 0 && (value & (value - 1)) == 0;
        }

        /// <summary>
        /// Returns the number of set bits in an integer
        /// </summary>
        public static int PopCount(this int value)
        {
            int count = 0;
            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }
            return count;
        }

        /// <summary>
        /// Returns the number of set bits in a long
        /// </summary>
        public static int PopCount(this long value)
        {
            int count = 0;
            while (value != 0)
            {
                count += (int)(value & 1);
                value >>= 1;
            }
            return count;
        }
    }
}
