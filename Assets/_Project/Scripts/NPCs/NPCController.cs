using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.NPCs
{
    public class NPCController : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] private string npcName = "Noble Citizen";
        [SerializeField] private NPCType npcType = NPCType.Civilian;
        [SerializeField] private Faction npcFaction = Faction.Citizens;
        [SerializeField] private int npcLevel = 1;
        
        [Header("Dialogue")]
        [SerializeField] private DialogueSystem.Dialogue[] dialogues;
        [SerializeField] private DialogueSystem.Dialogue currentDialogue;
        [SerializeField] private bool hasRandomDialogues = true;
        
        [Header("Behavior")]
        [SerializeField] private NPCBehavior behavior = NPCBehavior.Idle;
        [SerializeField] private float patrolRadius = 10f;
        [SerializeField] private float waitTime = 5f;
        [SerializeField] private Transform[] waypoints;
        
        [Header("Services")]
        [SerializeField] private bool isMerchant = false;
        [SerializeField] private bool isQuestGiver = false;
        [SerializeField] private bool isTrainer = false;
        
        [Header("Interaction")]
        [SerializeField] private float interactionRadius = 3f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private GameObject interactionPrompt;
        
        [Header("Visual")]
        [SerializeField] private Renderer npcRenderer;
        [SerializeField] private Material friendlyMaterial;
        [SerializeField] private Material hostileMaterial;
        [SerializeField] private Material neutralMaterial;
        
        [Header("Audio")]
        [SerializeField] private AudioClip[] greetingSounds;
        [SerializeField] private AudioClip[] dialogueSounds;
        
        private bool isInteracting = false;
        private bool canInteract = false;
        private bool hasMetPlayer = false;
        private int relationshipLevel = 0;
        private Vector3 originalPosition;
        private int currentWaypointIndex = 0;
        private float waitTimer = 0f;
        
        private UnityEngine.AI.NavMeshAgent agent;
        private Animator animator;
        
        public string NPCName => npcName;
        public NPCType Type => npcType;
        public Faction NpcFaction => npcFaction;
        public int RelationshipLevel => relationshipLevel;
        
        private void Start()
        {
            originalPosition = transform.position;
            
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            animator = GetComponent<Animator>();
            
            SetupNPC();
        }
        
        private void SetupNPC()
        {
            if (agent != null)
            {
                agent.autoBraking = true;
            }
            
            if (isMerchant)
            {
                SetupMerchant();
            }
            
            UpdateVisuals();
        }
        
        private void SetupMerchant()
        {
        }
        
        private void Update()
        {
            UpdateBehavior();
            CheckForPlayer();
            HandleInteraction();
        }
        
        private void UpdateBehavior()
        {
            switch (behavior)
            {
                case NPCBehavior.Idle:
                    HandleIdleBehavior();
                    break;
                case NPCBehavior.Patrol:
                    HandlePatrolBehavior();
                    break;
                case NPCBehavior.Follow:
                    HandleFollowBehavior();
                    break;
                case NPCBehavior.Wander:
                    HandleWanderBehavior();
                    break;
            }
        }
        
        private void HandleIdleBehavior()
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }
        }
        
        private void HandlePatrolBehavior()
        {
            if (waypoints == null || waypoints.Length == 0)
                return;
            
            if (agent != null && agent.isOnNavMeshPath)
            {
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    waitTimer += Time.deltaTime;
                    
                    if (waitTimer >= waitTime)
                    {
                        waitTimer = 0f;
                        NextWaypoint();
                    }
                }
            }
        }
        
        private void NextWaypoint()
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            
            if (waypoints[currentWaypointIndex] != null && agent != null)
            {
                agent.SetDestination(waypoints[currentWaypointIndex].position);
            }
        }
        
        private void HandleFollowBehavior()
        {
        }
        
        private void HandleWanderBehavior()
        {
            if (agent != null && !agent.pathPending && agent.remainingDistance < 0.5f)
            {
                Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
                randomDirection += originalPosition;
                
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, UnityEngine.AI.NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
        }
        
        private void CheckForPlayer()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRadius);
            
            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    canInteract = true;
                    
                    if (!hasMetPlayer)
                    {
                        hasMetPlayer = true;
                        PlayGreeting();
                    }
                    
                    ShowInteractionPrompt(true);
                    return;
                }
            }
            
            canInteract = false;
            ShowInteractionPrompt(false);
        }
        
        private void HandleInteraction()
        {
            if (canInteract && Input.GetKeyDown(interactKey))
            {
                StartInteraction();
            }
            
            if (isInteracting && Input.GetKeyDown(KeyCode.Escape))
            {
                EndInteraction();
            }
        }
        
        private void StartInteraction()
        {
            isInteracting = true;
            
            DialogueSystem dialogueSystem = FindObjectOfType<DialogueSystem>();
            if (dialogueSystem != null)
            {
                SelectDialogue();
                dialogueSystem.StartDialogue(currentDialogue);
                dialogueSystem.OnDialogueEnded += OnDialogueEnded;
            }
            
            if (animator != null)
            {
                animator.SetTrigger("Interact");
            }
            
            ShowInteractionPrompt(false);
        }
        
        private void EndInteraction()
        {
            isInteracting = false;
            
            DialogueSystem dialogueSystem = FindObjectOfType<DialogueSystem>();
            if (dialogueSystem != null)
            {
                dialogueSystem.EndDialogue();
                dialogueSystem.OnDialogueEnded -= OnDialogueEnded;
            }
        }
        
        private void OnDialogueEnded()
        {
            isInteracting = false;
        }
        
        private void SelectDialogue()
        {
            if (currentDialogue != null)
            {
                return;
            }
            
            if (hasRandomDialogues && dialogues != null && dialogues.Length > 0)
            {
                int randomIndex = Random.Range(0, dialogues.Length);
                currentDialogue = dialogues[randomIndex];
            }
        }
        
        private void ShowInteractionPrompt(bool show)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(show);
            }
        }
        
        private void PlayGreeting()
        {
            if (greetingSounds != null && greetingSounds.Length > 0)
            {
                AudioClip clip = greetingSounds[Random.Range(0, greetingSounds.Length)];
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }
        
        public void SetRelationship(int level)
        {
            relationshipLevel = Mathf.Clamp(level, -100, 100);
            UpdateVisuals();
        }
        
        public void ModifyRelationship(int amount)
        {
            relationshipLevel = Mathf.Clamp(relationshipLevel + amount, -100, 100);
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            if (npcRenderer == null)
                return;
            
            Material materialToUse = neutralMaterial;
            
            if (relationshipLevel > 0)
            {
                materialToUse = friendlyMaterial;
            }
            else if (relationshipLevel < 0)
            {
                materialToUse = hostileMaterial;
            }
            
            if (materialToUse != null)
            {
                npcRenderer.material = materialToUse;
            }
        }
        
        public bool CanGiveQuest()
        {
            return isQuestGiver;
        }
        
        public QuestManager.Quest GetNextQuest()
        {
            if (!isQuestGiver)
                return null;
            
            return null;
        }
        
        public void StartTrade()
        {
            if (!isMerchant)
                return;
            
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowMerchantUI(npcName);
            }
        }
        
        public void OnCombatStarted()
        {
            if (behavior != NPCBehavior.Flee)
            {
                behavior = NPCBehavior.Idle;
            }
        }
        
        public void OnPlayerEnteredCombat()
        {
            if (IsHostileToPlayer())
            {
                AlertNearbyHostiles();
            }
        }
        
        private bool IsHostileToPlayer()
        {
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats == null)
                return false;
            
            return npcFaction != playerStats.PlayerFaction && IsFactionHostile(npcFaction);
        }
        
        private bool IsFactionHostile(Faction faction)
        {
            return faction == Faction.Koloss || faction == Faction.SteelMinistry;
        }
        
        private void AlertNearbyHostiles()
        {
            Collider[] nearbyNPCs = Physics.OverlapSphere(transform.position, 20f);
            
            foreach (var npc in nearbyNPCs)
            {
                NPCController npcController = npc.GetComponent<NPCController>();
                if (npcController != null && npcController.NpcFaction == npcFaction)
                {
                    npcController.ModifyRelationship(-10);
                }
            }
        }
        
        public NPCData GetNPCData()
        {
            return new NPCData
            {
                name = npcName,
                type = npcType,
                faction = npcFaction,
                relationshipLevel = relationshipLevel,
                hasMetPlayer = hasMetPlayer
            };
        }
        
        public void LoadNPCData(NPCData data)
        {
            npcName = data.name;
            npcType = data.type;
            npcFaction = data.faction;
            relationshipLevel = data.relationshipLevel;
            hasMetPlayer = data.hasMetPlayer;
            
            UpdateVisuals();
        }
    }
    
    public enum NPCType
    {
        Civilian,
        Noble,
        Guard,
        Merchant,
        QuestGiver,
        Trainer,
        Enemy,
        Ally
    }
    
    public enum NPCBehavior
    {
        Idle,
        Patrol,
        Follow,
        Wander,
        Flee,
        Guard
    }
    
    [System.Serializable]
    public class NPCData
    {
        public string name;
        public NPCType type;
        public Faction faction;
        public int relationshipLevel;
        public bool hasMetPlayer;
    }
}
