using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Utilities
{
    public class TagsAndLayers : MonoBehaviour
    {
        [Header("Tags")]
        public static readonly string PlayerTag = "Player";
        public static readonly string EnemyTag = "Enemy";
        public static readonly string ProjectileTag = "Projectile";
        public static readonly string PickupTag = "Pickup";
        public static readonly string InteractableTag = "Interactable";
        
        [Header("Layers")]
        public const int DefaultLayer = 0;
        public const int TransparentFXLayer = 1;
        public const int IgnoreRaycastLayer = 2;
        public const int WaterLayer = 4;
        public const int UILayer = 5;
        
        public static bool HasTag(GameObject obj, string tag)
        {
            return obj.CompareTag(tag);
        }
        
        public static bool HasAnyTag(GameObject obj, params string[] tags)
        {
            foreach (string tag in tags)
            {
                if (obj.CompareTag(tag))
                    return true;
            }
            return false;
        }
        
        public static bool IsOnLayer(GameObject obj, int layer)
        {
            return obj.layer == layer;
        }
        
        public static bool IsOnAnyLayer(GameObject obj, params int[] layers)
        {
            foreach (int layer in layers)
            {
                if (obj.layer == layer)
                    return true;
            }
            return false;
        }
        
        public static void SetLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }
        
        public static LayerMask CreateLayerMask(params int[] layers)
        {
            LayerMask mask = 0;
            foreach (int layer in layers)
            {
                mask |= (1 << layer);
            }
            return mask;
        }
    }
    
    public class LayerMaskExtensions
    {
        public static bool Contains(LayerMask mask, int layer)
        {
            return (mask & (1 << layer)) != 0;
        }
        
        public static LayerMask AddLayer(LayerMask mask, int layer)
        {
            return mask | (1 << layer);
        }
        
        public static LayerMask RemoveLayer(LayerMask mask, int layer)
        {
            return mask & ~(1 << layer);
        }
        
        public static LayerMask ToggleLayer(LayerMask mask, int layer)
        {
            return mask ^ (1 << layer);
        }
    }
    
    public class TagLookup
    {
        private static Dictionary<string, List<GameObject>> taggedObjects = new Dictionary<string, List<GameObject>>();
        
        public static void RegisterObject(GameObject obj, string tag)
        {
            if (!taggedObjects.ContainsKey(tag))
            {
                taggedObjects[tag] = new List<GameObject>();
            }
            
            if (!taggedObjects[tag].Contains(obj))
            {
                taggedObjects[tag].Add(obj);
            }
        }
        
        public static void UnregisterObject(GameObject obj, string tag)
        {
            if (taggedObjects.ContainsKey(tag))
            {
                taggedObjects[tag].Remove(obj);
            }
        }
        
        public static List<GameObject> GetObjectsWithTag(string tag)
        {
            if (taggedObjects.ContainsKey(tag))
            {
                return new List<GameObject>(taggedObjects[tag]);
            }
            return new List<GameObject>();
        }
        
        public static GameObject FindClosestWithTag(string tag, Vector3 position)
        {
            List<GameObject> objects = GetObjectsWithTag(tag);
            
            GameObject closest = null;
            float closestDistance = float.MaxValue;
            
            foreach (GameObject obj in objects)
            {
                if (obj == null)
                    continue;
                
                float distance = Vector3.Distance(position, obj.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = obj;
                }
            }
            
            return closest;
        }
    }
}
