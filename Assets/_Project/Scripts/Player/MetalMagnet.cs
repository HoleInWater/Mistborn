using UnityEngine;

public class MetalMagnet : MonoBehaviour
{
    [Header("Magnet Settings")]
    public float magnetRange = 3f;
    public float magnetStrength = 10f;
    public float metalCostPerSecond = 1f;
    public LayerMask metalLayer;
    
    [Header("References")]
    public Allomancer allomancer;
    
    private float metalReserve = 100f;
    private bool isMagnetActive = false;
    
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (!isMagnetActive)
            {
                isMagnetActive = true;
                Debug.Log("Metal Magnet Active");
            }
            
            AttractMetals();
            DrainMetal();
        }
        else
        {
            isMagnetActive = false;
        }
    }
    
    void AttractMetals()
    {
        Collider[] metals = Physics.OverlapSphere(transform.position, magnetRange, metalLayer);
        
        foreach (Collider metal in metals)
        {
            if (metal.attachedRigidbody != null)
            {
                Vector3 direction = (transform.position - metal.transform.position).normalized;
                metal.attachedRigidbody.AddForce(direction * magnetStrength * Time.deltaTime);
            }
        }
    }
    
    void DrainMetal()
    {
        metalReserve -= metalCostPerSecond * Time.deltaTime;
        if (metalReserve <= 0)
        {
            metalReserve = 0;
            isMagnetActive = false;
            Debug.Log("Metal depleted!");
        }
    }
    
    public float GetMetalReserve() => metalReserve;
}
