using System.Collections.Generic;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Extension methods for Transform class
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Resets the transform to world origin (position zero, rotation identity, scale one)
        /// </summary>
        public static void Reset(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Resets the position to world origin
        /// </summary>
        public static void ResetPosition(this Transform transform)
        {
            transform.position = Vector3.zero;
        }

        /// <summary>
        /// Resets the rotation to identity
        /// </summary>
        public static void ResetRotation(this Transform transform)
        {
            transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Resets the scale to one
        /// </summary>
        public static void ResetScale(this Transform transform)
        {
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Sets the X position
        /// </summary>
        public static void SetX(this Transform transform, float x)
        {
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }

        /// <summary>
        /// Sets the Y position
        /// </summary>
        public static void SetY(this Transform transform, float y)
        {
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }

        /// <summary>
        /// Sets the Z position
        /// </summary>
        public static void SetZ(this Transform transform, float z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
        }

        /// <summary>
        /// Translates the transform by a vector
        /// </summary>
        public static void Translate(this Transform transform, Vector3 translation)
        {
            transform.position += translation;
        }

        /// <summary>
        /// Translates the transform by a vector in local space
        /// </summary>
        public static void TranslateLocal(this Transform transform, Vector3 translation)
        {
            transform.localPosition += translation;
        }

        /// <summary>
        /// Gets the first child with a specific name
        /// </summary>
        public static Transform GetChildByName(this Transform transform, string childName)
        {
            foreach (Transform child in transform)
            {
                if (child.name == childName)
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all children of a transform
        /// </summary>
        public static List<Transform> GetAllChildren(this Transform transform)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in transform)
            {
                children.Add(child);
            }
            return children;
        }

        /// <summary>
        /// Removes all children from a transform
        /// </summary>
        public static void RemoveAllChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Sets the parent of a transform, optionally keeping world position
        /// </summary>
        public static void SetParent(this Transform transform, Transform parent, bool worldPositionStays = true)
        {
            transform.SetParent(parent, worldPositionStays);
        }

        /// <summary>
        /// Looks at a target position with optional up vector
        /// </summary>
        public static void LookAt(this Transform transform, Vector3 target, Vector3? up = null)
        {
            transform.LookAt(target, up ?? Vector3.up);
        }

        /// <summary>
        /// Rotates the transform around a point and axis
        /// </summary>
        public static void RotateAround(this Transform transform, Vector3 point, Vector3 axis, float angle)
        {
            transform.RotateAround(point, axis, angle);
        }

        /// <summary>
        /// Gets the forward direction of the transform
        /// </summary>
        public static Vector3 Forward(this Transform transform)
        {
            return transform.forward;
        }

        /// <summary>
        /// Gets the right direction of the transform
        /// </summary>
        public static Vector3 Right(this Transform transform)
        {
            return transform.right;
        }

        /// <summary>
        /// Gets the up direction of the transform
        /// </summary>
        public static Vector3 Up(this Transform transform)
        {
            return transform.up;
        }

        /// <summary>
        /// Gets the position of a child by name
        /// </summary>
        public static Vector3? GetChildPosition(this Transform transform, string childName)
        {
            Transform child = GetChildByName(transform, childName);
            return child?.position;
        }

        /// <summary>
        /// Gets the rotation of a child by name
        /// </summary>
        public static Quaternion? GetChildRotation(this Transform transform, string childName)
        {
            Transform child = GetChildByName(transform, childName);
            return child?.rotation;
        }

        /// <summary>
        /// Gets the lossy scale of a child by name
        /// </summary>
        public static Vector3? GetChildScale(this Transform transform, string childName)
        {
            Transform child = GetChildByName(transform, childName);
            return child?.lossyScale;
        }

        /// <summary>
        /// Flips the X scale
        /// </summary>
        public static void FlipX(this Transform transform)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }

        /// <summary>
        /// Flips the Y scale
        /// </summary>
        public static void FlipY(this Transform transform)
        {
            transform.localScale = new Vector3(transform.localScale.x, -transform.localScale.y, transform.localScale.z);
        }

        /// <summary>
        /// Flips the Z scale
        /// </summary>
        public static void FlipZ(this Transform transform)
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, -transform.localScale.z);
        }

        /// <summary>
        /// Gets the distance to another transform
        /// </summary>
        public static float DistanceTo(this Transform transform, Transform other)
        {
            return Vector3.Distance(transform.position, other.position);
        }

        /// <summary>
        /// Gets the direction to another transform
        /// </summary>
        public static Vector3 DirectionTo(this Transform transform, Transform other)
        {
            return (other.position - transform.position).normalized;
        }

        /// <summary>
        /// Checks if this transform is facing another transform within a cone angle
        /// </summary>
        public static bool IsFacing(this Transform transform, Transform target, float maxAngle = 45f)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, direction);
            return angle <= maxAngle;
        }
    }
}
