using UnityEngine;
using System.Collections;

/// <summary>
/// Simple singleton to handle camera shake effects globally.
/// </summary>
public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }

    [Header("Settings")]
    public Transform cameraTransform;
    
    private Vector3 originalPos;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (cameraTransform == null) cameraTransform = Camera.main?.transform;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Shake(float duration, float magnitude)
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main?.transform;
            if (cameraTransform == null) return;
        }

        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        originalPos = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = 1f - (elapsed / duration); // Ease out
            cameraTransform.localPosition = originalPos + Random.insideUnitSphere * magnitude * strength;
            yield return null;
        }

        cameraTransform.localPosition = originalPos;
        shakeCoroutine = null;
    }
}
