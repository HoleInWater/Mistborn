/// <summary>
/// Weapon holder that points toward camera look direction.
/// Usage: Attach to weapon attachment point.
/// </summary>
public class WeaponHolder : MonoBehaviour
{
    // SETTINGS
    public bool alwaysFaceCamera = true;
    public float rotationSmoothing = 10f;
    
    void Update()
    {
        if (alwaysFaceCamera && Camera.main != null)
        {
            // Point weapon toward where camera is looking
            Vector3 lookDir = Camera.main.transform.forward;
            lookDir.y = 0; // Keep horizontal
            
            if (lookDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothing * Time.deltaTime);
            }
        }
    }
}
