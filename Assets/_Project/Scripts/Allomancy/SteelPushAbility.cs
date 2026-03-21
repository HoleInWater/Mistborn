using System.Collections.Generic;
using UnityEngine;

namespace Mistborn.Allomancy
{
    public class SteelPushAbility : MonoBehaviour
    {
        [Header("Push Settings")]
        [SerializeField] private float pushForce = 500f;
        [SerializeField] private float pushRange = 30f;
        [SerializeField] private int maxTargets = 5;
        [SerializeField] private KeyCode activationKey = KeyCode.Mouse1;

        [Header("Physics")]
        [SerializeField] private float playerWeight = 80f;
        [SerializeField] private float anchorMassThreshold = 50f;
        [SerializeField] private bool allowFlaring = true;

        private AllomancerController allomancer;
        private Rigidbody playerRigidbody;
        private List<AllomanticTarget> currentTargets = new List<AllomanticTarget>();

        public float PushRange => pushRange;
        public List<AllomanticTarget> CurrentTargets => currentTargets;

        private void Awake()
        {
            allomancer = GetComponent<AllomancerController>();
            playerRigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (allomancer == null || !allomancer.IsInitialized)
                return;

            bool isFlaring = allowFlaring && Input.GetKey(KeyCode.LeftShift);

            if (Input.GetKey(activationKey) && allomancer.CanBurn(AllomanticMetal.Steel))
            {
                if (!allomancer.GetReserve(AllomanticMetal.Steel).IsBurning)
                    allomancer.StartBurning(AllomanticMetal.Steel);

                FindMetalTargets();
                PushAllTargets(isFlaring);
            }
            else if (allomancer.GetReserve(AllomanticMetal.Steel)?.IsBurning == true)
            {
                allomancer.StopBurning(AllomanticMetal.Steel);
                currentTargets.Clear();
            }
        }

        public void FindMetalTargets()
        {
            currentTargets.Clear();
            int count = 0;

            foreach (Collider hit in Physics.OverlapSphere(transform.position, pushRange))
            {
                if (count >= maxTargets) break;

                if (hit.TryGetComponent(out AllomanticTarget target) && target.MetalType == AllomanticMetal.Steel)
                {
                    currentTargets.Add(target);
                    count++;
                }
            }
        }

        public void PushAllTargets(bool isFlaring)
        {
            foreach (AllomanticTarget target in currentTargets)
            {
                PushTarget(target, isFlaring);
            }
        }

        public void PushTarget(AllomanticTarget target, bool isFlaring)
        {
            if (target == null || target.Rigidbody == null) return;

            float distance = Vector3.Distance(transform.position, target.transform.position);
            bool isAnchored = target.IsAnchored || target.MetalMass > anchorMassThreshold;
            float force = CalculatePushForce(target, distance, isAnchored, isFlaring);
            Vector3 pushDirection = target.GetPushDirection(transform.position);

            if (isAnchored)
            {
                ApplyRecoilToPlayer(pushDirection, force);
            }
            else
            {
                target.Rigidbody.AddForce(pushDirection * force, ForceMode.Impulse);
            }
        }

        private float CalculatePushForce(AllomanticTarget target, float distance, bool isAnchored, bool isFlaring)
        {
            float force = pushForce;

            // Distance falloff
            float distanceFactor = 1f - (distance / pushRange);
            distanceFactor = Mathf.Clamp01(distanceFactor);
            force *= distanceFactor;

            // Anchored objects push back harder
            if (isAnchored)
            {
                force *= (playerWeight / target.MetalMass) * 0.5f;
            }

            // Flaring doubles the force
            if (isFlaring)
            {
                force *= 2f;
            }

            return force;
        }

        public void ApplyRecoilToPlayer(Vector3 direction, float force)
        {
            if (playerRigidbody != null)
            {
                float recoil = (force / playerWeight) * 0.5f;
                playerRigidbody.AddForce(direction * recoil, ForceMode.Impulse);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, pushRange);
        }
    }
}
