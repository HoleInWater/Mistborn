using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;
    public int coinCount = 10;
    public Vector3 spawnArea = new Vector3(10, 2, 10);
    public Vector3 spawnCenter = Vector3.zero;
    
    void Start()
    {
        SpawnCoins();
    }
    
    void SpawnCoins()
    {
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 randomPos = spawnCenter + new Vector3(
                Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                Random.Range(0, spawnArea.y),
                Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
            );
            
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coin.name = $"TestCoin_{i}";
            coin.transform.position = randomPos;
            coin.transform.localScale = new Vector3(0.05f, 0.05f, 0.01f);
            
            coin.layer = LayerMask.NameToLayer("Metal");
            
            Rigidbody rb = coin.AddComponent<Rigidbody>();
            rb.mass = 0.01f;
            rb.drag = 0.5f;
            rb.angularDrag = 0.5f;
            
            AllomanticTarget target = coin.AddComponent<AllomanticTarget>();
            target.canBePushed = true;
            target.canBePulled = true;
            target.isAnchored = false;
            target.metalType = AllomancySkill.MetalType.Steel;
            target.mass = 0.01f;
            
            Renderer renderer = coin.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = new Color(0.8f, 0.7f, 0.2f);
            }
        }
    }
}