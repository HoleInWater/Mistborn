/// <summary>
/// Smooth follow camera with orbit controls.
/// Usage: Attach to camera, set target to player.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    // SETTINGS
    public Transform target;                  // Player to follow
    public float distance = 5f;             // Distance from target
    public float height = 2f;               // Height above target
    public float rotationSpeed = 5f;        // Mouse sensitivity
    public float followSpeed = 5f;         // How fast to follow
    
    // LIMITS
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    
    // INTERNAL
    private float currentYaw;
    private float currentPitch;
    private float currentDistance;
    
    void Start()
    {
        currentDistance = distance;
        
        // Initialize rotation
        Vector3 angles = transform.eulerAngles;
        currentYaw = angles.y;
        currentPitch = angles.x;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Get mouse input
        currentYaw += Input.GetAxis("Mouse X") * rotationSpeed;
        currentPitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
        
        // Zoom with scroll
        currentDistance -= Input.GetAxis("Mouse ScrollWheel") * 2f;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        
        // Calculate rotation
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        
        // Calculate position
        Vector3 offset = rotation * new Vector3(0, 0, -currentDistance);
        Vector3 targetPosition = target.position + Vector3.up * height + offset;
        
        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        
        // Look at target
        transform.LookAt(target.position + Vector3.up * height);
    }
    
    // Reset camera to behind player
    public void ResetToBehind()
    {
        currentYaw = target.eulerAngles.y;
        currentPitch = 20f;
    }
}
