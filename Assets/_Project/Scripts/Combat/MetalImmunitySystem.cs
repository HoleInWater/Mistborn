using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mistborn.Combat
{
    public class MetalImmunitySystem : MonoBehaviour
    {
        [Header("Metal Resistances")]
        [SerializeField] private List<MetalResistance> m_resistances = new List<MetalResistance>();

        private Dictionary<AllomanticMetal, float> m_resistanceLookup = new Dictionary<AllomanticMetal, float>();
        private List<StatusEffect> m_activeEffects = new List<StatusEffect>();

        public event Action<AllomanticMetal, float> OnResistanceChanged;
        public event Action<StatusEffect> OnEffectApplied;
        public event Action<StatusEffect> OnEffectRemoved;

        private void Awake()
        {
            BuildResistanceLookup();
        }

        private void BuildResistanceLookup()
        {
            m_resistanceLookup.Clear();
            foreach (MetalResistance resistance in m_resistances)
            {
                if (resistance.metal != AllomanticMetal.Steel)
                {
                    m_resistanceLookup[resistance.metal] = resistance.resistancePercent;
                }
            }
        }

        public float GetResistance(AllomanticMetal metal)
        {
            if (m_resistanceLookup.TryGetValue(metal, out float resistance))
            {
                return resistance;
            }
            return 0f;
        }

        public void SetResistance(AllomanticMetal metal, float resistance)
        {
            m_resistanceLookup[metal] = Mathf.Clamp01(resistance);
            OnResistanceChanged?.Invoke(metal, resistance);
        }

        public void AddResistance(AllomanticMetal metal, float amount)
        {
            float current = GetResistance(metal);
            SetResistance(metal, current + amount);
        }

        public void RemoveResistance(AllomanticMetal metal, float amount)
        {
            float current = GetResistance(metal);
            SetResistance(metal, current - amount);
        }

        public DamageData ApplyMetalEffect(AllomanticMetal metal, DamageData baseDamage)
        {
            float resistance = GetResistance(metal);
            float effectiveDamage = baseDamage.amount * (1f - resistance);

            return new DamageData(effectiveDamage, metal.ToString());
        }

        public void ApplyStatusEffect(StatusEffect effect)
        {
            m_activeEffects.Add(effect);
            OnEffectApplied?.Invoke(effect);
        }

        public void RemoveStatusEffect(StatusEffect effect)
        {
            if (m_activeEffects.Remove(effect))
            {
                OnEffectRemoved?.Invoke(effect);
            }
        }

        public bool HasEffect(Type effectType)
        {
            foreach (StatusEffect effect in m_activeEffects)
            {
                if (effect.GetType() == effectType)
                    return true;
            }
            return false;
        }

        public List<StatusEffect> GetActiveEffects()
        {
            return new List<StatusEffect>(m_activeEffects);
        }

        public void ClearAllEffects()
        {
            m_activeEffects.Clear();
        }

        private void Update()
        {
            for (int i = m_activeEffects.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = m_activeEffects[i];
                effect.Update(Time.deltaTime);

                if (effect.IsExpired)
                {
                    effect.OnExpire();
                    RemoveStatusEffect(effect);
                }
            }
        }

        private void OnDestroy()
        {
            ClearAllEffects();
        }
    }

    [Serializable]
    public class MetalResistance
    {
        public AllomanticMetal metal;
        [Range(0f, 1f)]
        public float resistancePercent;
    }

    public abstract class StatusEffect : MonoBehaviour
    {
        [Header("Effect Settings")]
        [SerializeField] protected float m_duration = 5f;
        [SerializeField] protected float m_tickRate = 1f;
        [SerializeField] protected bool m_canStack = false;
        [SerializeField] protected int m_maxStacks = 3;

        protected float m_currentDuration;
        protected int m_currentStacks;
        protected float m_tickTimer;
        protected bool m_isExpired;
        protected GameObject m_target;

        public bool IsExpired => m_isExpired;
        public int CurrentStacks => m_currentStacks;

        public virtual void Initialize(GameObject target)
        {
            m_target = target;
            m_currentDuration = m_duration;
            m_currentStacks = 1;
            m_tickTimer = 0f;
            m_isExpired = false;
        }

        public virtual void Update(float deltaTime)
        {
            m_currentDuration -= deltaTime;
            m_tickTimer -= deltaTime;

            if (m_tickTimer <= 0)
            {
                OnTick();
                m_tickTimer = m_tickRate;
            }

            if (m_currentDuration <= 0)
            {
                m_isExpired = true;
            }
        }

        protected abstract void OnTick();

        public virtual void OnExpire()
        {
        }

        public virtual void Stack()
        {
            if (!m_canStack) return;

            if (m_currentStacks < m_maxStacks)
            {
                m_currentStacks++;
                m_currentDuration = m_duration;
            }
        }

        public float GetDurationPercent()
        {
            return m_currentDuration / m_duration;
        }
    }

    public class BurningEffect : StatusEffect
    {
        [SerializeField] private float m_damagePerTick = 2f;
        [SerializeField] private float m_spreadChance = 0.1f;

        protected override void OnTick()
        {
            if (m_target == null) return;

            PlayerHealth health = m_target.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(new DamageData(m_damagePerTick * m_currentStacks));

                if (Random.value < m_spreadChance)
                {
                    TrySpreadBurning();
                }
            }
        }

        private void TrySpreadBurning()
        {
            if (m_target == null) return;

            Collider[] nearby = Physics.OverlapSphere(m_target.transform.position, 3f);
            foreach (Collider col in nearby)
            {
                if (col.CompareTag("Enemy"))
                {
                    MetalImmunitySystem immunity = col.GetComponent<MetalImmunitySystem>();
                    if (immunity != null && !immunity.HasEffect(typeof(BurningEffect)))
                    {
                        BurningEffect burning = col.gameObject.AddComponent<BurningEffect>();
                        burning.Initialize(col.gameObject);
                        immunity.ApplyStatusEffect(burning);
                    }
                    break;
                }
            }
        }
    }

    public class BleedingEffect : StatusEffect
    {
        [SerializeField] private float m_damagePerTick = 1f;
        [SerializeField] private float m_healthPercentLoss = 0.02f;
        [SerializeField] private bool m_reducesHealing = true;

        protected override void OnTick()
        {
            if (m_target == null) return;

            PlayerHealth health = m_target.GetComponent<PlayerHealth>();
            if (health != null)
            {
                float damage = m_damagePerTick * m_currentStacks;
                damage += health.MaxHealth * m_healthPercentLoss;
                health.TakeDamage(new DamageData(damage));
            }
        }
    }

    public class PoisonedEffect : StatusEffect
    {
        [SerializeField] private float m_damagePerTick = 0.5f;
        [SerializeField] private bool m_slowsMovement = true;
        [SerializeField] private float m_slowPercent = 0.3f;

        private PlayerController m_playerController;

        protected override void Initialize(GameObject target)
        {
            base.Initialize(target);
            m_playerController = target.GetComponent<PlayerController>();
        }

        protected override void OnTick()
        {
            if (m_target == null) return;

            PlayerHealth health = m_target.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(new DamageData(m_damagePerTick * m_currentStacks));
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (m_slowsMovement && m_playerController != null)
            {
            }
        }
    }

    public class ImmunityAura : MonoBehaviour
    {
        [Header("Aura Settings")]
        [SerializeField] private float m_radius = 5f;
        [SerializeField] private LayerMask m_targetLayers;
        [SerializeField] private AllomanticMetal[] m_grantedImmunities;

        [Header("Effects")]
        [SerializeField] private bool m_immunityExpiresOnLeave = true;

        private List<GameObject> m_affectedTargets = new List<GameObject>();

        private void Update()
        {
            UpdateAffectedTargets();
        }

        private void UpdateAffectedTargets()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, m_radius, m_targetLayers);

            foreach (Collider hit in hits)
            {
                if (!m_affectedTargets.Contains(hit.gameObject))
                {
                    ApplyImmunities(hit.gameObject);
                    m_affectedTargets.Add(hit.gameObject);
                }
            }

            for (int i = m_affectedTargets.Count - 1; i >= 0; i--)
            {
                GameObject target = m_affectedTargets[i];
                if (target == null)
                {
                    m_affectedTargets.RemoveAt(i);
                    continue;
                }

                float dist = Vector3.Distance(transform.position, target.transform.position);
                if (dist > m_radius)
                {
                    if (m_immunityExpiresOnLeave)
                    {
                        RemoveImmunities(target);
                    }
                    m_affectedTargets.RemoveAt(i);
                }
            }
        }

        private void ApplyImmunities(GameObject target)
        {
            MetalImmunitySystem immunity = target.GetComponent<MetalImmunitySystem>();
            if (immunity == null) return;

            foreach (AllomanticMetal metal in m_grantedImmunities)
            {
                immunity.AddResistance(metal, 1f);
            }
        }

        private void RemoveImmunities(GameObject target)
        {
            MetalImmunitySystem immunity = target.GetComponent<MetalImmunitySystem>();
            if (immunity == null) return;

            foreach (AllomanticMetal metal in m_grantedImmunities)
            {
                immunity.RemoveResistance(metal, 1f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_radius);
        }
    }
}
