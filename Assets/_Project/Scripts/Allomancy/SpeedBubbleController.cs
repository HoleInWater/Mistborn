using UnityEngine;

public class SpeedBubbleController : MonoBehaviour
{
    [Header("Bubble Settings")]
    public float bubbleRadius = 3f;
    public float timeDilationRatio = 0.1f;
    public bool isActive = false;
    
    [Header("References")]
    public GameObject bubbleVisual;
    
    private float cachedTimeScale;
    private float cachedFixedDeltaTime;
    
    void Start()
    {
        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(false);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBubble();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        
        SpeedAffected entity = other.GetComponent<SpeedAffected>();
        if (entity != null)
        {
            entity.EnterBubble(timeDilationRatio);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        SpeedAffected entity = other.GetComponent<SpeedAffected>();
        if (entity != null)
        {
            entity.ExitBubble();
        }
    }
    
    public void ToggleBubble()
    {
        isActive = !isActive;
        
        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(isActive);
        }
        
        Debug.Log($"Speed Bubble {(isActive ? "activated" : "deactivated")}");
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isActive ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, bubbleRadius);
    }
}

public class SpeedAffected : MonoBehaviour
{
    private float originalTimeScale = 1f;
    private bool isInBubble = false;
    
    public void EnterBubble(float dilationRatio)
    {
        isInBubble = true;
        Time.timeScale = dilationRatio;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        Debug.Log($"{gameObject.name} entered speed bubble");
    }
    
    public void ExitBubble()
    {
        isInBubble = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        Debug.Log($"{gameObject.name} exited speed bubble");
    }
    
    void OnDestroy()
    {
        if (isInBubble)
        {
            ExitBubble();
        }
    }
}
