using UnityEngine;

namespace Mistborn.World
{
    /// <summary>
    /// Manages checkpoints throughout the game world.
    /// Players respawn at the last activated checkpoint.
    /// </summary>
    public class CheckpointSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool m_activateOnTouch = true;
        [SerializeField] private bool m_showActivationEffect = true;
        
        [Header("Effects")]
        [SerializeField] private GameObject m_activationParticles;
        [SerializeField] private AudioClip m_activationSound;
        
        [Header("Respawn")]
        [SerializeField] private float m_respawnDelay = 1f;

        private bool m_isActivated;
        private static CheckpointSystem m_currentCheckpoint;

        public bool isActivated => m_isActivated;
        public static CheckpointSystem current => m_currentCheckpoint;

        private void Start()
        {
            if (GetComponent<StartingCheckpoint>() != null)
            {
                Activate();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!m_activateOnTouch || m_isActivated) return;
            if (other.CompareTag("Player"))
            {
                Activate();
            }
        }

        /// <summary>Activates this checkpoint as the current respawn point.</summary>
        public void Activate()
        {
            if (m_isActivated) return;
            
            m_isActivated = true;
            m_currentCheckpoint = this;
            
            GameManager.Instance?.SetCheckpoint(transform);
            
            if (m_showActivationEffect)
            {
                PlayEffects();
            }
        }

        private void PlayEffects()
        {
            if (m_activationParticles != null)
            {
                Instantiate(m_activationParticles, transform.position, Quaternion.identity);
            }
            
            if (m_activationSound != null)
            {
                SoundManager.Instance?.PlaySound(m_activationSound);
            }
        }

        /// <summary>Respawns the player at this checkpoint.</summary>
        public void RespawnPlayer()
        {
            StartCoroutine(RespawnRoutine());
        }

        private System.Collections.IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(m_respawnDelay);
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = transform.position;
                player.transform.rotation = Quaternion.identity;
                player.GetComponent<PlayerHealth>()?.Respawn();
            }
        }

        public static void RespawnAtCurrent()
        {
            m_currentCheckpoint?.RespawnPlayer();
        }
    }

    /// <summary>Marker component for starting checkpoint.</summary>
    public class StartingCheckpoint : MonoBehaviour { }
}
