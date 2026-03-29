using UnityEngine;

/// <summary>
/// Fixes Allomancy serialized values on start.
/// Unity caches old Inspector values — this forces the correct ones.
/// Add to Player GameObject. Only touches Allomancy components.
/// </summary>
[DefaultExecutionOrder(-100)]
[PlayerComponent("Core", order: 10)]
public class PlayerAutoSetup : MonoBehaviour
{
    void Awake()
    {
        // Steel Push
        SteelPush push = GetComponent<SteelPush>();
        if (push != null)
        {
            // Scale: 2 Unity units = 5 feet (see WorldScale.cs)
            push.pushSpeed = 32f;        // ~80 ft/s launch off building
            push.maxRecoilSpeed = 24f;   // ~60 ft/s cap
            push.loosePushForce = 40f;   // Coin at ~100 ft/s
            push.pushCooldown = 0.2f;
            push.maxRange = 40f; // WorldScale: ~100 ft coin push range
            push.minDistance = 1f;
            push.inverseDistanceScaling = true;
            push.metalCostPerSecond = 2f;
        }

        // Iron Pull
        IronPull pull = GetComponent<IronPull>();
        if (pull != null)
        {
            // Scale: 2 Unity units = 5 feet (see WorldScale.cs)
            pull.pullSpeed = 24f;        // ~60 ft/s yank toward target
            pull.maxPullSpeed = 20f;     // ~50 ft/s cap
            pull.loosePullForce = 32f;   // Objects at ~80 ft/s toward player
            pull.maxRange = 40f; // WorldScale: ~100 ft pull range
            pull.minDistance = 1f;
            pull.inverseDistanceScaling = true;
            pull.metalCostPerSecond = 2f;
        }

        // Metal Line Renderer (Allomantic Sight)
        MetalLineRenderer mlr = GetComponent<MetalLineRenderer>();
        if (mlr != null)
        {
            mlr.maxRange = 80f; // WorldScale: ~200 ft metal sight range
            mlr.closestHighlightColor = new Color(0.1f, 0.15f, 0.5f);
        }

        // Rigidbody — only if it exists
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearDamping = 0.5f; // Low drag so push/pull carries you far
            rb.angularDamping = 5f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.sleepThreshold = 0f;
        }
    }
}
