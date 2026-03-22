using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace MistbornGame.SkillTree
{
    public class SkillTreeController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject skillTreePanel;
        [SerializeField] private Transform skillNodeContainer;
        [SerializeField] private GameObject skillNodePrefab;
        
        [Header("Tooltip")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipTitle;
        [SerializeField] private TextMeshProUGUI tooltipDescription;
        [SerializeField] private TextMeshProUGUI tooltipStats;
        [SerializeField] private TextMeshProUGUI tooltipCost;
        
        [Header("Skill Categories")]
        [SerializeField] private Button allomancyTabButton;
        [SerializeField] private Button feruchemyTabButton;
        [SerializeField] private Button combatTabButton;
        [SerializeField] private GameObject allomancyPanel;
        [SerializeField] private GameObject feruchemyPanel;
        [SerializeField] private GameObject combatPanel;
        
        [Header("Resources")]
        [SerializeField] private TextMeshProUGUI skillPointsText;
        [SerializeField] private TextMeshProUGUI metalCountText;
        
        [Header("Skill Trees")]
        [SerializeField] private AllomancySkillTree allomancyTree;
        [SerializeField] private FeruchemySkillTree feruchemyTree;
        [SerializeField] private CombatSkillTree combatTree;
        
        [Header("Audio")]
        [SerializeField] private AudioClip purchaseSound;
        [SerializeField] private AudioClip refundSound;
        [SerializeField] private AudioClip tabSwitchSound;
        
        private Dictionary<string, SkillNode> allSkillNodes = new Dictionary<string, SkillNode>();
        private SkillNode selectedNode;
        private bool isInitialized = false;
        
        private void Start()
        {
            InitializeSkillTree();
        }
        
        private void InitializeSkillTree()
        {
            if (isInitialized)
                return;
            
            CreateSkillNodes();
            
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.OnSkillPointsChanged += UpdateUI;
                playerStats.OnExperienceChanged += UpdateUI;
            }
            
            InventorySystem inventory = FindObjectOfType<InventorySystem>();
            if (inventory != null)
            {
                inventory.OnMetalChanged += UpdateUI;
            }
            
            isInitialized = true;
            UpdateUI();
        }
        
        private void CreateSkillNodes()
        {
            if (allomancyTree != null)
                CreateNodesForTree(allomancyTree);
            
            if (feruchemyTree != null)
                CreateNodesForTree(feruchemyTree);
            
            if (combatTree != null)
                CreateNodesForTree(combatTree);
        }
        
        private void CreateNodesForTree(SkillTreeBase tree)
        {
            if (tree == null || skillNodeContainer == null)
                return;
            
            foreach (var nodeData in tree.GetAllNodes())
            {
                GameObject nodeObj = Instantiate(skillNodePrefab, skillNodeContainer);
                
                SkillNode node = nodeObj.GetComponent<SkillNode>();
                if (node != null)
                {
                    node.Initialize(null);
                    allSkillNodes[node.SkillId] = node;
                    
                    node.OnSkillStateChanged += OnSkillStateChanged;
                }
            }
        }
        
        private void OnSkillStateChanged(SkillNode node)
        {
            AudioSource.PlayClipAtPoint(purchaseSound, Camera.main.transform.position);
            UpdateUI();
        }
        
        public void SelectSkillNode(SkillNode node)
        {
            selectedNode = node;
            ShowTooltip();
        }
        
        public void ShowTooltip()
        {
            if (selectedNode == null)
                return;
            
            tooltipPanel.SetActive(true);
            
            tooltipTitle.text = selectedNode.SkillName;
            tooltipDescription.text = selectedNode.SkillDescription;
            tooltipStats.text = $"Rank: {selectedNode.CurrentRank}/{selectedNode.MaxRank}\nBonus: {selectedNode.CurrentBonus:F1}%";
            
            if (selectedNode.CurrentState == SkillNodeState.Maxed)
            {
                tooltipCost.text = "MAXED";
            }
            else if (selectedNode.CurrentState == SkillNodeState.Locked)
            {
                tooltipCost.text = "LOCKED - Prerequisites Required";
            }
            else
            {
                tooltipCost.text = $"Cost: {selectedNode.GetComponent<SkillBonus>() != null}\n" +
                                  $"Metal: {GetMetalCost()} | Points: {GetSkillPointCost()} | XP: {GetExperienceCost()}";
            }
        }
        
        public void HideTooltip()
        {
            tooltipPanel.SetActive(false);
        }
        
        public void PurchaseSelectedSkill()
        {
            if (selectedNode == null)
                return;
            
            if (selectedNode.Purchase())
            {
                AudioSource.PlayClipAtPoint(purchaseSound, Camera.main.transform.position);
            }
            else
            {
                Debug.Log("Cannot purchase skill: " + selectedNode.SkillName);
            }
        }
        
        public void RefundSelectedSkill()
        {
            if (selectedNode == null)
                return;
            
            selectedNode.Refund();
            AudioSource.PlayClipAtPoint(refundSound, Camera.main.transform.position);
        }
        
        public void SwitchTab(int tabIndex)
        {
            AudioSource.PlayClipAtPoint(tabSwitchSound, Camera.main.transform.position);
            
            allomancyPanel?.SetActive(tabIndex == 0);
            feruchemyPanel?.SetActive(tabIndex == 1);
            combatPanel?.SetActive(tabIndex == 2);
            
            if (allomancyTabButton != null)
                allomancyTabButton.interactable = tabIndex != 0;
            if (feruchemyTabButton != null)
                feruchemyTabButton.interactable = tabIndex != 1;
            if (combatTabButton != null)
                combatTabButton.interactable = tabIndex != 2;
        }
        
        public void UpdateConnectedNodes(SkillNode sourceNode)
        {
            foreach (var kvp in allSkillNodes)
            {
                if (kvp.Value != sourceNode)
                {
                    kvp.Value.CheckPrerequisites();
                }
            }
        }
        
        public SkillNode GetSkillNode(string skillId)
        {
            if (allSkillNodes.ContainsKey(skillId))
                return allSkillNodes[skillId];
            return null;
        }
        
        public void UpdateUI()
        {
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                if (skillPointsText != null)
                    skillPointsText.text = $"Skill Points: {playerStats.SkillPoints}";
            }
            
            InventorySystem inventory = FindObjectOfType<InventorySystem>();
            if (inventory != null && metalCountText != null)
            {
                metalCountText.text = $"Metal: {inventory.TotalMetal}";
            }
            
            foreach (var node in allSkillNodes.Values)
            {
                node.CheckPrerequisites();
            }
        }
        
        public void OpenSkillTree()
        {
            skillTreePanel?.SetActive(true);
            Time.timeScale = 0f;
            UpdateUI();
        }
        
        public void CloseSkillTree()
        {
            skillTreePanel?.SetActive(false);
            Time.timeScale = 1f;
        }
        
        public void ToggleSkillTree()
        {
            if (skillTreePanel.activeSelf)
                CloseSkillTree();
            else
                OpenSkillTree();
        }
        
        public void SaveSkillTree()
        {
            SkillTreeSaveData saveData = new SkillTreeSaveData();
            
            foreach (var kvp in allSkillNodes)
            {
                saveData.skillRanks[kvp.Key] = kvp.Value.CurrentRank;
            }
            
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("SkillTreeData", json);
            PlayerPrefs.Save();
        }
        
        public void LoadSkillTree()
        {
            if (!PlayerPrefs.HasKey("SkillTreeData"))
                return;
            
            string json = PlayerPrefs.GetString("SkillTreeData");
            SkillTreeSaveData saveData = JsonUtility.FromJson<SkillTreeSaveData>(json);
            
            foreach (var kvp in saveData.skillRanks)
            {
                SkillNode node = GetSkillNode(kvp.Key);
                if (node != null)
                {
                    for (int i = 0; i < kvp.Value; i++)
                    {
                        node.Purchase();
                    }
                }
            }
        }
        
        private int GetMetalCost()
        {
            return selectedNode != null ? 1 : 0;
        }
        
        private int GetSkillPointCost()
        {
            return selectedNode != null ? 1 : 0;
        }
        
        private int GetExperienceCost()
        {
            return selectedNode != null ? 100 : 0;
        }
    }
    
    public abstract class SkillTreeBase : ScriptableObject
    {
        public abstract IEnumerable<SkillNodeData> GetAllNodes();
    }
    
    [System.Serializable]
    public class SkillNodeData
    {
        public string skillId;
        public string skillName;
        public string description;
        public int tier;
        public int maxRank;
        public string[] requiredSkills;
        public int[] requiredRanks;
        public float[] bonuses;
    }
    
    [CreateAssetMenu(fileName = "AllomancySkillTree", menuName = "Mistborn/Allomancy Skill Tree")]
    public class AllomancySkillTree : SkillTreeBase
    {
        [SerializeField] private SkillNodeData[] nodes;
        
        public override IEnumerable<SkillNodeData> GetAllNodes()
        {
            return nodes;
        }
    }
    
    [CreateAssetMenu(fileName = "FeruchemySkillTree", menuName = "Mistborn/Feruchemy Skill Tree")]
    public class FeruchemySkillTree : SkillTreeBase
    {
        [SerializeField] private SkillNodeData[] nodes;
        
        public override IEnumerable<SkillNodeData> GetAllNodes()
        {
            return nodes;
        }
    }
    
    [CreateAssetMenu(fileName = "CombatSkillTree", menuName = "Mistborn/Combat Skill Tree")]
    public class CombatSkillTree : SkillTreeBase
    {
        [SerializeField] private SkillNodeData[] nodes;
        
        public override IEnumerable<SkillNodeData> GetAllNodes()
        {
            return nodes;
        }
    }
}
