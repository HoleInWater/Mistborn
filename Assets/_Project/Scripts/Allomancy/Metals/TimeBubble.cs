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
    private bool registered = false;

    // Set by the creator (Bendalloy/Cadmium) before the bubble is active
    [HideInInspector] public Transform creator;

    void Awake()
    {
        bubbleCollider = GetComponent<SphereCollider>();
        if (bubbleCollider == null) bubbleCollider = gameObject.AddComponent<SphereCollider>();
        bubbleCollider.isTrigger = true;

        Renderer r = GetComponent<Renderer>();
        if (r != null) bubbleMaterial = r.material;
    }

    void Start()
    {
        // Register AFTER the creator has set the real timeScaleMultiplier value
        if (MistbornTimeManager.Instance != null)
        {
            MistbornTimeManager.Instance.RegisterBubbleModifier(timeScaleMultiplier);
            registered = true;
        }
    }

    void Update()
    {
        // Follow the creator so the bubble stays centered on the player
        if (creator != null)
            transform.position = creator.position;

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
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !affectedRigidbodies.Contains(rb))
        {
            affectedRigidbodies.Add(rb);
            rb.linearVelocity *= timeScaleMultiplier;
        }

        AIController ai = other.GetComponentInParent<AIController>();
        if (ai != null && !affectedAI.Contains(ai))
        {
            affectedAI.Add(ai);
            ai.externalTimeScaleMultiplier = timeScaleMultiplier;
        }

        BasicPlayerMove player = other.GetComponentInParent<BasicPlayerMove>();
        if (player != null && !affectedPlayers.Contains(player))
        {
            affectedPlayers.Add(player);
            player.externalSpeedMultiplier *= timeScaleMultiplier;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && affectedRigidbodies.Contains(rb))
        {
            if (timeScaleMultiplier > 0.001f) rb.linearVelocity /= timeScaleMultiplier;
            affectedRigidbodies.Remove(rb);
        }

        AIController ai = other.GetComponentInParent<AIController>();
        if (ai != null && affectedAI.Contains(ai))
        {
            ai.externalTimeScaleMultiplier = 1f;
            affectedAI.Remove(ai);
        }

        BasicPlayerMove player = other.GetComponentInParent<BasicPlayerMove>();
        if (player != null && affectedPlayers.Contains(player))
        {
            if (timeScaleMultiplier > 0.001f) player.externalSpeedMultiplier /= timeScaleMultiplier;
            affectedPlayers.Remove(player);
        }
    }

    public void Shutdown()
    {
        targetAlpha = 0f;

        foreach (var rb in affectedRigidbodies)
        {
            if (rb != null && timeScaleMultiplier > 0.001f) rb.linearVelocity /= timeScaleMultiplier;
        }
        foreach (var ai in affectedAI)
        {
            if (ai != null) ai.externalTimeScaleMultiplier = 1f;
        }
        foreach (var player in affectedPlayers)
        {
            if (player != null && timeScaleMultiplier > 0.001f) player.externalSpeedMultiplier /= timeScaleMultiplier;
        }

        if (registered && MistbornTimeManager.Instance != null)
            MistbornTimeManager.Instance.UnregisterBubbleModifier(timeScaleMultiplier);
        registered = false;

        Destroy(gameObject, 0.5f);
    }

    void OnDestroy()
    {
        if (registered && MistbornTimeManager.Instance != null)
            MistbornTimeManager.Instance.UnregisterBubbleModifier(timeScaleMultiplier);
    }
}
