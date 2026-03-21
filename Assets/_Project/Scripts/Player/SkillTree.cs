using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mistborn.Player
{
    [CreateAssetMenu(fileName = "SkillTree", menuName = "Mistborn/Skill Tree")]
    public class SkillTree : ScriptableObject
    {
        [Serializable]
        public class SkillNode
        {
            public string skillId;
            public string skillName;
            public string description;
            public Sprite icon;
            public int cost;
            public List<string> prerequisites = new List<string>();
            public bool isUnlocked;
            public int tier;
        }

        [SerializeField] private List<SkillNode> m_nodes = new List<SkillNode>();
        [SerializeField] private int m_totalSkillPoints;

        public List<SkillNode> Nodes => m_nodes;
        public int AvailablePoints => m_totalSkillPoints;

        public SkillNode GetNode(string skillId)
        {
            return m_nodes.Find(n => n.skillId == skillId);
        }

        public bool CanUnlock(string skillId)
        {
            SkillNode node = GetNode(skillId);
            if (node == null || node.isUnlocked) return false;
            if (node.cost > m_totalSkillPoints) return false;

            foreach (string prereq in node.prerequisites)
            {
                SkillNode prereqNode = GetNode(prereq);
                if (prereqNode == null || !prereqNode.isUnlocked)
                    return false;
            }

            return true;
        }

        public bool Unlock(string skillId)
        {
            if (!CanUnlock(skillId)) return false;

            SkillNode node = GetNode(skillId);
            if (node == null) return false;

            node.isUnlocked = true;
            m_totalSkillPoints -= node.cost;
            return true;
        }

        public void Reset()
        {
            foreach (SkillNode node in m_nodes)
            {
                node.isUnlocked = false;
            }
        }
    }

    public class SkillTreeManager : MonoBehaviour
    {
        [SerializeField] private SkillTree m_skillTree;
        [SerializeField] private int m_startingPoints = 5;

        public event System.Action<SkillTree.SkillNode> OnSkillUnlocked;
        public event System.Action<int> OnPointsChanged;

        private void Start()
        {
            if (m_skillTree != null)
            {
                m_skillTree.Nodes.Clear();
            }
        }

        public bool UnlockSkill(string skillId)
        {
            if (m_skillTree == null) return false;

            bool success = m_skillTree.Unlock(skillId);
            if (success)
            {
                SkillTree.SkillNode node = m_skillTree.GetNode(skillId);
                OnSkillUnlocked?.Invoke(node);
                OnPointsChanged?.Invoke(m_skillTree.AvailablePoints);
                ApplySkillEffect(node);
            }
            return success;
        }

        public bool CanUnlock(string skillId)
        {
            return m_skillTree?.CanUnlock(skillId) ?? false;
        }

        public int GetAvailablePoints()
        {
            return m_skillTree?.AvailablePoints ?? 0;
        }

        private void ApplySkillEffect(SkillTree.SkillNode node)
        {
            if (node == null) return;

            switch (node.skillId)
            {
                case "pewter_mastery":
                    ApplyPewterMastery();
                    break;
                case "steel_push_mastery":
                    ApplySteelMastery();
                    break;
                case "iron_pull_mastery":
                    ApplyIronMastery();
                    break;
                case "tin_mastery":
                    ApplyTinMastery();
                    break;
            }
        }

        private void ApplyPewterMastery()
        {
            if (TryGetComponent(out AllomancerController allomancer))
            {
                allomancer.GetReserve(Mistborn.Allomancy.AllomanticMetal.Pewter)?.SetDrainRate(0.8f);
            }
        }

        private void ApplySteelMastery()
        {
        }

        private void ApplyIronMastery()
        {
        }

        private void ApplyTinMastery()
        {
            if (TryGetComponent(out AllomancerController allomancer))
            {
                allomancer.GetReserve(Mistborn.Allomancy.AllomanticMetal.Tin)?.SetDrainRate(0.8f);
            }
        }

        public void AddSkillPoints(int amount)
        {
            if (m_skillTree != null)
            {
                m_skillTree.Nodes[0].cost += amount;
            }
        }
    }
}
