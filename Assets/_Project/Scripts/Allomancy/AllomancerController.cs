using UnityEngine;

namespace Mistborn.Allomancy
{
    [RequireComponent(typeof(CharacterController))]
    public class AllomancerController : MonoBehaviour
    {
        [Header("Metal Reserves")]
        [SerializeField] private float[] startingAmounts = new float[16];

        [Header("Burn Rates (per second)")]
        [SerializeField] private float steelBurnRate = 8f;
        [SerializeField] private float ironBurnRate = 8f;
        [SerializeField] private float pewterBurnRate = 10f;
        [SerializeField] private float tinBurnRate = 7f;
        [SerializeField] private float brassBurnRate = 5f;
        [SerializeField] private float zincBurnRate = 5f;
        [SerializeField] private float copperBurnRate = 5f;
        [SerializeField] private float bronzeBurnRate = 5f;
        [SerializeField] private float bendalloyBurnRate = 10f;

        [Header("Flare Settings")]
        [SerializeField] private float flareMultiplier = 2f;

        private MetalReserve[] reserves;
        
        public MetalReserve[] Reserves => reserves;
        public bool IsInitialized { get; private set; }
        public float FlareMultiplier => flareMultiplier;

        public event System.Action<AllomanticMetal> OnMetalDepleted;
        public event System.Action<AllomanticMetal> OnStartBurning;
        public event System.Action<AllomanticMetal> OnStopBurning;

        private void Awake()
        {
            InitializeReserves();
        }

        private void InitializeReserves()
        {
            int metalCount = System.Enum.GetValues(typeof(AllomanticMetal)).Length;
            reserves = new MetalReserve[metalCount];

            for (int i = 0; i < metalCount; i++)
            {
                float amount = i < startingAmounts.Length ? startingAmounts[i] : 100f;
                reserves[i] = new MetalReserve((AllomanticMetal)i, amount, GetBurnRate((AllomanticMetal)i));
            }

            IsInitialized = true;
        }

        private float GetBurnRate(AllomanticMetal metal)
        {
            return metal switch
            {
                AllomanticMetal.Steel => steelBurnRate,
                AllomanticMetal.Iron => ironBurnRate,
                AllomanticMetal.Pewter => pewterBurnRate,
                AllomanticMetal.Tin => tinBurnRate,
                AllomanticMetal.Brass => brassBurnRate,
                AllomanticMetal.Zinc => zincBurnRate,
                AllomanticMetal.Copper => copperBurnRate,
                AllomanticMetal.Bronze => bronzeBurnRate,
                AllomanticMetal.Bendalloy => bendalloyBurnRate,
                _ => 5f
            };
        }

        private void Update()
        {
            if (!IsInitialized) return;

            foreach (MetalReserve reserve in reserves)
            {
                if (reserve.IsBurning)
                {
                    reserve.Consume(Time.deltaTime);

                    if (reserve.IsEmpty)
                    {
                        reserve.StopBurning();
                        OnMetalDepleted?.Invoke(reserve.MetalType);
                    }
                }
            }
        }

        public MetalReserve GetReserve(AllomanticMetal metal)
        {
            return reserves[(int)metal];
        }

        public bool CanBurn(AllomanticMetal metal)
        {
            return !GetReserve(metal).IsEmpty;
        }

        public void StartBurning(AllomanticMetal metal)
        {
            MetalReserve reserve = GetReserve(metal);
            if (!reserve.IsEmpty)
            {
                reserve.StartBurning();
                OnStartBurning?.Invoke(metal);
            }
        }

        public void StopBurning(AllomanticMetal metal)
        {
            GetReserve(metal).StopBurning();
            OnStopBurning?.Invoke(metal);
        }

        public void RefillAllMetals()
        {
            foreach (MetalReserve reserve in reserves)
            {
                reserve.Refill();
            }
        }

        [ContextMenu("Debug: Drain All Metals")]
        private void DebugDrain()
        {
            foreach (MetalReserve reserve in reserves)
            {
                reserve.Consume(90f);
            }
        }

        [ContextMenu("Debug: Fill All Metals")]
        private void DebugFill()
        {
            RefillAllMetals();
        }
    }
}
