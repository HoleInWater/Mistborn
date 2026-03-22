using UnityEngine;


using UnityEngine;
/// <summary>
/// Tag manager for common tags.
/// Attach this enum to GameObjects via tag dropdown.
/// </summary>
public static class Tags
{
    public const string Player = "Player";
    public const string Enemy = "Enemy";
    public const string Ground = "Ground";
    public const string Metal = "Metal";
    public const string Pickup = "Pickup";
    public const string Interactable = "Interactable";
    public const string Projectile = "Projectile";
    public const string Hazard = "Hazard";
    
    /// <summary>Check if object has any of the given tags</summary>
    public static bool HasAnyTag(this GameObject obj, params string[] tags)
    {
        foreach (string tag in tags)
        {
            if (obj.CompareTag(tag))
                return true;
        }
        return false;
    }
}

/// <summary>
/// Layer manager for common layers.
/// Use with: (1 << Layers.Metal)
/// </summary>
public static class Layers
{
    public const int Default = 0;
    public const int TransparentFX = 1;
    public const int IgnoreRaycast = 2;
    public const int Water = 4;
    public const int UI = 5;
    
    // Custom layers - assign in Unity Editor
    public const int Player = 8;
    public const int Enemy = 9;
    public const int Ground = 10;
    public const int Metal = 11;
    public const int Projectile = 12;
    public const int Hazard = 13;
    
    /// <summary>Check if object is on any of the given layers</summary>
    public static bool IsOnAnyLayer(this GameObject obj, params int[] layers)
    {
        foreach (int layer in layers)
        {
            if (obj.layer == layer)
                return true;
        }
        return false;
    }
}
