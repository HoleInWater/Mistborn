// ============================================================
// FILE: AllomanticMetal.cs
// SYSTEM: Allomancy
// STATUS: STUB — Not yet implemented
// AUTHOR: 
//
// PURPOSE:
//   Enum defining all 16 metals used in Mistborn Era One Allomancy.
//   Metals are grouped by their Allomantic effects (push/pull pairs).
//
// DEPENDENCIES:
//   - Referenced by MetalReserve, SteelPushAbility, IronPullAbility
//
// TODO:
//   - Implement full burn rate modifiers per metal
//   - Add XML documentation for visual metal types (Bendalloy, Cadmium)
//
// TODO (Team):
//   - Confirm burn rate balance for Pewter enhancement
//   - Decide on Atium implementation (future sprint)
//
// LAST UPDATED: 2026-03-20
// ============================================================

namespace Mistborn.Allomancy
{
    /// <summary>
    /// All 16 Allomantic metals from Mistborn Era One.
    /// Grouped by effect type: Physical, Enhancement, Mental, Temporal, and Noble metals.
    /// </summary>
    public enum AllomanticMetal
    {
        // ═══════════════════════════════════════════════════════
        // PHYSICAL METALS — External push/pull effects on metal objects
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// ACTIVE — Sprint 1
        /// Pushes outward on nearby metal objects.
        /// Creates a propulsive force away from the Allomancer.
        /// Key uses: launching coins, aerial traversal, disarming enemies.
        /// </summary>
        Steel,

        /// <summary>
        /// ACTIVE — Sprint 1
        /// Pulls metal objects toward the Allomancer.
        /// Creates an attractive force toward the user.
        /// Key uses: disarming, self-transportation toward anchors, catching projectiles.
        /// </summary>
        Iron,

        // ═══════════════════════════════════════════════════════
        // PHYSICAL ENHANCEMENT METALS — Internal bodily effects
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Enhances the Allomancer's physical strength and durability.
        /// User becomes stronger and more resistant to damage.
        /// </summary>
        Pewter,

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Enhances the Allomancer's senses (sight, hearing, etc.).
        /// User becomes more perceptive and aware.
        /// </summary>
        Tin,

        // ═══════════════════════════════════════════════════════
        // MENTAL METALS — Effects on other minds
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Soothes emotional states in others.
        /// Reduces anger, fear, aggression in target individuals.
        /// </summary>
        Brass,

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Provokes emotional states in others.
        /// Increases anger, fear, passion in target individuals.
        /// </summary>
        Zinc,

        // ═══════════════════════════════════════════════════════
        // COGNITIVE METALS — Enhancement of mental abilities
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Masks the Allomancer from detection by Bronze Allomancy.
        /// Creates a "bubble" of silence around the user.
        /// </summary>
        Copper,

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Allows the Allomancer to detect when someone is burning metals nearby.
        /// "Soother/ Rioter detection" ability.
        /// </summary>
        Bronze,

        // ═══════════════════════════════════════════════════════
        // TEMPORAL METALS — Time-related effects
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Allows the Allomancer to see possible futures.
        /// User sees ghosts/afterimages of their possible selves.
        /// </summary>
        Gold,

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Grants access to alternate possible futures.
        /// User can see outcomes of different choices.
        /// </summary>
        Electrum,

        /// <summary>
        /// INACTIVE — Future Sprint
        /// The rarest and most powerful metal.
        /// Grants complete future sight and enhanced physical abilities.
        /// </summary>
        Atium,

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Allows the Allomancer to see the true nature of things.
        /// Reveals hidden truths and actual futures.
        /// </summary>
        Malatium,

        // ═══════════════════════════════════════════════════════
        // NOBLE METALS — Rare metals with unique properties
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Drains all metals in the Allomancer simultaneously.
        /// </summary>
        Aluminum,

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Drains the metal reserves of others when burned.
        /// </summary>
        Duralumin,

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Stores the Allomancer's own metals temporarily.
        /// </summary>
        Chromium,

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Stores others' metal reserves temporarily.
        /// </summary>
        Nicrosil,

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Creates a localized time distortion bubble.
        /// </summary>
        Bendalloy,

        /// <summary>
        /// INACTIVE — Future Sprint
        /// Creates a time compression bubble.
        /// </summary>
        Cadmium
    }
}
