using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Mistborn.UI
{
    public class QuickUseSystem : MonoBehaviour
    {
        public static QuickUseSystem Instance { get; private set; }

        [Header("Slots")]
        [SerializeField] private int m_quickSlotCount = 6;
        [SerializeField] private QuickUseSlot[] m_slots;

        [Header("UI")]
        [SerializeField] private GameObject m_quickUsePanel;
        [SerializeField] private Transform m_slotContainer;
        [SerializeField] private GameObject m_slotPrefab;
        [SerializeField] private Text m_cooldownText;

        [Header("Settings")]
        [SerializeField] private KeyCode[] m_slotKeys = new KeyCode[]
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
            KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6
        };

        private Dictionary<int, QuickUseItem> m_quickItems = new Dictionary<int, QuickUseItem>();
        private float[] m_cooldowns;
        private bool m_isInitialized;

        public event System.Action<int, QuickUseItem> OnItemAssigned;
        public event System.Action<int> OnItemUsed;
        public event System.Action<int, float> OnCooldownChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Initialize();
        }

        private void Initialize()
        {
            m_cooldowns = new float[m_quickSlotCount];

            if (m_slots == null || m_slots.Length != m_quickSlotCount)
            {
                CreateSlots();
            }

            m_isInitialized = true;
        }

        private void CreateSlots()
        {
            m_slots = new QuickUseSlot[m_quickSlotCount];

            if (m_slotContainer != null && m_slotPrefab != null)
            {
                foreach (Transform child in m_slotContainer)
                {
                    Destroy(child.gameObject);
                }

                for (int i = 0; i < m_quickSlotCount; i++)
                {
                    GameObject slotObj = Instantiate(m_slotPrefab, m_slotContainer);
                    QuickUseSlot slot = slotObj.GetComponent<QuickUseSlot>();
                    if (slot == null)
                    {
                        slot = slotObj.AddComponent<QuickUseSlot>();
                    }

                    slot.Initialize(i, m_slotKeys[i]);
                    slot.OnSlotClicked += HandleSlotClicked;
                    slot.OnSlotHovered += HandleSlotHovered;

                    m_slots[i] = slot;
                }
            }
        }

        private void Update()
        {
            UpdateCooldowns();
            CheckKeyInput();
        }

        private void UpdateCooldowns()
        {
            for (int i = 0; i < m_quickSlotCount; i++)
            {
                if (m_cooldowns[i] > 0)
                {
                    m_cooldowns[i] -= Time.deltaTime;
                    if (m_cooldowns[i] < 0) m_cooldowns[i] = 0;

                    if (m_slots[i] != null)
                    {
                        m_slots[i].SetCooldown(m_cooldowns[i]);
                    }

                    OnCooldownChanged?.Invoke(i, m_cooldowns[i]);
                }
            }
        }

        private void CheckKeyInput()
        {
            for (int i = 0; i < m_quickSlotCount; i++)
            {
                if (Input.GetKeyDown(m_slotKeys[i]))
                {
                    UseSlot(i);
                }
            }

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                for (int i = 0; i < m_quickSlotCount; i++)
                {
                    if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
                    {
                        AssignMode(i);
                        break;
                    }
                }
            }
        }

        private void HandleSlotClicked(int slotIndex)
        {
            UseSlot(slotIndex);
        }

        private void HandleSlotHovered(int slotIndex)
        {
            if (m_slots[slotIndex] != null && m_slots[slotIndex].CurrentItem != null)
            {
            }
        }

        public void AssignItem(int slotIndex, QuickUseItem item)
        {
            if (slotIndex < 0 || slotIndex >= m_quickSlotCount) return;

            m_quickItems[slotIndex] = item;

            if (m_slots[slotIndex] != null)
            {
                m_slots[slotIndex].SetItem(item);
            }

            OnItemAssigned?.Invoke(slotIndex, item);
        }

        public void UnassignSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= m_quickSlotCount) return;

            m_quickItems.Remove(slotIndex);

            if (m_slots[slotIndex] != null)
            {
                m_slots[slotIndex].ClearItem();
            }
        }

        public bool UseSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= m_quickSlotCount) return false;

            if (m_cooldowns[slotIndex] > 0) return false;

            if (!m_quickItems.TryGetValue(slotIndex, out QuickUseItem item)) return false;

            if (item == null) return false;

            if (!item.CanUse()) return false;

            bool success = item.Use();

            if (success)
            {
                m_cooldowns[slotIndex] = item.Cooldown;
                OnItemUsed?.Invoke(slotIndex);

                if (m_slots[slotIndex] != null)
                {
                    m_slots[slotIndex].PlayUseAnimation();
                }
            }

            return success;
        }

        private void AssignMode(int slotIndex)
        {
        }

        public QuickUseItem GetSlotItem(int slotIndex)
        {
            m_quickItems.TryGetValue(slotIndex, out QuickUseItem item);
            return item;
        }

        public void ClearAllSlots()
        {
            m_quickItems.Clear();
            m_cooldowns = new float[m_quickSlotCount];

            foreach (QuickUseSlot slot in m_slots)
            {
                slot?.ClearItem();
            }
        }

        public void ShowPanel(bool show)
        {
            if (m_quickUsePanel != null)
            {
                m_quickUsePanel.SetActive(show);
            }
        }
    }

    public class QuickUseSlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image m_iconImage;
        [SerializeField] private Image m_cooldownOverlay;
        [SerializeField] private Text m_keyText;
        [SerializeField] private Text m_countText;
        [SerializeField] private Image m_readyIndicator;

        [Header("Settings")]
        [SerializeField] private int m_slotIndex;
        [SerializeField] private KeyCode m_slotKey;

        private QuickUseItem m_currentItem;
        private float m_currentCooldown;
        private bool m_isHovered;

        public QuickUseItem CurrentItem => m_currentItem;

        public event System.Action<int> OnSlotClicked;
        public event System.Action<int> OnSlotHovered;

        public void Initialize(int index, KeyCode key)
        {
            m_slotIndex = index;
            m_slotKey = key;

            if (m_keyText != null)
            {
                m_keyText.text = key.ToString().Replace("Alpha", "");
            }

            ClearItem();
        }

        public void SetItem(QuickUseItem item)
        {
            m_currentItem = item;

            if (m_iconImage != null && item != null)
            {
                m_iconImage.sprite = item.Icon;
                m_iconImage.enabled = true;
            }
            else if (m_iconImage != null)
            {
                m_iconImage.enabled = false;
            }

            UpdateCount();
        }

        public void ClearItem()
        {
            m_currentItem = null;
            m_currentCooldown = 0f;

            if (m_iconImage != null)
                m_iconImage.enabled = false;

            if (m_cooldownOverlay != null)
                m_cooldownOverlay.fillAmount = 0f;

            if (m_countText != null)
                m_countText.text = "";
        }

        public void SetCooldown(float cooldown)
        {
            m_currentCooldown = cooldown;

            if (m_currentItem != null && m_currentItem.Cooldown > 0)
            {
                float fill = 1f - (cooldown / m_currentItem.Cooldown);
                if (m_cooldownOverlay != null)
                    m_cooldownOverlay.fillAmount = fill;
            }
        }

        private void UpdateCount()
        {
            if (m_countText != null && m_currentItem != null)
            {
                m_countText.text = m_currentItem.Quantity > 1 ? m_currentItem.Quantity.ToString() : "";
            }
        }

        public void PlayUseAnimation()
        {
        }

        public void OnPointerClick()
        {
            OnSlotClicked?.Invoke(m_slotIndex);
        }

        public void OnPointerEnter()
        {
            m_isHovered = true;
            OnSlotHovered?.Invoke(m_slotIndex);
        }

        public void OnPointerExit()
        {
            m_isHovered = false;
        }
    }

    public abstract class QuickUseItem : ScriptableObject
    {
        public string itemName;
        public Sprite icon;
        public float Cooldown => cooldown;
        public int Quantity => quantity;

        [SerializeField] protected float cooldown = 1f;
        [SerializeField] protected int quantity = 1;

        public abstract bool CanUse();
        public abstract bool Use();
        public abstract string GetDescription();
    }

    public class HealthPotionItem : QuickUseItem
    {
        [SerializeField] private float healAmount = 25f;

        public override bool CanUse()
        {
            return quantity > 0;
        }

        public override bool Use()
        {
            PlayerHealth health = FindObjectOfType<PlayerHealth>();
            if (health != null && health.CurrentHealth < health.MaxHealth)
            {
                health.Heal(healAmount);
                quantity--;
                return true;
            }
            return false;
        }

        public override string GetDescription()
        {
            return $"Restores {healAmount} health";
        }
    }
}
