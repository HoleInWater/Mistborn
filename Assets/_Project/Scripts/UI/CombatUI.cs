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
        public float healthBarHeight = 1.5f;

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

            Vector3 screenPos = WorldToScreen(worldPosition);
            if (screenPos.z < 0) return;

            GameObject damageNum = Instantiate(damageNumberPrefab, screenPos, Quaternion.identity, combatCanvas.transform);
            TextMeshProUGUI text = damageNum.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = Mathf.RoundToInt(damage).ToString();
                text.color = isCritical ? Color.yellow : Color.white;
                text.fontSize = isCritical ? 36 : 24;
            }

            Destroy(damageNum, damageNumberDuration);
        }

        public void ShowEnemyHealthBar(Transform enemy, float healthPercent)
        {
            if (healthBarPrefab == null) return;

            Vector3 screenPos = WorldToScreen(enemy.position + Vector3.up * healthBarHeight);
            if (screenPos.z < 0) return;

            GameObject healthBar = Instantiate(healthBarPrefab, screenPos, Quaternion.identity, combatCanvas.transform);
            Slider slider = healthBar.GetComponentInChildren<Slider>();
            if (slider != null)
            {
                slider.value = healthPercent;
            }
        }

        private Vector3 WorldToScreen(Vector3 worldPos)
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            
            if (mainCamera != null)
                return mainCamera.WorldToScreenPoint(worldPos);
            
            return Vector3.zero;
        }
    }
}
