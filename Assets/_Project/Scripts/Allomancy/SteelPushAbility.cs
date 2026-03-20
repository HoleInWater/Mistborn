// ============================================================
// FILE: SteelPushAbility.cs
// SYSTEM: Allomancy
// STATUS: IMPLEMENTED — Uses physics formulas
// AUTHOR: 
//
// PURPOSE:
//   Implements Steel Allomancy (Steelpushing) - pushes metal objects
//   away from the Allomancer. Heavier/anchored objects cause the
//   Allomancer to move instead (Newton's 3rd Law).
//
// LORE:
//   "A steel Misting is known as a Coinshot... pushes on nearby metals." — Coppermind
//   "The strength of your push is roughly proportional to your physical weight." — Coppermind
//
// DEPENDENCIES:
//   - AllomancerController
//   - AllomanticTarget
//   - AllomanticMetal.Steel
//   - AllomancyPhysicsFormulas
//
// RANGE:
//   ~100 paces (~75m) max, with distance falloff
//
// TODO (AI Agent):
//   - Add audio feedback
//
// TODO (Team):
//   - Tune base push force
//   - Adjust mass threshold for "anchored" behavior
//
// LAST UPDATED: 2026-03-20
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using Mistborn.Allomancy.Physics;

namespace Mistborn.Allomancy
{
    public class SteelPushAbility : MonoBehaviour
    {
        [Header("Steel — Steelpush")]
        public float basePushForce = 500f;
        public float pushRange = 30f;
        public int maxTargets = 5;
        public KeyCode activationKey = KeyCode.Mouse1;
        
        [Header("Physics Settings")]
        public float playerWeight = 80f;
        public float anchorMassThreshold = 50f;
        public bool useFlaring = true;

        private AllomancerController allomancer;
        private Rigidbody playerRb;
        private List<AllomanticTarget> currentTargets = new List<AllomanticTarget>();

        private void Start()
        {
            allomancer = GetComponent<AllomancerController>();
            playerRb = GetComponent<Rigidbody>();
            
            if (allomancer == null)
            {
                Debug.LogError("SteelPushAbility requires AllomancerController on same GameObject");
            }
        }

        private void Update()
        {
            bool isFlaring = useFlaring && Input.GetKey(KeyCode.LeftShift);
            
            if (Input.GetKey(activationKey) && allomancer.CanBurn(AllomanticMetal.Steel))
            {
                if (!allomancer.GetReserve(AllomanticMetal.Steel).isBurning)
                {
                    allomancer.StartBurning(AllomanticMetal.Steel);
                }
                FindMetalTargetsInRange();
                PushAllTargets(isFlaring);
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

        public void PushTarget(AllomanticTarget target, bool isFlaring)
        {
            if (target == null || target.rb == null) return;

            float distance = Vector3.Distance(transform.position, target.transform.position);
            bool isAnchored = target.isAnchored || target.metalMass > anchorMassThreshold;
            
            float force = AllomancyPhysicsFormulas.CalculatePushForce(
                playerWeight,
                target.metalMass,
                distance,
                isAnchored,
                isFlaring
            );
            
            Vector3 pushDirection = target.GetPushDirection(transform.position);
            
            if (isAnchored)
            {
                ApplyRecoilToPlayer(pushDirection, force);
            }
            else
            {
                target.rb.AddForce(pushDirection * force, ForceMode.Impulse);
            }
        }

        public void ApplyRecoilToPlayer(Vector3 direction, float force)
        {
            if (playerRb != null)
            {
                float recoil = AllomancyPhysicsFormulas.CalculateRecoilForce(force, playerWeight, 100f);
                playerRb.AddForce(direction * recoil, ForceMode.Impulse);
            }
        }

        private void PushAllTargets(bool isFlaring)
        {
            foreach (AllomanticTarget target in currentTargets)
            {
                PushTarget(target, isFlaring);
            }
        }
    }
}
