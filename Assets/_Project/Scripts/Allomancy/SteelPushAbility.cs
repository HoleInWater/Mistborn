// ============================================================
// FILE: SteelPushAbility.cs
// SYSTEM: Allomancy
// STATUS: STUB — Not yet implemented
// AUTHOR: 
//
// PURPOSE:
//   Implements Steel Allomancy (Steelpushing) - pushes metal objects
//   away from the Allomancer. Heavier/anchored objects cause the
//   Allomancer to move instead (Newton's 3rd Law).
//
// DEPENDENCIES:
//   - AllomancerController
//   - AllomanticTarget
//   - AllomanticMetal.Steel
//
// TODO (AI Agent):
//   - Implement visual effects for steel push
//   - Add audio feedback
//
// TODO (Team):
//   - Tune push force values
//   - Define max targets limit
//
// LAST UPDATED: 2026-03-20
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace Mistborn.Allomancy
{
    public class SteelPushAbility : MonoBehaviour
    {
        [Header("Steel — Steelpush")]
        public float pushForce = 500f;
        public float pushRange = 30f;
        public int maxTargets = 5;
        public KeyCode activationKey = KeyCode.Mouse1;

        private AllomancerController allomancer;
        private List<AllomanticTarget> currentTargets = new List<AllomanticTarget>();

        private void Start()
        {
            allomancer = GetComponent<AllomancerController>();
            if (allomancer == null)
            {
                Debug.LogError("SteelPushAbility requires AllomancerController on same GameObject");
            }
        }

        private void Update()
        {
            if (Input.GetKey(activationKey) && allomancer.CanBurn(AllomanticMetal.Steel))
            {
                if (!allomancer.GetReserve(AllomanticMetal.Steel).isBurning)
                {
                    allomancer.StartBurning(AllomanticMetal.Steel);
                }
                FindMetalTargetsInRange();
                PushAllTargets();
            }
            else if (allomancer.GetReserve(AllomanticMetal.Steel)?.isBurning == true)
            {
                allomancer.StopBurning(AllomanticMetal.Steel);
                currentTargets.Clear();
            }
        }

        public void FindMetalTargetsInRange()
        {
            currentTargets.Clear();
            
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, pushRange);
            
            int targetCount = 0;
            foreach (Collider hit in hitColliders)
            {
                AllomanticTarget target = hit.GetComponent<AllomanticTarget>();
                if (target != null && target.metalType == AllomanticMetal.Steel && targetCount < maxTargets)
                {
                    currentTargets.Add(target);
                    targetCount++;
                }
            }
        }

        public void PushTarget(AllomanticTarget target)
        {
            if (target == null || target.rb == null) return;

            Vector3 pushDirection = target.GetPushDirection(transform.position);
            
            if (target.isAnchored || target.metalMass > 10f)
            {
                ApplyRecoilToPlayer(pushDirection);
            }
            else
            {
                target.rb.AddForce(pushDirection * pushForce);
            }
        }

        public void ApplyRecoilToPlayer(Vector3 direction)
        {
            Rigidbody playerRb = GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(direction * pushForce * 0.5f);
            }
        }

        private void PushAllTargets()
        {
            foreach (AllomanticTarget target in currentTargets)
            {
                PushTarget(target);
            }
        }
    }
}
