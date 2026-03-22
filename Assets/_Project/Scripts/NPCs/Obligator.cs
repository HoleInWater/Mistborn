using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.NPCs
{
    public class Obligator : NPCController
    {
        [Header("Obligator Specific")]
        [SerializeField] private ObligatorRank obligatorRank = ObligatorRank.Standard;
        [SerializeField] private int obligatorNumber = 0;
        [SerializeField] private string assignedDuty = "Tax Collection";
        
        [Header("Pewter Allomancy")]
        [SerializeField] private bool hasPewter = true;
        [SerializeField] private float pewterBoostLevel = 1.5f;
        
        [Header("Spikes")]
        [SerializeField] private bool hasSpikes = false;
        [SerializeField] private int spikeCount = 4;
        [SerializeField] private bool isInquisitor = false;
        
        [Header("Combat")]
        [SerializeField] private int combatSkillLevel = 5;
        [SerializeField] private bool usesFlare = false;
        [SerializeField] private bool canCommandKoloss = false;
        
        [Header("Authority")]
        [SerializeField] private float authorityRadius = 50f;
        [SerializeField] private bool hasAuthorityOverGuards = true;
        [SerializeField] private bool hasAuthorityOverCitizens = true;
        
        [Header("Resources")]
        [SerializeField] private int confiscatedGoods = 0;
        [SerializeField] private bool hasSeizedProperty = false;
        
        private bool isInquisitorMode = false;
        private bool isHuntingAllomancer = false;
        private float authorityLevel = 1f;
        
        protected override void Start()
        {
            base.Start();
            
            SetupObligatorAbilities();
            
            SetRelationship(-50);
        }
        
        private void SetupObligatorAbilities()
        {
            if (hasPewter)
            {
                ThugPowers pewterAbility = gameObject.AddComponent<ThugPowers>();
            }
            
            if (hasSpikes)
            {
                SetupHemalurgySpikes();
            }
            
            if (isInquisitor)
            {
                SetupInquisitorAbilities();
            }
            
            UpdateAuthorityLevel();
        }
        
        private void SetupHemalurgySpikes()
        {
        }
        
        private void SetupInquisitorAbilities()
        {
        }
        
        private void UpdateAuthorityLevel()
        {
            switch (obligatorRank)
            {
                case ObligatorRank.Standard:
                    authorityLevel = 1f;
                    break;
                case ObligatorRank.Senior:
                    authorityLevel = 2f;
                    break;
                case ObligatorRank.Precentor:
                    authorityLevel = 3f;
                    break;
                case ObligatorRank.Kandra:
                    authorityLevel = 5f;
                    break;
            }
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (isInquisitorMode)
            {
                UpdateInquisitorMode();
            }
            
            if (isHuntingAllomancer)
            {
                UpdateAllomancerHunt();
            }
        }
        
        private void UpdateInquisitorMode()
        {
            if (Physics.CheckSphere(transform.position, authorityRadius))
            {
                ApplyAuthorityEffect();
            }
        }
        
        private void ApplyAuthorityEffect()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, authorityRadius);
            
            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    PlayerStats playerStats = col.GetComponent<PlayerStats>();
                    if (playerStats != null)
                    {
                        playerStats.AddStatusEffect(PlayerStats.StatusEffectType.Fear, 1f, 0.1f);
                    }
                }
                
                if (hasAuthorityOverCitizens)
                {
                    NPCController npc = col.GetComponent<NPCController>();
                    if (npc != null)
                    {
                        npc.ModifyRelationship(-5);
                    }
                }
            }
        }
        
        private void UpdateAllomancerHunt()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, authorityRadius);
            
            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    Allomancy.Allomancer allomancer = col.GetComponent<Allomancy.Allomancer>();
                    if (allomancer != null && allomancer.IsBurningMetal)
                    {
                        OnAllomancerDetected(col.transform);
                    }
                }
            }
        }
        
        private void OnAllomancerDetected(Transform allomancer)
        {
            if (obligatorRank == ObligatorRank.Kandra || isInquisitor)
            {
                StartInquisitorMode();
            }
            else
            {
                AlertMinistry();
            }
        }
        
        private void StartInquisitorMode()
        {
            isInquisitorMode = true;
            
            if (animator != null)
            {
                animator.SetBool("InquisitorMode", true);
            }
            
            if (usesFlare)
            {
                UseFlare();
            }
        }
        
        private void UseFlare()
        {
            if (hasPewter)
            {
                ThugPowers pewter = GetComponent<ThugPowers>();
                if (pewter != null)
                {
                    pewter.ActivatePewterBoost();
                }
            }
            
            Collider[] affected = Physics.OverlapSphere(transform.position, 30f);
            foreach (var col in affected)
            {
                if (col.CompareTag("Player"))
                {
                    Allomancy.Allomancer allomancer = col.GetComponent<Allomancy.Allomancer>();
                    if (allomancer != null)
                    {
                        allomancer.InterruptBurning();
                    }
                }
            }
        }
        
        private void AlertMinistry()
        {
        }
        
        public void StartHuntingAllomancer()
        {
            isHuntingAllomancer = true;
        }
        
        public void StopHuntingAllomancer()
        {
            isHuntingAllomancer = false;
        }
        
        public override void ModifyRelationship(int amount)
        {
            int newRelationship = GetRelationshipLevel() + amount;
            
            if (newRelationship < -75 && !isInquisitorMode)
            {
                StartHuntingAllomancer();
            }
            
            base.ModifyRelationship(amount);
        }
        
        public void PerformObligatorDuty(string duty)
        {
            switch (duty)
            {
                case "Tax Collection":
                    PerformTaxCollection();
                    break;
                case "Inquisition":
                    PerformInquisition();
                    break;
                case "Patrol":
                    PerformPatrol();
                    break;
                case "Investigation":
                    PerformInvestigation();
                    break;
            }
        }
        
        private void PerformTaxCollection()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, authorityRadius);
            
            foreach (var col in colliders)
            {
                NPCController npc = col.GetComponent<NPCController>();
                if (npc != null && npc.Type == NPCType.Noble)
                {
                    int taxAmount = (npc as NobleNPC).GetWealthLevel() * 10;
                    confiscatedGoods += taxAmount;
                    
                    npc.ModifyRelationship(-20);
                }
            }
        }
        
        private void PerformInquisition()
        {
            if (!isInquisitor && obligatorRank != ObligatorRank.Kandra)
                return;
            
            StartInquisitorMode();
        }
        
        private void PerformPatrol()
        {
            SetBehavior(NPCBehavior.Patrol);
        }
        
        private void PerformInvestigation()
        {
        }
        
        public override void OnPlayerEnteredCombat()
        {
            base.OnPlayerEnteredCombat();
            
            if (IsHostileToPlayer())
            {
                if (hasAuthorityOverGuards)
                {
                    CommandGuards();
                }
                
                if (canCommandKoloss)
                {
                    CommandKoloss();
                }
            }
        }
        
        private void CommandGuards()
        {
            Collider[] guards = Physics.OverlapSphere(transform.position, authorityRadius);
            
            foreach (var guard in guards)
            {
                Enemy.SteelGuard steelGuard = guard.GetComponent<Enemy.SteelGuard>();
                if (steelGuard != null)
                {
                    steelGuard.GetComponent<UnityEngine.AI.NavMeshAgent>()?.SetDestination(transform.position);
                }
            }
        }
        
        private void CommandKoloss()
        {
            Collider[] koloss = Physics.OverlapSphere(transform.position, authorityRadius);
            
            foreach (var k in koloss)
            {
                Enemy.Koloss kolossEnemy = k.GetComponent<Enemy.Koloss>();
                if (kolossEnemy != null)
                {
                    kolossEnemy.transform.LookAt(FindObjectOfType<PlayerStats>()?.transform);
                }
            }
        }
        
        public int GetObligatorNumber()
        {
            return obligatorNumber;
        }
        
        public string GetAssignedDuty()
        {
            return assignedDuty;
        }
        
        public float GetAuthorityLevel()
        {
            return authorityLevel;
        }
        
        public override DialogueSystem.Dialogue GetNextQuest()
        {
            if (!CanGiveQuest())
                return null;
            
            return new DialogueSystem.Dialogue
            {
                speakerName = npcName,
                dialogueText = $"The Ministry requires your service. Complete this duty for the Lord Ruler."
            };
        }
    }
    
    public enum ObligatorRank
    {
        Standard,
        Senior,
        Precentor,
        Kandra
    }
}
