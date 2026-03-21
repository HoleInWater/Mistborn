namespace Mistborn.Allomancy
{
    [System.Serializable]
    public class MetalReserve
    {
        [UnityEngine.SerializeField] private AllomanticMetal metalType;
        [UnityEngine.SerializeField] private float maxAmount = 100f;
        [UnityEngine.SerializeField] private float currentAmount = 100f;
        [UnityEngine.SerializeField] private float burnRate = 5f;
        [UnityEngine.SerializeField] private bool isBurning;

        public AllomanticMetal MetalType => metalType;
        public float CurrentAmount => currentAmount;
        public float MaxAmount => maxAmount;
        public float BurnRate => burnRate;
        
        public bool IsBurning
        {
            get => isBurning;
            set => isBurning = value;
        }

        public MetalReserve(AllomanticMetal type, float max = 100f, float rate = 5f)
        {
            metalType = type;
            maxAmount = max;
            currentAmount = max;
            burnRate = rate;
            isBurning = false;
        }

        public void StartBurning()
        {
            if (!IsEmpty)
                isBurning = true;
        }

        public void StopBurning()
        {
            isBurning = false;
        }

        public bool IsEmpty => currentAmount <= 0;
        public bool CanBurn => currentAmount > 0;

        public float GetPercentage()
        {
            return (currentAmount / maxAmount) * 100f;
        }

        public void Refill()
        {
            currentAmount = maxAmount;
        }

        public void SetAmount(float amount)
        {
            currentAmount = Mathf.Clamp(amount, 0f, maxAmount);
        }

        public void Consume(float deltaTime)
        {
            if (!isBurning || currentAmount <= 0)
                return;

            currentAmount -= burnRate * deltaTime;

            if (currentAmount <= 0)
            {
                currentAmount = 0;
                isBurning = false;
            }
        }
    }
}
