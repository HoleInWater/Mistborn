using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Systems
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        
        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float currentStamina = 100f;
        [SerializeField] private float staminaRegenRate = 10f;
        
        [Header("Mental Energy")]
        [SerializeField] private float maxMentalEnergy = 100f;
        [SerializeField] private float currentMentalEnergy = 100f;
        
        [Header("Combat Stats")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float critChance = 0.05f;
        [SerializeField] private float critMultiplier = 2f;
        
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 5f;
        
        [Header("Metal")]
        [SerializeField] private int metal = 50;
        [SerializeField] private int skillPoints = 0;
        
        [Header("Level")]
        [SerializeField] private int level = 1;
        [SerializeField] private float experience = 0f;
        [SerializeField] private float experienceToNextLevel = 1000f;
        
        [Header("Defense")]
        [SerializeField] private float armor = 0f;
        [SerializeField] private float defenseBonus = 0f;
        
        [Header("Status Effects")]
        [SerializeField] private List<StatusEffect> activeEffects = new List<StatusEffect>();
        
        [Header("Modifiers")]
        private List<StatModifier> damageModifiers = new List<StatModifier>();
        private List<StatModifier> speedModifiers = new List<StatModifier>();
        private List<StatModifier> defenseModifiers = new List<StatModifier>();
        
        private bool isDead = false;
        
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public float CurrentMentalEnergy => currentMentalEnergy;
        public float MaxMentalEnergy => maxMentalEnergy;
        public float BaseDamage => baseDamage;
        public float CritChance => critChance;
        public float CritMultiplier => critMultiplier;
        public float MoveSpeed => moveSpeed;
        public float JumpForce => jumpForce;
        public int Metal => metal;
        public int SkillPoints => skillPoints;
        public int Level => level;
        public float Experience => experience;
        public bool IsDead => isDead;
        
        public Faction PlayerFaction => Faction.Player;
        
        public event System.Action OnDeath;
        public event System.Action OnLevelUp;
        public event System.Action<float> OnHealthChanged;
        public event System.Action<float> OnStaminaChanged;
        
        private void Update()
        {
            RegenerateStamina();
            UpdateStatusEffects();
        }
        
        private void RegenerateStamina()
        {
            if (currentStamina < maxStamina)
            {
                float regen = staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina + regen, maxStamina);
            }
        }
        
        public void TakeDamage(float damage, DamageType type = DamageType.Normal)
        {
            if (isDead)
                return;
            
            float finalDamage = damage;
            
            float totalDefense = armor + defenseBonus;
            finalDamage *= (1f - totalDefense * 0.01f);
            finalDamage = Mathf.Max(finalDamage, 1f);
            
            currentHealth -= finalDamage;
            
            OnHealthChanged?.Invoke(currentHealth);
            
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }
        
        public void Heal(float amount)
        {
            if (isDead)
                return;
            
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(currentHealth);
        }
        
        public void UseStamina(float amount)
        {
            currentStamina = Mathf.Max(currentStamina - amount, 0f);
            OnStaminaChanged?.Invoke(currentStamina);
        }
        
        public void AddStamina(float amount)
        {
            currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
            OnStaminaChanged?.Invoke(currentStamina);
        }
        
        public void UseMentalEnergy(float amount)
        {
            currentMentalEnergy = Mathf.Max(currentMentalEnergy - amount, 0f);
        }
        
        public void UseMetal(int amount)
        {
            metal = Mathf.Max(metal - amount, 0);
        }
        
        public void AddMetal(int amount)
        {
            metal += amount;
        }
        
        public bool SpendMetal(int amount)
        {
            if (metal >= amount)
            {
                metal -= amount;
                return true;
            }
            return false;
        }
        
        public void AddExperience(float amount)
        {
            experience += amount;
            
            while (experience >= experienceToNextLevel)
            {
                LevelUp();
            }
        }
        
        private void LevelUp()
        {
            experience -= experienceToNextLevel;
            level++;
            experienceToNextLevel *= 1.5f;
            
            maxHealth *= 1.1f;
            maxStamina *= 1.1f;
            
            currentHealth = maxHealth;
            currentStamina = maxStamina;
            
            skillPoints++;
            
            OnLevelUp?.Invoke();
        }
        
        public void AddSkillPoints(int amount)
        {
            skillPoints += amount;
        }
        
        public void UseSkillPoints(int amount)
        {
            skillPoints -= amount;
        }
        
        public void Die()
        {
            if (isDead)
                return;
            
            isDead = true;
            OnDeath?.Invoke();
        }
        
        public void AddDamageMultiplier(float amount)
        {
            damageModifiers.Add(new StatModifier(amount, "Bonus"));
        }
        
        public void RemoveDamageMultiplier(float amount)
        {
            damageModifiers.RemoveAll(m => m.Value == amount);
        }
        
        public void AddSpeedBonus(float amount)
        {
            speedModifiers.Add(new StatModifier(amount, "Bonus"));
        }
        
        public void RemoveSpeedBonus(float amount)
        {
            speedModifiers.RemoveAll(m => m.Value == amount);
        }
        
        public void AddDefenseBonus(float amount)
        {
            defenseModifiers.Add(new StatModifier(amount, "Bonus"));
        }
        
        public void RemoveDefenseBonus(float amount)
        {
            defenseModifiers.RemoveAll(m => m.Value == amount);
        }
        
        public void AddTemporaryModifier(StatType type, float amount, string source)
        {
        }
        
        public void RemoveTemporaryModifier(StatType type, string source)
        {
        }
        
        public void AddStatusEffect(StatusEffectType type, float duration, float intensity)
        {
            StatusEffect effect = activeEffects.Find(e => e.Type == type);
            if (effect != null)
            {
                effect.Duration = duration;
                effect.Intensity = intensity;
            }
            else
            {
                activeEffects.Add(new StatusEffect(type, duration, intensity));
            }
        }
        
        private void UpdateStatusEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                activeEffects[i].Duration -= Time.deltaTime;
                if (activeEffects[i].Duration <= 0)
                {
                    activeEffects.RemoveAt(i);
                }
            }
        }
        
        public void SetVisibilityMultiplier(float multiplier)
        {
        }
        
        public void SetMovementSpeedMultiplier(float multiplier)
        {
        }
        
        public void AddStrengthBonus(float bonus)
        {
        }
        
        public void RemoveStrengthBonus(float bonus)
        {
        }
        
        public void ReduceRage(float amount)
        {
        }
        
        public void ReduceStamina(float amount)
        {
            UseStamina(amount);
        }
        
        public void AddTemporarySpeedBonus(float bonus, float duration)
        {
            AddSpeedBonus(bonus);
            Invoke(nameof(RemoveTemporarySpeedBonus), duration);
        }
        
        private void RemoveTemporarySpeedBonus()
        {
            RemoveSpeedBonus(0.5f);
        }
        
        public bool HasMetalEquipped()
        {
            return false;
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
        MentalSpeed,
        AirCapacity
    }
    
    public enum Faction
    {
        Neutral,
        Obligators,
        SteelMinistry,
        NobleHouses,
        Koloss,
        Citizens,
        Player
    }
    
    public enum DamageType
    {
        Normal,
        Heavy,
        Light,
        Perfect,
        Critical,
        Allomantic,
        Hemalurgic,
        Piercing,
        Fire,
        Ice
    }
    
    public enum StatusEffectType
    {
        None,
        Poison,
        Burn,
        Slow,
        Stun,
        Fear,
        Blinded,
        Silenced,
        Rooted
    }
    
    public class StatusEffect
    {
        public StatusEffectType Type { get; set; }
        public float Duration { get; set; }
        public float Intensity { get; set; }
        
        public StatusEffect(StatusEffectType type, float duration, float intensity)
        {
            Type = type;
            Duration = duration;
            Intensity = intensity;
        }
    }
    
    public class StatModifier
    {
        public float Value { get; set; }
        public string Source { get; set; }
        
        public StatModifier(float value, string source)
        {
            Value = value;
            Source = source;
        }
    }
}
