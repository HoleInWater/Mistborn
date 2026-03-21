using UnityEngine;

namespace Mistborn.Allomancy
{
    public class DuraluminBurst : MonoBehaviour
    {
        [Header("Burst")]
        [SerializeField] private float burstMultiplier = 5f;
        [SerializeField] private KeyCode activationKey = KeyCode.R;

        private AllomancerController allomancer;

        public bool IsInitialized => allomancer != null;

        private void Awake()
        {
            allomancer = GetComponent<AllomancerController>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(activationKey))
            {
                TryBurst();
            }
        }

        public void TryBurst()
        {
            if (allomancer == null) return;

            AllomanticMetal? burningMetal = null;

            foreach (MetalReserve reserve in allomancer.Reserves)
            {
                if (reserve.IsBurning)
                {
                    burningMetal = reserve.MetalType;
                    break;
                }
            }

            if (!burningMetal.HasValue) return;

            float amount = allomancer.GetReserve(burningMetal.Value).CurrentAmount;
            allomancer.StopBurning(burningMetal.Value);

            foreach (MetalReserve reserve in allomancer.Reserves)
            {
                reserve.Consume(100f);
                reserve.StopBurning();
            }

            ApplyEffect(burningMetal.Value, amount * burstMultiplier);
        }

        private void ApplyEffect(AllomanticMetal metal, float power)
        {
            switch (metal)
            {
                case AllomanticMetal.Steel:
                    GetComponent<SteelPushAbility>()?.FindMetalTargets();
                    break;
                case AllomanticMetal.Iron:
                    GetComponent<IronPullAbility>()?.FindMetalTargets();
                    break;
            }
        }

        public bool CanBurst()
        {
            if (allomancer == null) return false;

            foreach (MetalReserve reserve in allomancer.Reserves)
            {
                if (reserve.IsBurning)
                    return true;
            }
            return false;
        }
    }
}
