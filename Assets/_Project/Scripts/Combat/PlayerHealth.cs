// ============================================================
// FILE: PlayerHealth.cs
// AGENT: gemini-3-flash via Antigravity
// DATE: 2026-03-23
// ------------------------------------------------------------
// PROBLEM BEING SOLVED:
//   Restoring missing player health system required for combat and Pewter Mend.
//
// APPROACH CHOSEN:
//   Standardized health script implementing IDamageable. Integrates with GameManager.
//
// FILES TOUCHED:
//   [NEW] PlayerHealth.cs
//
// THENBUZZARD100 FILES NEARBY (READ-ONLY):
//   None in this folder.
// ============================================================

using UnityEngine;
using UnityEngine.Events;
using Mistborn.Combat;

/// <summary>
/// Manages player health, damage taking, and healing.
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isInvulnerable = false;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnDeath;

    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Applies damage to the player.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDead || isInvulnerable) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heals the player by a specified amount.
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        
        Debug.Log("[PLAYER] Died!");
        OnDeath?.Invoke();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    public void SetInvincible(bool val) => isInvulnerable = val;

    public float GetMaxHealth() => maxHealth;
    public float GetCurrentHealth() => currentHealth;
}

