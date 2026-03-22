using UnityEngine;

namespace MistbornGame.UI
{
    public class MetalHUD : MonoBehaviour
    {
        [Header("Metal Slots")]
        [SerializeField] private MetalSlot[] metalSlots;
        
        [Header("Visual")]
        [SerializeField] private GameObject metalPanel;
        [SerializeField] private float fadeSpeed = 2f;
        
        private Allomancy.Allomancer allomancer;
        
        public static MetalHUD instance;
        
        private void Awake()
        {
            instance = this;
        }
        
        private void Start()
        {
            allomancer = FindObjectOfType<Allomancy.Allomancer>();
        }
        
        private void Update()
        {
            UpdateMetalDisplay();
        }
        
        private void UpdateMetalDisplay()
        {
            if (allomancer == null)
                return;
            
            foreach (var slot in metalSlots)
            {
                float reserve = allomancer.GetMetalReserve(slot.metalType);
                float percentage = reserve / 100f;
                
                slot.fillImage.fillAmount = percentage;
                
                if (allomancer.IsBurning(slot.metalType))
                {
                    slot.burningEffect.SetActive(true);
                    slot.backgroundImage.color = slot.activeColor;
                }
                else
                {
                    slot.burningEffect.SetActive(false);
                    slot.backgroundImage.color = slot.normalColor;
                }
            }
        }
        
        public void ShowMetalBurning(Allomancy.MetalType metal)
        {
            foreach (var slot in metalSlots)
            {
                if (slot.metalType == metal)
                {
                    slot.burningEffect.SetActive(true);
                    slot.backgroundImage.color = slot.activeColor;
                    break;
                }
            }
        }
        
        public void HideMetalBurning(Allomancy.MetalType metal)
        {
            foreach (var slot in metalSlots)
            {
                if (slot.metalType == metal)
                {
                    slot.burningEffect.SetActive(false);
                    slot.backgroundImage.color = slot.normalColor;
                    break;
                }
            }
        }
        
        public void SetMetalAmount(Allomancy.MetalType metal, float amount)
        {
            foreach (var slot in metalSlots)
            {
                if (slot.metalType == metal)
                {
                    slot.fillImage.fillAmount = amount;
                    break;
                }
            }
        }
        
        public void Show()
        {
            if (metalPanel != null)
            {
                metalPanel.SetActive(true);
            }
        }
        
        public void Hide()
        {
            if (metalPanel != null)
            {
                metalPanel.SetActive(false);
            }
        }
        
        [System.Serializable]
        public class MetalSlot
        {
            public Allomancy.MetalType metalType;
            public UnityEngine.UI.Image backgroundImage;
            public UnityEngine.UI.Image fillImage;
            public GameObject burningEffect;
            public Color normalColor = Color.gray;
            public Color activeColor = Color.yellow;
        }
    }
}
