// ============================================================
// FILE: AllomancerController.cs
// SYSTEM: Allomancy
// STATUS: STUB — Not yet implemented
// AUTHOR: 
//
// PURPOSE:
//   Core component that manages all metal reserves for the player.
//   Handles reserve consumption, burning states, and metal detection.
//
// DEPENDENCIES:
//   - MetalReserve
//   - AllomanticMetal enum
//   - SteelPushAbility, IronPullAbility
//
// TODO (AI Agent):
//   - Implement event system for metal depletion
//   - Add flaring mechanic support
//
// TODO (Team):
//   - Define starting reserve amounts
//
// LAST UPDATED: 2026-03-20
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mistborn.Allomancy
{
    public class AllomancerController : MonoBehaviour
    {
        public List<MetalReserve> reserves = new List<MetalReserve>();

        public event Action<AllomanticMetal> OnMetalDepleted;
        public event Action<AllomanticMetal> OnMetalStartBurning;
        public event Action<AllomanticMetal> OnMetalStopBurning;

        private void Awake()
        {
            InitializeReserves();
        }

        private void InitializeReserves()
        {
            reserves = new List<MetalReserve>();
            
            foreach (AllomanticMetal metal in Enum.GetValues(typeof(AllomanticMetal)))
            {
                reserves.Add(new MetalReserve(metal));
            }
        }

        public MetalReserve GetReserve(AllomanticMetal metal)
        {
            return reserves.Find(r => r.metalType == metal);
        }

        public bool CanBurn(AllomanticMetal metal)
        {
            MetalReserve reserve = GetReserve(metal);
            return reserve != null && reserve.CanBurn();
        }

        public void StartBurning(AllomanticMetal metal)
        {
            MetalReserve reserve = GetReserve(metal);
            if (reserve != null && reserve.CanBurn())
            {
                reserve.StartBurning();
                OnMetalStartBurning?.Invoke(metal);
            }
        }

        public void StopBurning(AllomanticMetal metal)
        {
            MetalReserve reserve = GetReserve(metal);
            if (reserve != null)
            {
                reserve.StopBurning();
                OnMetalStopBurning?.Invoke(metal);
            }
        }

        private void Update()
        {
            foreach (MetalReserve reserve in reserves)
            {
                if (reserve.isBurning)
                {
                    reserve.Consume(Time.deltaTime);
                    
                    if (reserve.IsEmpty())
                    {
                        OnMetalDepleted?.Invoke(reserve.metalType);
                    }
                }
            }
        }
    }
}
