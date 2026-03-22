using UnityEngine;

public class WallRun : MonoBehaviour
{
    [Header("Wall Run Settings")]
    public float wallRunSpeed = 10f;
    public float wallStickDistance = 2f;
    public float maxWallAngle = 60f;
    public float gravityMultiplier = 0.5f;
    public float jumpForce = 8f;
    public LayerMask wallLayer;
    
    [Header("References")]
    public Camera playerCamera;
    public Rigidbody rb;
    
    private bool isWallRunning = false;
    private bool isNearWall = false;
    private Vector3 wallNormal;
    
    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        CheckForWall();
        
        if (isNearWall && Input.GetKey(KeyCode.W))
        {
            StartWallRun();
        }
        
        if (isWallRunning)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                WallJump();
            }
        }
    }
    
    void CheckForWall()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.right, out hit, wallStickDistance, wallLayer) ||
            Physics.Raycast(transform.position, -transform.right, out hit, wallStickDistance, wallLayer))
        {
            isNearWall = true;
            wallNormal = hit.normal;
        }
        else
        {
            isNearWall = false;
        }
    }
    
    void StartWallRun()
    {
        if (isWallRunning) return;
        
        isWallRunning = true;
        Debug.Log("Wall running!");
        
        Invoke("EndWallRun", 2f);
    }
    
    void EndWallRun()
    {
        isWallRunning = false;
    }
    
    void WallJump()
    {
        isWallRunning = false;
        
        Vector3 jumpDir = wallNormal + Vector3.up;
        rb.linearVelocity = jumpDir * jumpForce;
        
        Debug.Log("Wall jump!");
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = isNearWall ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, transform.right * wallStickDistance);
        Gizmos.DrawRay(transform.position, -transform.right * wallStickDistance);
    }
}
