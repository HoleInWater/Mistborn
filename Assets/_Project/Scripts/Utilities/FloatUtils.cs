using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for float operations
    /// </summary>
    public static class FloatUtils
    {
        /// <summary>
        /// Clamps a float between min and max
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }

        /// <summary>
        /// Clamps a float between 0 and 1
        /// </summary>
        public static float Clamp01(float value)
        {
            return Mathf.Clamp01(value);
        }

        /// <summary>
        /// Linearly interpolates between two floats
        /// </summary>
        public static float Lerp(float a, float b, float t)
        {
            return Mathf.Lerp(a, b, t);
        }

        /// <summary>
        /// Linearly interpolates between two floats without clamping t
        /// </summary>
        public static float LerpUnclamped(float a, float b, float t)
        {
            return Mathf.LerpUnclamped(a, b, t);
        }

        /// <summary>
        /// Calculates the percentage of a value between min and max
        /// </summary>
        public static float Percentage(float value, float min, float max)
        {
            float range = max - min;
            if (Mathf.Abs(range) < Mathf.Epsilon) return 0f;
            return (value - min) / range;
        }

        /// <summary>
        /// Checks if two floats are approximately equal
        /// </summary>
        public static bool Approximately(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        /// <summary>
        /// Moves a current value towards a target at a given speed
        /// </summary>
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            return Mathf.MoveTowards(current, target, maxDelta);
        }

        /// <summary>
        /// Smoothly damps between two values
        /// </summary>
        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed = float.MaxValue, float deltaTime = -1f)
        {
            if (deltaTime < 0) deltaTime = Time.deltaTime;
            return Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        /// <summary>
        /// Returns the sign of a float (-1, 0, or 1)
        /// </summary>
        public static float Sign(float value)
        {
            return Mathf.Sign(value);
        }

        /// <summary>
        /// Returns the absolute value of a float
        /// </summary>
        public static float Abs(float value)
        {
            return Mathf.Abs(value);
        }

        /// <summary>
        /// Returns the minimum of two floats
        /// </summary>
        public static float Min(float a, float b)
        {
            return Mathf.Min(a, b);
        }

        /// <summary>
        /// Returns the maximum of two floats
        /// </summary>
        public static float Max(float a, float b)
        {
            return Mathf.Max(a, b);
        }

        /// <summary>
        /// Returns the power of a float
        /// </summary>
        public static float Pow(float value, float power)
        {
            return Mathf.Pow(value, power);
        }

        /// <summary>
        /// Returns the square root of a float
        /// </summary>
        public static float Sqrt(float value)
        {
            return Mathf.Sqrt(value);
        }

        /// <summary>
        /// Returns the logarithm of a float
        /// </summary>
        public static float Log(float value)
        {
            return Mathf.Log(value);
        }

        /// <summary>
        /// Returns the natural logarithm of a float
        /// </summary>
        public static float Log(float value, float newBase)
        {
            return Mathf.Log(value, newBase);
        }

        /// <summary>
        /// Returns the exponential of a float
        /// </summary>
        public static float Exp(float power)
        {
            return Mathf.Exp(power);
        }

        /// <summary>
        /// Returns the sine of a float (in radians)
        /// </summary>
        public static float Sin(float f)
        {
            return Mathf.Sin(f);
        }

        /// <summary>
        /// Returns the cosine of a float (in radians)
        /// </summary>
        public static float Cos(float f)
        {
            return Mathf.Cos(f);
        }

        /// <summary>
        /// Returns the tangent of a float (in radians)
        /// </summary>
        public static float Tan(float f)
        {
            return Mathf.Tan(f);
        }

        /// <summary>
        /// Returns the arcsine of a float (in radians)
        /// </summary>
        public static float Asin(float f)
        {
            return Mathf.Asin(f);
        }

        /// <summary>
        /// Returns the arccosine of a float (in radians)
        /// </summary>
        public static float Acos(float f)
        {
            return Mathf.Acos(f);
        }

        /// <summary>
        /// Returns the arctangent of a float (in radians)
        /// </summary>
        public static float Atan(float f)
        {
            return Mathf.Atan(f);
        }

        /// <summary>
        /// Returns the arctangent of y/x (in radians)
        /// </summary>
        public static float Atan2(float y, float x)
        {
            return Mathf.Atan2(y, x);
        }
    }
}
