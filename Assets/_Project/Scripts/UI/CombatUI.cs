// ============================================================
// FILE: CombatUI.cs
// SYSTEM: UI
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Handles combat HUD elements like damage numbers,
//   enemy health bars, and combat indicators.
//
// TODO:
//   - Hook up to actual damage system
//   - Add visual effects for damage numbers
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Mistborn.UI
{
    public class CombatUI : MonoBehaviour
    {
        [Header("References")]
        public Canvas combatCanvas;
        public Camera mainCamera;
        
        [Header("Damage Number Settings")]
        public GameObject damageNumberPrefab;
        public float damageNumberDuration = 1f;
        public float damageNumberOffset = 1f;
        
        [Header("Enemy Health Bar Settings")]
        public GameObject healthBarPrefab;
        public float healthBarHeight = 1.5f; // Above enemy
        
        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }
        
        public void ShowDamageNumber(Vector3 worldPosition, float damage, bool isCritical = false)
        {
            if (damageNumberPrefab == null) return;
            
            // Convert world position to screen position
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
            
            // Create damage number
            GameObject damageNumber = Instantiate(damageNumberPrefab, combatCanvas.transform);
            
            // Position it
            RectTransform rect = damageNumber.GetComponent<RectTransform>();
            rect.position = screenPos;
            rect.localPosition += Vector3.up * damageNumberOffset;
            
            // Set text
            TextMeshProUGUI text = damageNumber.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = Mathf.RoundToInt(damage).ToString();
                
                if (isCritical)
                {
                    text.color = Color.yellow;
                    text.fontSize = 24;
                }
                else
                {
                    text.color = Color.white;
                    text.fontSize = 18;
                }
            }
            
            // Animate
            StartCoroutine(AnimateDamageNumber(damageNumber, screenPos));
            
            // Destroy after duration
            Destroy(damageNumber, damageNumberDuration);
        }
        
        private System.Collections.IEnumerator AnimateDamageNumber(GameObject damageNumber, Vector3 startPos)
        {
            float elapsed = 0;
            float duration = damageNumberDuration;
            
            RectTransform rect = damageNumber.GetComponent<RectTransform>();
            Vector3 endPos = startPos + Vector3.up * 50f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Move up
                rect.localPosition = Vector3.Lerp(startPos + Vector3.up * damageNumberOffset, endPos, t);
                
                // Fade out
                TextMeshProUGUI text = damageNumber.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    Color c = text.color;
                    c.a = 1f - t;
                    text.color = c;
                }
                
                yield return null;
            }
        }
        
        public void ShowEnemyHealthBar(EnemyBase enemy)
        {
            if (healthBarPrefab == null) return;
            
            // Create health bar above enemy
            GameObject healthBar = Instantiate(healthBarPrefab, combatCanvas.transform);
            
            // Attach to enemy
            EnemyHealthBar barComponent = healthBar.AddComponent<EnemyHealthBar>();
            barComponent.Initialize(enemy, mainCamera, healthBarHeight);
        }
    }
    
    public class EnemyHealthBar : MonoBehaviour
    {
        public EnemyBase targetEnemy;
        public Camera mainCamera;
        public float heightOffset;
        
        private Image healthFill;
        private Text healthText;
        
        public void Initialize(EnemyBase enemy, Camera camera, float height)
        {
            targetEnemy = enemy;
            mainCamera = camera;
            heightOffset = height;
            
            healthFill = GetComponentInChildren<Image>();
            healthText = GetComponentInChildren<Text>();
        }
        
        private void Update()
        {
            if (targetEnemy == null)
            {
                Destroy(gameObject);
                return;
            }
            
            // Position above enemy
            Vector3 worldPos = targetEnemy.transform.position + Vector3.up * heightOffset;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            
            RectTransform rect = GetComponent<RectTransform>();
            rect.position = screenPos;
            
            // Face camera (billboard effect)
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                           mainCamera.transform.rotation * Vector3.up);
            
            // Update health
            if (healthFill != null)
            {
                healthFill.fillAmount = targetEnemy.health / targetEnemy.maxHealth;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{Mathf.RoundToInt(targetEnemy.health)}/{Mathf.RoundToInt(targetEnemy.maxHealth)}";
            }
        }
    }
}
