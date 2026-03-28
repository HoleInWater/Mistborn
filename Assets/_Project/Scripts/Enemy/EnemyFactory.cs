using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Builds a fully-functional enemy GameObject at runtime without any prefabs.
/// Used by EnemySpawner when its prefab array is empty, enabling testing
/// directly in a scene with only a NavMesh baked — no asset setup required.
///
/// Component stack created:
///   CapsuleCollider (from CreatePrimitive)
///   Rigidbody       — kinematic; NavMeshAgent controls movement
///   NavMeshAgent    — pathfinding
///   AIController    — MistbornRegistry marker + emotion (auto-added via EnemyAI RequireComponent)
///   EnemyAI         — full state machine
///   EnemyHealth     — health / IDamageable (synced from EnemyAI type defaults)
///   EnemyHitFlash   — red flash on damage
///   EnemySenses     — physics sight cone + hearing ring
/// </summary>
public static class EnemyFactory
{
    // Visual tint per enemy type so you can tell them apart in the test scene
    private static Color TypeColor(EnemyAI.EnemyType type)
    {
        switch (type)
        {
            case EnemyAI.EnemyType.Guard:          return new Color(0.55f, 0.55f, 0.6f);   // grey
            case EnemyAI.EnemyType.NobleGuard:     return new Color(0.3f,  0.3f,  0.7f);   // blue
            case EnemyAI.EnemyType.Coinshot:       return new Color(0.9f,  0.8f,  0.2f);   // gold
            case EnemyAI.EnemyType.Lurcher:        return new Color(0.2f,  0.7f,  0.9f);   // cyan
            case EnemyAI.EnemyType.Thug:           return new Color(0.7f,  0.2f,  0.2f);   // red
            case EnemyAI.EnemyType.Smoker:         return new Color(0.3f,  0.5f,  0.3f);   // dark green
            case EnemyAI.EnemyType.Rioter:         return new Color(0.9f,  0.4f,  0.0f);   // orange
            case EnemyAI.EnemyType.Seeker:         return new Color(0.7f,  0.7f,  0.1f);   // yellow
            case EnemyAI.EnemyType.Koloss:         return new Color(0.15f, 0.1f,  0.05f);  // dark brown
            case EnemyAI.EnemyType.SteelInquisitor:return new Color(0.1f,  0.1f,  0.15f);  // near black
            case EnemyAI.EnemyType.Mistwraith:     return new Color(0.5f,  0.5f,  0.6f);   // pale blue-grey
            case EnemyAI.EnemyType.Obligator:      return new Color(0.6f,  0.5f,  0.3f);   // tan
            case EnemyAI.EnemyType.SkaaRebel:      return new Color(0.4f,  0.25f, 0.15f);  // brown
            default:                               return Color.grey;
        }
    }

    // Scale per type (Koloss is huge; Mistwraith is shorter)
    private static Vector3 TypeScale(EnemyAI.EnemyType type)
    {
        switch (type)
        {
            case EnemyAI.EnemyType.Koloss:          return new Vector3(1.6f, 2.4f, 1.6f);
            case EnemyAI.EnemyType.SteelInquisitor: return new Vector3(1.1f, 1.3f, 1.1f);
            case EnemyAI.EnemyType.Mistwraith:      return new Vector3(0.9f, 0.7f, 0.9f);
            case EnemyAI.EnemyType.Thug:            return new Vector3(1.2f, 1.2f, 1.2f);
            default:                                return Vector3.one;
        }
    }

    /// <summary>
    /// Creates a fully functional enemy at <paramref name="position"/>.
    /// Returns the EnemyAI component for further configuration if needed.
    /// </summary>
    public static EnemyAI Create(EnemyAI.EnemyType type, Vector3 position,
                                  Quaternion rotation = default)
    {
        if (rotation == default) rotation = Quaternion.identity;

        // ── Base capsule ───────────────────────────────────────────────────
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = $"Enemy_{type}";
        go.transform.position = position;
        go.transform.rotation = rotation;
        go.transform.localScale = TypeScale(type);

        // Apply type colour to the capsule renderer
        Renderer rend = go.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(rend.sharedMaterial);
            mat.color = TypeColor(type);
            rend.material = mat;
        }

        // ── Physics ────────────────────────────────────────────────────────
        // Rigidbody must be kinematic; NavMeshAgent drives movement.
        // Non-kinematic + NavMeshAgent causes jitter.
        Rigidbody rb       = go.AddComponent<Rigidbody>();
        rb.isKinematic     = true;
        rb.useGravity      = false;
        rb.interpolation   = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        // ── NavMeshAgent ───────────────────────────────────────────────────
        NavMeshAgent agent = go.AddComponent<NavMeshAgent>();
        agent.radius       = 0.4f;
        agent.height       = 2f * TypeScale(type).y;
        agent.angularSpeed = 360f;
        agent.acceleration = 12f;
        agent.autoBraking  = true;

        // ── AI ─────────────────────────────────────────────────────────────
        // EnemyAI has [RequireComponent(typeof(AIController))] so AIController
        // is auto-added first when we AddComponent<EnemyAI>().
        EnemyAI ai       = go.AddComponent<EnemyAI>();
        ai.enemyType     = type;
        ai.navAgent      = agent;

        // ── Health ─────────────────────────────────────────────────────────
        go.AddComponent<EnemyHealth>();   // EnemyAI.Start() will sync maxHealth from type defaults

        // ── Visual feedback ────────────────────────────────────────────────
        go.AddComponent<EnemyHitFlash>();

        // ── Physics-based senses ───────────────────────────────────────────
        EnemySenses senses = go.AddComponent<EnemySenses>();
        // Exclude nothing by default so sight rays block on all static geometry
        // (user can adjust per-enemy in the Inspector)

        // ── Knockback support ──────────────────────────────────────────────
        EnemyKnockback knockback = go.AddComponent<EnemyKnockback>();
        knockback.navAgent = agent;

        // ── World-space health bar ─────────────────────────────────────────
        // Attach as a child so it follows the enemy.
        // EnemyHealthBarUI.Start() auto-creates its own Canvas + Images.
        GameObject hbGo = new GameObject("HealthBar");
        hbGo.transform.SetParent(go.transform, false);
        // Position above the capsule — height scales with type
        float hbHeight = agent.height + 0.4f;
        hbGo.transform.localPosition = new Vector3(0f, hbHeight, 0f);
        hbGo.AddComponent<EnemyHealthBarUI>();

        return ai;
    }
}
