// NOTE: Lines 47 and 57 contain Debug.Log which should be removed for production
using UnityEngine;

[PlayerComponent("Movement", order: 50)]
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
        if (Input.GetKeyDown(Keybinds.Jump) && !isVaulting)
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
            }
        }
    }
    
    void StartVault(Vector3 topPoint)
    {
        isVaulting  = true;
        vaultTarget = topPoint + transform.forward * 1.2f; // landing spot past the obstacle

        if (playerCamera == null) playerCamera = Camera.main;

        GetComponent<Animator>()?.SetTrigger("Vault");
        StartCoroutine(PerformVault(topPoint));
    }

    System.Collections.IEnumerator PerformVault(Vector3 topPoint)
    {
        rb.useGravity = false;

        Vector3 startPos  = rb.position;
        Vector3 landPos   = vaultTarget;
        const float upTime  = 0.2f;
        const float fwdTime = 0.2f;

        // Phase 1 — rise to the top of the obstacle
        for (float t = 0f; t < upTime; t += Time.deltaTime)
        {
            rb.MovePosition(Vector3.Lerp(startPos, topPoint, t / upTime));
            yield return null;
        }

        // Phase 2 — push forward to the landing spot
        for (float t = 0f; t < fwdTime; t += Time.deltaTime)
        {
            rb.MovePosition(Vector3.Lerp(topPoint, landPos, t / fwdTime));
            yield return null;
        }

        rb.MovePosition(landPos);
        rb.useGravity = true;
        isVaulting    = false;
    }
}
