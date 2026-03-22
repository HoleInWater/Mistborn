using UnityEngine;

public class GoldPast : MonoBehaviour
{
    [Header("Settings")]
    public float metalCostPerSecond = 8f;
    public float ghostDuration = 3f;
    
    private float metalReserve = 100f;
    private bool isBurning = false;
    private GameObject pastSelf;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartBurning();
        }
        
        if (Input.GetKey(KeyCode.G) && isBurning)
        {
            ShowPast();
            DrainMetal();
        }
        
        if (Input.GetKeyUp(KeyCode.G))
        {
            StopBurning();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Gold - Seeing the past!");
    }
    
    void StopBurning()
    {
        isBurning = false;
        ClearPast();
        Debug.Log("Stopped burning Gold");
    }
    
    void ShowPast()
    {
        if (pastSelf != null) return;
        
        pastSelf = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        pastSelf.transform.position = transform.position;
        pastSelf.transform.rotation = transform.rotation;
        pastSelf.name = "GoldGhost";
        
        Renderer r = pastSelf.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Standard"));
        r.material.color = new Color(1f, 0.8f, 0.4f, 0.5f);
        
        Destroy(pastSelf.GetComponent<Collider>());
    }
    
    void ClearPast()
    {
        if (pastSelf != null)
        {
            Destroy(pastSelf);
            pastSelf = null;
        }
    }
    
    void DrainMetal()
    {
        metalReserve -= metalCostPerSecond * Time.deltaTime;
        if (metalReserve <= 0)
        {
            metalReserve = 0;
            StopBurning();
        }
    }
    
    public float GetMetalReserve() => metalReserve;
    
    void OnDestroy()
    {
        ClearPast();
    }
}
