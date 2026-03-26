using UnityEngine;

/// <summary>
/// Fixes Allomancy serialized values on start.
/// Unity caches old Inspector values — this forces the correct ones.
/// Add to Player GameObject. Only touches Allomancy components.
/// </summary>
[DefaultExecutionOrder(-100)]
public class PlayerAutoSetup : MonoBehaviour
{
    void Awake()
    {
        // Steel Push
        SteelPush push = GetComponent<SteelPush>();
        if (push != null)
        {
            push.pushSpeed = 60f;
            push.maxRecoilSpeed = 50f;
            push.loosePushForce = 80f;
            push.pushCooldown = 0.2f;
            push.maxRange = 30f;
            push.minDistance = 1f;
            push.inverseDistanceScaling = true;
            push.metalCostPerSecond = 2f;
        }

        // Iron Pull
        IronPull pull = GetComponent<IronPull>();
        if (pull != null)
        {
            pull.pullSpeed = 45f;
            pull.maxPullSpeed = 40f;
            pull.loosePullForce = 60f;
            pull.maxRange = 30f;
            pull.minDistance = 1f;
            pull.inverseDistanceScaling = true;
            pull.metalCostPerSecond = 2f;
        }

        // Metal Line Renderer (Allomantic Sight)
        MetalLineRenderer mlr = GetComponent<MetalLineRenderer>();
        if (mlr != null)
        {
            mlr.maxRange = 30f;
            mlr.closestHighlightColor = new Color(0.1f, 0.15f, 0.5f);
        }

        // Rigidbody — only if it exists
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearDamping = 2f;
            rb.angularDamping = 5f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.sleepThreshold = 0f;
        }
    }
}
