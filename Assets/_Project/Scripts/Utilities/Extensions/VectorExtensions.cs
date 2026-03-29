using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Extension methods for Vector2 and Vector3 structs
    /// </summary>
    public static class VectorExtensions
    {
        // Vector2 extensions
        
        /// <summary>
        /// Returns a Vector2 with components clamped to a range
        /// </summary>
        public static Vector2 Clamped(this Vector2 vector, float min, float max)
        {
            return new Vector2(
                Mathf.Clamp(vector.x, min, max),
                Mathf.Clamp(vector.y, min, max)
            );
        }

        /// <summary>
        /// Returns a Vector2 with components clamped to individual ranges
        /// </summary>
        public static Vector2 Clamped(this Vector2 vector, Vector2 min, Vector2 max)
        {
            return new Vector2(
                Mathf.Clamp(vector.x, min.x, max.x),
                Mathf.Clamp(vector.y, min.y, max.y)
            );
        }

        /// <summary>
        /// Returns a Vector2 with absolute components
        /// </summary>
        public static Vector2 Abs(this Vector2 vector)
        {
            return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }

        /// <summary>
        /// Returns the angle between this vector and another in degrees
        /// </summary>
        public static float AngleTo(this Vector2 from, Vector2 to)
        {
            return Vector2.SignedAngle(from, to);
        }

        /// <summary>
        /// Returns a new vector rotated by angle (in degrees)
        /// </summary>
        public static Vector2 Rotated(this Vector2 vector, float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
        }

        /// <summary>
        /// Returns the perpendicular vector (rotated 90 degrees clockwise)
        /// </summary>
        public static Vector2 Perpendicular(this Vector2 vector)
        {
            return new Vector2(-vector.y, vector.x);
        }

        /// <summary>
        /// Returns the perpendicular vector (rotated 90 degrees counter-clockwise)
        /// </summary>
        public static Vector2 PerpendicularCCW(this Vector2 vector)
        {
            return new Vector2(vector.y, -vector.x);
        }

        /// <summary>
        /// Returns a new vector with the given magnitude
        /// </summary>
        public static Vector2 WithMagnitude(this Vector2 vector, float magnitude)
        {
            return vector.normalized * magnitude;
        }

        /// <summary>
        /// Returns the distance to another vector squared
        /// </summary>
        public static float SqrDistanceTo(this Vector2 from, Vector2 to)
        {
            return (to - from).sqrMagnitude;
        }

        // Vector3 extensions

        /// <summary>
        /// Returns a Vector3 with components clamped to a range
        /// </summary>
        public static Vector3 Clamped(this Vector3 vector, float min, float max)
        {
            return new Vector3(
                Mathf.Clamp(vector.x, min, max),
                Mathf.Clamp(vector.y, min, max),
                Mathf.Clamp(vector.z, min, max)
            );
        }

        /// <summary>
        /// Returns a Vector3 with components clamped to individual ranges
        /// </summary>
        public static Vector3 Clamped(this Vector3 vector, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Mathf.Clamp(vector.x, min.x, max.x),
                Mathf.Clamp(vector.y, min.y, max.y),
                Mathf.Clamp(vector.z, min.z, max.z)
            );
        }

        /// <summary>
        /// Returns a Vector3 with absolute components
        /// </summary>
        public static Vector3 Abs(this Vector3 vector)
        {
            return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
        }

        /// <summary>
        /// Returns the minimum component of the vector
        /// </summary>
        public static float MinComponent(this Vector3 vector)
        {
            return Mathf.Min(vector.x, Mathf.Min(vector.y, vector.z));
        }

        /// <summary>
        /// Returns the maximum component of the vector
        /// </summary>
        public static float MaxComponent(this Vector3 vector)
        {
            return Mathf.Max(vector.x, Mathf.Max(vector.y, vector.z));
        }

        /// <summary>
        /// Returns a new vector with the given magnitude
        /// </summary>
        public static Vector3 WithMagnitude(this Vector3 vector, float magnitude)
        {
            return vector.normalized * magnitude;
        }

        /// <summary>
        /// Returns the distance to another vector squared
        /// </summary>
        public static float SqrDistanceTo(this Vector3 from, Vector3 to)
        {
            return (to - from).sqrMagnitude;
        }

        /// <summary>
        /// Returns a vector rotated around an axis by angle (in degrees)
        /// </summary>
        public static Vector3 RotatedAroundAxis(this Vector3 vector, Vector3 axis, float angle)
        {
            return Quaternion.AngleAxis(angle, axis) * vector;
        }

        /// <summary>
        /// Returns the 2D projection of a 3D vector (drops z)
        /// </summary>
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        /// <summary>
        /// Converts a 2D vector to 3D with given z value
        /// </summary>
        public static Vector3 ToVector3(this Vector2 vector, float z = 0f)
        {
            return new Vector3(vector.x, vector.y, z);
        }
    }
}
