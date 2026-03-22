using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class TrapSystem : MonoBehaviour
    {
        [Header("Trap Settings")]
        [SerializeField] private TrapType trapType = TrapType.Spikes;
        [SerializeField] private float trapDamage = 20f;
        [SerializeField] private float trapCooldown = 3f;
        [SerializeField] private float triggerRadius = 1f;
        
        [Header("Trap Effects")]
        [SerializeField] private float slowDuration = 2f;
        [SerializeField] private float slowAmount = 0.5f;
        [SerializeField] private float stunDuration = 1f;
        
        [Header("Stealth Settings")]
        [SerializeField] private bool canBeRevealed = true;
        [SerializeField] private bool isRevealed = false;
        [SerializeField] private float revealDistance = 5f;
        
        [Header("Visual Settings")]
        [SerializeField] private GameObject trapVisual;
        [SerializeField] private ParticleSystem triggerEffect;
        [SerializeField] private AudioClip triggerSound;
        
        [Header("Allomancer Detection")]
        [SerializeField] private bool detectWithTinEyes = true;
        [SerializeField] private bool detectWithMalatium = true;
        
        private bool isTriggered = false;
        private float lastTriggerTime = 0f;
        private List<GameObject> affectedTargets = new List<GameObject>();
        private AudioSource audioSource;
        private Renderer[] trapRenderers;
        
        public enum TrapType
        {
            Spikes,
            Smoke,
            Sound,
            MetalWire,
            TripWire
        }
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            trapRenderers = GetComponentsInChildren<Renderer>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            if (!isRevealed && canBeRevealed)
            {
                HideTrap();
            }
            
            SetupTrapVisuals();
        }
        
        private void SetupTrapVisuals()
        {
            switch (trapType)
            {
                case TrapType.Spikes:
                    SetupSpikeTrap();
                    break;
                case TrapType.Smoke:
                    SetupSmokeTrap();
                    break;
                case TrapType.Sound:
                    SetupSoundTrap();
                    break;
                case TrapType.MetalWire:
                    SetupWireTrap();
                    break;
                case TrapType.TripWire:
                    SetupTripWire();
                    break;
            }
        }
        
        private void SetupSpikeTrap()
        {
            if (trapVisual != null)
            {
                trapVisual.transform.localScale = new Vector3(triggerRadius * 2f, 1f, triggerRadius * 2f);
            }
        }
        
        private void SetupSmokeTrap()
        {
        }
        
        private void SetupSoundTrap()
        {
        }
        
        private void SetupWireTrap()
        {
        }
        
        private void SetupTripWire()
        {
        }
        
        private void Update()
        {
            if (isTriggered && Time.time - lastTriggerTime >= trapCooldown)
            {
                ResetTrap();
            }
            
            if (isRevealed)
            {
                CheckAllomancerDetection();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (isTriggered)
            {
                return;
            }
            
            if (other.CompareTag("Player") || other.CompareTag("Enemy"))
            {
                TriggerTrap(other.gameObject);
            }
        }
        
        private void TriggerTrap(GameObject target)
        {
            if (!isRevealed && canBeRevealed)
            {
                return;
            }
            
            isTriggered = true;
            lastTriggerTime = Time.time;
            
            ApplyTrapEffect(target);
            
            if (triggerEffect != null)
            {
                triggerEffect.Play();
            }
            
            if (audioSource != null && triggerSound != null)
            {
                audioSource.PlayOneShot(triggerSound);
            }
            
            if (OnTrapTriggered != null)
            {
                OnTrapTriggered(target);
            }
        }
        
        private void ApplyTrapEffect(GameObject target)
        {
            PlayerStats stats = target.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(trapDamage);
            }
            
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(trapDamage);
                
                switch (trapType)
                {
                    case TrapType.Spikes:
                        break;
                    case TrapType.Smoke:
                        ApplySlowEffect(target);
                        break;
                    case TrapType.Sound:
                        ApplyStunEffect(target);
                        break;
                    case TrapType.MetalWire:
                        ApplySlowEffect(target);
                        break;
                    case TrapType.TripWire:
                        ApplyStunEffect(target);
                        break;
                }
            }
            
            if (trapType == TrapType.Sound)
            {
                AlertNearbyEnemies();
            }
        }
        
        private void ApplySlowEffect(GameObject target)
        {
            BasicPlayerMove playerMove = target.GetComponent<BasicPlayerMove>();
            if (playerMove != null)
            {
                playerMove.ModifySpeed(slowAmount);
                Invoke(nameof(RemoveSlowEffect), slowDuration);
            }
        }
        
        private void RemoveSlowEffect()
        {
            BasicPlayerMove playerMove = GetComponent<BasicPlayerMove>();
            if (playerMove != null)
            {
                playerMove.ModifySpeed(1f / slowAmount);
            }
        }
        
        private void ApplyStunEffect(GameObject target)
        {
            AIController ai = target.GetComponent<AIController>();
            if (ai != null)
            {
                ai.SetStunned(true);
                Invoke(nameof(RemoveStunEffect), stunDuration);
            }
        }
        
        private void RemoveStunEffect()
        {
            AIController ai = GetComponent<AIController>();
            if (ai != null)
            {
                ai.SetStunned(false);
            }
        }
        
        private void AlertNearbyEnemies()
        {
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, 20f);
            
            foreach (Collider collider in nearbyEnemies)
            {
                if (collider.CompareTag("Enemy"))
                {
                    AIController ai = collider.GetComponent<AIController>();
                    if (ai != null)
                    {
                        ai.InvestigatePoint(transform.position);
                    }
                }
            }
        }
        
        private void ResetTrap()
        {
            isTriggered = false;
            affectedTargets.Clear();
        }
        
        private void HideTrap()
        {
            foreach (Renderer renderer in trapRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
        }
        
        private void ShowTrap()
        {
            foreach (Renderer renderer in trapRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
        }
        
        private void CheckAllomancerDetection()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            
            if (distanceToPlayer > revealDistance)
            {
                return;
            }
            
            TinEyes tinEyes = player.GetComponent<TinEyes>();
            if (tinEyes != null && detectWithTinEyes && tinEyes.IsActive())
            {
                SetRevealed(true);
                return;
            }
            
            MalatiumStore malatium = player.GetComponent<MalatiumStore>();
            if (malatium != null && detectWithMalatium && malatium.IsMalatiumActive())
            {
                SetRevealed(true);
            }
        }
        
        public void SetRevealed(bool revealed)
        {
            isRevealed = revealed;
            
            if (revealed)
            {
                ShowTrap();
            }
            else
            {
                HideTrap();
            }
        }
        
        public bool IsRevealed()
        {
            return isRevealed;
        }
        
        public bool IsTriggered()
        {
            return isTriggered;
        }
        
        public TrapType GetTrapType()
        {
            return trapType;
        }
        
        public void SetTrapType(TrapType type)
        {
            trapType = type;
            SetupTrapVisuals();
        }
        
        public void SetTrapDamage(float damage)
        {
            trapDamage = damage;
        }
        
        public void SetCooldown(float cooldown)
        {
            trapCooldown = cooldown;
        }
        
        public event System.Action<GameObject> OnTrapTriggered;
    }
}
