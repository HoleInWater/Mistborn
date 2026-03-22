using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MistbornGame.SkillTree
{
    public enum SkillNodeState
    {
        Locked,
        Available,
        Purchased,
        Maxed
    }
    
    public class SkillNode : MonoBehaviour
    {
        [Header("Node Configuration")]
        [SerializeField] private string skillId;
        [SerializeField] private string skillName;
        [SerializeField] [TextArea] private string skillDescription;
        [SerializeField] private int skillTier = 1;
        [SerializeField] private int maxRank = 5;
        [SerializeField] private int currentRank = 0;
        
        [Header("Costs")]
        [SerializeField] private int metalCost = 1;
        [SerializeField] private int skillPointCost = 1;
        [SerializeField] private int experienceCost = 100;
        
        [Header("Prerequisites")]
        [SerializeField] private string[] requiredSkillIds;
        [SerializeField] private int[] requiredSkillRanks;
        
        [Header("Bonus Per Rank")]
        [SerializeField] private float[] rankBonuses;
        
        [Header("Visual")]
        [SerializeField] private Image nodeIcon;
        [SerializeField] private Image nodeBackground;
        [SerializeField] private Image progressRing;
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject purchaseButton;
        
        [Header("Colors")]
        [SerializeField] private Color lockedColor = Color.gray;
        [SerializeField] private Color availableColor = Color.yellow;
        [SerializeField] private Color purchasedColor = Color.green;
        [SerializeField] private Color maxedColor = Color.cyan;
        
        [Header("Connection Lines")]
        [SerializeField] private LineRenderer[] connectionLines;
        [SerializeField] private GameObject[] connectedNodes;
        
        private SkillNodeState currentState = SkillNodeState.Locked;
        private SkillTreeController skillTreeController;
        private SkillNodeEvent onSkillStateChanged;
        
        public string SkillId => skillId;
        public string SkillName => skillName;
        public string SkillDescription => skillDescription;
        public int CurrentRank => currentRank;
        public int MaxRank => maxRank;
        public float CurrentBonus => currentRank > 0 && rankBonuses.Length >= currentRank ? rankBonuses[currentRank - 1] : 0f;
        public SkillNodeState CurrentState => currentState;
        
        public event SkillNodeEvent OnSkillStateChanged
        {
            add { onSkillStateChanged += value; }
            remove { onSkillStateChanged -= value; }
        }
        
        private void Start()
        {
            skillTreeController = FindObjectOfType<SkillTreeController>();
            UpdateVisuals();
            CheckPrerequisites();
        }
        
        public void Initialize(SkillTreeSaveData saveData)
        {
            if (saveData != null && saveData.skillRanks.ContainsKey(skillId))
            {
                currentRank = saveData.skillRanks[skillId];
            }
            UpdateVisuals();
            CheckPrerequisites();
        }
        
        public void CheckPrerequisites()
        {
            if (skillTreeController == null) return;
            
            if (currentRank >= maxRank)
            {
                SetState(SkillNodeState.Maxed);
                return;
            }
            
            if (currentRank > 0)
            {
                SetState(SkillNodeState.Purchased);
                return;
            }
            
            bool allPrerequisitesMet = true;
            
            for (int i = 0; i < requiredSkillIds.Length; i++)
            {
                SkillNode requiredNode = skillTreeController.GetSkillNode(requiredSkillIds[i]);
                if (requiredNode == null || requiredNode.CurrentRank < requiredSkillRanks[i])
                {
                    allPrerequisitesMet = false;
                    break;
                }
            }
            
            SetState(allPrerequisitesMet ? SkillNodeState.Available : SkillNodeState.Locked);
        }
        
        public bool CanPurchase()
        {
            if (currentState == SkillNodeState.Maxed || currentState == SkillNodeState.Locked)
                return false;
            
            if (currentRank >= maxRank)
                return false;
            
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats == null)
                return false;
            
            if (playerStats.SkillPoints < skillPointCost)
                return false;
            
            if (playerStats.Experience < experienceCost)
                return false;
            
            InventorySystem inventory = FindObjectOfType<InventorySystem>();
            if (inventory != null && !inventory.HasMetal(metalCost))
                return false;
            
            return true;
        }
        
        public bool Purchase()
        {
            if (!CanPurchase())
                return false;
            
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats == null)
                return false;
            
            playerStats.UseSkillPoints(skillPointCost);
            playerStats.UseExperience(experienceCost);
            
            InventorySystem inventory = FindObjectOfType<InventorySystem>();
            inventory?.UseMetal(metalCost);
            
            currentRank++;
            
            ApplySkillBonus();
            
            CheckPrerequisites();
            skillTreeController?.UpdateConnectedNodes(this);
            
            onSkillStateChanged?.Invoke(this);
            
            return true;
        }
        
        public void Refund()
        {
            if (currentRank <= 0)
                return;
            
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats == null)
                return;
            
            for (int i = 0; i < currentRank; i++)
            {
                playerStats.AddSkillPoints(skillPointCost);
                playerStats.AddExperience(experienceCost);
            }
            
            InventorySystem inventory = FindObjectOfType<InventorySystem>();
            inventory?.AddMetal(metalCost * currentRank);
            
            RemoveSkillBonus();
            
            currentRank = 0;
            
            CheckPrerequisites();
            skillTreeController?.UpdateConnectedNodes(this);
            
            onSkillStateChanged?.Invoke(this);
        }
        
        private void ApplySkillBonus()
        {
            PlayerStats playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
                playerStats = FindObjectOfType<PlayerStats>();
            
            if (playerStats == null)
                return;
            
            SkillBonus skillBonus = GetSkillBonus();
            if (skillBonus != null)
            {
                skillBonus.Apply(playerStats, CurrentBonus);
            }
        }
        
        private void RemoveSkillBonus()
        {
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats == null)
                return;
            
            SkillBonus skillBonus = GetSkillBonus();
            if (skillBonus != null)
            {
                skillBonus.Remove(playerStats, CurrentBonus);
            }
        }
        
        private SkillBonus GetSkillBonus()
        {
            return GetComponent<SkillBonus>() ?? gameObject.AddComponent<SkillBonus>();
        }
        
        private void SetState(SkillNodeState newState)
        {
            if (currentState == newState)
                return;
            
            currentState = newState;
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            if (nodeBackground != null)
            {
                switch (currentState)
                {
                    case SkillNodeState.Locked:
                        nodeBackground.color = lockedColor;
                        break;
                    case SkillNodeState.Available:
                        nodeBackground.color = availableColor;
                        break;
                    case SkillNodeState.Purchased:
                        nodeBackground.color = purchasedColor;
                        break;
                    case SkillNodeState.Maxed:
                        nodeBackground.color = maxedColor;
                        break;
                }
            }
            
            if (progressRing != null)
            {
                float fillAmount = (float)currentRank / maxRank;
                progressRing.fillAmount = fillAmount;
            }
            
            if (rankText != null)
            {
                rankText.text = $"{currentRank}/{maxRank}";
            }
            
            if (nameText != null)
            {
                nameText.text = skillName;
            }
            
            if (lockIcon != null)
            {
                lockIcon.SetActive(currentState == SkillNodeState.Locked);
            }
            
            if (purchaseButton != null)
            {
                purchaseButton.SetActive(currentState == SkillNodeState.Available || currentState == SkillNodeState.Purchased);
            }
            
            UpdateConnectionLines();
        }
        
        private void UpdateConnectionLines()
        {
            if (connectionLines == null)
                return;
            
            foreach (var line in connectionLines)
            {
                if (line == null)
                    continue;
                
                SkillNode targetNode = line.GetComponent<SkillNode>();
                if (targetNode == null)
                    continue;
                
                bool isConnected = false;
                
                for (int i = 0; i < requiredSkillIds.Length; i++)
                {
                    if (requiredSkillIds[i] == targetNode.SkillId)
                    {
                        isConnected = true;
                        break;
                    }
                }
                
                if (isConnected)
                {
                    Color lineColor = currentState == SkillNodeState.Purchased || currentState == SkillNodeState.Maxed
                        ? purchasedColor : lockedColor;
                    line.startColor = lineColor;
                    line.endColor = lineColor;
                }
            }
        }
        
        public void ShowTooltip()
        {
            if (skillTreeController != null)
            {
                skillTreeController.ShowTooltip(this);
            }
        }
        
        public void HideTooltip()
        {
            if (skillTreeController != null)
            {
                skillTreeController.HideTooltip();
            }
        }
        
        public void OnClick()
        {
            if (skillTreeController != null)
            {
                skillTreeController.SelectSkillNode(this);
            }
        }
        
        public SkillNodeSaveData GetSaveData()
        {
            return new SkillNodeSaveData
            {
                skillId = skillId,
                currentRank = currentRank
            };
        }
    }
    
    public delegate void SkillNodeEvent(SkillNode node);
    
    [System.Serializable]
    public class SkillNodeSaveData
    {
        public string skillId;
        public int currentRank;
    }
    
    [System.Serializable]
    public class SkillTreeSaveData
    {
        public System.Collections.Generic.Dictionary<string, int> skillRanks = new System.Collections.Generic.Dictionary<string, int>();
    }
    
    public class SkillBonus : MonoBehaviour
    {
        public enum BonusType
        {
            Damage,
            Defense,
            Speed,
            AllomanticPower,
            FeruchemyPower,
            ResourceGain,
            CooldownReduction
        }
        
        public BonusType bonusType;
        
        public void Apply(PlayerStats stats, float bonus)
        {
            switch (bonusType)
            {
                case BonusType.Damage:
                    stats.AddDamageMultiplier(bonus);
                    break;
                case BonusType.Defense:
                    stats.AddDefenseBonus(bonus);
                    break;
                case BonusType.Speed:
                    stats.AddSpeedBonus(bonus);
                    break;
                case BonusType.AllomanticPower:
                    stats.AddAllomanticPowerBonus(bonus);
                    break;
                case BonusType.CooldownReduction:
                    stats.AddCooldownReduction(bonus);
                    break;
            }
        }
        
        public void Remove(PlayerStats stats, float bonus)
        {
            switch (bonusType)
            {
                case BonusType.Damage:
                    stats.RemoveDamageMultiplier(bonus);
                    break;
                case BonusType.Defense:
                    stats.RemoveDefenseBonus(bonus);
                    break;
                case BonusType.Speed:
                    stats.RemoveSpeedBonus(bonus);
                    break;
                case BonusType.AllomanticPower:
                    stats.RemoveAllomanticPowerBonus(bonus);
                    break;
                case BonusType.CooldownReduction:
                    stats.RemoveCooldownReduction(bonus);
                    break;
            }
        }
    }
}
