using System.Collections.Generic;
using UnityEngine;

namespace Mistborn.Allomancy
{
    public class IronPullAbility : MonoBehaviour
    {
        [Header("Pull Settings")]
        [SerializeField] private float pullForce = 500f;
        [SerializeField] private float pullRange = 30f;
        [SerializeField] private int maxTargets = 5;
        [SerializeField] private KeyCode activationKey = KeyCode.Mouse0;

        [Header("Physics")]
        [SerializeField] private float playerWeight = 80f;
        [SerializeField] private float anchorMassThreshold = 10f;
        [SerializeField] private float recoilMultiplier = 0.5f;

        private AllomancerController allomancer;
        private Rigidbody playerRigidbody;
        private List<AllomanticTarget> currentTargets = new List<AllomanticTarget>();

        public float PullRange => pullRange;
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

            if (Input.GetKey(activationKey) && allomancer.CanBurn(AllomanticMetal.Iron))
            {
                if (!allomancer.GetReserve(AllomanticMetal.Iron).IsBurning)
                    allomancer.StartBurning(AllomanticMetal.Iron);

                FindMetalTargets();
                PullAllTargets();
            }
            else if (allomancer.GetReserve(AllomanticMetal.Iron)?.IsBurning == true)
            {
                allomancer.StopBurning(AllomanticMetal.Iron);
                currentTargets.Clear();
            }
        }

        public void FindMetalTargets()
        {
            currentTargets.Clear();
            int count = 0;

            foreach (Collider hit in Physics.OverlapSphere(transform.position, pullRange))
            {
                if (count >= maxTargets) break;

                if (hit.TryGetComponent(out AllomanticTarget target) && target.MetalType == AllomanticMetal.Iron)
                {
                    currentTargets.Add(target);
                    count++;
                }
            }
        }

        public void PullAllTargets()
        {
            foreach (AllomanticTarget target in currentTargets)
            {
                PullTarget(target);
            }
        }

        public void PullTarget(AllomanticTarget target)
        {
            if (target == null || target.Rigidbody == null) return;

            Vector3 pullDirection = target.GetPullDirection(transform.position);

            if (target.IsAnchored || target.MetalMass > anchorMassThreshold)
            {
                ApplyRecoilToPlayer(-pullDirection);
            }
            else
            {
                target.Rigidbody.AddForce(pullDirection * pullForce);
            }
        }

        public void ApplyRecoilToPlayer(Vector3 direction)
        {
            if (playerRigidbody != null)
            {
                playerRigidbody.AddForce(direction * pullForce * recoilMultiplier);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pullRange);
        }
    }
}
