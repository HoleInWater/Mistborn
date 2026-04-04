using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Pewter Metallurgy (Thug / Ironhide) — lore-accurate physical enhancement.
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
[PlayerComponent("Metallurgy Metals", order: 30)]
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
    [Tooltip("Metal reserve drained per second at base burn. MAG: 5 min real burn → PewterDrainRate ≈ 0.333/s")]
    public float baseDrainPerSecond = MetallurgyConstants.PewterDrainRate;
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
    [Tooltip("While Pewter is active, ALL stamina drain is suppressed — fatigue banks silently. " +
             "When Pewter stops, the banked fatigue hits at once with interest. " +
             "1.5 = you owe 50% more stamina than you would have spent normally.")]
    [Range(1f, 3f)]
    public float fatigueInterestMultiplier = 1.5f;

    [Header("Vignette")]
    [Tooltip("Vignette intensity at base burn — subtle red rim indicating enhanced strength.")]
    [Range(0f, 0.35f)]
    public float vignetteIntensity = 0.13f;
    [Tooltip("Vignette intensity at max flare — stronger crimson edge during peak pewter.")]
    [Range(0f, 0.5f)]
    public float vignetteFlaringIntensity = 0.28f;
    [Tooltip("Matches Pewter's color on the metal wheel.")]
    public Color vignetteColor = new Color(0.8f, 0.2f, 0.2f);

    [Header("Damage Reduction")]
    [Tooltip("Fraction of incoming damage taken at base burn. 0.65 = 35% reduction. " +
             "Lore: Pewter doubles durability — halving damage is conservative but fair.")]
    [Range(0.2f, 1f)]
    public float baseDamageMultiplier = 0.65f;
    [Tooltip("Fraction of incoming damage taken at max flare. 0.3 = 70% reduction. " +
             "Lore: Pewter dragging — you can survive things that would kill anyone else.")]
    [Range(0.1f, 0.8f)]
    public float maxDamageMultiplier = 0.30f;

    [Header("Knockback Resistance")]
    [Tooltip("Divides incoming knockback force at base burn. 2 = half knockback.")]
    [Range(1f, 5f)]
    public float baseKnockbackResistance = 2f;
    [Tooltip("Knockback resistance at max flare.")]
    [Range(1f, 10f)]
    public float maxKnockbackResistance = 6f;

    [Header("References")]
    public Metallurgist      metallurgist;
    public BasicPlayerMove playerMove;

    // ── Private state ─────────────────────────────────────────────────────────

    private bool  isBurning          = false;
    private bool  _pewterToggled     = false;
    private float originalMass       = -1f;
    private float originalJumpVelocity;
    private float originalSprintDrainRate;
    private float _pushCooldownTimer = 0f;
    private float _dragTimer         = 0f;
    private float _crashTimer        = 0f;
    private bool  _isCrashing        = false;

    private Rigidbody            playerRigidbody;
    private HealthBarTransitions healthSystem;
    private PlayerStamina        stamina;

    // HDRP vignette — own dedicated Volume so it doesn't conflict with Tin's vignette
    private Volume   _pewterVolume;
    private Vignette _vignette;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        if (metallurgist == null) metallurgist = GetComponentInParent<Metallurgist>();
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

        if (playerMove != null)
            originalSprintDrainRate = playerMove.drainRate;

        SetupVignette();
    }

    void OnDestroy()
    {
        if (_pewterVolume != null)
            Destroy(_pewterVolume.gameObject);
    }

    void SetupVignette()
    {
        var volObj = new GameObject("Pewter_Volume");
        _pewterVolume          = volObj.AddComponent<Volume>();
        _pewterVolume.isGlobal = true;
        _pewterVolume.weight   = 1f;
        _pewterVolume.priority = 1f; // sits above the scene's base Global Volume
        DontDestroyOnLoad(volObj);

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        _pewterVolume.profile = profile;

        _vignette = profile.Add<Vignette>(true);
        _vignette.color.overrideState     = true;
        _vignette.color.value             = vignetteColor;
        _vignette.intensity.overrideState = true;
        _vignette.intensity.value         = 0f;
    }

    void Update()
    {
        bool wasBurning = isBurning;

        MetalSelector sel     = metallurgist?.GetComponent<MetalSelector>();
        bool burnSession      = FlareManager.Instance != null && FlareManager.Instance.IsBurning;
        bool ePressed         = Input.GetKeyDown(Keybinds.Ability1);
        bool qPressed         = Input.GetKeyDown(Keybinds.Ability2);

        // Releasing Left Ctrl ends the burn session — toggle resets.
        if (!burnSession) _pewterToggled = false;

        if (sel != null && burnSession)
        {
            if (sel.GetPrimaryMetal()   == MetallurgySkill.MetalType.Pewter && ePressed) _pewterToggled = !_pewterToggled;
            if (sel.GetSecondaryMetal() == MetallurgySkill.MetalType.Pewter && qPressed) _pewterToggled = !_pewterToggled;
        }

        float reserve = metallurgist != null
            ? metallurgist.GetMetalReserve(MetallurgySkill.MetalType.Pewter)
            : 0f;

        // Auto-off when reserve depletes.
        if (reserve <= 0f) _pewterToggled = false;

        isBurning = metallurgist != null && _pewterToggled && reserve > 0f;

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

        UpdateVignette();

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

        if (healthSystem != null)
            healthSystem.incomingDamageMultiplier = 1f;

        // Crash is the punishment — clear the suppressed fatigue bank and trigger crash exhaustion
        if (stamina != null)
        {
            stamina.SuppressDrain = false;
            stamina.ClearSuppressedFatigue(); // Crash exhaustion covers it (don't double-apply)
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
        if (sel.GetPrimaryMetal()   == MetallurgySkill.MetalType.Pewter) return Keybinds.Ability3; // F
        if (sel.GetSecondaryMetal() == MetallurgySkill.MetalType.Pewter) return Keybinds.Ability4; // V
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
                kb.ApplyMetallurgicKnockback(dir, pushKnockbackForce * flareMult);
            }
        }

        metallurgist?.DrainMetal(MetallurgySkill.MetalType.Pewter, pushMetalCost);
        CameraShakeManager.Instance?.Shake(0.15f, 0.08f * flareMult);
        SoundManager.Instance?.PlayPushSound();

        _pushCooldownTimer = pushCooldown;
    }

    // ── Vignette ──────────────────────────────────────────────────────────────

    void UpdateVignette()
    {
        if (_vignette == null) return;

        float targetIntensity = 0f;
        if (isBurning)
        {
            float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            float t = Mathf.Clamp01((flareMult - 1f) / 9f);
            targetIntensity      = Mathf.Lerp(vignetteIntensity, vignetteFlaringIntensity, t);
            _vignette.color.value = vignetteColor;
        }

        _vignette.intensity.value = Mathf.Lerp(
            _vignette.intensity.value, targetIntensity, Time.deltaTime * 5f);
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

        // Damage reduction — Pewter toughness
        if (healthSystem != null)
            healthSystem.incomingDamageMultiplier = Mathf.Lerp(baseDamageMultiplier, maxDamageMultiplier, t);

        // Stamina: fully suppress drain while Pewter is active — fatigue banks silently
        if (stamina != null)
            stamina.SuppressDrain = true;

        playerMove.drainRate = 0f; // Sprint costs no stamina; it's all suppressed
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
        if (healthSystem != null)
            healthSystem.incomingDamageMultiplier = 1f;

        if (stamina != null)
        {
            // Release suppression — deferred fatigue hits now with interest
            stamina.SuppressDrain = false;
            stamina.DumpSuppressedFatigue(fatigueInterestMultiplier);
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
        if (metallurgist == null) return;
        float t     = Mathf.Clamp01((flareMult - 1f) / 9f);
        float drain = baseDrainPerSecond * Mathf.Lerp(1f, flareDrainMultiplier, t);
        metallurgist.DrainMetal(MetallurgySkill.MetalType.Pewter, drain * Time.deltaTime);
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
