using UnityEngine;
using System.Collections.Generic;

public class TimeBubble : MonoBehaviour
{
    [Header("Settings")]
    public float bubbleRadius = 5f;
    public float speedInsideBubble = 0.25f;
    
    private GameObject bubbleEffect;
    private bool isInsideBubble = false;
    private float originalTimeScale = 1f;
    
    public enum BubbleType { SpeedUp, SlowDown }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8))
        {
            CreateBubble(BubbleType.SpeedUp);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9))
        {
            CreateBubble(BubbleType.SlowDown);
        }
    }
    
    void CreateBubble(BubbleType bubbleType)
    {
        if (bubbleEffect != null) Destroy(bubbleEffect);
        
        bubbleEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bubbleEffect.transform.localScale = Vector3.one * bubbleRadius * 2;
        bubbleEffect.name = "TimeBubble";
        
        Renderer r = bubbleEffect.GetComponent<Renderer>();
        if (bubbleType == BubbleType.SpeedUp)
        {
            r.material.color = new Color(1f, 0.8f, 0.2f, 0.2f);
        }
        else
        {
            r.material.color = new Color(0.2f, 0.5f, 1f, 0.2f);
        }
        
        Destroy(bubbleEffect.GetComponent<Collider>());
    }
    
    void OnDestroy()
    {
        if (bubbleEffect != null)
            Destroy(bubbleEffect);
    }
}
