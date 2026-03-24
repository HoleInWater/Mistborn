using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the actual time-distortion effect within a Bendalloy or Cadmium bubble.
/// </summary>
public class TimeBubble : MonoBehaviour
{
    [Header("Settings")]
    public float timeScaleMultiplier = 1.0f;
    public float fadeSpeed = AllomancyConstants.BubbleFadeSpeed;

    private List<Rigidbody> affectedRigidbodies = new List<Rigidbody>();
    private List<AIController> affectedAI = new List<AIController>();
    private List<BasicPlayerMove> affectedPlayers = new List<BasicPlayerMove>();

    private SphereCollider bubbleCollider;
    private Material bubbleMaterial;
    private float targetAlpha = AllomancyConstants.BubbleAlpha;

    private float currentAlpha = 0f;

    void Awake()
    {
        bubbleCollider = GetComponent<SphereCollider>();
        if (bubbleCollider == null) bubbleCollider = gameObject.AddComponent<SphereCollider>();
        bubbleCollider.isTrigger = true;

        Renderer r = GetComponent<Renderer>();
        if (r != null) bubbleMaterial = r.material;

        if (MistbornTimeManager.Instance != null)
            MistbornTimeManager.Instance.RegisterBubbleModifier(timeScaleMultiplier);
    }

    void Update()
    {
        // Smoothly fade in/out the bubble visual
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        if (bubbleMaterial != null)
        {
            Color c = bubbleMaterial.color;
            bubbleMaterial.color = new Color(c.r, c.g, c.b, currentAlpha);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && !affectedRigidbodies.Contains(rb))
        {
            affectedRigidbodies.Add(rb);
            rb.linearVelocity *= timeScaleMultiplier;
        }

        AIController ai = other.GetComponent<AIController>();
        if (ai != null && !affectedAI.Contains(ai))
        {
            affectedAI.Add(ai);
            ai.externalTimeScaleMultiplier = timeScaleMultiplier;
        }

        BasicPlayerMove player = other.GetComponent<BasicPlayerMove>();
        if (player != null && !affectedPlayers.Contains(player))
        {
            affectedPlayers.Add(player);
            player.externalSpeedMultiplier *= timeScaleMultiplier;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && affectedRigidbodies.Contains(rb))
        {
            rb.linearVelocity /= timeScaleMultiplier;
            affectedRigidbodies.Remove(rb);
        }

        AIController ai = other.GetComponent<AIController>();
        if (ai != null && affectedAI.Contains(ai))
        {
            ai.externalTimeScaleMultiplier = 1f;
            affectedAI.Remove(ai);
        }

        BasicPlayerMove player = other.GetComponent<BasicPlayerMove>();
        if (player != null && affectedPlayers.Contains(player))
        {
            player.externalSpeedMultiplier /= timeScaleMultiplier;
            affectedPlayers.Remove(player);
        }
    }

    public void Shutdown()
    {
        targetAlpha = 0f;
        
        foreach (var rb in affectedRigidbodies)
        {
            if (rb != null) rb.linearVelocity /= timeScaleMultiplier;
        }
        foreach (var ai in affectedAI)
        {
            if (ai != null) ai.externalTimeScaleMultiplier = 1f;
        }
        foreach (var player in affectedPlayers)
        {
            if (player != null) player.externalSpeedMultiplier /= timeScaleMultiplier;
        }
        
        if (MistbornTimeManager.Instance != null)
            MistbornTimeManager.Instance.UnregisterBubbleModifier(timeScaleMultiplier);
            
        Destroy(gameObject, 0.5f);
    }

    void OnDestroy()
    {
        if (MistbornTimeManager.Instance != null)
            MistbornTimeManager.Instance.UnregisterBubbleModifier(timeScaleMultiplier);
    }
}
