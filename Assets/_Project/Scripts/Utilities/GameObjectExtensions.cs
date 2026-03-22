using System.Collections.Generic;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Extension methods for GameObject class
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Gets a component of type T, or adds one if missing
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if (go == null) return null;
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// Gets a component of type T in children, including inactive objects
        /// </summary>
        public static T GetComponentInChildrenIncludingInactive<T>(this GameObject go) where T : Component
        {
            if (go == null) return null;
            return go.GetComponentInChildren<T>(true);
        }

        /// <summary>
        /// Gets all components of type T in children, including inactive objects
        /// </summary>
        public static T[] GetComponentsInChildrenIncludingInactive<T>(this GameObject go) where T : Component
        {
            if (go == null) return new T[0];
            return go.GetComponentsInChildren<T>(true);
        }

        /// <summary>
        /// Checks if a component of type T exists on this GameObject
        /// </summary>
        public static bool HasComponent<T>(this GameObject go) where T : Component
        {
            if (go == null) return false;
            return go.GetComponent<T>() != null;
        }

        /// <summary>
        /// Removes a component of type T from this GameObject
        /// </summary>
        public static bool RemoveComponent<T>(this GameObject go) where T : Component
        {
            if (go == null) return false;
            T component = go.GetComponent<T>();
            if (component != null)
            {
                Object.Destroy(component);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the layer of this GameObject and optionally all children
        /// </summary>
        public static void SetLayerRecursive(this GameObject go, int layer, bool includeInactive = false)
        {
            if (go == null) return;
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                if (child.gameObject.activeSelf || includeInactive)
                {
                    SetLayerRecursive(child.gameObject, layer, includeInactive);
                }
            }
        }

        /// <summary>
        /// Gets the first child with a specific name
        /// </summary>
        public static Transform GetChildByName(this GameObject go, string childName)
        {
            if (go == null) return null;
            foreach (Transform child in go.transform)
            {
                if (child.name == childName)
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all children of this GameObject
        /// </summary>
        public static List<Transform> GetAllChildren(this GameObject go)
        {
            List<Transform> children = new List<Transform>();
            if (go == null) return children;
            foreach (Transform child in go.transform)
            {
                children.Add(child);
            }
            return children;
        }

        /// <summary>
        /// Removes all children from this GameObject
        /// </summary>
        public static void RemoveAllChildren(this GameObject go)
        {
            if (go == null) return;
            for (int i = go.transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(go.transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Gets the distance to another GameObject
        /// </summary>
        public static float DistanceTo(this GameObject go, GameObject other)
        {
            if (go == null || other == null) return float.MaxValue;
            return Vector3.Distance(go.transform.position, other.transform.position);
        }

        /// <summary>
        /// Gets the direction to another GameObject
        /// </summary>
        public static Vector3 DirectionTo(this GameObject go, GameObject other)
        {
            if (go == null || other == null) return Vector3.zero;
            return (other.transform.position - go.transform.position).normalized;
        }

        /// <summary>
        /// Checks if this GameObject is within range of another
        /// </summary>
        public static bool IsWithinRange(this GameObject go, GameObject other, float range)
        {
            return DistanceTo(go, other) <= range;
        }

        /// <summary>
        /// Duplicates this GameObject
        /// </summary>
        public static GameObject Duplicate(this GameObject go, string newName = null)
        {
            if (go == null) return null;
            GameObject duplicate = Object.Instantiate(go);
            duplicate.name = newName ?? go.name + " (Clone)";
            return duplicate;
        }

        /// <summary>
        /// Sets the active state of this GameObject
        /// </summary>
        public static void SetActive(this GameObject go, bool active)
        {
            if (go != null)
            {
                go.SetActive(active);
            }
        }

        /// <summary>
        /// Toggles the active state of this GameObject
        /// </summary>
        public static void ToggleActive(this GameObject go)
        {
            if (go != null)
            {
                go.SetActive(!go.activeSelf);
            }
        }

        /// <summary>
        /// Sets the tag of this GameObject
        /// </summary>
        public static void SetTag(this GameObject go, string tag)
        {
            if (go != null)
            {
                go.tag = tag;
            }
        }

        /// <summary>
        /// Checks if this GameObject has a specific tag
        /// </summary>
        public static bool HasTag(this GameObject go, string tag)
        {
            if (go == null) return false;
            return go.CompareTag(tag);
        }
    }
}
