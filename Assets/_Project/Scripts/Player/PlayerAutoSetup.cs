using UnityEngine;

/// <summary>
/// Fixes Allomancy serialized values after all Awake() calls finish.
/// Unity caches old Inspector values — this forces the correct ones.
/// Must run in Start() so Allomancer.Awake() has already run
/// EnsureAllomancyComponents() and added SteelPush/IronPull/etc.
/// Add to Player GameObject. Only touches Allomancy components.
/// </summary>
[PlayerComponent("Core", order: 10)]
public class PlayerAutoSetup : MonoBehaviour
{
    void Start()
    {
        // Steel Push
        SteelPush push = GetComponentInChildren<SteelPush>();
        if (push != null)
        {
            push.pushSpeed              = 32f;
            push.maxRecoilSpeed         = 24f;
            push.loosePushForce         = 40f;
            push.pushCooldown           = 0.2f;
            push.maxRange               = 40f;
            push.minDistance            = 1f;
            push.inverseDistanceScaling = true;
            push.metalCostPerSecond     = 2f;
        }

        // Iron Pull
        IronPull pull = GetComponentInChildren<IronPull>();
        if (pull != null)
        {
            pull.pullSpeed              = 24f;
            pull.maxPullSpeed           = 20f;
            pull.loosePullForce         = 32f;
            pull.maxRange               = 40f;
            pull.minDistance            = 1f;
            pull.inverseDistanceScaling = true;
            pull.metalCostPerSecond     = 2f;
        }

        // Metal Line Renderer (Allomantic Sight)
        MetalLineRenderer mlr = GetComponentInChildren<MetalLineRenderer>();
        if (mlr != null)
        {
            mlr.maxRange = 80f;

            // Explicitly wire MetalSelector so the Iron/Steel gate works
            // regardless of Start() execution order.
            if (mlr.metalSelector == null)
                mlr.metalSelector = GetComponentInChildren<MetalSelector>();
        }

        // Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearDamping          = 0.5f;
            rb.angularDamping         = 5f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints            = RigidbodyConstraints.FreezeRotation;
            rb.sleepThreshold         = 0f;
        }
    }
}
