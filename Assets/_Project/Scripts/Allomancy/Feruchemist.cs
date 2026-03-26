/* Feruchemist.cs
 *
 * PURPOSE:
 * Core Feruchemy system that manages metalminds, storing/tapping attributes,
 * and integration with Allomancer for Compounding detection.
 *
 * LORE-ACCURATE FERUCHEMY:
 * ========================
 * Feruchemy is end-neutral — it doesn't create or destroy power, only stores
 * and retrieves it. A Feruchemist stores an attribute (speed, weight, health, etc.)
 * into a metalmind at the cost of reduced ability NOW, then taps it later for a burst.
 *
 * KEY MECHANICS:
 * - Each of 16 metals stores a specific attribute
 * - Storage reduces the attribute while active (e.g., storing speed = moving slower)
 * - Tapping enhances the attribute (e.g., tapping speed = moving faster)
 * - Diminishing returns on both storage rate and tap output
 * - Metalminds have finite capacity based on size/material
 * - A character with BOTH Allomancy and Feruchemy for the same metal can Compound
 *
 * METALS & ATTRIBUTES:
 * - Steel:    Speed          - Iron:      Weight
 * - Pewter:   Strength       - Tin:       Senses
 * - Zinc:     Mental Speed   - Brass:     Warmth
 * - Copper:   Memories       - Bronze:    Wakefulness
 * - Gold:     Health         - Electrum:  Determination
 * - Cadmium:  Breath         - Bendalloy: Energy
 * - Aluminum: Identity       - Duralumin: Connection
 * - Chromium: Fortune        - Nicrosil:  Investiture
 */

using UnityEngine;
using System;

/// <summary>
/// The 16 Feruchemical attributes, one per metal.
/// Enum order matches AllomancySkill.MetalType for easy cross-referencing.
/// </summary>
public enum FeruchemicalAttribute
{
    Speed,          // Steel   (0)
    Weight,         // Iron    (1)
    Strength,       // Pewter  (2)
    Senses,         // Tin     (3)
    MentalSpeed,    // Zinc    (4)
    Warmth,         // Brass   (5)
    Memories,       // Copper  (6)
    Wakefulness,    // Bronze  (7)
    Health,         // Gold    (8) — Lord Ruler's immortality via compounding
    Determination,  // Electrum(9)
    Fortune,        // Chromium(10) — mapped to Atium's slot for gameplay alignment
    Investiture,    // Nicrosil(11) — mapped to Malatium's slot for gameplay alignment
    // The following use indices 12-15 to align with AllomancySkill.MetalType 12-17
    // but we consolidate to 16 attributes matching the 16 standard metals
    Identity,       // Aluminum  (12)
    Connection,     // Duralumin (13)
    Breath,         // Bendalloy (14) — note: lore swaps Cadmium=Breath, Bendalloy=Energy
    Energy          // Cadmium   (15)
}

/// <summary>
/// Represents a single metalmind that stores one Feruchemical attribute.
/// </summary>
[Serializable]
public class Metalmind
{
    public FeruchemicalAttribute attribute;
    public float currentCharge;
    public float maxCapacity;

    /// <summary>Base rate at which this metalmind stores per second.</summary>
    public float baseStoreRate;

    /// <summary>Base rate at which this metalmind taps per second.</summary>
    public float baseTapRate;

    /// <summary>Diminishing returns factor (lambda). Higher = faster diminishment.</summary>
    public float diminishingFactor;

    public Metalmind(FeruchemicalAttribute attr, float capacity, float storeRate, float tapRate, float diminishing)
    {
        attribute = attr;
        maxCapacity = capacity;
        currentCharge = 0f;
        baseStoreRate = storeRate;
        baseTapRate = tapRate;
        diminishingFactor = diminishing;
    }

    /// <summary>
    /// How full this metalmind is, 0-1.
    /// </summary>
    public float ChargePercent => maxCapacity > 0 ? currentCharge / maxCapacity : 0f;

    /// <summary>
    /// Whether this metalmind has any charge to tap.
    /// </summary>
    public bool HasCharge => currentCharge > 0.01f;

    /// <summary>
    /// Whether this metalmind can accept more charge.
    /// </summary>
    public bool CanStore => currentCharge < maxCapacity - 0.01f;
}

/// <summary>
/// Core Feruchemy component. Attach to any character that can use Feruchemy.
/// Manages 16 metalminds with store/tap mechanics and diminishing returns.
/// </summary>
public class Feruchemist : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    public const int MetalmindCount = 16;
    public const float DefaultCapacity = 500f;
    public const float DefaultStoreRate = 10f;
    public const float DefaultTapRate = 15f;
    public const float DefaultDiminishingFactor = 0.05f;

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════════════════

    [Header("Metalminds")]
    public Metalmind[] metalminds = new Metalmind[MetalmindCount];

    [Header("Active State")]
    public bool[] isStoring = new bool[MetalmindCount];
    public bool[] isTapping = new bool[MetalmindCount];

    [Header("Store/Tap Intensity")]
    [Range(0.1f, 3f)]
    public float storeIntensity = 1f;
    [Range(0.1f, 3f)]
    public float tapIntensity = 1f;

    [Header("Attribute Modifiers (applied to gameplay systems)")]
    public float[] attributeModifiers = new float[MetalmindCount];

    [Header("Unlocked Metals")]
    public bool[] unlockedMetals = new bool[MetalmindCount];

    // Cached reference to Allomancer for Compounding detection
    private Allomancer allomancer;

    // Accumulated store time per metalmind for diminishing returns calculation
    private float[] accumulatedStoreTime = new float[MetalmindCount];
    private float[] accumulatedTapTime = new float[MetalmindCount];

    // ═══════════════════════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════════════════════

    void Awake()
    {
        MistbornRegistry.RegisterFeruchemist(this);
        InitializeMetalminds();

        // Start with all metals unlocked for testing
        for (int i = 0; i < MetalmindCount; i++)
        {
            unlockedMetals[i] = true;
            attributeModifiers[i] = 1f; // Neutral modifier
        }

        allomancer = GetComponent<Allomancer>();
    }

    void OnDestroy()
    {
        MistbornRegistry.UnregisterFeruchemist(this);
    }

    void Update()
    {
        ProcessStoringAndTapping();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════════════════════

    private void InitializeMetalminds()
    {
        // Each metal gets a metalmind with tuned capacity/rates per attribute type
        // Physical metals have higher caps; mental/spiritual are lower but more impactful
        metalminds[0]  = new Metalmind(FeruchemicalAttribute.Speed,         600f, 12f, 18f, 0.04f);  // Steel
        metalminds[1]  = new Metalmind(FeruchemicalAttribute.Weight,        800f, 15f, 20f, 0.03f);  // Iron
        metalminds[2]  = new Metalmind(FeruchemicalAttribute.Strength,      600f, 12f, 18f, 0.04f);  // Pewter
        metalminds[3]  = new Metalmind(FeruchemicalAttribute.Senses,        400f, 10f, 15f, 0.05f);  // Tin
        metalminds[4]  = new Metalmind(FeruchemicalAttribute.MentalSpeed,   300f,  8f, 12f, 0.06f);  // Zinc
        metalminds[5]  = new Metalmind(FeruchemicalAttribute.Warmth,        400f, 10f, 15f, 0.05f);  // Brass
        metalminds[6]  = new Metalmind(FeruchemicalAttribute.Memories,      500f, 10f, 15f, 0.05f);  // Copper
        metalminds[7]  = new Metalmind(FeruchemicalAttribute.Wakefulness,   400f, 10f, 15f, 0.05f);  // Bronze
        metalminds[8]  = new Metalmind(FeruchemicalAttribute.Health,        700f, 10f, 20f, 0.03f);  // Gold — key for Lord Ruler
        metalminds[9]  = new Metalmind(FeruchemicalAttribute.Determination, 300f,  8f, 12f, 0.06f);  // Electrum
        metalminds[10] = new Metalmind(FeruchemicalAttribute.Fortune,       200f,  5f, 10f, 0.08f);  // Chromium
        metalminds[11] = new Metalmind(FeruchemicalAttribute.Investiture,   200f,  5f, 10f, 0.08f);  // Nicrosil
        metalminds[12] = new Metalmind(FeruchemicalAttribute.Identity,      250f,  6f, 10f, 0.07f);  // Aluminum
        metalminds[13] = new Metalmind(FeruchemicalAttribute.Connection,    250f,  6f, 10f, 0.07f);  // Duralumin
        metalminds[14] = new Metalmind(FeruchemicalAttribute.Breath,        400f, 10f, 15f, 0.05f);  // Bendalloy
        metalminds[15] = new Metalmind(FeruchemicalAttribute.Energy,        500f, 12f, 18f, 0.04f);  // Cadmium
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CORE STORE / TAP LOOP
    // ═══════════════════════════════════════════════════════════════════════════

    private void ProcessStoringAndTapping()
    {
        float dt = Time.deltaTime;

        for (int i = 0; i < MetalmindCount; i++)
        {
            if (!unlockedMetals[i]) continue;

            Metalmind mind = metalminds[i];

            if (isStoring[i] && mind.CanStore)
            {
                // Storage: diminishing returns over continuous store time
                // S'(t) = baseRate × e^(-λ × t) × intensity
                // From PHYSICS-MATH-BOOK.md: S(t) = (k/λ) × (1 - e^(-λt))
                accumulatedStoreTime[i] += dt;
                float effectiveRate = mind.baseStoreRate
                    * Mathf.Exp(-mind.diminishingFactor * accumulatedStoreTime[i])
                    * storeIntensity;

                // Asymptotic cap: harder to store as metalmind fills
                // S = (C_max × r) / (C_max + r)
                float fillFactor = 1f - (mind.currentCharge / mind.maxCapacity);
                effectiveRate *= Mathf.Max(0.1f, fillFactor);

                float storeAmount = effectiveRate * dt;
                storeAmount = Mathf.Min(storeAmount, mind.maxCapacity - mind.currentCharge);

                mind.currentCharge += storeAmount;

                // Reduce the attribute while storing (Feruchemy is end-neutral)
                // Modifier < 1 means reduced ability
                float reductionFraction = (effectiveRate / mind.baseStoreRate) * 0.5f;
                attributeModifiers[i] = Mathf.Max(0.2f, 1f - reductionFraction);
            }
            else if (isTapping[i] && mind.HasCharge)
            {
                // Tapping: diminishing returns over continuous tap time
                accumulatedTapTime[i] += dt;
                float effectiveRate = mind.baseTapRate
                    * Mathf.Exp(-mind.diminishingFactor * accumulatedTapTime[i])
                    * tapIntensity;

                // Diminishing output as metalmind empties
                float chargeFactor = mind.currentCharge / mind.maxCapacity;
                effectiveRate *= Mathf.Max(0.1f, chargeFactor);

                float tapAmount = effectiveRate * dt;
                tapAmount = Mathf.Min(tapAmount, mind.currentCharge);

                mind.currentCharge -= tapAmount;

                // Enhance the attribute while tapping
                // Modifier > 1 means enhanced ability
                float boostFraction = (effectiveRate / mind.baseTapRate) * 0.8f;
                attributeModifiers[i] = 1f + boostFraction;
            }
            else
            {
                // Not storing or tapping — reset modifier to neutral
                attributeModifiers[i] = 1f;

                // Reset accumulated time when not actively storing/tapping
                if (!isStoring[i]) accumulatedStoreTime[i] = 0f;
                if (!isTapping[i]) accumulatedTapTime[i] = 0f;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC API — Store / Tap Controls
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Start storing an attribute into its metalmind.
    /// Cannot store and tap the same metal simultaneously.
    /// </summary>
    public void StartStoring(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= MetalmindCount) return;
        if (!unlockedMetals[metalIndex]) return;
        if (!metalminds[metalIndex].CanStore) return;

        isTapping[metalIndex] = false;
        accumulatedTapTime[metalIndex] = 0f;
        isStoring[metalIndex] = true;

    }

    /// <summary>
    /// Start tapping an attribute from its metalmind.
    /// </summary>
    public void StartTapping(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= MetalmindCount) return;
        if (!unlockedMetals[metalIndex]) return;
        if (!metalminds[metalIndex].HasCharge) return;

        isStoring[metalIndex] = false;
        accumulatedStoreTime[metalIndex] = 0f;
        isTapping[metalIndex] = true;

    }

    /// <summary>
    /// Stop storing a specific metal.
    /// </summary>
    public void StopStoring(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= MetalmindCount) return;
        isStoring[metalIndex] = false;
        accumulatedStoreTime[metalIndex] = 0f;
        attributeModifiers[metalIndex] = 1f;
    }

    /// <summary>
    /// Stop tapping a specific metal.
    /// </summary>
    public void StopTapping(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= MetalmindCount) return;
        isTapping[metalIndex] = false;
        accumulatedTapTime[metalIndex] = 0f;
        attributeModifiers[metalIndex] = 1f;
    }

    /// <summary>
    /// Toggle storing for a metal. If already storing, stops. If tapping, switches to storing.
    /// </summary>
    public void ToggleStore(int metalIndex)
    {
        if (isStoring[metalIndex])
            StopStoring(metalIndex);
        else
            StartStoring(metalIndex);
    }

    /// <summary>
    /// Toggle tapping for a metal. If already tapping, stops. If storing, switches to tapping.
    /// </summary>
    public void ToggleTap(int metalIndex)
    {
        if (isTapping[metalIndex])
            StopTapping(metalIndex);
        else
            StartTapping(metalIndex);
    }

    /// <summary>
    /// Stop all storing and tapping.
    /// </summary>
    public void StopAll()
    {
        for (int i = 0; i < MetalmindCount; i++)
        {
            StopStoring(i);
            StopTapping(i);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC API — Queries
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get the attribute modifier for a given metal index.
    /// Values less than 1 = storing (reduced), greater than 1 = tapping (enhanced).
    /// </summary>
    public float GetAttributeModifier(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= MetalmindCount) return 1f;
        return attributeModifiers[metalIndex];
    }

    /// <summary>
    /// Get the attribute modifier by attribute enum.
    /// </summary>
    public float GetAttributeModifier(FeruchemicalAttribute attr)
    {
        return GetAttributeModifier((int)attr);
    }

    /// <summary>
    /// Get the metalmind for a specific index.
    /// </summary>
    public Metalmind GetMetalmind(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= MetalmindCount) return null;
        return metalminds[metalIndex];
    }

    /// <summary>
    /// Get the charge percentage (0-1) for a metalmind.
    /// </summary>
    public float GetChargePercent(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= MetalmindCount) return 0f;
        return metalminds[metalIndex].ChargePercent;
    }

    /// <summary>
    /// Whether the character is currently storing any attribute.
    /// </summary>
    public bool IsStoringAny()
    {
        for (int i = 0; i < MetalmindCount; i++)
            if (isStoring[i]) return true;
        return false;
    }

    /// <summary>
    /// Whether the character is currently tapping any attribute.
    /// </summary>
    public bool IsTappingAny()
    {
        for (int i = 0; i < MetalmindCount; i++)
            if (isTapping[i]) return true;
        return false;
    }

    /// <summary>
    /// Whether this character is storing a specific metal.
    /// </summary>
    public bool IsStoring(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= MetalmindCount) return false;
        return isStoring[metalIndex];
    }

    /// <summary>
    /// Whether this character is tapping a specific metal.
    /// </summary>
    public bool IsTapping(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= MetalmindCount) return false;
        return isTapping[metalIndex];
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPOUNDING DETECTION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Check if this character can Compound a specific metal.
    /// Requires both Allomancy and Feruchemy for the same metal.
    /// </summary>
    public bool CanCompound(int metalIndex)
    {
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        if (allomancer == null) return false;
        if (metalIndex < 0 || metalIndex >= MetalmindCount) return false;
        if (!unlockedMetals[metalIndex]) return false;

        // Must have Allomantic access to this metal
        AllomancySkill.MetalType metalType = MetalIndexToAllomanticType(metalIndex);
        return allomancer.unlockedMetals[(int)metalType];
    }

    /// <summary>
    /// Check if this character is actively Compounding a specific metal.
    /// Active Compounding = burning the metal Allomantically while having a charged metalmind.
    /// </summary>
    public bool IsCompounding(int metalIndex)
    {
        if (!CanCompound(metalIndex)) return false;
        if (!metalminds[metalIndex].HasCharge) return false;

        AllomancySkill.MetalType metalType = MetalIndexToAllomanticType(metalIndex);
        return allomancer.IsMetalBurning(metalType);
    }

    /// <summary>
    /// Get all metals this character can Compound.
    /// </summary>
    public bool[] GetCompoundableMetals()
    {
        bool[] result = new bool[MetalmindCount];
        for (int i = 0; i < MetalmindCount; i++)
        {
            result[i] = CanCompound(i);
        }
        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // METALMIND MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Directly add charge to a metalmind (e.g., from Compounding output).
    /// </summary>
    public void AddCharge(int metalIndex, float amount)
    {
        if (metalIndex < 0 || metalIndex >= MetalmindCount) return;
        metalminds[metalIndex].currentCharge = Mathf.Min(
            metalminds[metalIndex].currentCharge + amount,
            metalminds[metalIndex].maxCapacity
        );
    }

    /// <summary>
    /// Directly drain charge from a metalmind.
    /// </summary>
    public void DrainCharge(int metalIndex, float amount)
    {
        if (metalIndex < 0 || metalIndex >= MetalmindCount) return;
        metalminds[metalIndex].currentCharge = Mathf.Max(
            0f,
            metalminds[metalIndex].currentCharge - amount
        );
    }

    /// <summary>
    /// Unlock a Feruchemical metal for this character.
    /// </summary>
    public void UnlockMetal(int metalIndex)
    {
        if (metalIndex < 0 || metalIndex >= MetalmindCount) return;
        unlockedMetals[metalIndex] = true;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // UTILITY
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Maps Feruchemist metal index (0-15) to the corresponding AllomancySkill.MetalType.
    /// Indices 0-7 map directly. Indices 8-9 map to Gold/Electrum (skipping Atium/Malatium
    /// which are god metals without Feruchemical equivalents). Indices 10-15 map to the
    /// remaining standard metals.
    /// </summary>
    public static AllomancySkill.MetalType MetalIndexToAllomanticType(int metalIndex)
    {
        switch (metalIndex)
        {
            case 0:  return AllomancySkill.MetalType.Steel;
            case 1:  return AllomancySkill.MetalType.Iron;
            case 2:  return AllomancySkill.MetalType.Pewter;
            case 3:  return AllomancySkill.MetalType.Tin;
            case 4:  return AllomancySkill.MetalType.Zinc;
            case 5:  return AllomancySkill.MetalType.Brass;
            case 6:  return AllomancySkill.MetalType.Copper;
            case 7:  return AllomancySkill.MetalType.Bronze;
            case 8:  return AllomancySkill.MetalType.Gold;
            case 9:  return AllomancySkill.MetalType.Electrum;
            case 10: return AllomancySkill.MetalType.Chromium;
            case 11: return AllomancySkill.MetalType.Nicrosil;
            case 12: return AllomancySkill.MetalType.Aluminum;
            case 13: return AllomancySkill.MetalType.Duralumin;
            case 14: return AllomancySkill.MetalType.Bendalloy;
            case 15: return AllomancySkill.MetalType.Cadmium;
            default: return AllomancySkill.MetalType.Steel;
        }
    }

    /// <summary>
    /// Maps AllomancySkill.MetalType to Feruchemist metal index.
    /// Returns -1 for god metals (Atium, Malatium) which have no Feruchemical equivalent.
    /// </summary>
    public static int AllomanticTypeToMetalIndex(AllomancySkill.MetalType metalType)
    {
        switch (metalType)
        {
            case AllomancySkill.MetalType.Steel:     return 0;
            case AllomancySkill.MetalType.Iron:      return 1;
            case AllomancySkill.MetalType.Pewter:     return 2;
            case AllomancySkill.MetalType.Tin:        return 3;
            case AllomancySkill.MetalType.Zinc:       return 4;
            case AllomancySkill.MetalType.Brass:      return 5;
            case AllomancySkill.MetalType.Copper:     return 6;
            case AllomancySkill.MetalType.Bronze:     return 7;
            case AllomancySkill.MetalType.Gold:       return 8;
            case AllomancySkill.MetalType.Electrum:   return 9;
            case AllomancySkill.MetalType.Chromium:   return 10;
            case AllomancySkill.MetalType.Nicrosil:   return 11;
            case AllomancySkill.MetalType.Aluminum:   return 12;
            case AllomancySkill.MetalType.Duralumin:  return 13;
            case AllomancySkill.MetalType.Bendalloy:  return 14;
            case AllomancySkill.MetalType.Cadmium:    return 15;
            case AllomancySkill.MetalType.Atium:      return -1; // God metal
            case AllomancySkill.MetalType.Malatium:   return -1; // God metal
            default: return -1;
        }
    }

    /// <summary>
    /// Get a lore-accurate description of what storing/tapping each attribute feels like.
    /// </summary>
    public static string GetAttributeDescription(FeruchemicalAttribute attr)
    {
        switch (attr)
        {
            case FeruchemicalAttribute.Speed:         return "Physical speed — storing makes you sluggish, tapping makes you a blur";
            case FeruchemicalAttribute.Weight:        return "Body weight — storing makes you lighter, tapping makes you heavier";
            case FeruchemicalAttribute.Strength:      return "Physical strength — storing weakens you, tapping grants immense power";
            case FeruchemicalAttribute.Senses:        return "Sensory acuity — storing dulls senses, tapping sharpens them beyond normal";
            case FeruchemicalAttribute.MentalSpeed:   return "Cognitive speed — storing makes you slow-witted, tapping accelerates thought";
            case FeruchemicalAttribute.Warmth:        return "Body heat — storing makes you cold, tapping warms you";
            case FeruchemicalAttribute.Memories:      return "Memories — storing removes memories temporarily, tapping recalls with perfect clarity";
            case FeruchemicalAttribute.Wakefulness:   return "Wakefulness — storing makes you drowsy, tapping keeps you alert for days";
            case FeruchemicalAttribute.Health:        return "Health and vitality — storing makes you sickly, tapping heals wounds rapidly";
            case FeruchemicalAttribute.Determination: return "Determination — storing makes you apathetic, tapping fills you with resolve";
            case FeruchemicalAttribute.Fortune:       return "Fortune — storing brings bad luck, tapping brings good luck";
            case FeruchemicalAttribute.Investiture:   return "Investiture — storing reduces magical power, tapping amplifies it";
            case FeruchemicalAttribute.Identity:      return "Identity — storing blurs your sense of self, tapping solidifies it";
            case FeruchemicalAttribute.Connection:    return "Connection — storing isolates you, tapping lets you bond with others instantly";
            case FeruchemicalAttribute.Breath:        return "Breath — storing makes breathing labored, tapping lets you hold breath indefinitely";
            case FeruchemicalAttribute.Energy:        return "Energy — storing makes you fatigued, tapping gives boundless stamina";
            default: return "Unknown attribute";
        }
    }
}
