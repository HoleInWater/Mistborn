/// <summary>
/// Extension methods for common operations.
/// Usage: vector.Normalize2D() or transform.GetOrAddComponent<T>()
/// </summary>
public static class Extensions
{
    // Vector3 extensions
    public static Vector3 Normalize2D(this Vector3 v)
    {
        v.y = 0;
        return v.normalized;
    }
    
    public static float Magnitude2D(this Vector3 v)
    {
        v.y = 0;
        return v.magnitude;
    }
    
    public static Vector3 WithY(this Vector3 v, float y)
    {
        return new Vector3(v.x, y, v.z);
    }
    
    public static Vector3 WithX(this Vector3 v, float x)
    {
        return new Vector3(x, v.y, v.z);
    }
    
    public static Vector3 WithZ(this Vector3 v, float z)
    {
        return new Vector3(v.x, v.y, z);
    }
    
    // Transform extensions
    public static T GetOrAddComponent<T>(this Component component) where T : Component
    {
        return component.gameObject.GetOrAddComponent<T>();
    }
    
    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        if (component == null)
        {
            component = obj.AddComponent<T>();
        }
        return component;
    }
    
    // Component extensions
    public static bool TryGetComponent<T>(this Component component, out T result) where T : class
    {
        result = component.GetComponent<T>();
        return result != null;
    }
    
    // Float extensions
    public static bool Approximately(this float a, float b, float threshold = 0.01f)
    {
        return Mathf.Abs(a - b) < threshold;
    }
    
    // Array extensions
    public static T RandomElement<T>(this T[] array)
    {
        if (array == null || array.Length == 0) return default(T);
        return array[Random.Range(0, array.Length)];
    }
    
    public static T RandomElement<T>(this System.Collections.Generic.List<T> list)
    {
        if (list == null || list.Count == 0) return default(T);
        return list[Random.Range(0, list.Count)];
    }
    
    // LayerMask extensions
    public static bool Contains(this LayerMask mask, int layer)
    {
        return (mask.value & (1 << layer)) != 0;
    }
    
    // GameObject extensions
    public static void SetLayerRecursively(this GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            child.gameObject.SetLayerRecursively(layer);
        }
    }
}
