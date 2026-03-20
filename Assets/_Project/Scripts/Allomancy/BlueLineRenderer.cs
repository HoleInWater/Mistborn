// ============================================================
// FILE: BlueLineRenderer.cs
// SYSTEM: Allomancy / Visual Effects
// STATUS: PLANNED — For Sprint 2
// AUTHOR: 
//
// PURPOSE:
//   Renders the iconic blue Allomantic lines from player to metal targets.
//   Uses LineRenderer for smooth, performant rendering.
//
// LORE:
//   "When burning steel, blue lines emerge from the Coinshot and 
//    connect themselves to pieces of nearby metal" — Coppermind
//
//   Lines exist on the Spiritual Realm and pass through walls!
//   Line brightness indicates metal mass.
//
// VFX ASSET RECOMMENDATION:
//   Invested project uses "Volumetric Lines" by Johannes Unterguggenberger
//   https://assetstore.unity.com/packages/tools/particles-effects/volumetric-lines-29160
//
// TODO (AI Agent):
//   - Implement LineRenderer version for Sprint 1
//   - Upgrade to Volumetric Lines in Sprint 2
//   - Add line thickness based on mass
//
// TODO (Team):
//   - Choose VFX library (Volumetric Lines vs custom shader)
//   - Set default line color/brightness
//   - Decide if lines pass through walls (should, lore-wise)
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.Allomancy.VFX
{
    public class BlueLineRenderer : MonoBehaviour
    {
        [Header("Line Settings")]
        public Color lineColor = new Color(0.2f, 0.5f, 1f, 0.8f); // Blue-ish
        public float lineWidth = 0.05f;
        public float maxLineLength = 50f;
        
        [Header("Mass Scaling")]
        public float minLineWidth = 0.02f;
        public float maxLineWidth = 0.15f;
        public float minMassForMaxWidth = 50f;
        
        private LineRenderer lineRenderer;
        private List<GameObject> linePool = new List<GameObject>();
        private const int MAX_LINES = 20;
        
        private void Awake()
        {
            InitializeLinePool();
        }
        
        private void InitializeLinePool()
        {
            for (int i = 0; i < MAX_LINES; i++)
            {
                GameObject lineObj = new GameObject("AllomancyLine");
                lineObj.transform.SetParent(transform);
                lineObj.SetActive(false);
                
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = lineColor;
                lr.endColor = lineColor;
                lr.startWidth = lineWidth;
                lr.endWidth = lineWidth;
                lr.useWorldSpace = true;
                
                linePool.Add(lineObj);
            }
        }
        
        public void UpdateLines(List<AllomanticTarget> targets, Vector3 playerCenter)
        {
            // Hide all lines first
            foreach (GameObject line in linePool)
            {
                line.SetActive(false);
            }
            
            // Show lines to targets
            for (int i = 0; i < targets.Count && i < MAX_LINES; i++)
            {
                if (targets[i] == null) continue;
                
                GameObject lineObj = linePool[i];
                lineObj.SetActive(true);
                
                LineRenderer lr = lineObj.GetComponent<LineRenderer>();
                lr.SetPosition(0, playerCenter);
                lr.SetPosition(1, targets[i].transform.position);
                
                // Scale width based on target mass (lore accurate)
                float width = Mathf.Lerp(minLineWidth, maxLineWidth, 
                    targets[i].metalMass / minMassForMaxWidth);
                lr.startWidth = width;
                lr.endWidth = width * 0.5f; // Taper toward target
                
                // Brightness based on distance (closer = brighter)
                float dist = Vector3.Distance(playerCenter, targets[i].transform.position);
                float brightness = 1f - (dist / maxLineLength);
                Color displayColor = new Color(lineColor.r, lineColor.g, lineColor.b, 
                    Mathf.Clamp01(brightness));
                lr.startColor = displayColor;
                lr.endColor = new Color(displayColor.r, displayColor.g, displayColor.b, 
                    displayColor.a * 0.5f);
            }
        }
        
        public void HideAllLines()
        {
            foreach (GameObject line in linePool)
            {
                line.SetActive(false);
            }
        }
    }
}
