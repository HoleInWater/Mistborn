/* FeruchemyController.cs
 *
 * PURPOSE:
 * Player input handling and gameplay effect application for the Feruchemy system.
 * Bridges Feruchemist.cs data/mechanics with player systems (movement, health, etc.).
 *
 * CONTROLS:
 * - Tab: Toggle Feruchemy mode (metalmind selection overlay)
 * - 1-9, 0, -, =, etc.: Select metalmind index while in Feruchemy mode
 * - Z: Start/stop storing into selected metalmind
 * - X: Start/stop tapping from selected metalmind
 * - C: Stop all storing/tapping
 *
 * GAMEPLAY EFFECTS:
 * - Speed (Steel):      Modifies movement speed via BasicPlayerMove.externalSpeedMultiplier
 * - Weight (Iron):      Modifies Rigidbody mass
 * - Strength (Pewter):  Modifies jump velocity and melee damage
 * - Senses (Tin):       Modifies camera FOV for peripheral awareness
 * - Health (Gold):      Applies heal/damage over time
 * - Energy (Cadmium):   Modifies stamina regen rate
 * - Warmth (Brass):     Reduces/increases cold damage resistance (flag only)
 * - Wakefulness (Bronze): Modifies sprint stamina drain
 * - MentalSpeed (Zinc):   Visual time perception (slight Time.timeScale feel via post-processing)
 * - Determination (Electrum): Modifies XP gain rate
 */

using UnityEngine;

public class FeruchemyController : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════════════════
    // REFERENCES
    // ═══════════════════════════════════════════════════════════════════════════

    [Header("References")]
    public Feruchemist feruchemist;
    public BasicPlayerMove playerMove;
    public PlayerStamina playerStamina;
    public HealthBarTransitions healthSystem;

    [Header("State")]
    public int selectedMetalmind = 0;
    public bool feruchemyModeActive = false;

    // ── Cached originals for resetting ──
    private float originalMoveSpeed;
    private float originalSprintSpeed;
    private float originalJumpVelocity;
    private float originalMass;
    private float originalStaminaRegen;
    private float originalFOV;
    private Rigidbody playerRb;
    private Camera mainCamera;

    // ── Effect tuning ──
    [Header("Effect Scaling")]
    [Tooltip("Max speed multiplier when tapping Steel at full power")]
    public float maxSpeedMult = 2.5f;
    [Tooltip("Max mass multiplier when tapping Iron weight")]
    public float maxMassMult = 3f;
    [Tooltip("Max jump multiplier when tapping Pewter strength")]
    public float maxJumpMult = 2f;
    [Tooltip("FOV bonus when tapping Tin senses")]
    public float maxFOVBonus = 30f;
    [Tooltip("Health per second when tapping Gold")]
    public float goldHealRate = 5f;
    [Tooltip("Health drain per second when storing Gold")]
    public float goldStoreDrain = 2f;
    [Tooltip("Stamina regen multiplier when tapping Energy")]
    public float maxStaminaRegenMult = 3f;

    // ═══════════════════════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════════════════════

    void Start()
    {
        if (feruchemist == null)
            feruchemist = GetComponent<Feruchemist>();
        if (playerMove == null)
            playerMove = GetComponent<BasicPlayerMove>();
        if (playerStamina == null)
            playerStamina = GetComponent<PlayerStamina>();
        if (healthSystem == null)
            healthSystem = GetComponent<HealthBarTransitions>();

        playerRb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        CacheOriginalValues();
    }

    void Update()
    {
        if (feruchemist == null) return;

        HandleInput();
        ApplyGameplayEffects();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INPUT
    // ═══════════════════════════════════════════════════════════════════════════

    private void HandleInput()
    {
        // Tab toggles Feruchemy mode
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            feruchemyModeActive = !feruchemyModeActive;
            if (!feruchemyModeActive)
            {
                // Exiting Feruchemy mode doesn't stop active store/tap
                Debug.Log("[FERUCHEMY] Mode deactivated. Active stores/taps continue.");
            }
            else
            {
                Debug.Log($"[FERUCHEMY] Mode activated. Selected: {feruchemist.metalminds[selectedMetalmind].attribute}");
            }
        }

        if (!feruchemyModeActive) return;

        // Number keys select metalmind (1-9 = index 0-8, 0 = index 9)
        HandleMetalmindSelection();

        // Z = toggle store, X = toggle tap, C = stop all
        if (Input.GetKeyDown(KeyCode.Z))
        {
            feruchemist.ToggleStore(selectedMetalmind);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            feruchemist.ToggleTap(selectedMetalmind);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            feruchemist.StopAll();
            Debug.Log("[FERUCHEMY] Stopped all storing and tapping.");
        }
    }

    private void HandleMetalmindSelection()
    {
        // Alpha keys 1-9 map to metalminds 0-8
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectMetalmind(i);
                return;
            }
        }

        // Alpha0 = metalmind 9
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SelectMetalmind(9);
            return;
        }

        // Minus = 10, Equals = 11
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            SelectMetalmind(10);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            SelectMetalmind(11);
            return;
        }

        // Bracket keys for remaining 12-15
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            SelectMetalmind(12);
            return;
        }

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            SelectMetalmind(13);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            SelectMetalmind(14);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Semicolon))
        {
            SelectMetalmind(15);
            return;
        }
    }

    private void SelectMetalmind(int index)
    {
        if (index < 0 || index >= Feruchemist.MetalmindCount) return;
        if (!feruchemist.unlockedMetals[index])
        {
            Debug.Log($"[FERUCHEMY] {feruchemist.metalminds[index].attribute} is locked.");
            return;
        }

        selectedMetalmind = index;
        Metalmind mind = feruchemist.metalminds[index];
        Debug.Log($"[FERUCHEMY] Selected: {mind.attribute} ({mind.ChargePercent * 100f:F0}% charged)");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GAMEPLAY EFFECTS
    // ═══════════════════════════════════════════════════════════════════════════

    private void CacheOriginalValues()
    {
        if (playerMove != null)
        {
            originalMoveSpeed = playerMove.moveSpeed;
            originalSprintSpeed = playerMove.sprintSpeed;
            originalJumpVelocity = playerMove.jumpVelocity;
        }

        if (playerRb != null)
            originalMass = playerRb.mass;

        if (playerStamina != null)
            originalStaminaRegen = playerStamina.regenRate;

        if (mainCamera != null)
            originalFOV = mainCamera.fieldOfView;
    }

    private void ApplyGameplayEffects()
    {
        float dt = Time.deltaTime;

        for (int i = 0; i < Feruchemist.MetalmindCount; i++)
        {
            float mod = feruchemist.GetAttributeModifier(i);
            if (Mathf.Approximately(mod, 1f)) continue; // Neutral — no effect

            FeruchemicalAttribute attr = feruchemist.metalminds[i].attribute;

            switch (attr)
            {
                case FeruchemicalAttribute.Speed:
                    ApplySpeedEffect(mod);
                    break;

                case FeruchemicalAttribute.Weight:
                    ApplyWeightEffect(mod);
                    break;

                case FeruchemicalAttribute.Strength:
                    ApplyStrengthEffect(mod);
                    break;

                case FeruchemicalAttribute.Senses:
                    ApplySensesEffect(mod);
                    break;

                case FeruchemicalAttribute.Health:
                    ApplyHealthEffect(mod, dt);
                    break;

                case FeruchemicalAttribute.Energy:
                    ApplyEnergyEffect(mod);
                    break;

                case FeruchemicalAttribute.Wakefulness:
                    ApplyWakefulnessEffect(mod);
                    break;
            }
        }
    }

    /// <summary>
    /// Steel: Speed. Storing = slower, tapping = faster.
    /// Uses externalSpeedMultiplier on BasicPlayerMove.
    /// </summary>
    private void ApplySpeedEffect(float mod)
    {
        if (playerMove == null) return;
        // mod < 1 = storing (slower), mod > 1 = tapping (faster)
        float clampedMult = Mathf.Clamp(mod, 0.3f, maxSpeedMult);
        playerMove.externalSpeedMultiplier = clampedMult;
    }

    /// <summary>
    /// Iron: Weight. Storing = lighter (less mass), tapping = heavier (more mass).
    /// </summary>
    private void ApplyWeightEffect(float mod)
    {
        if (playerRb == null) return;
        // mod < 1 = lighter, mod > 1 = heavier
        float clampedMult = Mathf.Clamp(mod, 0.2f, maxMassMult);
        playerRb.mass = originalMass * clampedMult;
    }

    /// <summary>
    /// Pewter: Strength. Storing = weaker, tapping = stronger jumps.
    /// </summary>
    private void ApplyStrengthEffect(float mod)
    {
        if (playerMove == null) return;
        float clampedMult = Mathf.Clamp(mod, 0.5f, maxJumpMult);
        playerMove.jumpVelocity = originalJumpVelocity * clampedMult;
    }

    /// <summary>
    /// Tin: Senses. Storing = reduced FOV, tapping = wider FOV.
    /// </summary>
    private void ApplySensesEffect(float mod)
    {
        if (mainCamera == null) return;
        // Map modifier to FOV bonus: mod 0.5 = -15 FOV, mod 1.5 = +15 FOV
        float fovDelta = (mod - 1f) * maxFOVBonus;
        mainCamera.fieldOfView = Mathf.Clamp(originalFOV + fovDelta, 40f, 120f);
    }

    /// <summary>
    /// Gold: Health. Storing = slow HP drain, tapping = heal over time.
    /// The Lord Ruler's key to immortality via compounding.
    /// </summary>
    private void ApplyHealthEffect(float mod, float dt)
    {
        if (healthSystem == null) return;

        if (mod < 1f)
        {
            // Storing: drain health slowly (sacrifice for later healing)
            float drainAmount = goldStoreDrain * (1f - mod) * dt;
            healthSystem.Heal(-drainAmount);
        }
        else if (mod > 1f)
        {
            // Tapping: heal over time
            float healAmount = goldHealRate * (mod - 1f) * dt;
            healthSystem.Heal(healAmount);
        }
    }

    /// <summary>
    /// Cadmium: Energy. Storing = reduced stamina regen, tapping = enhanced stamina regen.
    /// </summary>
    private void ApplyEnergyEffect(float mod)
    {
        if (playerStamina == null) return;
        float clampedMult = Mathf.Clamp(mod, 0.2f, maxStaminaRegenMult);
        playerStamina.regenRate = originalStaminaRegen * clampedMult;
    }

    /// <summary>
    /// Bronze: Wakefulness. Storing = increased sprint drain, tapping = reduced sprint drain.
    /// </summary>
    private void ApplyWakefulnessEffect(float mod)
    {
        if (playerMove == null) return;
        // Invert: storing (mod < 1) means MORE drain, tapping (mod > 1) means LESS drain
        float drainMult = Mathf.Clamp(1f / mod, 0.3f, 3f);
        playerMove.drainRate = 25f * drainMult; // 25f is the default drainRate
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RESET
    // ═══════════════════════════════════════════════════════════════════════════

    void OnDisable()
    {
        ResetAllEffects();
    }

    private void ResetAllEffects()
    {
        if (playerMove != null)
        {
            playerMove.externalSpeedMultiplier = 1f;
            playerMove.jumpVelocity = originalJumpVelocity;
            playerMove.drainRate = 25f;
        }

        if (playerRb != null)
            playerRb.mass = originalMass;

        if (playerStamina != null)
            playerStamina.regenRate = originalStaminaRegen;

        if (mainCamera != null)
            mainCamera.fieldOfView = originalFOV;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get the currently selected metalmind index.
    /// </summary>
    public int GetSelectedMetalmind() => selectedMetalmind;

    /// <summary>
    /// Whether Feruchemy input mode is active.
    /// </summary>
    public bool IsFeruchemyModeActive() => feruchemyModeActive;

    /// <summary>
    /// Get a display string for the current Feruchemy state (for HUD).
    /// </summary>
    public string GetStatusString()
    {
        if (!feruchemyModeActive) return "";

        Metalmind mind = feruchemist.metalminds[selectedMetalmind];
        string state = "Idle";
        if (feruchemist.IsStoring(selectedMetalmind)) state = "STORING";
        else if (feruchemist.IsTapping(selectedMetalmind)) state = "TAPPING";

        bool compounding = feruchemist.IsCompounding(selectedMetalmind);
        string compStr = compounding ? " [COMPOUNDING]" : "";

        return $"{mind.attribute}: {mind.ChargePercent * 100f:F0}% | {state}{compStr}";
    }
}
