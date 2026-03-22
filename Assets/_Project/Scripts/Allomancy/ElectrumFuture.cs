// NOTE: Lines 35 and 42 contain Debug.Log which should be removed for production
using UnityEngine;

public class ElectrumFuture : MonoBehaviour
{
    [Header("Settings")]
    public float metalCostPerSecond = 8f;
    public float ghostDuration = 3f;
    
    private float metalReserve = 100f;
    private bool isBurning = false;
    private GameObject futureSelf;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            StartBurning();
        }
        
        if (Input.GetKey(KeyCode.Alpha5) && isBurning)
        {
            ShowFuture();
            DrainMetal();
        }
        
        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            StopBurning();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Electrum - Seeing your future!");
    }
    
    void StopBurning()
    {
        isBurning = false;
        ClearFuture();
        Debug.Log("Stopped burning Electrum");
    }
    
    void ShowFuture()
    {
        if (futureSelf != null) return;
        
        futureSelf = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        futureSelf.transform.position = transform.position + transform.forward * 2f;
        futureSelf.transform.rotation = transform.rotation;
        futureSelf.name = "ElectrumGhost";
        
        Renderer r = futureSelf.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Standard"));
        r.material.color = new Color(1f, 1f, 0f, 0.5f);
        
        Destroy(futureSelf.GetComponent<Collider>());
    }
    
    void ClearFuture()
    {
        if (futureSelf != null)
        {
            Destroy(futureSelf);
            futureSelf = null;
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
        ClearFuture();
    }
}
