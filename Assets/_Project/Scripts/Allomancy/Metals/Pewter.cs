using UnityEngine;

/// <summary>
/// Pewter Allomancy (Thug / Pewterarm) — lore-accurate physical enhancement.
///
/// LORE (Coppermind):
///   Doubles strength at normal burn, triples when flared.
///   Enhances speed, balance, reaction time, durability, and endurance.
///   Grants accelerated healing. Fastest-burning of the 8 basic metals.
///   PEWTER DRAG CRASH: Stopping after extended flaring causes a sudden surge
///   of suppressed fatigue — can be lethal. Implemented as a health/speed penalty.
///
/// ACTIVATION: Left Ctrl (burn session) + E (primary) or Q (secondary) to toggle.
/// SCALING:    All effects scale smoothly with FlareMultiplier (scroll wheel 1–10).
/// PUSH:       F (primary) / V (secondary) — forward lunge + enemy knockback.
/// </summary>
[PlayerComponent("Allomancy Metals", order: 30)]
public class Pewter : MonoBehaviour
{
    [Header("Strength")]
    [Tooltip("Damage multiplier at base burn (~2x per lore)")]
    [Range(1f, 3f)]
    public float baseStrengthMultiplier = 2f;
    [Tooltip("Damage multiplier at max flare (~3x per lore)")]
    [Range(1f, 5f)]
    public float maxStrengthMultiplier = 3f;

    [Header("Speed & Jump")]
    [Tooltip("Speed multiplier at base burn")]
    [Range(1f, 2f)]
    public float baseSpeedMultiplier = 1.5f;
    [Tooltip("Speed multiplier at max flare (pewter dragging pace — near horse speed)")]
    [Range(1f, 3f)]
    public float maxSpeedMultiplier = 2.2f;

    [Tooltip("Jump multiplier at base burn")]
    [Range(1f, 2f)]
    public float baseJumpMultiplier = 1.5f;
    [Tooltip("Jump multiplier at max flare")]
    [Range(1f, 4f)]
    public float maxJumpMultiplier = 3f;

    [Header("Mass (Muscle Density)")]
    [Tooltip("Rigidbody mass multiplier at base burn")]
    [Range(1f, 1.5f)]
    public float baseMassMultiplier = 1.1f;
    [Tooltip("Rigidbody mass multiplier at max flare")]
    [Range(1f, 2f)]
    public float maxMassMultiplier = 1.5f;

    [Header("Mend (Healing)")]
    [Tooltip("Health restored per second at base burn")]
    public float baseHealRate = 1f;
    [Tooltip("Health restored per second at max flare")]
    public float maxHealRate = 5f;

    [Header("Drain — Fastest of 8 Basic Metals")]
    [Tooltip("Metal reserve drained per second at base burn. MAG: 5 in-game min = 75 real sec → 1.33/s")]
    public float baseDrainPerSecond = 1.33f;
    [Tooltip("Drain multiplier at max flare. Pewter dragging burns 6x faster than base.")]
    [Range(1f, 8f)]
    public float flareDrainMultiplier = 6f;

    [Header("Pewter Drag Crash")]
    [Tooltip("Seconds of continuous flaring before crash risk begins. Lore: 'long time' — 30s minimum.")]
    public float dragCrashThreshold = 30f;
    [Tooltip("Health damage dealt on crash (reserve runs out after dragging)")]
    public float crashDamage = 30f;
    [Tooltip("Speed penalty multiplier during crash recovery")]
    [Range(0.1f, 0.9f)]
    public float crashSpeedPenalty = 0.4f;
    [Tooltip("How long the crash exhaustion lasts")]
    public float crashDuration = 5f;

    [Header("Pewter Push (F / V)")]
    [Tooltip("Forward lunge speed applied to the player")]
    public float pushLungeForce = 18f;
    [Tooltip("Knockback force applied to enemies in front")]
    public float pushKnockbackForce = 25f;
    [Tooltip("Radius of the knockback sphere in front of the player")]
    public float pushRange = 2.5f;
    [Tooltip("Seconds between pushes")]
    public float pushCooldown = 0.6f;
    [Tooltip("Extra metal drained per push")]
    public float pushMetalCost = 5f;

    [Header("Endurance (Stamina)")]
    [Tooltip("Aerobic regen rate multiplier at base burn. Pewter keeps the body going longer.")]
    [Range(1f, 4f)]
    public float baseStaminaRegenMult = 2f;
    [Tooltip("Aerobic regen rate multiplier at max flare.")]
    [Range(1f, 8f)]
    public float maxStaminaRegenMult = 5f;
    [Tooltip("Sprint drain rate multiplier at base burn (0.3 = 70% reduction).")]
    [Range(0.05f, 1f)]
    public float baseSprintDrainMult = 0.3f;
    [Tooltip("Sprint drain rate multiplier at max flare (near-zero — pewter dragging).")]
    [Range(0.01f, 0.5f)]
    public float maxSprintDrainMult = 0.05f;

    [Header("Knockback Resistance")]
    [Tooltip("Divides incoming knockback force at base burn. 2 = half knockback.")]
    [Range(1f, 5f)]
    public float baseKnockbackResistance = 2f;
    [Tooltip("Knockback resistance at max flare.")]
    [Range(1f, 10f)]
    public float maxKnockbackResistance = 6f;

    [Header("References")]
    public Allomancer      allomancer;
    public BasicPlayerMove playerMove;

    // ── Private state ─────────────────────────────────────────────────────────

    private bool  isBurning          = false;
    private bool  _pewterToggled     = false;
    private float originalMass       = -1f;
    private float originalJumpVelocity;
    private float originalSprintDrainRate;
    private float originalAerobicRegenRate;
    private float originalDebtAccumulationRate;
    private float _pushCooldownTimer = 0f;
    private float _dragTimer         = 0f;
    private float _crashTimer        = 0f;
    private bool  _isCrashing        = false;

    private Rigidbody            playerRigidbody;
    private HealthBarTransitions healthSystem;
    private PlayerStamina        stamina;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        if (allomancer == null) allomancer = GetComponentInParent<Allomancer>();
        if (playerMove  == null) playerMove  = GetComponentInParent<BasicPlayerMove>();
        healthSystem = GetComponentInParent<HealthBarTransitions>();

        stamina = GetComponentInParent<PlayerStamina>();

        if (playerMove != null)
        {
            playerRigidbody      = playerMove.GetComponent<Rigidbody>();
            originalJumpVelocity = playerMove.jumpVelocity;
            if (playerRigidbody != null)
                originalMass = playerRigidbody.mass;
        }

        if (stamina != null)
        {
            originalAerobicRegenRate    = stamina.aerobicRegenRate;
            originalDebtAccumulationRate = stamina.debtAccumulationRate;
        }

        if (playerMove != null)
            originalSprintDrainRate = playerMove.drainRate;
    }

    void Update()
    {
        bool wasBurning = isBurning;

        MetalSelector sel     = allomancer?.GetComponent<MetalSelector>();
        bool burnSession      = FlareManager.Instance != null && FlareManager.Instance.IsBurning;
        bool ePressed         = Input.GetKeyDown(Keybinds.Ability1);
        bool qPressed         = Input.GetKeyDown(Keybinds.Ability2);

        // Releasing Left Ctrl ends the burn session — toggle resets.
        if (!burnSession) _pewterToggled = false;

        if (sel != null && burnSession)
        {
            if (sel.GetPrimaryMetal()   == AllomancySkill.MetalType.Pewter && ePressed) _pewterToggled = !_pewterToggled;
            if (sel.GetSecondaryMetal() == AllomancySkill.MetalType.Pewter && qPressed) _pewterToggled = !_pewterToggled;
        }

        float reserve = allomancer != null
            ? allomancer.GetMetalReserve(AllomancySkill.MetalType.Pewter)
            : 0f;

        // Auto-off when reserve depletes.
        if (reserve <= 0f) _pewterToggled = false;

        isBurning = allomancer != null && _pewterToggled && reserve > 0f;

        if (_pushCooldownTimer > 0f) _pushCooldownTimer -= Time.deltaTime;

        // ── Crash recovery tick ───────────────────────────────────────────────
        if (_isCrashing)
        {
            _crashTimer -= Time.deltaTime;
            if (_crashTimer <= 0f)
            {
                _isCrashing = false;
                if (playerMove != null)
                    playerMove.externalSpeedMultiplier = 1f;
            }
        }

        if (isBurning)
        {
            float flareMult = FlareManager.Instance.FlareMultiplier;

            // Track drag time — only counts while actively flaring at high intensity
            if (flareMult >= 2f)
                _dragTimer += Time.deltaTime;
            else
                _dragTimer = Mathf.Max(0f, _dragTimer - Time.deltaTime * 0.5f);

            ApplyEffects(flareMult);
            HandleHealing(flareMult);
            DrainReserve(flareMult);

            // ── Pewter Push: F (primary) / V (secondary) ──────────────────────
            if (_pushCooldownTimer <= 0f)
            {
                KeyCode pushKey = GetSpecialKey(sel);
                if (pushKey != KeyCode.None && Input.GetKeyDown(pushKey))
                    PewterPush(flareMult);
            }
        }
        else if (wasBurning)
        {
            // Reserve ran out — check for pewter drag crash
            if (_dragTimer >= dragCrashThreshold && !_isCrashing)
                TriggerDragCrash();
            else
                ResetEffects();

            _dragTimer = 0f;
        }
    }

    // ── Pewter Drag Crash ─────────────────────────────────────────────────────

    void TriggerDragCrash()
    {
        _isCrashing = true;
        _crashTimer  = crashDuration;

        // Sudden fatigue surge: deal health damage, drain stamina, slow the player
        if (healthSystem != null)
        {
            float newHealth = Mathf.Max(1f, healthSystem.GetCurrentHealth() - crashDamage);
            healthSystem.health = newHealth;
        }

        // Tank the stamina system — all suppressed debt hits at once (2x exhaustion duration)
        if (stamina != null)
        {
            stamina.aerobicRegenRate     = originalAerobicRegenRate;
            stamina.debtAccumulationRate = originalDebtAccumulationRate;
            stamina.TriggerCrashExhaustion(2f);
        }

        if (playerMove != null)
        {
            playerMove.externalSpeedMultiplier = crashSpeedPenalty;
            playerMove.drainRate               = originalSprintDrainRate;
        }

        // Reset physical stats (no pewter left to maintain them)
        if (playerRigidbody != null && originalMass > 0f)
            playerRigidbody.mass = originalMass;
        if (playerMove != null)
            playerMove.jumpVelocity = originalJumpVelocity;

        CameraShakeManager.Instance?.Shake(0.8f, 0.3f);
    }

    // ── Special key ──────────────────────────────────────────────────────────

    private KeyCode GetSpecialKey(MetalSelector sel)
    {
        if (sel == null) return Keybinds.Ability3;
        if (sel.GetPrimaryMetal()   == AllomancySkill.MetalType.Pewter) return Keybinds.Ability3; // F
        if (sel.GetSecondaryMetal() == AllomancySkill.MetalType.Pewter) return Keybinds.Ability4; // V
        return KeyCode.None;
    }

    // ── Pewter Push ───────────────────────────────────────────────────────────

    void PewterPush(float flareMult)
    {
        if (playerRigidbody == null) return;

        Vector3 forward = playerRigidbody.transform.forward;

        playerRigidbody.AddForce(forward * pushLungeForce * flareMult, ForceMode.VelocityChange);

        Vector3 origin = playerRigidbody.position + forward * (pushRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(origin, pushRange);
        foreach (var col in hits)
        {
            if (col.gameObject == playerRigidbody.gameObject) continue;
            EnemyKnockback kb = col.GetComponentInParent<EnemyKnockback>();
            if (kb != null)
            {
                Vector3 dir = (col.transform.position - playerRigidbody.position).normalized;
                kb.ApplyAllomanticKnockback(dir, pushKnockbackForce * flareMult);
            }
        }

        allomancer?.DrainMetal(AllomancySkill.MetalType.Pewter, pushMetalCost);
        CameraShakeManager.Instance?.Shake(0.15f, 0.08f * flareMult);
        SoundManager.Instance?.PlayPushSound();

        _pushCooldownTimer = pushCooldown;
    }

    // ── Effects ───────────────────────────────────────────────────────────────

    void ApplyEffects(float flareMult)
    {
        if (playerMove == null) return;

        float t = Mathf.Clamp01((flareMult - 1f) / 9f);

        if (!_isCrashing)
            playerMove.externalSpeedMultiplier = Mathf.Lerp(baseSpeedMultiplier, maxSpeedMultiplier, t);

        playerMove.jumpVelocity = originalJumpVelocity * Mathf.Lerp(baseJumpMultiplier, maxJumpMultiplier, t);

        if (playerRigidbody != null && originalMass > 0f)
            playerRigidbody.mass = originalMass * Mathf.Lerp(baseMassMultiplier, maxMassMultiplier, t);

        // Stamina: suppress fatigue accumulation, boost recovery
        if (stamina != null)
        {
            stamina.aerobicRegenRate      = originalAerobicRegenRate    * Mathf.Lerp(baseStaminaRegenMult, maxStaminaRegenMult, t);
            stamina.debtAccumulationRate  = originalDebtAccumulationRate * Mathf.Lerp(baseSprintDrainMult, maxSprintDrainMult, t);
        }

        // Sprint drain: pewter dragging barely costs stamina at max flare
        playerMove.drainRate = originalSprintDrainRate * Mathf.Lerp(baseSprintDrainMult, maxSprintDrainMult, t);
    }

    void ResetEffects()
    {
        if (playerMove != null)
        {
            playerMove.externalSpeedMultiplier = 1f;
            playerMove.jumpVelocity            = originalJumpVelocity;
            playerMove.drainRate               = originalSprintDrainRate;
        }
        if (playerRigidbody != null && originalMass > 0f)
            playerRigidbody.mass = originalMass;
        if (stamina != null)
        {
            stamina.aerobicRegenRate     = originalAerobicRegenRate;
            stamina.debtAccumulationRate = originalDebtAccumulationRate;
        }
    }

    void HandleHealing(float flareMult)
    {
        if (healthSystem == null) return;
        if (healthSystem.GetCurrentHealth() >= healthSystem.GetMaxHealth()) return;

        float t        = Mathf.Clamp01((flareMult - 1f) / 9f);
        float healRate = Mathf.Lerp(baseHealRate, maxHealRate, t);
        healthSystem.Heal(healRate * Time.deltaTime);
    }

    void DrainReserve(float flareMult)
    {
        if (allomancer == null) return;
        float t     = Mathf.Clamp01((flareMult - 1f) / 9f);
        float drain = baseDrainPerSecond * Mathf.Lerp(1f, flareDrainMultiplier, t);
        allomancer.DrainMetal(AllomancySkill.MetalType.Pewter, drain * Time.deltaTime);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public bool  IsBurningPewter() => isBurning;
    public bool  IsCrashing()      => _isCrashing;

    /// <summary>
    /// Divides incoming knockback force. 1 = no resistance, 6 = nearly immovable.
    /// </summary>
    public float GetKnockbackResistance()
    {
        if (!isBurning) return 1f;
        float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float t = Mathf.Clamp01((flareMult - 1f) / 9f);
        return Mathf.Lerp(baseKnockbackResistance, maxKnockbackResistance, t);
    }

    /// <summary>
    /// Strength multiplier for combat system to use on damage calculations.
    /// Returns 1 when not burning, 2–3x when active per lore.
    /// </summary>
    public float GetStrengthMultiplier()
    {
        if (!isBurning) return 1f;
        float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float t = Mathf.Clamp01((flareMult - 1f) / 9f);
        return Mathf.Lerp(baseStrengthMultiplier, maxStrengthMultiplier, t);
    }
}
