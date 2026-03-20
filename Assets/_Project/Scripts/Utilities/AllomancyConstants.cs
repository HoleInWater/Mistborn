// ============================================================
// FILE: AllomancyConstants.cs
// SYSTEM: Utilities
// STATUS: STUB — Not yet implemented
// AUTHOR: 
//
// PURPOSE:
//   Static class containing game-wide constants for Allomancy.
//   Centralized values for easy tuning and balance adjustments.
//
// DEPENDENCIES:
//   - Referenced by all Allomancy scripts
//
// TODO:
//   - Add constants for remaining metals
//   - Document all constants
//
// TODO (Team):
//   - Tune all values during playtesting
//
// LAST UPDATED: 2026-03-20
// ============================================================

namespace Mistborn.Utilities
{
    public static class AllomancyConstants
    {
        // Burn Rates
        public const float BASE_BURN_RATE = 1f;
        public const float STEEL_BURN_RATE = 2f;
        public const float IRON_BURN_RATE = 2f;

        // Push/Pull Forces
        public const float STEEL_PUSH_FORCE = 500f;
        public const float IRON_PULL_FORCE = 500f;

        // Detection
        public const float ALLOMANTIC_SIGHT_RANGE = 50f;
        public const float METAL_DETECTION_RANGE = 30f;

        // Thresholds
        public const float ANCHOR_MASS_THRESHOLD = 10f;
        public const float MAX_TARGETS = 5f;

        // Player
        public const float PLAYER_RECOIL_MULTIPLIER = 0.5f;
    }
}
