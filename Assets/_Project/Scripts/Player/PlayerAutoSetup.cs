using UnityEngine;

/// <summary>
/// AUTO-SETUP: Add this ONE script to your Player GameObject.
/// It will add all required components with correct values.
/// Run once in Editor or at runtime. Fixes all serialized value issues.
///
/// HOW TO USE:
/// 1. Add this script to Player
/// 2. Click the checkbox "Run Setup" in Inspector
/// 3. Or just press Play — it runs in Awake
/// </summary>
[DefaultExecutionOrder(-100)] // Run before everything else
public class PlayerAutoSetup : MonoBehaviour
{
    [Header("Run this to fix all components")]
    public bool runSetup = true;

    void Awake()
    {
        if (runSetup)
        {
            SetupRigidbody();
            SetupCollider();
            AddAllComponents();
            FixSerializedValues();
            runSetup = false;
        }
    }

    void SetupRigidbody()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = 80f;
        rb.linearDamping = 2f;
        rb.angularDamping = 5f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.sleepThreshold = 0f;
    }

    void SetupCollider()
    {
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col == null) col = gameObject.AddComponent<CapsuleCollider>();

        col.height = 2f;
        col.radius = 0.4f;
        col.center = new Vector3(0, 1f, 0);
    }

    void AddAllComponents()
    {
        // Core
        EnsureComponent<Allomancer>();
        EnsureComponent<BasicPlayerMove>();
        EnsureComponent<PlayerHealth>();
        EnsureComponent<PlayerStamina>();
        EnsureComponent<PlayerExperience>();
        EnsureComponent<Inventory>();

        // Allomancy
        EnsureComponent<SteelPush>();
        EnsureComponent<IronPull>();
        EnsureComponent<MetalLineRenderer>();
        EnsureComponent<FlareManager>();
        EnsureComponent<MetalSelector>();
        EnsureComponent<CoinPouch>();
        EnsureComponent<AllomanticFlight>();
        EnsureComponent<EmotionalAllomancy>();
        EnsureComponent<CognitiveAllomancy>();
        EnsureComponent<TemporalAllomancy>();

        // Combat
        EnsureComponent<PlayerCombat>();
        EnsureComponent<BlockAbility>();
        EnsureComponent<DodgeRoll>();
        EnsureComponent<StatusEffects>();

        // Movement
        EnsureComponent<Sprint>();
        EnsureComponent<CrouchSystem>();
        EnsureComponent<ParkourSystem>();
        EnsureComponent<WallRun>();
        EnsureComponent<FallDamage>();
        EnsureComponent<GrappleSystem>();
        EnsureComponent<MovementExtras>();

        // Systems
        EnsureComponent<MetalVialSystem>();
        EnsureComponent<PlayerInteractor>();
        EnsureComponent<MetalMagnet>();
        EnsureComponent<CoinTrajectoryPreview>();
    }

    /// <summary>
    /// Force-set serialized values to code defaults.
    /// Unity caches old Inspector values even when code changes — this fixes that.
    /// </summary>
    void FixSerializedValues()
    {
        // Steel Push — force correct values
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
        }

        // Iron Pull — force correct values
        IronPull pull = GetComponent<IronPull>();
        if (pull != null)
        {
            pull.pullSpeed = 45f;
            pull.maxPullSpeed = 40f;
            pull.loosePullForce = 60f;
            pull.maxRange = 30f;
            pull.minDistance = 1f;
            pull.inverseDistanceScaling = true;
        }

        // Metal Line Renderer
        MetalLineRenderer mlr = GetComponent<MetalLineRenderer>();
        if (mlr != null)
        {
            mlr.maxRange = 30f;
            mlr.closestHighlightColor = new Color(0.1f, 0.15f, 0.5f);
        }

        // Player Health
        PlayerHealth hp = GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.maxHealth = 100f;
            hp.currentHealth = 100f;
        }

        // Player Stamina
        PlayerStamina stam = GetComponent<PlayerStamina>();
        if (stam != null)
        {
            stam.maxStamina = 100f;
            stam.currentStamina = 100f;
        }

        // Tag
        gameObject.tag = "Player";

        // Layer
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    T EnsureComponent<T>() where T : Component
    {
        T comp = GetComponent<T>();
        if (comp == null) comp = gameObject.AddComponent<T>();
        return comp;
    }
}
