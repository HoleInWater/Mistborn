using UnityEngine;

namespace MistbornGame.Player
{
    public class Mistcoat : MonoBehaviour
    {
        [Header("Mistcoat Configuration")]
        [SerializeField] private float mistcoatBonus = 1.2f;
        [SerializeField] private float critChanceBonus = 0.1f;
        [SerializeField] private float movementSpeedBonus = 0.15f;
        [SerializeField] private float damageReductionBonus = 0.1f;
        
        [Header("Visual")]
        [SerializeField] private GameObject mistcoatModel;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material mistcoatMaterial;
        [SerializeField] private ParticleSystem mistParticles;
        
        [Header("Combat Effects")]
        [SerializeField] private bool addsFearEffect = true;
        [SerializeField] private float fearRadius = 10f;
        [SerializeField] private float fearDuration = 2f;
        [SerializeField] private float fearIntensity = 0.3f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip equipSound;
        [SerializeField] private AudioClip mistcoatActivateSound;
        
        private bool isEquipped = false;
        private PlayerStats playerStats;
        private Animator animator;
        
        public bool IsEquipped => isEquipped;
        
        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            animator = GetComponent<Animator>();
            
            if (mistcoatModel != null)
                mistcoatModel.SetActive(false);
        }
        
        private void Update()
        {
            if (isEquipped && hasMetallicVision)
            {
                UpdateMistcoatVisuals();
            }
        }
        
        public void EquipMistcoat()
        {
            if (isEquipped)
                return;
            
            isEquipped = true;
            
            if (mistcoatModel != null)
                mistcoatModel.SetActive(true);
            
            ApplyMistcoatBonuses();
            
            AudioSource.PlayClipAtPoint(equipSound, transform.position);
            
            if (mistParticles != null)
            {
                mistParticles.Play();
            }
            
            animator?.SetTrigger("EquipMistcoat");
        }
        
        public void UnequipMistcoat()
        {
            if (!isEquipped)
                return;
            
            isEquipped = false;
            
            RemoveMistcoatBonuses();
            
            if (mistcoatModel != null)
                mistcoatModel.SetActive(false);
            
            if (mistParticles != null)
            {
                mistParticles.Stop();
            }
        }
        
        public void ToggleMistcoat()
        {
            if (isEquipped)
                UnequipMistcoat();
            else
                EquipMistcoat();
        }
        
        private void ApplyMistcoatBonuses()
        {
            if (playerStats == null)
                return;
            
            playerStats.AddDamageMultiplier(mistcoatBonus - 1f);
            playerStats.AddCritChance(critChanceBonus);
            playerStats.AddSpeedBonus(movementSpeedBonus);
            playerStats.AddDefenseBonus(damageReductionBonus);
        }
        
        private void RemoveMistcoatBonuses()
        {
            if (playerStats == null)
                return;
            
            playerStats.RemoveDamageMultiplier(mistcoatBonus - 1f);
            playerStats.RemoveCritChance(critChanceBonus);
            playerStats.RemoveSpeedBonus(movementSpeedBonus);
            playerStats.RemoveDefenseBonus(damageReductionBonus);
        }
        
        private bool hasMetallicVision = false;
        
        private void UpdateMistcoatVisuals()
        {
            if (mistcoatMaterial != null && mistcoatModel != null)
            {
                Renderer renderer = mistcoatModel.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color currentColor = Color.Lerp(normalMaterial.color, mistcoatMaterial.color, 0.5f);
                    currentColor = Color.Lerp(currentColor, Color.gray, 0.3f);
                    renderer.material.color = currentColor;
                }
            }
        }
        
        public void ActivateMistcoatAbility()
        {
            if (!isEquipped)
                return;
            
            AudioSource.PlayClipAtPoint(mistcoatActivateSound, transform.position);
            
            if (addsFearEffect)
            {
                ApplyFearEffect();
            }
            
            ApplySpeedBurst();
            
            if (mistParticles != null)
            {
                mistParticles.Emit(100);
            }
        }
        
        private void ApplyFearEffect()
        {
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, fearRadius);
            
            foreach (var enemy in nearbyEnemies)
            {
                Enemy.Enemy enemyScript = enemy.GetComponent<Enemy.Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.ApplyFear(fearDuration, fearIntensity);
                }
            }
        }
        
        private void ApplySpeedBurst()
        {
            if (playerStats != null)
            {
                playerStats.AddTemporarySpeedBonus(0.5f, 2f);
            }
        }
        
        public void OnMetallicVisionActivated()
        {
            hasMetallicVision = true;
        }
        
        public void OnMetallicVisionDeactivated()
        {
            hasMetallicVision = false;
        }
        
        public float GetMistcoatBonus(StatType stat)
        {
            if (!isEquipped)
                return 0f;
            
            switch (stat)
            {
                case StatType.Damage:
                    return mistcoatBonus - 1f;
                case StatType.CritChance:
                    return critChanceBonus;
                case StatType.MoveSpeed:
                    return movementSpeedBonus;
                case StatType.Defense:
                    return damageReductionBonus;
                default:
                    return 0f;
            }
        }
    }
    
    public enum StatType
    {
        Health,
        Stamina,
        Damage,
        Defense,
        MoveSpeed,
        CritChance,
        Armor,
        MentalSpeed
    }
}
