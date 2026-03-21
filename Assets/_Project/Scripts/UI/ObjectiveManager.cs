using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.UI
{
    public class ObjectiveManager : MonoBehaviour
    {
        public static ObjectiveManager Instance { get; private set; }

        [System.Serializable]
        public class Objective
        {
            public string id;
            public string title;
            [TextArea(2, 4)]
            public string description;
            public bool isCompleted;
            public bool isActive;
            public Objective[] subObjectives;
        }

        [Header("Objectives")]
        [SerializeField] private List<Objective> m_objectives = new List<Objective>();
        [SerializeField] private int m_maxActiveObjectives = 3;

        [Header("UI")]
        [SerializeField] private GameObject m_objectiveUIElement;
        [SerializeField] private Transform m_objectiveContainer;

        private List<Objective> m_activeObjectives = new List<Objective>();
        private Dictionary<string, Objective> m_objectiveLookup = new Dictionary<string, Objective>();
        private List<GameObject> m_uiElements = new List<GameObject>();

        public event System.Action<Objective> OnObjectiveActivated;
        public event System.Action<Objective> OnObjectiveCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildLookup();
        }

        private void BuildLookup()
        {
            m_objectiveLookup.Clear();
            foreach (Objective obj in m_objectives)
            {
                m_objectiveLookup[obj.id] = obj;
                if (obj.subObjectives != null)
                {
                    foreach (Objective sub in obj.subObjectives)
                    {
                        m_objectiveLookup[sub.id] = sub;
                    }
                }
            }
        }

        public void ActivateObjective(string objectiveId)
        {
            if (!m_objectiveLookup.TryGetValue(objectiveId, out Objective objective))
            {
                Debug.LogWarning($"Objective '{objectiveId}' not found");
                return;
            }

            if (objective.isCompleted || objective.isActive) return;

            objective.isActive = true;
            m_activeObjectives.Add(objective);

            if (m_activeObjectives.Count > m_maxActiveObjectives)
            {
                Objective oldest = m_activeObjectives[0];
                oldest.isActive = false;
                m_activeObjectives.RemoveAt(0);
            }

            OnObjectiveActivated?.Invoke(objective);
            UpdateUI();
        }

        public void CompleteObjective(string objectiveId)
        {
            if (!m_objectiveLookup.TryGetValue(objectiveId, out Objective objective))
                return;

            if (objective.isCompleted) return;

            objective.isCompleted = true;
            objective.isActive = false;
            m_activeObjectives.Remove(objective);

            if (objective.subObjectives != null)
            {
                bool allSubComplete = true;
                foreach (Objective sub in objective.subObjectives)
                {
                    if (!sub.isCompleted)
                    {
                        allSubComplete = false;
                        break;
                    }
                }

                if (allSubComplete)
                {
                    OnObjectiveCompleted?.Invoke(objective);
                }
                else
                {
                    foreach (Objective sub in objective.subObjectives)
                    {
                        if (!sub.isCompleted)
                        {
                            ActivateObjective(sub.id);
                            break;
                        }
                    }
                }
            }
            else
            {
                OnObjectiveCompleted?.Invoke(objective);
            }

            UpdateUI();
        }

        public bool IsObjectiveCompleted(string objectiveId)
        {
            return m_objectiveLookup.TryGetValue(objectiveId, out Objective obj) && obj.isCompleted;
        }

        public bool IsObjectiveActive(string objectiveId)
        {
            return m_objectiveLookup.TryGetValue(objectiveId, out Objective obj) && obj.isActive;
        }

        public List<Objective> GetActiveObjectives()
        {
            return new List<Objective>(m_activeObjectives);
        }

        private void UpdateUI()
        {
            ClearUI();

            foreach (Objective obj in m_activeObjectives)
            {
                CreateUIElement(obj);
            }
        }

        private void CreateUIElement(Objective objective)
        {
            if (m_objectiveUIElement == null || m_objectiveContainer == null) return;

            GameObject element = Instantiate(m_objectiveUIElement, m_objectiveContainer);

            UnityEngine.UI.Text[] texts = element.GetComponentsInChildren<UnityEngine.UI.Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = $"• {objective.title}";
                texts[1].text = objective.description;
            }

            m_uiElements.Add(element);
        }

        private void ClearUI()
        {
            foreach (GameObject element in m_uiElements)
            {
                if (element != null)
                    Destroy(element);
            }
            m_uiElements.Clear();
        }

        public void ResetAllObjectives()
        {
            foreach (Objective obj in m_objectives)
            {
                obj.isCompleted = false;
                obj.isActive = false;
            }
            m_activeObjectives.Clear();
            UpdateUI();
        }
    }
}
