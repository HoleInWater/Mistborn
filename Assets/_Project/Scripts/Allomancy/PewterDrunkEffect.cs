using UnityEngine;

namespace MistbornGameplay
{
    public class PewterDrunkEffect : MonoBehaviour
    {
        [Header("Effect Settings")]
        [SerializeField] private float strengthBonus = 2f;
        [SerializeField] private float speedPenalty = 0.5f;
        [SerializeField] private float accuracyPenalty = 0.3f;
        [SerializeField] private float coordinationPenalty = 0.4f;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem pewterGlow;
        [SerializeField] private float cameraShakeIntensity = 0.1f;
        [SerializeField] private float effectRadius = 5f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip pewterDrinkSound;
        [SerializeField] private AudioClip pewterLoopSound;
        
        private BasicPlayerMove playerMove;
        private PlayerStats playerStats;
        private AudioSource audioSource;
        private bool isEffectActive = false;
        private float effectEndTime = 0f;
        
        private void Awake()
        {
            playerMove = GetComponent<BasicPlayerMove>();
            playerStats = GetComponent<PlayerStats>();
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            if (pewterGlow != null)
            {
                pewterGlow.Stop();
            }
        }
        
        public void Activate(float duration)
        {
            if (isEffectActive)
            {
                effectEndTime += duration;
                return;
            }
            
            isEffectActive = true;
            effectEndTime = Time.time + duration;
            
            ApplyEffects();
            
            if (audioSource != null && pewterDrinkSound != null)
            {
                audioSource.clip = pewterDrinkSound;
                audioSource.Play();
                
                Invoke(nameof(StartLoopSound), pewterDrinkSound.length);
            }
            
            if (pewterGlow != null)
            {
                pewterGlow.Play();
            }
            
            if (OnEffectActivated != null)
            {
                OnEffectActivated();
            }
        }
        
        public void Deactivate()
        {
            if (!isEffectActive)
            {
                return;
            }
            
            isEffectActive = false;
            
            RemoveEffects();
            
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            
            if (pewterGlow != null)
            {
                pewterGlow.Stop();
            }
            
            if (OnEffectDeactivated != null)
            {
                OnEffectDeactivated();
            }
        }
        
        private void StartLoopSound()
        {
            if (pewterLoopSound != null && audioSource != null && isEffectActive)
            {
                audioSource.clip = pewterLoopSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        
        private void ApplyEffects()
        {
            if (playerStats != null)
            {
                playerStats.ModifyStat(StatType.AttackPower, strengthBonus);
                playerStats.ModifyStat(StatType.RangedAccuracy, -accuracyPenalty);
                playerStats.ModifyStat(StatType.Coordination, -coordinationPenalty);
            }
            
            if (playerMove != null)
            {
                playerMove.moveSpeed *= speedPenalty;
            }
        }
        
        private void RemoveEffects()
        {
            if (playerStats != null)
            {
                playerStats.ModifyStat(StatType.AttackPower, -strengthBonus);
                playerStats.ModifyStat(StatType.RangedAccuracy, accuracyPenalty);
                playerStats.ModifyStat(StatType.Coordination, coordinationPenalty);
            }
            
            if (playerMove != null)
            {
                playerMove.moveSpeed /= speedPenalty;
            }
        }
        
        private void Update()
        {
            if (isEffectActive)
            {
                if (Time.time >= effectEndTime)
                {
                    Deactivate();
                }
            }
        }
        
        public bool IsActive()
        {
            return isEffectActive;
        }
        
        public float GetRemainingTime()
        {
            return Mathf.Max(0f, effectEndTime - Time.time);
        }
        
        public void SetEffectStrength(float strength, float speed)
        {
            strengthBonus = strength;
            speedPenalty = speed;
        }
        
        public event System.Action OnEffectActivated;
        public event System.Action OnEffectDeactivated;
    }
}
