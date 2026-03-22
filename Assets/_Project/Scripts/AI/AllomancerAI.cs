using UnityEngine;
using System.Collections;

namespace MistbornGame.AI
{
    public class AllomancerAI : MonoBehaviour
    {
        [Header("Allomancy Configuration")]
        [SerializeField] private Allomancy.Allomancer.MetalType[] availableMetals;
        [SerializeField] private bool useAllomancyCombat = true;
        
        [Header("Push/Pull")]
        [SerializeField] private float pushRange = 30f;
        [SerializeField] private float pullRange = 25f;
        [SerializeField] private float pushForce = 20f;
        [SerializeField] private float pullForce = 15f;
        [SerializeField] private float metalSenseRange = 20f;
        
        [Header("Combat Timing")]
        [SerializeField] private float allomancyCooldown = 2f;
        [SerializeField] private float burstDuration = 1f;
        [SerializeField] private float burstCooldown = 10f;
        
        [Header("Defense")]
        [SerializeField] private bool useDefenseFlares = true;
        [SerializeField] private float defenseThreshold = 0.3f;
        [SerializeField] private float defenseDuration = 0.5f;
        
        [Header("Offense")]
        [SerializeField] private bool useOffenseFlares = true;
        [SerializeField] private float offenseThreshold = 0.7f;
        
        [Header("Metal Burning")]
        [SerializeField] private bool burnIron = true;
        [SerializeField] private bool burnSteel = true;
        [SerializeField] private bool burnTin = true;
        [SerializeField] private bool burnPewter = true;
        [SerializeField] private bool burnCopper = false;
        [SerializeField] private bool burnBronze = false;
        
        private Enemy.Enemy enemy;
        private Transform player;
        private float lastAllomancyUse = -100f;
        private float lastBurstUse = -100f;
        private bool isBurningMetal = false;
        private bool hasMetallicAdvantage = false;
        
        private void Start()
        {
            enemy = GetComponent<Enemy.Enemy>();
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        private void Update()
        {
            if (!useAllomancyCombat || player == null)
                return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= metalSenseRange)
            {
                hasMetallicAdvantage = CheckMetallicAdvantage();
            }
            
            if (Time.time - lastAllomancyUse > allomancyCooldown)
            {
                TryAllomanticAction(distanceToPlayer);
            }
            
            if (Time.time - lastBurstUse > burstCooldown && hasMetallicAdvantage)
            {
                TryBurstFlare();
            }
            
            UpdateDefense();
        }
        
        private bool CheckMetallicAdvantage()
        {
            Allomancy.Allomancer playerAllomancer = player.GetComponent<Allomancy.Allomancer>();
            if (playerAllomancer == null)
                return false;
            
            Allomancy.Allomancer.MetalType[] playerMetals = playerAllomancer.GetActiveMetals();
            
            bool hasIron = HasMetal(Allomancy.Allomancer.MetalType.Iron);
            bool hasSteel = HasMetal(Allomancy.Allomancer.MetalType.Steel);
            bool playerHasPewter = false;
            bool playerHasTin = false;
            
            foreach (var metal in playerMetals)
            {
                if (metal == Allomancy.Allomancer.MetalType.Pewter)
                    playerHasPewter = true;
                if (metal == Allomancy.Allomancer.MetalType.Tin)
                    playerHasTin = true;
            }
            
            return (hasIron || hasSteel) && !playerHasPewter && !playerHasTin;
        }
        
        private bool HasMetal(Allomancy.Allomancer.MetalType metal)
        {
            foreach (var m in availableMetals)
            {
                if (m == metal)
                    return true;
            }
            return false;
        }
        
        private void TryAllomanticAction(float distanceToPlayer)
        {
            if (distanceToPlayer > pushRange && distanceToPlayer > pullRange)
                return;
            
            if (enemy.CurrentHealth / enemy.MaxHealth < defenseThreshold && useDefenseFlares)
            {
                TryDefenseFlare();
                return;
            }
            
            if (distanceToPlayer > enemy.AttackRange * 2)
            {
                if (HasMetal(Allomancy.Allomancer.MetalType.Iron) && burnIron)
                {
                    TryIronPull(distanceToPlayer);
                }
            }
            else if (distanceToPlayer < enemy.AttackRange * 1.5f)
            {
                if (HasMetal(Allomancy.Allomancer.MetalType.Steel) && burnSteel)
                {
                    TrySteelPush(distanceToPlayer);
                }
            }
        }
        
        private void TryIronPull(float distanceToPlayer)
        {
            if (!HasMetal(Allomancy.Allomancer.MetalType.Iron) || !burnIron)
                return;
            
            if (distanceToPlayer > pullRange)
                return;
            
            Vector3 direction = (transform.position - player.position).normalized;
            direction.y = 0;
            
            RaycastHit hit;
            if (Physics.Raycast(player.position + Vector3.up, direction, out hit, pullRange))
            {
                if (hit.transform == transform)
                {
                    ExecuteIronPull(direction);
                }
            }
        }
        
        private void ExecuteIronPull(Vector3 direction)
        {
            lastAllomancyUse = Time.time;
            isBurningMetal = true;
            
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(direction * pullForce * 100f, ForceMode.Impulse);
            }
            
            enemy.Animator?.SetTrigger("IronPull");
            
            SpawnMetalVfx("IronPullVFX", direction);
        }
        
        private void TrySteelPush(float distanceToPlayer)
        {
            if (!HasMetal(Allomancy.Allomancer.MetalType.Steel) || !burnSteel)
                return;
            
            if (distanceToPlayer > pushRange)
                return;
            
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, direction, out hit, pushRange))
            {
                if (hit.transform == player)
                {
                    ExecuteSteelPush(direction);
                }
            }
        }
        
        private void ExecuteSteelPush(Vector3 direction)
        {
            lastAllomancyUse = Time.time;
            isBurningMetal = true;
            
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(direction * pushForce * 100f, ForceMode.Impulse);
            }
            
            enemy.Animator?.SetTrigger("SteelPush");
            
            SpawnMetalVfx("SteelPushVFX", direction);
            
            StartCoroutine(EndMetalBurning());
        }
        
        private void TryDefenseFlare()
        {
            if (!useDefenseFlares || !HasMetal(Allomancy.Allomancer.MetalType.Pewter) || !burnPewter)
                return;
            
            StartCoroutine(DefenseFlare());
        }
        
        private IEnumerator DefenseFlare()
        {
            lastAllomancyUse = Time.time;
            isBurningMetal = true;
            
            enemy.Agent.speed *= 1.5f;
            enemy.Damage *= 1.3f;
            
            enemy.Animator?.SetBool("PewterBoost", true);
            
            yield return new WaitForSeconds(defenseDuration);
            
            enemy.Agent.speed /= 1.5f;
            enemy.Damage /= 1.3f;
            
            enemy.Animator?.SetBool("PewterBoost", false);
            
            isBurningMetal = false;
        }
        
        private void TryBurstFlare()
        {
            if (enemy.CurrentHealth / enemy.MaxHealth < offenseThreshold && useOffenseFlares)
            {
                if (HasMetal(Allomancy.Allomancer.MetalType.Iron) && HasMetal(Allomancy.Allomancer.MetalType.Steel))
                {
                    StartCoroutine(BurstFlare());
                }
            }
        }
        
        private IEnumerator BurstFlare()
        {
            lastBurstUse = Time.time;
            
            float elapsed = 0f;
            
            while (elapsed < burstDuration)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                direction.y = 0;
                
                ExecuteSteelPush(direction * 1.5f);
                
                yield return new WaitForSeconds(0.3f);
                elapsed += 0.3f;
            }
            
            isBurningMetal = false;
        }
        
        private void UpdateDefense()
        {
            if (!useDefenseFlares)
                return;
            
            Allomancy.Allomancer playerAllomancer = player?.GetComponent<Allomancy.Allomancer>();
            if (playerAllomancer == null || !playerAllomancer.IsBurningMetal)
                return;
            
            Allomancy.Allomancer.MetalType[] activeMetals = playerAllomancer.GetActiveMetals();
            bool playerBurningOffensiveMetal = false;
            
            foreach (var metal in activeMetals)
            {
                if (metal == Allomancy.Allomancer.MetalType.Iron || metal == Allomancy.Allomancer.MetalType.Steel)
                {
                    playerBurningOffensiveMetal = true;
                    break;
                }
            }
            
            if (playerBurningOffensiveMetal)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                
                if (distance < pullRange && HasMetal(Allomancy.Allomancer.MetalType.Copper) && burnCopper)
                {
                    ActivateCopperCloud();
                }
            }
        }
        
        private void ActivateCopperCloud()
        {
            enemy.Animator?.SetTrigger("CopperCloud");
            
            Collider[] affectedEnemies = Physics.OverlapSphere(transform.position, metalSenseRange);
            foreach (var col in affectedEnemies)
            {
                Allomancy.Allomancer allomancer = col.GetComponent<Allomancy.Allomancer>();
                if (allomancer != null && allomancer != this)
                {
                    allomancer.DisableMetalSense(metalSenseRange * 0.5f);
                }
            }
        }
        
        private void SpawnMetalVfx(string vfxName, Vector3 direction)
        {
            GameObject vfx = Resources.Load<GameObject>("Effects/" + vfxName);
            if (vfx != null)
            {
                GameObject instance = Instantiate(vfx, transform.position + Vector3.up, Quaternion.LookRotation(direction));
                Destroy(instance, 2f);
            }
        }
        
        private IEnumerator EndMetalBurning()
        {
            yield return new WaitForSeconds(0.5f);
            isBurningMetal = false;
        }
        
        public bool IsBurningMetal => isBurningMetal;
        public bool HasMetallicAdvantage => hasMetallicAdvantage;
    }
}
