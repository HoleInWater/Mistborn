using UnityEngine;

public class MetalSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject metalPrefab;
    public int maxMetals = 10;
    public float spawnRadius = 5f;
    
    void Update()
    {
        // Press T to spawn metal
        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnMetal();
        }
    }
    
    public void SpawnMetal()
    {
        // Count existing metals
        int metalCount = FindObjectsOfType<AllomanticTarget>().Length;
        if (metalCount >= maxMetals) return;
        
        // Random position around spawner
        Vector3 offset = Random.insideUnitSphere * spawnRadius;
        offset.y = Mathf.Abs(offset.y);
        Vector3 spawnPos = transform.position + offset;
        
        GameObject metal;
        
        if (metalPrefab != null)
        {
            metal = Instantiate(metalPrefab, spawnPos, Random.rotation);
        }
        else
        {
            // Create a default metal sphere
            metal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            metal.transform.position = spawnPos;
            metal.transform.localScale = Vector3.one * 0.2f; // Small coin size
            
            // Add AllomanticTarget
            AllomanticTarget target = metal.AddComponent<AllomanticTarget>();
            target.metalType = AllomancySkill.MetalType.Steel;
            target.canBePushed = true;
            target.canBePulled = true;
            target.mass = 0.01f; // 10 gram coin
            
            // Ensure Rigidbody exists
            Rigidbody rb = metal.GetComponent<Rigidbody>();
            if (rb == null) rb = metal.AddComponent<Rigidbody>();
            rb.mass = 0.01f;
            rb.useGravity = true;
            rb.drag = 0.1f;
        }
        
            Debug.Log($"[MetalSpawner] Spawned metal at {spawnPos} with AllomanticTarget");
    }
}
