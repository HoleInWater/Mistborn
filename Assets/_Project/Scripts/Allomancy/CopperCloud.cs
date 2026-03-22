// NOTE: Lines 47 and 54 contain Debug.Log which should be removed for production
using UnityEngine;

public class CopperCloud : MonoBehaviour
{
    [Header("Settings")]
    public float cloudRadius = 15f;
    public float metalCostPerSecond = 4f;
    public float visualRadius = 5f;
    
    [Header("References")]
    public Transform playerTransform;
    
    private float metalReserve = 100f;
    private bool isBurning = false;
    private bool cloudActive = false;
    private GameObject cloudEffect;
    
    void Start()
    {
        if (playerTransform == null)
            playerTransform = transform;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartBurning();
        }
        
        if (Input.GetKey(KeyCode.C) && isBurning)
        {
            MaintainCloud();
            DrainMetal();
        }
        
        if (Input.GetKeyUp(KeyCode.C))
        {
            StopBurning();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        CreateCloud();
        Debug.Log("Burning Copper - Hiding Allomantic pulses!");
    }
    
    void StopBurning()
    {
        isBurning = false;
        DestroyCloud();
        Debug.Log("Stopped burning Copper");
    }
    
    void CreateCloud()
    {
        if (cloudEffect != null) Destroy(cloudEffect);
        
        cloudEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cloudEffect.transform.position = playerTransform.position;
        cloudEffect.transform.localScale = Vector3.one * cloudRadius * 2;
        cloudEffect.name = "CopperCloud";
        
        Renderer renderer = cloudEffect.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = new Color(0.3f, 0.2f, 0.5f, 0.2f);
        renderer.material.SetFloat("_Mode", 3);
        
        Destroy(cloudEffect.GetComponent<Collider>());
        cloudActive = true;
    }
    
    void MaintainCloud()
    {
        if (cloudEffect != null)
        {
            cloudEffect.transform.position = playerTransform.position;
        }
    }
    
    void DestroyCloud()
    {
        if (cloudEffect != null)
        {
            Destroy(cloudEffect);
            cloudEffect = null;
        }
        cloudActive = false;
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
    public bool IsCloudActive() => cloudActive;
}
