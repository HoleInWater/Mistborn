using UnityEngine;

namespace MistbornGame.World
{
    public class NobleCompound : MonoBehaviour
    {
        [Header("Compound Configuration")]
        [SerializeField] private string compoundName = "House Venture Compound";
        [SerializeField] private string houseName = "Venture";
        [SerializeField] private Faction controllingHouse;
        [SerializeField] private int compoundLevel = 1;
        
        [Header("Security")]
        [SerializeField] private bool hasSecurity = true;
        [SerializeField] private int guardCount = 5;
        [SerializeField] private int eliteGuardCount = 1;
        [SerializeField] private bool hasInquisitor = false;
        [SerializeField] private float detectionRadius = 30f;
        
        [Header("Services")]
        [SerializeField] private bool hasMerchant = true;
        [SerializeField] private bool hasWeaponsmith = false;
        [SerializeField] private bool hasMetalSeller = true;
        [SerializeField] private bool hasInformationBroker = false;
        
        [Header("Loot")]
        [SerializeField] private LootTier lootTier = LootTier.Standard;
        [SerializeField] private int metalReserve = 50;
        [SerializeField] private bool hasSecretVault = false;
        [SerializeField] private GameObject secretVaultPrefab;
        
        [Header("Events")]
        [SerializeField] private bool randomEventsEnabled = true;
        [SerializeField] private float eventChance = 0.1f;
        
        [Header("Territory")]
        [SerializeField] private float territoryRadius = 100f;
        [SerializeField] private Faction[] hostileFactions;
        [SerializeField] private Faction[] alliedFactions;
        
        private bool isDiscovered = false;
        private bool isInCombat = false;
        private bool vaultOpened = false;
        private bool guardsAlerted = false;
        
        private Collider[] guardColliders;
        private PlayerStats playerStats;
        private Faction playerFaction;
        
        private void Start()
        {
            InitializeCompound();
        }
        
        private void InitializeCompound()
        {
            if (hasSecurity)
            {
                SpawnGuards();
            }
            
            if (hasSecretVault && secretVaultPrefab != null)
            {
                SetupSecretVault();
            }
            
            UpdateCompoundSecurity();
        }
        
        private void SpawnGuards()
        {
            guardColliders = new Collider[guardCount + eliteGuardCount];
        }
        
        private void SetupSecretVault()
        {
            if (secretVaultPrefab != null)
            {
                GameObject vault = Instantiate(secretVaultPrefab, transform.position, Quaternion.identity);
                VaultController vaultController = vault.GetComponent<VaultController>();
                if (vaultController != null)
                {
                    vaultController.Initialize(lootTier, houseName);
                }
            }
        }
        
        private void UpdateCompoundSecurity()
        {
            if (!hasSecurity)
                return;
            
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            
            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    PlayerStats stats = col.GetComponent<PlayerStats>();
                    Faction faction = stats != null ? stats.PlayerFaction : Faction.Neutral;
                    
                    if (IsHostile(faction))
                    {
                        AlertSecurity();
                        break;
                    }
                }
            }
        }
        
        private bool IsHostile(Faction faction)
        {
            foreach (var hostile in hostileFactions)
            {
                if (hostile == faction)
                    return true;
            }
            return false;
        }
        
        private void AlertSecurity()
        {
            if (guardsAlerted)
                return;
            
            guardsAlerted = true;
            
            Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius);
            
            foreach (var enemy in enemies)
            {
                Enemy.Enemy enemyScript = enemy.GetComponent<Enemy.Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.SetAlerted(true);
                }
            }
            
            OnCompoundAlerted();
        }
        
        private void OnCompoundAlerted()
        {
            UIManager uiManager = FindObjectOfType<UIManager>();
            uiManager?.ShowNotification($"{compoundName} guards are on alert!");
            
            QuestManager questManager = FindObjectOfType<QuestManager>();
            questManager?.OnCompoundAlerted(compoundName);
        }
        
        private void Update()
        {
            if (randomEventsEnabled && Random.value < eventChance * Time.deltaTime)
            {
                TryTriggerRandomEvent();
            }
        }
        
        private void TryTriggerRandomEvent()
        {
            if (isInCombat)
                return;
            
            float eventRoll = Random.value;
            
            if (eventRoll < 0.3f && hasMerchant)
            {
                TriggerMerchantEvent();
            }
            else if (eventRoll < 0.5f)
            {
                TriggerPatrolEvent();
            }
            else if (eventRoll < 0.7f && hasInformationBroker)
            {
                TriggerInformationEvent();
            }
        }
        
        private void TriggerMerchantEvent()
        {
            if (!hasMerchant)
                return;
            
            UIManager uiManager = FindObjectOfType<UIManager>();
            uiManager?.ShowMerchantUI(houseName);
        }
        
        private void TriggerPatrolEvent()
        {
            if (!hasSecurity)
                return;
            
            StartCoroutine(PatrolEventCoroutine());
        }
        
        private IEnumerator PatrolEventCoroutine()
        {
            yield return new WaitForSeconds(5f);
            
            AlertSecurity();
        }
        
        private void TriggerInformationEvent()
        {
            if (!hasInformationBroker)
                return;
            
            DialogueSystem dialogue = FindObjectOfType<DialogueSystem>();
            if (dialogue != null)
            {
                dialogue.StartDialogue(GetInformationDialogue());
            }
        }
        
        private DialogueSystem.Dialogue GetInformationDialogue()
        {
            return new DialogueSystem.Dialogue
            {
                speakerName = "Information Broker",
                dialogueText = $"I've heard rumors about {houseName}..."
            };
        }
        
        public void OnCombatStarted()
        {
            isInCombat = true;
            
            if (!guardsAlerted)
            {
                AlertSecurity();
            }
        }
        
        public void OnCombatEnded()
        {
            isInCombat = false;
            
            if (lootTier != LootTier.None)
            {
                SpawnLoot();
            }
        }
        
        private void SpawnLoot()
        {
            if (metalReserve > 0)
            {
                int lootAmount = Mathf.CeilToInt(metalReserve * GetLootMultiplier());
                
                GameObject metalPickup = Resources.Load<GameObject>("Pickups/MetalPickup");
                if (metalPickup != null)
                {
                    for (int i = 0; i < lootAmount; i++)
                    {
                        Vector3 offset = Random.insideUnitSphere * 3f;
                        offset.y = 1f;
                        Instantiate(metalPickup, transform.position + offset, Quaternion.identity);
                    }
                }
            }
        }
        
        private float GetLootMultiplier()
        {
            switch (lootTier)
            {
                case LootTier.Standard:
                    return 1f;
                case LootTier.Rich:
                    return 2f;
                case LootTier.LordRuler:
                    return 5f;
                default:
                    return 1f;
            }
        }
        
        public void OnVaultOpened()
        {
            vaultOpened = true;
            
            QuestManager questManager = FindObjectOfType<QuestManager>();
            questManager?.OnVaultOpened(houseName);
        }
        
        public bool CanAccessMerchant()
        {
            if (!hasMerchant)
                return false;
            
            if (guardsAlerted && isInCombat)
                return false;
            
            return IsAllied(playerFaction);
        }
        
        private bool IsAllied(Faction faction)
        {
            foreach (var allied in alliedFactions)
            {
                if (allied == faction)
                    return true;
            }
            return false;
        }
        
        public void Discover()
        {
            if (isDiscovered)
                return;
            
            isDiscovered = true;
            
            if (showOnMapOnDiscovery)
            {
                ShowOnMap();
            }
        }
        
        public void ShowOnMap()
        {
        }
        
        public string CompoundName => compoundName;
        public string HouseName => houseName;
        public Faction ControllingHouse => controllingHouse;
        public bool IsInCombat => isInCombat;
        public bool GuardsAlerted => guardsAlerted;
    }
    
    public class VaultController : MonoBehaviour
    {
        [SerializeField] private LootTier lootTier;
        [SerializeField] private string vaultHouse;
        
        public void Initialize(LootTier tier, string house)
        {
            lootTier = tier;
            vaultHouse = house;
        }
        
        public void OpenVault()
        {
            SpawnVaultLoot();
        }
        
        private void SpawnVaultLoot()
        {
        }
    }
    
    public enum LootTier
    {
        None,
        Standard,
        Rich,
        LordRuler
    }
}
