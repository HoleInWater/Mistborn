using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for vector operations
    /// </summary>
    public static class VectorUtils
    {
        /// <summary>
        /// Returns the average of an array of vectors
        /// </summary>
        public static Vector3 Average(Vector3[] vectors)
        {
            if (vectors == null || vectors.Length == 0) return Vector3.zero;
            
            Vector3 sum = Vector3.zero;
            foreach (Vector3 v in vectors)
            {
                sum += v;
            }
            return sum / vectors.Length;
        }

        /// <summary>
        /// Returns the average of a list of vectors
        /// </summary>
        public static Vector3 Average(System.Collections.Generic.List<Vector3> vectors)
        {
            if (vectors == null || vectors.Count == 0) return Vector3.zero;
            
            Vector3 sum = Vector3.zero;
            foreach (Vector3 v in vectors)
            {
                sum += v;
            }
            return sum / vectors.Count;
        }

        /// <summary>
        /// Returns the minimum component of a vector
        /// </summary>
        public static float MinComponent(Vector3 vector)
        {
            return Mathf.Min(vector.x, Mathf.Min(vector.y, vector.z));
        }

        /// <summary>
        /// Returns the maximum component of a vector
        /// </summary>
        public static float MaxComponent(Vector3 vector)
        {
            return Mathf.Max(vector.x, Mathf.Max(vector.y, vector.z));
        }

        /// <summary>
        /// Returns a vector with absolute components
        /// </summary>
        public static Vector3 Abs(Vector3 vector)
        {
            return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
        }

        /// <summary>
        /// Returns a vector with clamped components
        /// </summary>
        public static Vector3 Clamp(Vector3 vector, float min, float max)
        {
            return new Vector3(
                Mathf.Clamp(vector.x, min, max),
                Mathf.Clamp(vector.y, min, max),
                Mathf.Clamp(vector.z, min, max)
            );
        }

        /// <summary>
        /// Returns a vector with components clamped to individual ranges
        /// </summary>
        public static Vector3 Clamp(Vector3 vector, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Mathf.Clamp(vector.x, min.x, max.x),
                Mathf.Clamp(vector.y, min.y, max.y),
                Mathf.Clamp(vector.z, min.z, max.z)
            );
        }

        /// <summary>
        /// Returns a vector reflected off a plane defined by normal
        /// </summary>
        public static Vector3 Reflect(Vector3 vector, Vector3 normal)
        {
            return Vector3.Reflect(vector, normal);
        }

        /// <summary>
        /// Returns a vector projected onto a plane defined by normal
        /// </summary>
        public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal)
        {
            return Vector3.ProjectOnPlane(vector, planeNormal);
        }

        /// <summary>
        /// Returns a vector rotated around an axis by angle (in degrees)
        /// </summary>
        public static Vector3 RotateAroundAxis(Vector3 vector, Vector3 axis, float angle)
        {
            return Quaternion.AngleAxis(angle, axis) * vector;
        }

        /// <summary>
        /// Returns a vector with a given magnitude
        /// </summary>
        public static Vector3 WithMagnitude(Vector3 vector, float magnitude)
        {
            return vector.normalized * magnitude;
        }

        /// <summary>
        /// Returns the 2D angle between two vectors in degrees
        /// </summary>
        public static float Angle2D(Vector2 from, Vector2 to)
        {
            return Vector2.SignedAngle(from, to);
        }

        /// <summary>
        /// Returns a 2D vector rotated by angle (in degrees)
        /// </summary>
        public static Vector2 Rotate2D(Vector2 vector, float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
        }

        /// <summary>
        /// Returns the 2D perpendicular vector (rotated 90 degrees clockwise)
        /// </summary>
        public static Vector2 Perpendicular2D(Vector2 vector)
        {
            return new Vector2(-vector.y, vector.x);
        }

        /// <summary>
        /// Returns the 2D perpendicular vector (rotated 90 degrees counter-clockwise)
        /// </summary>
        public static Vector2 Perpendicular2DCCW(Vector2 vector)
        {
            return new Vector2(vector.y, -vector.x);
        }

        /// <summary>
        /// Returns the distance between two points squared (faster than distance)
        /// </summary>
        public static float SqrDistance(Vector3 a, Vector3 b)
        {
            return (a - b).sqrMagnitude;
        }

        /// <summary>
        /// Returns the 2D distance between two points squared
        /// </summary>
        public static float SqrDistance2D(Vector2 a, Vector2 b)
        {
            return (a - b).sqrMagnitude;
        }
    }
}
