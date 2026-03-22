using UnityEngine;

namespace MistbornGameplay
{
    public class MetalVial : MonoBehaviour, HiddenObject
    {
        [Header("Vial Settings")]
        [SerializeField] private MetalType metalType = MetalType.Iron;
        [SerializeField] private int metalUnits = 10;
        [SerializeField] private int maxUnits = 20;
        [SerializeField] private bool isRevealed = false;
        
        [Header("Visual Settings")]
        [SerializeField] private Renderer vialRenderer;
        [SerializeField] private ParticleSystem glowEffect;
        [SerializeField] private AudioClip pickupSound;
        
        [Header("Metal Colors")]
        [SerializeField] private Color ironColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        [SerializeField] private Color steelColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        [SerializeField] private Color pewterColor = new Color(0.6f, 0.5f, 0.4f, 1f);
        [SerializeField] private Color tinColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        [SerializeField] private Color copperColor = new Color(0.7f, 0.4f, 0.2f, 1f);
        [SerializeField] private Color bronzeColor = new Color(0.6f, 0.4f, 0.1f, 1f);
        [SerializeField] private Color zincColor = new Color(0.5f, 0.5f, 0.6f, 1f);
        [SerializeField] private Color brassColor = new Color(0.8f, 0.7f, 0.3f, 1f);
        
        [Header("Stealth")]
        [SerializeField] private bool canBeHidden = true;
        
        public enum MetalType
        {
            Iron,
            Steel,
            Tin,
            Pewter,
            Copper,
            Bronze,
            Zinc,
            Brass,
            Duralumin,
            Nicrosil,
            Chromium,
            Bendalloy,
            Cadmium,
            Aluminum,
            Gold,
            Electrum,
            Atium,
            Malatium
        }
        
        private AudioSource audioSource;
        private Renderer[] renderers;
        private bool isHidden = false;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            renderers = GetComponentsInChildren<Renderer>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            SetMetalColor();
            
            if (isRevealed || !canBeHidden)
            {
                Show();
            }
            else
            {
                Hide();
            }
            
            if (glowEffect != null)
            {
                glowEffect.Stop();
            }
        }
        
        private void SetMetalColor()
        {
            Color metalColor = GetMetalColor();
            
            if (vialRenderer != null)
            {
                vialRenderer.material.color = metalColor;
            }
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.material.color = metalColor;
                }
            }
        }
        
        private Color GetMetalColor()
        {
            switch (metalType)
            {
                case MetalType.Iron:
                    return ironColor;
                case MetalType.Steel:
                    return steelColor;
                case MetalType.Pewter:
                    return pewterColor;
                case MetalType.Tin:
                    return tinColor;
                case MetalType.Copper:
                    return copperColor;
                case MetalType.Bronze:
                    return bronzeColor;
                case MetalType.Zinc:
                    return zincColor;
                case MetalType.Brass:
                    return brassColor;
                default:
                    return Color.gray;
            }
        }
        
        public void OnPickup(GameObject picker)
        {
            if (isHidden)
            {
                return;
            }
            
            Allomancer allomancer = picker.GetComponent<Allomancer>();
            if (allomancer != null)
            {
                allomancer.AddMetalReserve(metalType, metalUnits);
            }
            
            FeruchemyStore feruchemy = picker.GetComponent<FeruchemyStore>();
            if (feruchemy != null)
            {
                feruchemy.AddStoredMetal(metalUnits);
            }
            
            if (audioSource != null && pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
            
            if (OnVialPickedUp != null)
            {
                OnVialPickedUp(picker, metalType, metalUnits);
            }
            
            Destroy(gameObject, 0.1f);
        }
        
        public void Reveal()
        {
            if (!canBeHidden)
            {
                return;
            }
            
            isRevealed = true;
            isHidden = false;
            Show();
            
            if (glowEffect != null)
            {
                glowEffect.Play();
            }
            
            if (OnVialRevealed != null)
            {
                OnVialRevealed();
            }
        }
        
        public void Hide()
        {
            if (!canBeHidden)
            {
                return;
            }
            
            isHidden = true;
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
            
            if (glowEffect != null)
            {
                glowEffect.Stop();
            }
        }
        
        private void Show()
        {
            isHidden = false;
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
        }
        
        public MetalType GetMetalType()
        {
            return metalType;
        }
        
        public void SetMetalType(MetalType type)
        {
            metalType = type;
            SetMetalColor();
        }
        
        public int GetMetalUnits()
        {
            return metalUnits;
        }
        
        public void SetMetalUnits(int units)
        {
            metalUnits = Mathf.Clamp(units, 0, maxUnits);
        }
        
        public int GetMaxUnits()
        {
            return maxUnits;
        }
        
        public bool IsRevealed()
        {
            return isRevealed;
        }
        
        public bool IsHidden()
        {
            return isHidden;
        }
        
        public void Refill()
        {
            metalUnits = maxUnits;
        }
        
        public event System.Action<GameObject, MetalType, int> OnVialPickedUp;
        public event System.Action OnVialRevealed;
    }
}
