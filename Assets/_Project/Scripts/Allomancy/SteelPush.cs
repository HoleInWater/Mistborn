using UnityEngine;

public class SteelPush : MonoBehaviour
{
    [Header("Settings")]
    public float pushForce = 500f;
    public float maxRange = 50f;
    public float metalCostPerSecond = 2f;
    
    [Header("References")]
    public Camera playerCamera;
    public LayerMask metalLayer;
    
    private float metalReserve = 100f;
    private bool isBurning = false;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StartBurning();
        }
        
        if (Input.GetMouseButton(1) && isBurning)
        {
            PushMetals();
            DrainMetal();
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            StopBurning();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Steel - Push ready");
    }
    
    void StopBurning()
    {
        isBurning = false;
        Debug.Log("Stopped burning Steel");
    }
    
    void PushMetals()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRange, metalLayer);
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.rigidbody != null)
            {
                Vector3 pushDirection = (hit.point - playerCamera.transform.position).normalized;
                hit.rigidbody.AddForce(pushDirection * pushForce * Time.deltaTime);
            }
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
    public void RefillMetal(float amount) => metalReserve = Mathf.Min(metalReserve + amount, 100f);
}
