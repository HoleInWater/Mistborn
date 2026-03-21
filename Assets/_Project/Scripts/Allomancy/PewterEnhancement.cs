using UnityEngine;

namespace Mistborn.Allomancy
{
    public class PewterEnhancement : MonoBehaviour
    {
        [Header("Enhancement Settings")]
        [SerializeField] private float strengthMultiplier = 2f;
        [SerializeField] private float speedMultiplier = 1.5f;
        [SerializeField] private float painResistance = 0.5f;

        [Header("Drag Settings (when depleted)")]
        [SerializeField] private float dragSpeedPenalty = 0.5f;
        [SerializeField] private float dragStrengthPenalty = 0.5f;
        [SerializeField] private float dragDuration = 3f;

        [Header("Input")]
        [SerializeField] private KeyCode activationKey = KeyCode.Q;

        private AllomancerController allomancer;
        private bool isEnhanced;
        private bool isDragging;
        private float dragTimer;

        public bool IsEnhanced => isEnhanced;
        public bool IsDragging => isDragging;

        public event System.Action OnEnhancementStart;
        public event System.Action OnEnhancementEnd;
        public event System.Action OnDragStart;
        public event System.Action OnDragEnd;

        private void Awake()
        {
            allomancer = GetComponent<AllomancerController>();
        }

        private void Update()
        {
            // Handle drag state (can't do anything while dragging)
            if (isDragging)
            {
                dragTimer -= Time.deltaTime;
                if (dragTimer <= 0)
                {
                    EndDrag();
                }
                return;
            }

            // Toggle enhancement
            if (Input.GetKeyDown(activationKey) && CanUse())
            {
                StartEnhancement();
            }
            else if (Input.GetKeyUp(activationKey))
            {
                StopEnhancement();
            }

            // Check for depletion
            if (isEnhanced && allomancer != null)
            {
                if (allomancer.GetReserve(AllomanticMetal.Pewter).IsEmpty)
                {
                    TriggerDrag();
                }
            }
        }

        private bool CanUse()
        {
            return allomancer != null && allomancer.CanBurn(AllomanticMetal.Pewter);
        }

        private void StartEnhancement()
        {
            isEnhanced = true;
            allomancer.StartBurning(AllomanticMetal.Pewter);
            OnEnhancementStart?.Invoke();
        }

        private void StopEnhancement()
        {
            if (!isEnhanced) return;

            isEnhanced = false;
            allomancer.StopBurning(AllomanticMetal.Pewter);
            OnEnhancementEnd?.Invoke();
        }

        private void TriggerDrag()
        {
            isEnhanced = false;
            isDragging = true;
            dragTimer = dragDuration;
            allomancer.StopBurning(AllomanticMetal.Pewter);
            OnDragStart?.Invoke();
        }

        private void EndDrag()
        {
            isDragging = false;
            OnDragEnd?.Invoke();
        }

        public float GetDamageMultiplier()
        {
            if (isDragging) return 1f - dragStrengthPenalty;
            if (isEnhanced) return strengthMultiplier;
            return 1f;
        }

        public float GetSpeedMultiplier()
        {
            if (isDragging) return 1f - dragSpeedPenalty;
            if (isEnhanced) return speedMultiplier;
            return 1f;
        }

        public float GetPainResistance()
        {
            return isEnhanced ? painResistance : 0f;
        }
    }
}
