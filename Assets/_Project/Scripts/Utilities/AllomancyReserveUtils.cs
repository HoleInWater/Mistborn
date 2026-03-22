using System.Collections.Generic;
using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for managing Allomancy metal reserves in Mistborn
    /// </summary>
    public static class AllomancyReserveUtils
    {
        private static Dictionary<string, float> metalReserves = new Dictionary<string, float>();
        
        /// <summary>
        /// Initializes a metal reserve
        /// </summary>
        public static void InitializeReserve(string metalName, float maxAmount)
        {
            if (!metalReserves.ContainsKey(metalName))
            {
                metalReserves[metalName] = maxAmount;
            }
        }

        /// <summary>
        /// Gets the current amount of a metal reserve
        /// </summary>
        public static float GetReserve(string metalName)
        {
            return metalReserves.TryGetValue(metalName, out float amount) ? amount : 0f;
        }

        /// <summary>
        /// Sets the current amount of a metal reserve
        /// </summary>
        public static void SetReserve(string metalName, float amount)
        {
            if (metalReserves.ContainsKey(metalName))
            {
                metalReserves[metalName] = Mathf.Max(0f, amount);
            }
        }

        /// <summary>
        /// Drains metal from a reserve
        /// </summary>
        public static bool DrainMetal(string metalName, float amount)
        {
            if (!metalReserves.ContainsKey(metalName)) return false;
            
            float current = metalReserves[metalName];
            if (current < amount) return false;
            
            metalReserves[metalName] = current - amount;
            return true;
        }

        /// <summary>
        /// Refills a metal reserve
        /// </summary>
        public static void RefillMetal(string metalName, float amount, float maxAmount)
        {
            if (metalReserves.ContainsKey(metalName))
            {
                metalReserves[metalName] = Mathf.Min(metalReserves[metalName] + amount, maxAmount);
            }
        }

        /// <summary>
        /// Checks if a metal reserve has enough metal
        /// </summary>
        public static bool HasEnoughMetal(string metalName, float requiredAmount)
        {
            return metalReserves.TryGetValue(metalName, out float amount) && amount >= requiredAmount;
        }

        /// <summary>
        /// Gets the percentage of a metal reserve remaining (0-1)
        /// </summary>
        public static float GetReservePercentage(string metalName, float maxAmount)
        {
            if (!metalReserves.ContainsKey(metalName) || maxAmount <= 0f) return 0f;
            return Mathf.Clamp01(metalReserves[metalName] / maxAmount);
        }

        /// <summary>
        /// Gets all metal reserves
        /// </summary>
        public static Dictionary<string, float> GetAllReserves()
        {
            return new Dictionary<string, float>(metalReserves);
        }

        /// <summary>
        /// Clears all metal reserves
        /// </summary>
        public static void ClearAllReserves()
        {
            metalReserves.Clear();
        }

        /// <summary>
        /// Generates a random amount of metal for a pickup
        /// </summary>
        public static float GenerateRandomMetalAmount(float min = 10f, float max = 50f)
        {
            return Random.Range(min, max);
        }

        /// <summary>
        /// Gets the burn rate for a metal based on skill level
        /// </summary>
        public static float GetBurnRate(string metalName, int skillLevel)
        {
            float baseRate = 1f;
            float skillBonus = skillLevel * 0.1f;
            return baseRate + skillBonus;
        }

        /// <summary>
        /// Calculates total metal cost for an allomantic ability
        /// </summary>
        public static float CalculateMetalCost(float baseCost, float duration, float burnRate)
        {
            return baseCost * duration * burnRate;
        }

        /// <summary>
        /// Gets the metal type from a string (handles case-insensitivity)
        /// </summary>
        public static string NormalizeMetalName(string metalName)
        {
            if (string.IsNullOrEmpty(metalName)) return "";
            
            // Convert to lowercase for consistency
            string normalized = metalName.ToLower();
            
            // Handle common misspellings
            switch (normalized)
            {
                case "steele": return "steel";
                case "ironn": return "iron";
                case "pewterrr": return "pewter";
                case "tinn": return "tin";
                default: return normalized;
            }
        }

        /// <summary>
        /// Checks if two metals are compatible (for alloy creation)
        /// </summary>
        public static bool AreMetalsCompatible(string metal1, string metal2)
        {
            // Define compatible pairs for alloys
            Dictionary<string, string[]> compatiblePairs = new Dictionary<string, string[]>
            {
                { "iron", new[] { "steel" } },
                { "steel", new[] { "iron" } },
                { "tin", new[] { "pewter" } },
                { "pewter", new[] { "tin" } },
                { "zinc", new[] { "brass" } },
                { "brass", new[] { "zinc" } },
                { "copper", new[] { "bronze" } },
                { "bronze", new[] { "copper" } }
            };
            
            metal1 = NormalizeMetalName(metal1);
            metal2 = NormalizeMetalName(metal2);
            
            if (compatiblePairs.TryGetValue(metal1, out string[] compatibles))
            {
                foreach (string compatible in compatibles)
                {
                    if (compatible == metal2) return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Gets the alloy name from two metals
        /// </summary>
        public static string GetAlloyName(string metal1, string metal2)
        {
            metal1 = NormalizeMetalName(metal1);
            metal2 = NormalizeMetalName(metal2);
            
            if (AreMetalsCompatible(metal1, metal2))
            {
                // Sort alphabetically for consistent naming
                string[] metals = new string[] { metal1, metal2 };
                System.Array.Sort(metals);
                return $"{metals[0]}-{metals[1]} alloy";
            }
            
            return "Unknown alloy";
        }

        /// <summary>
        /// Gets all known metal types in Mistborn
        /// </summary>
        public static string[] GetKnownMetalTypes()
        {
            return new string[]
            {
                "iron", "steel", "tin", "pewter", "zinc", "brass", "copper", "bronze",
                "atium", "malatium", "gold", "electrum", "aluminum", "duralumin",
                "bendalloy", "cadmium"
            };
        }

        /// <summary>
        /// Checks if a metal is a basic metal (not a god metal)
        /// </summary>
        public static bool IsBasicMetal(string metalName)
        {
            string[] basicMetals = new string[]
            {
                "iron", "steel", "tin", "pewter", "zinc", "brass", "copper", "bronze"
            };
            
            metalName = NormalizeMetalName(metalName);
            foreach (string metal in basicMetals)
            {
                if (metal == metalName) return true;
            }
            return false;
        }
    }
}
