using System.Collections.Generic;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for managing combat combos in Mistborn
    /// </summary>
    public static class ComboSystemUtils
    {
        /// <summary>
        /// Tracks a combo sequence
        /// </summary>
        public static bool ValidateComboSequence(List<string> inputSequence, List<string> requiredSequence)
        {
            if (inputSequence == null || requiredSequence == null) return false;
            if (inputSequence.Count < requiredSequence.Count) return false;
            
            for (int i = 0; i < requiredSequence.Count; i++)
            {
                if (inputSequence[inputSequence.Count - requiredSequence.Count + i] != requiredSequence[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Calculates combo damage multiplier
        /// </summary>
        public static float CalculateDamageMultiplier(int comboCount, float baseMultiplier = 0.1f, float maxMultiplier = 3f)
        {
            float multiplier = 1f + (comboCount * baseMultiplier);
            return Mathf.Min(multiplier, maxMultiplier);
        }

        /// <summary>
        /// Calculates combo time window (time before combo resets)
        /// </summary>
        public static float CalculateComboWindow(int comboCount, float baseWindow = 2f, float minWindow = 0.5f)
        {
            float window = baseWindow - (comboCount * 0.1f);
            return Mathf.Max(window, minWindow);
        }

        /// <summary>
        /// Gets a combo rank based on combo count
        /// </summary>
        public static string GetComboRank(int comboCount)
        {
            if (comboCount >= 50) return "S";
            if (comboCount >= 30) return "A";
            if (comboCount >= 20) return "B";
            if (comboCount >= 10) return "C";
            if (comboCount >= 5) return "D";
            return "E";
        }

        /// <summary>
        /// Calculates combo score
        /// </summary>
        public static int CalculateComboScore(int comboCount, int baseScore = 100)
        {
            return comboCount * baseScore * Mathf.Max(1, comboCount / 10);
        }

        /// <summary>
        /// Checks if a combo should be considered "perfect" (no missed inputs)
        /// </summary>
        public static bool IsPerfectCombo(List<bool> inputHits)
        {
            if (inputHits == null || inputHits.Count == 0) return false;
            
            foreach (bool hit in inputHits)
            {
                if (!hit) return false;
            }
            return true;
        }

        /// <summary>
        /// Calculates combo efficiency (percentage of successful inputs)
        /// </summary>
        public static float CalculateComboEfficiency(List<bool> inputHits)
        {
            if (inputHits == null || inputHits.Count == 0) return 0f;
            
            int successfulHits = 0;
            foreach (bool hit in inputHits)
            {
                if (hit) successfulHits++;
            }
            
            return (float)successfulHits / inputHits.Count;
        }

        /// <summary>
        /// Gets visual effect intensity based on combo count
        /// </summary>
        public static float GetComboEffectIntensity(int comboCount)
        {
            if (comboCount >= 50) return 2f;
            if (comboCount >= 30) return 1.5f;
            if (comboCount >= 20) return 1.2f;
            if (comboCount >= 10) return 1f;
            if (comboCount >= 5) return 0.8f;
            return 0.5f;
        }

        /// <summary>
        /// Gets camera shake intensity based on combo count
        /// </summary>
        public static float GetCameraShakeIntensity(int comboCount)
        {
            if (comboCount >= 50) return 0.3f;
            if (comboCount >= 30) return 0.2f;
            if (comboCount >= 20) return 0.15f;
            if (comboCount >= 10) return 0.1f;
            if (comboCount >= 5) return 0.05f;
            return 0f;
        }

        /// <summary>
        /// Generates a random combo sequence for practice
        /// </summary>
        public static List<string> GenerateRandomComboSequence(int length, string[] possibleMoves)
        {
            List<string> sequence = new List<string>();
            for (int i = 0; i < length; i++)
            {
                sequence.Add(possibleMoves[Random.Range(0, possibleMoves.Length)]);
            }
            return sequence;
        }

        /// <summary>
        /// Calculates combo stability (how likely to maintain combo)
        /// </summary>
        public static float CalculateComboStability(int comboCount, float baseStability = 0.9f, float decayRate = 0.01f)
        {
            float stability = baseStability - (comboCount * decayRate);
            return Mathf.Clamp01(stability);
        }

        /// <summary>
        /// Gets a combo title based on combo count and type
        /// </summary>
        public static string GetComboTitle(int comboCount, string metalType = "Steel")
        {
            if (comboCount >= 100) return $"Legendary {metalType} Master";
            if (comboCount >= 50) return $"Expert {metalType} Brawler";
            if (comboCount >= 30) return $"Skilled {metalType} Fighter";
            if (comboCount >= 20) return $"{metalType} Warrior";
            if (comboCount >= 10) return $"{metalType} Novice";
            return "Apprentice";
        }

        /// <summary>
        /// Calculates combo energy regen bonus
        /// </summary>
        public static float CalculateEnergyRegenBonus(int comboCount, float baseRegen = 1f, float bonusPerCombo = 0.05f)
        {
            return baseRegen + (comboCount * bonusPerCombo);
        }
    }
}
