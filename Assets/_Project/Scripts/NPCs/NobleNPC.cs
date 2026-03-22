using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.NPCs
{
    public class NobleNPC : NPCController
    {
        [Header("Noble Specific")]
        [SerializeField] private string nobleHouseName = "Venture";
        [SerializeField] private NobleRank nobleRank = NobleRank.Master;
        [SerializeField] private bool hasKolossServants = false;
        [SerializeField] private int kolossServantCount = 2;
        
        [Header("Noble Privileges")]
        [SerializeField] private bool canUsePewter = false;
        [SerializeField] private bool canUseTin = false;
        [SerializeField] private bool canUseBrass = true;
        [SerializeField] private float pewterBonusStrength = 1.2f;
        
        [Header("Wealth")]
        [SerializeField] private int wealthLevel = 5;
        [SerializeField] private int metalReserve = 100;
        [SerializeField] private bool hasSecretTreasure = true;
        
        [Header("Alliances")]
        [SerializeField] private Faction[] alliedFactions;
        [SerializeField] private Faction[] hostileFactions;
        [SerializeField] private string[] alliedHouses;
        
        [Header("Combat")]
        [SerializeField] private bool isTrainedFighter = true;
        [SerializeField] private int combatSkill = 3;
        [SerializeField] private bool hasBodyguards = true;
        [SerializeField] private int bodyguardCount = 2;
        
        [Header("Speech Patterns")]
        [SerializeField] private SpeechPattern speechPattern = SpeechPattern.Arrogant;
        [SerializeField] private AudioClip[] nobleGreetings;
        
        private bool hasBetrayed = false;
        private bool isConspiring = false;
        private int politicalPower = 50;
        
        protected override void Start()
        {
            base.Start();
            
            SetRelationship(wealthLevel * 10);
            
            SetupNobleAbilities();
        }
        
        private void SetupNobleAbilities()
        {
            if (canUsePewter)
            {
                ThugPowers pewterAbility = gameObject.AddComponent<ThugPowers>();
            }
            
            if (canUseTin)
            {
                SetupTinSenses();
            }
            
            if (canUseBrass)
            {
                SetupBrassSoothe();
            }
        }
        
        private void SetupTinSenses()
        {
        }
        
        private void SetupBrassSoothe()
        {
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (isConspiring)
            {
                UpdateConspiracy();
            }
        }
        
        private void UpdateConspiracy()
        {
        }
        
        public void StartConspiracy()
        {
            isConspiring = true;
        }
        
        public void EndConspiracy()
        {
            isConspiring = false;
        }
        
        public override void ModifyRelationship(int amount)
        {
            base.ModifyRelationship(amount);
            
            if (amount < 0 && !hasBetrayed)
            {
                CheckForBetrayal();
            }
        }
        
        private void CheckForBetrayal()
        {
            if (GetRelationshipLevel() < -50)
            {
                hasBetrayed = true;
                OnBetrayal();
            }
        }
        
        private void OnBetrayal()
        {
            SetRelationship(-100);
            
            AlertAllies();
            
            QuestManager questManager = FindObjectOfType<QuestManager>();
            questManager?.OnNobleBetrayed(nobleHouseName);
        }
        
        private void AlertAllies()
        {
        }
        
        public void PerformPoliticalAction(PoliticalAction action)
        {
            switch (action)
            {
                case PoliticalAction.Bribe:
                    PerformBribe();
                    break;
                case PoliticalAction.Threaten:
                    PerformThreaten();
                    break;
                case PoliticalAction.FormAlliance:
                    FormAlliance();
                    break;
                case PoliticalAction.BreakAlliance:
                    BreakAlliance();
                    break;
            }
        }
        
        private void PerformBribe()
        {
            if (wealthLevel >= 3)
            {
                ModifyRelationship(20);
            }
        }
        
        private void PerformThreaten()
        {
            if (combatSkill >= 4)
            {
                ModifyRelationship(-10);
            }
        }
        
        private void FormAlliance()
        {
            politicalPower += 10;
        }
        
        private void BreakAlliance()
        {
            politicalPower -= 10;
        }
        
        public override void OnPlayerEnteredCombat()
        {
            base.OnPlayerEnteredCombat();
            
            if (IsHostileToPlayer())
            {
                if (hasBodyguards)
                {
                    SummonBodyguards();
                }
            }
        }
        
        private void SummonBodyguards()
        {
        }
        
        public int GetWealthLevel()
        {
            return wealthLevel;
        }
        
        public string GetHouseName()
        {
            return nobleHouseName;
        }
        
        public NobleRank GetNobleRank()
        {
            return nobleRank;
        }
        
        public int GetPoliticalPower()
        {
            return politicalPower;
        }
        
        public bool HasSecretTreasure()
        {
            return hasSecretTreasure;
        }
        
        public void RevealSecretTreasure()
        {
            if (hasSecretTreasure)
            {
                hasSecretTreasure = false;
                
                if (metalReserve > 0)
                {
                    SpawnMetalReserve();
                }
            }
        }
        
        private void SpawnMetalReserve()
        {
            GameObject metalPickup = Resources.Load<GameObject>("Pickups/MetalPickup");
            if (metalPickup != null)
            {
                for (int i = 0; i < metalReserve; i++)
                {
                    Vector3 offset = Random.insideUnitSphere * 2f;
                    offset.y = 1f;
                    Instantiate(metalPickup, transform.position + offset, Quaternion.identity);
                }
            }
        }
        
        protected override void PlayGreeting()
        {
            if (nobleGreetings != null && nobleGreetings.Length > 0)
            {
                AudioClip clip = nobleGreetings[Random.Range(0, nobleGreetings.Length)];
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
            else
            {
                base.PlayGreeting();
            }
        }
        
        public override DialogueSystem.Dialogue GetNextQuest()
        {
            if (!CanGiveQuest())
                return null;
            
            switch (nobleRank)
            {
                case NobleRank.Lord:
                    return GetLordQuest();
                case NobleRank.Master:
                    return GetMasterQuest();
                case NobleRank.Keeper:
                    return GetKeeperQuest();
                default:
                    return null;
            }
        }
        
        private DialogueSystem.Dialogue GetLordQuest()
        {
            return new DialogueSystem.Dialogue
            {
                speakerName = npcName,
                dialogueText = "I require your services for a delicate matter..."
            };
        }
        
        private DialogueSystem.Dialogue GetMasterQuest()
        {
            return new DialogueSystem.Dialogue
            {
                speakerName = npcName,
                dialogueText = $"As a member of House {nobleHouseName}, I have a task for you."
            };
        }
        
        private DialogueSystem.Dialogue GetKeeperQuest()
        {
            return new DialogueSystem.Dialogue
            {
                speakerName = npcName,
                dialogueText = "The Obligators grow suspicious. We need your help."
            };
        }
    }
    
    public enum NobleRank
    {
        Servant,
        Keeper,
        Master,
        Lord,
        HighLord
    }
    
    public enum PoliticalAction
    {
        Bribe,
        Threaten,
        FormAlliance,
        BreakAlliance,
        Spy,
        Sabotage
    }
}
