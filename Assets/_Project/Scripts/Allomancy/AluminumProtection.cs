using UnityEngine;

namespace Mistborn.Allomancy
{
    public class AluminumProtection : MonoBehaviour
    {
        [Header("Protection")]
        [SerializeField] private float protectionDuration = 3f;
        [SerializeField] private KeyCode activationKey = KeyCode.F;

        private AllomancerController allomancer;
        private bool isProtected;

        public bool IsProtected => isProtected;

        private void Awake()
        {
            allomancer = GetComponent<AllomancerController>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(activationKey))
            {
                BurnAluminum();
            }
        }

        public void BurnAluminum()
        {
            if (allomancer == null) return;

            foreach (MetalReserve reserve in allomancer.Reserves)
            {
                reserve.StopBurning();
                reserve.Consume(100f);
            }

            isProtected = true;
            Invoke(nameof(EndProtection), protectionDuration);
        }

        private void EndProtection()
        {
            isProtected = false;
        }

        public bool BlockNicroburst()
        {
            if (isProtected)
            {
                isProtected = false;
                return true;
            }
            return false;
        }
    }
}
