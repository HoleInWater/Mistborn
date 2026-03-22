using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 startPosition;
    public Vector3 endPosition;
    public float speed = 2f;
    public bool loop = true;
    public bool pingPong = false;
    
    private Vector3 currentTarget;
    private bool movingForward = true;
    
    void Start()
    {
        startPosition = transform.position;
        currentTarget = endPosition;
    }
    
    void Update()
    {
        MovePlatform();
    }
    
    void MovePlatform()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, speed * Time.deltaTime);
        
        if (transform.position == currentTarget)
        {
            if (pingPong)
            {
                movingForward = !movingForward;
                currentTarget = movingForward ? endPosition : startPosition;
            }
            else if (loop)
            {
                currentTarget = currentTarget == endPosition ? startPosition : endPosition;
            }
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(startPosition, 0.5f);
        Gizmos.DrawSphere(endPosition, 0.5f);
        Gizmos.DrawLine(startPosition, endPosition);
    }
}
