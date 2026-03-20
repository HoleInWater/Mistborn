// ============================================================
// FILE: MetalReserve.cs
// SYSTEM: Allomancy
// STATUS: STUB — Not yet implemented
// AUTHOR: 
//
// PURPOSE:
//   Serializable data class representing a single metal's reserve.
//   Tracks current amount, burn state, and consumption rate.
//
// DEPENDENCIES:
//   - AllomanticMetal enum
//   - Referenced by AllomancerController
//
// TODO (AI Agent):
//   - Implement burn rate modifiers for different metals
//   - Add flaring mechanic support
//
// TODO (Team):
//   - Define base burn rates for each metal
//
// LAST UPDATED: 2026-03-20
// ============================================================

using System;

namespace Mistborn.Allomancy
{
    [System.Serializable]
    public class MetalReserve
    {
        public AllomanticMetal metalType;
        public float currentAmount;
        public float maxAmount = 100f;
        public bool isBurning;
        public float burnRate = 1f;

        public MetalReserve(AllomanticMetal metal, float max = 100f, float burnRate = 1f)
        {
            metalType = metal;
            maxAmount = max;
            currentAmount = maxAmount;
            this.burnRate = burnRate;
            isBurning = false;
        }

        public void StartBurning()
        {
            if (currentAmount > 0)
            {
                isBurning = true;
            }
        }

        public void StopBurning()
        {
            isBurning = false;
        }

        public void Consume(float deltaTime)
        {
            if (isBurning && currentAmount > 0)
            {
                currentAmount -= burnRate * deltaTime;
                if (currentAmount <= 0)
                {
                    currentAmount = 0;
                    isBurning = false;
                }
            }
        }

        public bool IsEmpty()
        {
            return currentAmount <= 0;
        }

        public bool CanBurn()
        {
            return currentAmount > 0;
        }

        public void Refill()
        {
            currentAmount = maxAmount;
        }
    }
}
