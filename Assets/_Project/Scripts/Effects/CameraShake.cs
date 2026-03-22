using UnityEngine;

/// <summary>
/// Camera shake effect.
/// Usage: CameraShake.Shake(0.5f, 1f); // Medium shake for 1 second
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    
    // SETTINGS
    public float shakeAmount = 0.1f;
    public float shakeDuration = 0.5f;
    
    // INTERNAL
    private Vector3 originalPosition;
    private float currentShakeDuration;
    private float currentShakeAmount;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            originalPosition = transform.localPosition;
        }
    }
    
    void Update()
    {
        if (currentShakeDuration > 0)
        {
            // Random shake offset
            Vector3 shakeOffset = Random.insideUnitSphere * currentShakeAmount;
            transform.localPosition = originalPosition + shakeOffset;
            
            currentShakeDuration -= Time.deltaTime;
        }
        else
        {
            // Reset position
            transform.localPosition = originalPosition;
        }
    }
    
    // Trigger shake
    public static void Shake(float amount, float duration)
    {
        if (Instance != null)
        {
            Instance.currentShakeAmount = amount;
            Instance.currentShakeDuration = duration;
        }
    }
    
    // Quick shake
    public static void QuickShake()
    {
        Shake(0.2f, 0.2f);
    }
    
    // Heavy shake (damage, explosion)
    public static void HeavyShake()
    {
        Shake(0.5f, 0.5f);
    }
    
    // Light shake (footstep, landing)
    public static void LightShake()
    {
        Shake(0.05f, 0.1f);
    }
}
