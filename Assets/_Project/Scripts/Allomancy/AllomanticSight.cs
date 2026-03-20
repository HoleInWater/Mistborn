// ============================================================
// FILE: AllomanticSight.cs
// SYSTEM: Allomancy
// STATUS: STUB — Not yet implemented
// AUTHOR: 
//
// PURPOSE:
//   Implements the iconic Allomantic "blue lines" visual effect.
//   Displays blue lines from the Allomancer to all metal objects in range.
//   Currently uses Debug.DrawLine; VFX upgrade planned for Sprint 2.
//
// DEPENDENCIES:
//   - AllomanticTarget
//   - Toggle on Tab key
//
// TODO (AI Agent):
//   - Replace Debug.DrawLine with LineRenderer VFX in Sprint 2
//   - Add line thickness based on metal mass
//   - Add fading based on distance
//
// TODO (Team):
//   - Define line color preferences
//   - Choose VFX library for visual upgrade
//
// LAST UPDATED: 2026-03-20
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace Mistborn.Allomancy
{
    public class AllomanticSight : MonoBehaviour
    {
        [Header("Allomantic Sight Settings")]
        public Color lineColor = Color.blue;
        public float detectionRange = 50f;
        public bool sightActive = false;

        private List<AllomanticTarget> visibleTargets = new List<AllomanticTarget>();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                sightActive = !sightActive;
            }

            if (sightActive)
            {
                UpdateVisibleTargets();
                DrawAllomanticLines();
            }
        }

        public void UpdateVisibleTargets()
        {
            visibleTargets.Clear();
            
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange);
            
            foreach (Collider hit in hitColliders)
            {
                AllomanticTarget target = hit.GetComponent<AllomanticTarget>();
                if (target != null)
                {
                    visibleTargets.Add(target);
                }
            }
        }

        public void DrawAllomanticLines()
        {
            foreach (AllomanticTarget target in visibleTargets)
            {
                if (target != null)
                {
                    float lineWidth = Mathf.Lerp(0.5f, 3f, target.metalMass / 50f);
                    Debug.DrawLine(transform.position, target.transform.position, lineColor);
                }
            }
        }
    }
}
