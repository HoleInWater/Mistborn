using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for GameObject operations
    /// </summary>
    public static class GameObjectUtils
    {
        /// <summary>
        /// Finds a GameObject by name in the scene
        /// </summary>
        public static GameObject FindByName(string name)
        {
            return GameObject.Find(name);
        }

        /// <summary>
        /// Finds all GameObjects with a specific tag
        /// </summary>
        public static GameObject[] FindByTag(string tag)
        {
            return GameObject.FindGameObjectsWithTag(tag);
        }

        /// <summary>
        /// Creates an empty GameObject with a given name
        /// </summary>
        public static GameObject CreateEmpty(string name = "New GameObject")
        {
            return new GameObject(name);
        }

        /// <summary>
        /// Creates a GameObject with a specific component
        /// </summary>
        public static GameObject CreateWithComponent<T>(string name = "New GameObject") where T : Component
        {
            GameObject go = new GameObject(name);
            go.AddComponent<T>();
            return go;
        }

        /// <summary>
        /// Sets the layer of a GameObject and optionally all its children
        /// </summary>
        public static void SetLayerRecursive(GameObject go, int layer, bool includeInactive = false)
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
        /// Sets the active state of a GameObject
        /// </summary>
        public static void SetActive(GameObject go, bool active)
        {
            if (go != null)
            {
                go.SetActive(active);
            }
        }

        /// <summary>
        /// Gets the first child with a specific name
        /// </summary>
        public static Transform GetChildByName(GameObject parent, string childName)
        {
            if (parent == null) return null;
            
            foreach (Transform child in parent.transform)
            {
                if (child.name == childName)
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all children of a GameObject
        /// </summary>
        public static List<Transform> GetAllChildren(GameObject parent)
        {
            List<Transform> children = new List<Transform>();
            if (parent == null) return children;
            
            foreach (Transform child in parent.transform)
            {
                children.Add(child);
            }
            return children;
        }

        /// <summary>
        /// Removes all children from a GameObject
        /// </summary>
        public static void RemoveAllChildren(GameObject parent)
        {
            if (parent == null) return;
            
            for (int i = parent.transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(parent.transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Gets the distance between two GameObjects
        /// </summary>
        public static float GetDistance(GameObject a, GameObject b)
        {
            if (a == null || b == null) return float.MaxValue;
            return Vector3.Distance(a.transform.position, b.transform.position);
        }

        /// <summary>
        /// Gets the direction from one GameObject to another
        /// </summary>
        public static Vector3 GetDirection(GameObject from, GameObject to)
        {
            if (from == null || to == null) return Vector3.zero;
            return (to.transform.position - from.transform.position).normalized;
        }

        /// <summary>
        /// Checks if a GameObject is within range of another
        /// </summary>
        public static bool IsWithinRange(GameObject a, GameObject b, float range)
        {
            return GetDistance(a, b) <= range;
        }

        /// <summary>
        /// Duplicates a GameObject
        /// </summary>
        public static GameObject Duplicate(GameObject original, string newName = null)
        {
            if (original == null) return null;
            
            GameObject duplicate = Object.Instantiate(original);
            duplicate.name = newName ?? original.name + " (Clone)";
            return duplicate;
        }
    }
}
