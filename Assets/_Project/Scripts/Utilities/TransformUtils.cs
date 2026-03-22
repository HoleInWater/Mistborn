using UnityEngine;

namespace MistbornGame.Utilities
{
    public class TransformUtils : MonoBehaviour
    {
        /// <summary>
        /// Safely gets or adds a component to a GameObject
        /// </summary>
        public static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// Sets the layer of a GameObject and all its children
        /// </summary>
        public static void SetLayerRecursively(GameObject go, int layerNumber)
        {
            if (go == null)
                return;
                
            go.layer = layerNumber;
            
            foreach (Transform child in go.transform)
            {
                SetLayerRecursively(child.gameObject, layerNumber);
            }
        }

        /// <summary>
        /// Resets a transform's local position, rotation, and scale to defaults
        /// </br>
        /// (Position: 0,0,0, Rotation: 0,0,0, Scale: 1,1,1)
        /// </summary>
        public static void ResetTransform(Transform transform)
        {
            if (transform == null)
                return;
                
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Gets the bounds of a Renderer and all its children's renderers
        /// </summary>
        public static Bounds GetCombinedBounds(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(go.transform.position, Vector3.zero);
                
            Bounds combinedBounds = renderers[0].bounds;
            
            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }
            
            return combinedBounds;
        }

        /// <summary>
        /// Finds the closest point on a collider to a given point
        /// </summary>
        public static Vector3 ClosestPointOnCollider(Collider collider, Vector3 point)
        {
            if (collider == null)
                return point;
                
            return collider.ClosestPoint(point);
        }

        /// <summary>
        /// Checks if a point is inside a collider's bounds
        /// </summary>
        public static bool IsPointInCollider(Collider collider, Vector3 point)
        {
            if (collider == null)
                return false;
                
            return collider.bounds.Contains(point);
        }
    }
}