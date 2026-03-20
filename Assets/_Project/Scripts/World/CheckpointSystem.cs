// ============================================================
// FILE: CheckpointSystem.cs
// SYSTEM: World
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Manages checkpoints throughout the game world.
//   Players respawn at the last activated checkpoint.
//
// TODO:
//   - Add checkpoint activation effects
//   - Add sound effects
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.World
{
    public class CheckpointSystem : MonoBehaviour
    {
        [Header("Settings")]
        public bool activateOnTouch = true;
        public bool showActivationEffect = true;
        
        [Header("Effects")]
        public GameObject activationParticles;
        public AudioClip activationSound;
        
        [Header("Respawn Settings")]
        public float respawnDelay = 1f;
        public bool resetEnemyStates = false;
        
        private bool isActivated;
        private static CheckpointSystem currentCheckpoint;
        
        private void Start()
        {
            // Check if this is the starting checkpoint (auto-activated)
            if (GetComponent<StartingCheckpoint>() != null)
            {
                Activate();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!activateOnTouch) return;
            if (isActivated) return;
            
            // Check if player
            if (other.CompareTag("Player"))
            {
                Activate();
            }
        }
        
        public void Activate()
        {
            if (isActivated) return;
            
            isActivated = true;
            currentCheckpoint = this;
            
            // Notify GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCheckpoint(transform);
            }
            
            // Play effects
            if (showActivationEffect)
            {
                PlayActivationEffects();
            }
            
            Debug.Log($"Checkpoint activated: {gameObject.name}");
        }
        
        private void PlayActivationEffects()
        {
            // Particles
            if (activationParticles != null)
            {
                Instantiate(activationParticles, transform.position, Quaternion.identity);
            }
            
            // Sound
            if (activationSound != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound(activationSound);
            }
        }
        
        public void RespawnPlayer()
        {
            StartCoroutine(RespawnRoutine());
        }
        
        private System.Collections.IEnumerator RespawnRoutine()
        {
            // Delay before respawn
            yield return new WaitForSeconds(respawnDelay);
            
            // Reset player state
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Move to checkpoint
                player.transform.position = transform.position;
                player.transform.rotation = Quaternion.identity;
                
                // Reset health
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.Respawn();
                }
                
                // Reset enemy states if enabled
                if (resetEnemyStates)
                {
                    ResetEnemies();
                }
            }
        }
        
        private void ResetEnemies()
        {
            // Reset all enemies to their initial positions
            EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
            foreach (EnemyBase enemy in enemies)
            {
                enemy.gameObject.SetActive(true);
                // TODO: Move back to start position
            }
        }
        
        public static CheckpointSystem GetCurrentCheckpoint()
        {
            return currentCheckpoint;
        }
        
        public static void RespawnAtCheckpoint()
        {
            if (currentCheckpoint != null)
            {
                currentCheckpoint.RespawnPlayer();
            }
        }
    }
    
    public class StartingCheckpoint : MonoBehaviour
    {
        // Marker component for starting checkpoint
    }
}
