using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the actual time-distortion effect within a Bendalloy or Cadmium bubble.
/// </summary>
public class TimeBubble : MonoBehaviour
{
    [Header("Settings")]
    public float timeScaleMultiplier = 1.0f;
    public float fadeSpeed = MetallurgyConstants.BubbleFadeSpeed;

    private List<AIController> affectedAI = new List<AIController>();

    private SphereCollider bubbleCollider;
    private Material bubbleMaterial;
    private float targetAlpha = MetallurgyConstants.BubbleAlpha;
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
        if (AshwalkerTimeManager.Instance != null)
        {
            AshwalkerTimeManager.Instance.RegisterBubbleModifier(timeScaleMultiplier);
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
        // Rigidbody velocity and player externalSpeedMultiplier are intentionally NOT
        // touched here. AshwalkerTimeManager already changes Time.timeScale and
        // Time.fixedDeltaTime globally when this bubble is registered, so physics
        // objects and player movement are naturally sped/slowed by the engine.
        // Multiplying velocity directly on top would double the effect.

        // AI action-timing loops multiply their own deltaTime by externalTimeScaleMultiplier,
        // which is separate from the physics step — so we still need to set it here.
        AIController ai = other.GetComponentInParent<AIController>();
        if (ai != null && !affectedAI.Contains(ai))
        {
            affectedAI.Add(ai);
            ai.externalTimeScaleMultiplier = timeScaleMultiplier;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        AIController ai = other.GetComponentInParent<AIController>();
        if (ai != null && affectedAI.Contains(ai))
        {
            ai.externalTimeScaleMultiplier = 1f;
            affectedAI.Remove(ai);
        }
    }

    public void Shutdown()
    {
        targetAlpha = 0f;

        foreach (var ai in affectedAI)
        {
            if (ai != null) ai.externalTimeScaleMultiplier = 1f;
        }
        affectedAI.Clear();

        if (registered && AshwalkerTimeManager.Instance != null)
            AshwalkerTimeManager.Instance.UnregisterBubbleModifier(timeScaleMultiplier);
        registered = false;

        Destroy(gameObject, 0.5f);
    }

    void OnDestroy()
    {
        if (registered && AshwalkerTimeManager.Instance != null)
            AshwalkerTimeManager.Instance.UnregisterBubbleModifier(timeScaleMultiplier);
    }
}
