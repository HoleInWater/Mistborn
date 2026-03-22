// NOTE: Lines 47 and 57 contain Debug.Log which should be removed for production
using UnityEngine;

public class VaultJump : MonoBehaviour
{
    [Header("Vault Settings")]
    public float vaultSpeed = 5f;
    public float vaultHeight = 2f;
    public float detectionRange = 2f;
    public LayerMask vaultableLayer;
    
    [Header("References")]
    public Camera playerCamera;
    public Rigidbody rb;
    
    private bool isVaulting = false;
    private Vector3 vaultTarget;
    
    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isVaulting)
        {
            TryVault();
        }
    }
    
    void TryVault()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        
        if (Physics.Raycast(origin, transform.forward, out hit, detectionRange, vaultableLayer))
        {
            float height = hit.transform.position.y - transform.position.y;
            
            if (height > 0.5f && height < vaultHeight)
            {
                StartVault(hit.point + Vector3.up * 0.1f);
            }
            else if (height >= vaultHeight)
            {
                Debug.Log("Object too high to vault");
            }
        }
    }
    
    void StartVault(Vector3 target)
    {
        isVaulting = true;
        vaultTarget = target;
        
        Debug.Log("Vaulting!");
        
        Invoke("EndVault", 0.5f);
    }
    
    void EndVault()
    {
        isVaulting = false;
    }
}
