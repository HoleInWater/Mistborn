using UnityEngine;

namespace Mistborn.Allomancy
{
    public class TinEnhancement : MonoBehaviour
    {
        [Header("Sense Enhancement")]
        [SerializeField] private float sightMultiplier = 2f;
        [SerializeField] private float hearingMultiplier = 2f;

        [Header("Input")]
        [SerializeField] private KeyCode activationKey = KeyCode.E;

        [Header("Overload Settings")]
        [SerializeField] private float flashbangDuration = 1f;

        private AllomancerController allomancer;
        private Camera playerCamera;
        private bool isEnhanced;
        private bool isOverloaded;
        private float originalClipPlane;

        public bool IsEnhanced => isEnhanced;
        public bool IsOverloaded => isOverloaded;

        public event System.Action<float> OnOverload;

        private void Awake()
        {
            allomancer = GetComponent<AllomancerController>();
            playerCamera = GetComponentInChildren<Camera>();

            if (playerCamera != null)
            {
                originalClipPlane = playerCamera.farClipPlane;
            }
        }

        private void Update()
        {
            if (isOverloaded) return;

            if (Input.GetKeyDown(activationKey) && CanUse())
            {
                StartEnhancement();
            }
            else if (Input.GetKeyUp(activationKey))
            {
                StopEnhancement();
            }
        }

        private bool CanUse()
        {
            return allomancer != null && allomancer.CanBurn(AllomanticMetal.Tin);
        }

        private void StartEnhancement()
        {
            isEnhanced = true;
            allomancer.StartBurning(AllomanticMetal.Tin);

            if (playerCamera != null)
            {
                playerCamera.farClipPlane = originalClipPlane * sightMultiplier;
            }
        }

        private void StopEnhancement()
        {
            if (!isEnhanced) return;

            isEnhanced = false;
            allomancer.StopBurning(AllomanticMetal.Tin);

            if (playerCamera != null)
            {
                playerCamera.farClipPlane = originalClipPlane;
            }
        }

        public void TriggerOverload(float duration)
        {
            if (!isEnhanced) return;

            isOverloaded = true;
            StopEnhancement();

            OnOverload?.Invoke(duration);
            Invoke(nameof(EndOverload), duration);
        }

        private void EndOverload()
        {
            isOverloaded = false;
        }

        public float GetEnemyDetectionRange(float baseRange)
        {
            return isEnhanced ? baseRange * hearingMultiplier : baseRange;
        }

        public float GetWorldRenderDistance(float baseDistance)
        {
            return isEnhanced ? baseDistance * sightMultiplier : baseDistance;
        }
    }
}
