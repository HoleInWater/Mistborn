// ============================================================
// FILE: IronPullAbility.cs
// SYSTEM: Allomancy
// STATUS: STUB — Not yet implemented
// AUTHOR: 
//
// PURPOSE:
//   Implements Iron Allomancy (Ironpulling) - pulls metal objects
//   toward the Allomancer. Heavier/anchored objects cause the
//   Allomancer to move instead (Newton's 3rd Law).
//
// DEPENDENCIES:
//   - AllomancerController
//   - AllomanticTarget
//   - AllomanticMetal.Iron
//
// TODO:
//   - Implement visual effects for iron pull
//   - Add audio feedback
//
// TODO (Team):
//   - Tune pull force values
//
// LAST UPDATED: 2026-03-20
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace Mistborn.Allomancy
{
    public class IronPullAbility : MonoBehaviour
    {
        [Header("Iron — Ironpull")]
        public float pullForce = 500f;
        public float pullRange = 30f;
        public int maxTargets = 5;
        public KeyCode activationKey = KeyCode.Mouse0;

        private AllomancerController allomancer;
        private List<AllomanticTarget> currentTargets = new List<AllomanticTarget>();

        private void Start()
        {
            allomancer = GetComponent<AllomancerController>();
            if (allomancer == null)
            {
                Debug.LogError("IronPullAbility requires AllomancerController on same GameObject");
            }
        }

        private void Update()
        {
            if (Input.GetKey(activationKey) && allomancer.CanBurn(AllomanticMetal.Iron))
            {
                if (!allomancer.GetReserve(AllomanticMetal.Iron).isBurning)
                {
                    allomancer.StartBurning(AllomanticMetal.Iron);
                }
                FindMetalTargetsInRange();
                PullAllTargets();
            }
            else if (allomancer.GetReserve(AllomanticMetal.Iron)?.isBurning == true)
            {
                allomancer.StopBurning(AllomanticMetal.Iron);
                currentTargets.Clear();
            }
        }

        public void FindMetalTargetsInRange()
        {
            currentTargets.Clear();
            
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, pullRange);
            
            int targetCount = 0;
            foreach (Collider hit in hitColliders)
            {
                AllomanticTarget target = hit.GetComponent<AllomanticTarget>();
                if (target != null && target.metalType == AllomanticMetal.Iron && targetCount < maxTargets)
                {
                    currentTargets.Add(target);
                    targetCount++;
                }
            }
        }

        public void PullTarget(AllomanticTarget target)
        {
            if (target == null || target.rb == null) return;

            Vector3 pullDirection = (transform.position - target.transform.position).normalized;
            
            if (target.isAnchored || target.metalMass > 10f)
            {
                ApplyRecoilToPlayer(-pullDirection);
            }
            else
            {
                target.rb.AddForce(pullDirection * pullForce);
            }
        }

        public void ApplyRecoilToPlayer(Vector3 direction)
        {
            Rigidbody playerRb = GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(direction * pullForce * 0.5f);
            }
        }

        private void PullAllTargets()
        {
            foreach (AllomanticTarget target in currentTargets)
            {
                PullTarget(target);
            }
        }
    }
}
