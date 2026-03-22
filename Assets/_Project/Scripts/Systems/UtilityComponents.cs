using UnityEngine;


using UnityEngine;
/// <summary>
/// Auto-destroy component after time.
/// Usage: gameObject.AddComponent<AutoDestroy>().lifetime = 5f;
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 3f;
    
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}

/// <summary>
/// Auto-disable component after time.
/// Usage: gameObject.AddComponent<AutoDisable>().delay = 2f;
/// </summary>
public class AutoDisable : MonoBehaviour
{
    public float delay = 2f;
    
    void Start()
    {
        Invoke(nameof(DisableObject), delay);
    }
    
    void DisableObject()
    {
        gameObject.SetActive(false);
    }
}

/// <summary>
/// Look at target smoothly.
/// Usage: gameObject.AddComponent<SmoothLookAt>().target = playerTransform;
/// </summary>
public class SmoothLookAt : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 5f;
    
    void Update()
    {
        if (target == null) return;
        
        Vector3 lookDir = target.position - transform.position;
        lookDir.y = 0;
        
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}

/// <summary>
/// Rotate object continuously.
/// Usage: gameObject.AddComponent<ContinuousRotate>().rotationSpeed = 90f;
/// </summary>
public class ContinuousRotate : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 90, 0);
    
    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}

/// <summary>
/// Bob up and down.
/// Usage: gameObject.AddComponent<Bob>().bobAmount = 0.5f;
/// </summary>
public class Bob : MonoBehaviour
{
    public float bobSpeed = 2f;
    public float bobAmount = 0.5f;
    public float startY;
    
    void Start()
    {
        startY = transform.position.y;
    }
    
    void Update()
    {
        float y = startY + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}
