using UnityEngine;

/// <summary>
/// Pewter Allomancy — passively enhances strength, speed, jump, and endurance.
///
/// ACTIVATION: Left Ctrl (burn session) while Pewter is in primary or secondary slot.
/// SCALING:    All effects scale smoothly with FlareMultiplier (scroll wheel 1–10).
/// DRAIN:      Metal depletes each frame; effects stop when reserve hits 0.
///
/// EFFECTS:
///   Speed  — externalSpeedMultiplier on BasicPlayerMove (walk + sprint both scale)
///   Jump   — jumpVelocity on BasicPlayerMove scaled at burn time
///   Mass   — Rigidbody.mass slightly increased (lore: denser muscle)
///   Heal   — slow health regeneration while burning
/// </summary>
[PlayerComponent("Allomancy Metals", order: 30)]
public class Pewter : MonoBehaviour
{
    [Header("Speed & Jump")]
    [Tooltip("Speed multiplier at base burn (flare = 1)")]
    [Range(1f, 2f)]
    public float baseSpeedMultiplier = 1.25f;
    [Tooltip("Speed multiplier at max flare (flare = 10)")]
    [Range(1f, 3f)]
    public float maxSpeedMultiplier = 2f;

    [Tooltip("Jump multiplier at base burn")]
    [Range(1f, 2f)]
    public float baseJumpMultiplier = 1.35f;
    [Tooltip("Jump multiplier at max flare")]
    [Range(1f, 4f)]
    public float maxJumpMultiplier = 2.5f;

    [Header("Mass (Muscle Density)")]
    [Tooltip("Rigidbody mass multiplier at base burn")]
    [Range(1f, 1.5f)]
    public float baseMassMultiplier = 1.05f;
    [Tooltip("Rigidbody mass multiplier at max flare")]
    [Range(1f, 2f)]
    public float maxMassMultiplier = 1.4f;

    [Header("Mend (Healing)")]
    [Tooltip("Health restored per second at base burn")]
    public float baseHealRate = 0.5f;
    [Tooltip("Health restored per second at max flare")]
    public float maxHealRate = 3f;

    [Header("Drain")]
    [Tooltip("Metal reserve drained per second at base burn")]
    public float baseDrainPerSecond = 2f;
    [Tooltip("Drain multiplier at max flare (costs more to sustain a flare)")]
    [Range(1f, 5f)]
    public float flareDrainMultiplier = 3f;

    [Header("References")]
    public Allomancer allomancer;
    public BasicPlayerMove playerMove;

    // ── Private state ─────────────────────────────────────────────────────────

    private bool   isBurning          = false;
    private float  originalMass       = -1f;
    private float  originalJumpVelocity;
    private Rigidbody          playerRigidbody;
    private HealthBarTransitions healthSystem;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        if (allomancer == null) allomancer = GetComponentInParent<Allomancer>();
        if (playerMove  == null) playerMove  = GetComponentInParent<BasicPlayerMove>();
        healthSystem = GetComponentInParent<HealthBarTransitions>();

        if (playerMove != null)
        {
            playerRigidbody     = playerMove.GetComponent<Rigidbody>();
            originalJumpVelocity = playerMove.jumpVelocity;
            if (playerRigidbody != null)
                originalMass = playerRigidbody.mass;
        }
    }

    void Update()
    {
        bool wasBurning = isBurning;

        MetalSelector sel = allomancer?.GetComponent<MetalSelector>();
        bool pewterEquipped = sel == null   // no selector → fallback: treat as equipped
            || sel.GetPrimaryMetal()   == AllomancySkill.MetalType.Pewter
            || sel.GetSecondaryMetal() == AllomancySkill.MetalType.Pewter;

        isBurning = allomancer != null
                 && FlareManager.Instance != null && FlareManager.Instance.IsBurning
                 && pewterEquipped
                 && allomancer.GetMetalReserve(AllomancySkill.MetalType.Pewter) > 0;

        if (isBurning)
        {
            float flareMult = FlareManager.Instance.FlareMultiplier;
            ApplyEffects(flareMult);
            HandleHealing(flareMult);
            DrainReserve(flareMult);
        }
        else if (wasBurning)
        {
            ResetEffects();
        }
    }

    // ── Effects ───────────────────────────────────────────────────────────────

    void ApplyEffects(float flareMult)
    {
        if (playerMove == null) return;

        // t = 0 at base burn (flare 1), t = 1 at max flare (flare 10)
        float t = Mathf.Clamp01((flareMult - 1f) / 9f);

        playerMove.externalSpeedMultiplier = Mathf.Lerp(baseSpeedMultiplier, maxSpeedMultiplier, t);
        playerMove.jumpVelocity            = originalJumpVelocity * Mathf.Lerp(baseJumpMultiplier, maxJumpMultiplier, t);

        if (playerRigidbody != null && originalMass > 0f)
            playerRigidbody.mass = originalMass * Mathf.Lerp(baseMassMultiplier, maxMassMultiplier, t);
    }

    void ResetEffects()
    {
        if (playerMove != null)
        {
            playerMove.externalSpeedMultiplier = 1f;
            playerMove.jumpVelocity            = originalJumpVelocity;
        }
        if (playerRigidbody != null && originalMass > 0f)
            playerRigidbody.mass = originalMass;
    }

    void HandleHealing(float flareMult)
    {
        if (healthSystem == null) return;
        if (healthSystem.GetCurrentHealth() >= healthSystem.GetMaxHealth()) return;

        float t       = Mathf.Clamp01((flareMult - 1f) / 9f);
        float healRate = Mathf.Lerp(baseHealRate, maxHealRate, t);
        healthSystem.Heal(healRate * Time.deltaTime);
    }

    void DrainReserve(float flareMult)
    {
        if (allomancer == null) return;
        float t     = Mathf.Clamp01((flareMult - 1f) / 9f);
        float drain  = baseDrainPerSecond * Mathf.Lerp(1f, flareDrainMultiplier, t);
        allomancer.DrainMetal(AllomancySkill.MetalType.Pewter, drain * Time.deltaTime);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public bool IsBurningPewter() => isBurning;
}
