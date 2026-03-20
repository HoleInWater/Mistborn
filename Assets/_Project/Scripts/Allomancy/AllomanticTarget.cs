// ============================================================
// FILE: AllomanticTarget.cs
// SYSTEM: Allomancy
// STATUS: STUB — Not yet implemented
// AUTHOR: 
//
// PURPOSE:
//   Marks a game object as a metal target detectable by Allomancers.
//   Contains physics data and directional calculations for push/pull.
//
// DEPENDENCIES:
//   - AllomanticMetal enum
//   - Requires Rigidbody component
//
// TODO:
//   - Add visual editor customization
//   - Implement mass-based force calculations
//
// TODO (Team):
//   - Define standard mass values for different metal objects
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Allomancy
{
    [RequireComponent(typeof(Rigidbody))]
    public class AllomanticTarget : MonoBehaviour
    {
        [Header("Allomantic Properties")]
        public AllomanticMetal metalType = AllomanticMetal.Steel;
        public bool isAnchored = false;
        public float metalMass = 1f;

        public Rigidbody rb { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (metalMass <= 0)
            {
                metalMass = rb.mass;
            }
        }

        public Vector3 GetPushDirection(Vector3 fromPosition)
        {
            return (transform.position - fromPosition).normalized;
        }

        public Vector3 GetPullDirection(Vector3 toPosition)
        {
            return (toPosition - transform.position).normalized;
        }
    }
}
