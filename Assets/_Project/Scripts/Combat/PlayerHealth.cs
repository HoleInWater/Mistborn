// ============================================================
// FILE: PlayerHealth.cs
// SYSTEM: Combat
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Handles player health, damage, and death.
//   Integrates with Allomancy (pewter damage resistance, etc.)
//
// TODO:
//   - Add animation hooks
//   - Add death screen
//   - Add respawn system
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Combat
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        public float maxHealth = 100f;
        public float currentHealth;
        
        [Header("Invincibility")]
        public float invincibilityDuration = 0.5f;
        public bool isInvincible;
        private float invincibilityTimer;
        
        [Header("Death")]
        public bool isDead;
        public GameObject deathScreen;
        
        [Header("Audio")]
        public AudioClip damageSound;
        public AudioClip deathSound;
        
        [Header("Components")]
        public PewterEnhancement pewterEnhancement;
        
        private void Start()
        {
            currentHealth = maxHealth;
        }
        
        private void Update()
        {
            if (isInvincible)
            {
                invincibilityTimer -= Time.deltaTime;
                if (invincibilityTimer <= 0)
                {
                    isInvincible = false;
                }
            }
        }
        
        public void TakeDamage(float damage)
        {
            if (isDead || isInvincible) return;
            
            // Check for pewter damage resistance
            float actualDamage = damage;
            if (pewterEnhancement != null && pewterEnhancement.IsEnhanced())
            {
                actualDamage *= (1f - pewterEnhancement.GetPainResistance());
                Debug.Log($"Pewter reduced damage from {damage} to {actualDamage}");
            }
            
            currentHealth -= actualDamage;
            Debug.Log($"Player took {actualDamage} damage, {currentHealth}/{maxHealth} HP");
            
            // TODO: Play damage animation
            // TODO: Screen shake
            // TODO: Damage number popup
            
            if (SoundManager.Instance != null && damageSound != null)
            {
                SoundManager.Instance.PlaySound(damageSound);
            }
            
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartInvincibility();
            }
        }
        
        public void Heal(float amount)
        {
            if (isDead) return;
            
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            Debug.Log($"Player healed {amount}, now at {currentHealth}/{maxHealth} HP");
        }
        
        public void Die()
        {
            if (isDead) return;
            
            isDead = true;
            currentHealth = 0;
            
            Debug.Log("Player died");
            
            if (SoundManager.Instance != null && deathSound != null)
            {
                SoundManager.Instance.PlaySound(deathSound);
            }
            
            // TODO: Play death animation
            // TODO: Disable controls
            // TODO: Show death screen
            
            if (deathScreen != null)
            {
                deathScreen.SetActive(true);
            }
            
            // TODO: Auto-reload or show respawn option
        }
        
        private void StartInvincibility()
        {
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }
        
        public void Respawn()
        {
            isDead = false;
            currentHealth = maxHealth;
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
            
            if (deathScreen != null)
            {
                deathScreen.SetActive(false);
            }
            
            // TODO: Move player to checkpoint
            // TODO: Reset enemy states
        }
        
        public float GetHealthPercent()
        {
            return currentHealth / maxHealth;
        }
    }
}
