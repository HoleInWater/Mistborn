using UnityEngine;

public class IronPull : MonoBehaviour
{
    [Header("Settings")]
    public float pullForce = 500f;
    public float maxRange = 50f;
    public float metalCostPerSecond = 2f;
    
    [Header("References")]
    public Camera playerCamera;
    public LayerMask metalLayer;
    
    private float metalReserve = 100f;
    private bool isBurning = false;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartBurning();
        }
        
        if (Input.GetMouseButton(0) && isBurning)
        {
            PullMetals();
            DrainMetal();
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            StopBurning();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Iron - Pull ready");
    }
    
    void StopBurning()
    {
        isBurning = false;
        Debug.Log("Stopped burning Iron");
    }
    
    void PullMetals()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRange, metalLayer);
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.rigidbody != null)
            {
                Vector3 pullDirection = (playerCamera.transform.position - hit.point).normalized;
                hit.rigidbody.AddForce(pullDirection * pullForce * Time.deltaTime);
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
