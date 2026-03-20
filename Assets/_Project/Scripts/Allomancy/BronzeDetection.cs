// ============================================================
// FILE: BronzeDetection.cs
// SYSTEM: Allomancy
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Implements Bronze Allomancy — detects when nearby Allomancers
//   are burning metals.
//
// LORE:
//   "An unskilled Seeker only knows someone is burning something.
//    A skilled Seeker can identify which specific metal." — Coppermind
//
// TODO:
//   - Add skill system for identifying specific metals
//   - Add visual/audio feedback for detection
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.Allomancy
{
    public class BronzeDetection : MonoBehaviour
    {
        [Header("Detection Settings")]
        public float detectionRange = 30f;
        public float detectionInterval = 0.5f;
        public bool canIdentifyMetal = false; // Skill upgrade
        
        [Header("Audio")]
        public AudioClip detectionSound;
        
        private float detectionTimer;
        private List<DetectedAllomancer> detectedAllomancers = new List<DetectedAllomancer>();
        
        private class DetectedAllomancer
        {
            public GameObject target;
            public AllomanticMetal? activeMetal;
            public float distance;
            public bool isInCopperCloud;
        }
        
        private void Update()
        {
            detectionTimer -= Time.deltaTime;
            if (detectionTimer > 0) return;
            
            detectionTimer = detectionInterval;
            ScanForAllomancers();
        }
        
        private void ScanForAllomancers()
        {
            detectedAllomancers.Clear();
            
            // Find all Allomancers in range
            AllomancerController[] allomancers = Object.FindObjectsOfType<AllomancerController>();
            
            foreach (AllomancerController allomancer in allomancers)
            {
                if (allomancer.gameObject == gameObject) continue; // Don't detect self
                
                float distance = Vector3.Distance(transform.position, allomancer.transform.position);
                if (distance > detectionRange) continue;
                
                // Check if in coppercloud
                bool inCloud = CopperCloud.IsInAnyCopperCloud(allomancer.transform.position);
                
                // Find what metal is being burned
                AllomanticMetal? activeMetal = null;
                foreach (MetalReserve reserve in allomancer.reserves)
                {
                    if (reserve.isBurning)
                    {
                        activeMetal = reserve.metalType;
                        if (!canIdentifyMetal) break; // Unskilled seeker can't tell which metal
                    }
                }
                
                DetectedAllomancer detected = new DetectedAllomancer
                {
                    target = allomancer.gameObject,
                    activeMetal = activeMetal,
                    distance = distance,
                    isInCopperCloud = inCloud
                };
                
                detectedAllomancers.Add(detected);
                
                if (!inCloud)
                {
                    OnAllomancerDetected(detected);
                }
            }
        }
        
        private void OnAllomancerDetected(DetectedAllomancer detected)
        {
            Debug.Log($"Detected Allomancer at {detected.distance}m");
            
            if (detected.activeMetal.HasValue)
            {
                Debug.Log($"Burning: {detected.activeMetal.Value}");
            }
            else
            {
                Debug.Log("Burning unknown metal");
            }
            
            // TODO: Trigger alert behavior
            // TODO: Play sound
            // TODO: Show UI indicator
        }
        
        public List<DetectedAllomancer> GetDetectedAllomancers()
        {
            // Filter out those in copperclouds
            List<DetectedAllomancer> visible = new List<DetectedAllomancer>();
            foreach (DetectedAllomancer detected in detectedAllomancers)
            {
                if (!detected.isInCopperCloud)
                {
                    visible.Add(detected);
                }
            }
            return visible;
        }
        
        public bool HasDetectedAnyAllomancer()
        {
            foreach (DetectedAllomancer detected in detectedAllomancers)
            {
                if (!detected.isInCopperCloud)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
