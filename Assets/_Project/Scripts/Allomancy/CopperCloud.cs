// ============================================================
// FILE: CopperCloud.cs
// SYSTEM: Allomancy
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Implements Copper Allomancy — creates a concealment bubble
//   that hides Allomantic activity from Bronze detection.
//
// LORE:
//   "Burning copper creates a sphere around the Smoker that 
//    hides all Allomantic activity within it from Seekers." — Coppermind
//
//   Does NOT make you invisible — only hides Allomancy.
//
// TODO:
//   - Add visual effect for the cloud edge
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Allomancy
{
    public class CopperCloud : MonoBehaviour
    {
        [Header("Settings")]
        public KeyCode activationKey = KeyCode.C;
        public float cloudRadius = 15f;
        public float burnRate = 2f;
        
        [Header("Visual")]
        public bool showCloudEdge = false;
        public Color cloudColor = new Color(0.5f, 0.3f, 0.2f, 0.1f);
        
        private AllomancerController allomancer;
        private bool isActive;
        private Renderer cloudRenderer;
        
        private void Start()
        {
            allomancer = GetComponent<AllomancerController>();
            SetupCloudVisuals();
        }
        
        private void SetupCloudVisuals()
        {
            // Create sphere visualizer for cloud edge
            GameObject cloudObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cloudObj.name = "CopperCloud";
            cloudObj.transform.SetParent(transform);
            cloudObj.transform.localPosition = Vector3.zero;
            cloudObj.transform.localScale = Vector3.one * cloudRadius * 2;
            
            // Remove collider (we only need visual)
            Destroy(cloudObj.GetComponent<Collider>());
            
            // Set material
            Renderer renderer = cloudObj.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
            renderer.material.color = cloudColor;
            renderer.enabled = false;
            
            cloudRenderer = renderer;
        }
        
        private void Update()
        {
            // Toggle coppercloud
            if (Input.GetKeyDown(activationKey))
            {
                if (isActive)
                {
                    StopCloud();
                }
                else
                {
                    StartCloud();
                }
            }
            
            // Continue burning while active
            if (isActive && allomancer != null)
            {
                allomancer.GetReserve(AllomanticMetal.Copper).Consume(Time.deltaTime * burnRate);
                
                if (allomancer.GetReserve(AllomanticMetal.Copper).IsEmpty())
                {
                    StopCloud();
                }
            }
        }
        
        public void StartCloud()
        {
            if (allomancer == null || !allomancer.CanBurn(AllomanticMetal.Copper))
            {
                Debug.Log("Cannot burn copper - no reserves");
                return;
            }
            
            isActive = true;
            allomancer.StartBurning(AllomanticMetal.Copper);
            
            if (cloudRenderer != null && showCloudEdge)
            {
                cloudRenderer.enabled = true;
            }
            
            Debug.Log("Coppercloud active - hiding Allomancy");
        }
        
        public void StopCloud()
        {
            isActive = false;
            
            if (allomancer != null)
            {
                allomancer.StopBurning(AllomanticMetal.Copper);
            }
            
            if (cloudRenderer != null)
            {
                cloudRenderer.enabled = false;
            }
            
            Debug.Log("Coppercloud ended");
        }
        
        public bool IsPlayerInCloud(Vector3 playerPosition)
        {
            float distance = Vector3.Distance(transform.position, playerPosition);
            return distance <= cloudRadius && isActive;
        }
        
        public static bool IsInAnyCopperCloud(Vector3 position)
        {
            // Check all copper clouds in the scene
            CopperCloud[] clouds = Object.FindObjectsOfType<CopperCloud>();
            foreach (CopperCloud cloud in clouds)
            {
                if (cloud.isActive && cloud.IsPlayerInCloud(position))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
