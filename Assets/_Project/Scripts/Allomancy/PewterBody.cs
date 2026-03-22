using UnityEngine;

namespace MistbornGame.Allomancy
{
    public class PewterBody : MonoBehaviour
    {
        [Header("Pewter Enhancement")]
        [SerializeField] private float strengthBoost = 1.5f;
        [SerializeField] private float speedBoost = 1.3f;
        [SerializeField] private float toughnessBoost = 1.3f;
        [SerializeField] private float damageBoost = 1.4f;
        
        [Header("Visual Feedback")]
        [SerializeField] private ParticleSystem pewterGlow;
        [SerializeField] private Material pewterEnhancedMaterial;
        [SerializeField] private Renderer bodyRenderer;
        
        [Header("Combat")]
        [SerializeField] private bool enableHeavyAttacks = true;
        [SerializeField] private float heavyAttackDamageMultiplier = 2f;
        
        private Allomancer allomancer;
        private PlayerStats playerStats;
        private Rigidbody rb;
        
        private bool wasEnhanced = false;
        
        private void Start()
        {
            allomancer = GetComponent<Allomancer>();
            playerStats = GetComponent<PlayerStats>();
            rb = GetComponent<Rigidbody>();
        }
        
        private void Update()
        {
            bool isEnhanced = allomancer != null && allomancer.IsBurning(MetalType.Pewter);
            
            if (isEnhanced && !wasEnhanced)
            {
                ApplyPewterEnhancement();
            }
            else if (!isEnhanced && wasEnhanced)
            {
                RemovePewterEnhancement();
            }
            
            wasEnhanced = isEnhanced;
        }
        
        private void ApplyPewterEnhancement()
        {
            if (pewterGlow != null)
                pewterGlow.Play();
            
            if (bodyRenderer != null && pewterEnhancedMaterial != null)
                bodyRenderer.material = pewterEnhancedMaterial;
            
            if (rb != null)
                rb.mass *= toughnessBoost;
        }
        
        private void RemovePewterEnhancement()
        {
            if (pewterGlow != null)
                pewterGlow.Stop();
            
            if (rb != null)
                rb.mass /= toughnessBoost;
        }
        
        public float GetDamageMultiplier()
        {
            return allomancer != null && allomancer.IsBurning(MetalType.Pewter) ? damageBoost : 1f;
        }
        
        public float GetSpeedMultiplier()
        {
            return allomancer != null && allomancer.IsBurning(MetalType.Pewter) ? speedBoost : 1f;
        }
        
        public float GetToughnessMultiplier()
        {
            return allomancer != null && allomancer.IsBurning(MetalType.Pewter) ? toughnessBoost : 1f;
        }
        
        public bool IsEnhanced()
        {
            return allomancer != null && allomancer.IsBurning(MetalType.Pewter);
        }
        
        public void PerformHeavyAttack(Enemy.Enemy target)
        {
            if (!enableHeavyAttacks || !IsEnhanced())
                return;
            
            float damage = playerStats.BaseDamage * heavyAttackDamageMultiplier * damageBoost;
            target?.TakeDamage(damage, DamageType.Allomantic);
        }
    }
}
