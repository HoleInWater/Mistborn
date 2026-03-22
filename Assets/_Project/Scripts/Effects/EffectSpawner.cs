using UnityEngine;
/// <summary>
/// Simple particle effect spawner.
/// Usage: EffectSpawner.Spawn("metal_pickup", position);
/// </summary>
public class EffectSpawner : MonoBehaviour
{
    public static EffectSpawner Instance { get; private set; }
    
    // EFFECT PREFABS - Assign in Inspector
    public GameObject metalPickupEffect;
    public GameObject metalPushEffect;
    public GameObject metalPullEffect;
    public GameObject dashEffect;
    public GameObject rollEffect;
    public GameObject damageEffect;
    public GameObject healEffect;
    public GameObject checkpointEffect;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    // Spawn by name
    public static void Spawn(string effectName, Vector3 position, Quaternion rotation = default)
    {
        if (Instance == null) return;
        
        GameObject prefab = null;
        
        switch (effectName.ToLower())
        {
            case "metal_pickup": prefab = Instance.metalPickupEffect; break;
            case "metal_push": prefab = Instance.metalPushEffect; break;
            case "metal_pull": prefab = Instance.metalPullEffect; break;
            case "dash": prefab = Instance.dashEffect; break;
            case "roll": prefab = Instance.rollEffect; break;
            case "damage": prefab = Instance.damageEffect; break;
            case "heal": prefab = Instance.healEffect; break;
            case "checkpoint": prefab = Instance.checkpointEffect; break;
        }
        
        if (prefab != null)
        {
            Instantiate(prefab, position, rotation);
        }
    }
    
    // Specific spawn methods
    public static void MetalPickup(Vector3 position)
    {
        Spawn("metal_pickup", position);
    }
    
    public static void Dash(Vector3 position)
    {
        Spawn("dash", position);
    }
    
    public static void Damage(Vector3 position)
    {
        Spawn("damage", position);
    }
    
    public static void Heal(Vector3 position)
    {
        Spawn("heal", position);
    }
    
    public static void Checkpoint(Vector3 position)
    {
        Spawn("checkpoint", position);
    }
}
