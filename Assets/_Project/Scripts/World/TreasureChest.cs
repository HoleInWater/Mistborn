using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class TreasureChest : MonoBehaviour, HiddenObject
    {
        [Header("Chest Settings")]
        [SerializeField] private bool isLocked = true;
        [SerializeField] private bool requiresKey = false;
        [SerializeField] private bool requiresLockpick = true;
        [SerializeField] private string requiredKeyID = "";
        
        [Header("Loot Settings")]
        [SerializeField] private LootItem[] possibleLoot;
        [SerializeField] private int minLootCount = 1;
        [SerializeField] private int maxLootCount = 3;
        
        [Header("Visual Settings")]
        [SerializeField] private GameObject chestLid;
        [SerializeField] private ParticleSystem openEffect;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip lockedSound;
        [SerializeField] private SpriteRenderer revealIndicator;
        
        [Header("Stealth")]
        [SerializeField] private bool canBeHidden = true;
        [SerializeField] private bool isHidden = false;
        
        private bool isOpen = false;
        private bool isRevealed = false;
        private List<LootItem> awardedLoot = new List<LootItem>();
        private AudioSource audioSource;
        private Renderer[] chestRenderers;
        
        [System.Serializable]
        public class LootItem
        {
            public string itemName;
            public GameObject itemPrefab;
            public int quantity = 1;
            public float dropChance = 1f;
            public int goldValue = 0;
        }
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            chestRenderers = GetComponentsInChildren<Renderer>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            if (isHidden && canBeHidden)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
        
        public void TryOpen(GameObject opener)
        {
            if (isOpen)
            {
                return;
            }
            
            if (isLocked)
            {
                if (requiresKey && HasRequiredKey(opener))
                {
                    OpenChest();
                }
                else if (requiresLockpick)
                {
                    TryLockpick(opener);
                }
                else
                {
                    PlayLockedSound();
                }
            }
            else
            {
                OpenChest();
            }
        }
        
        private bool HasRequiredKey(GameObject player)
        {
            if (string.IsNullOrEmpty(requiredKeyID))
            {
                return true;
            }
            
            InventorySystem inventory = player.GetComponent<InventorySystem>();
            if (inventory != null)
            {
                return inventory.HasItem(requiredKeyID);
            }
            
            return false;
        }
        
        private void TryLockpick(GameObject player)
        {
            LockpickMinigame lockpick = player.GetComponent<LockpickMinigame>();
            if (lockpick != null)
            {
                lockpick.StartMinigame();
                lockpick.OnLockpickCompleted += OnLockpickResult;
            }
            else
            {
                PlayLockedSound();
            }
        }
        
        private void OnLockpickResult(bool success)
        {
            if (success)
            {
                OpenChest();
            }
            else
            {
                PlayLockedSound();
            }
        }
        
        public void OpenChest()
        {
            if (isOpen)
            {
                return;
            }
            
            isOpen = true;
            isLocked = false;
            
            if (audioSource != null && openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }
            
            if (chestLid != null)
            {
                OpenChestLid();
            }
            
            if (openEffect != null)
            {
                openEffect.Play();
            }
            
            GenerateLoot();
            AwardLoot();
            
            if (OnChestOpened != null)
            {
                OnChestOpened(awardedLoot);
            }
        }
        
        private void OpenChestLid()
        {
            StartCoroutine(AnimateChestLid());
        }
        
        private System.Collections.IEnumerator AnimateChestLid()
        {
            Vector3 closedRotation = chestLid.transform.localEulerAngles;
            Vector3 openRotation = closedRotation + new Vector3(-120f, 0f, 0f);
            
            float duration = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                chestLid.transform.localEulerAngles = Vector3.Lerp(closedRotation, openRotation, t);
                yield return null;
            }
            
            chestLid.transform.localEulerAngles = openRotation;
        }
        
        private void GenerateLoot()
        {
            int lootCount = Random.Range(minLootCount, maxLootCount + 1);
            
            List<LootItem> availableLoot = new List<LootItem>();
            foreach (LootItem loot in possibleLoot)
            {
                if (Random.value <= loot.dropChance)
                {
                    availableLoot.Add(loot);
                }
            }
            
            for (int i = 0; i < lootCount && availableLoot.Count > 0; i++)
            {
                LootItem selectedLoot = availableLoot[Random.Range(0, availableLoot.Count)];
                awardedLoot.Add(selectedLoot);
                availableLoot.Remove(selectedLoot);
            }
        }
        
        private void AwardLoot()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }
            
            foreach (LootItem loot in awardedLoot)
            {
                if (loot.itemPrefab != null)
                {
                    Instantiate(loot.itemPrefab, transform.position + Vector3.up, Quaternion.identity);
                }
                
                InventorySystem inventory = player.GetComponent<InventorySystem>();
                if (inventory != null)
                {
                    inventory.AddItem(loot.itemName, loot.quantity);
                }
                
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null && loot.goldValue > 0)
                {
                    stats.AddGold(loot.goldValue);
                }
            }
        }
        
        private void PlayLockedSound()
        {
            if (audioSource != null && lockedSound != null)
            {
                audioSource.PlayOneShot(lockedSound);
            }
        }
        
        public void Reveal()
        {
            if (!canBeHidden)
            {
                return;
            }
            
            isRevealed = true;
            Show();
            
            if (OnRevealed != null)
            {
                OnRevealed();
            }
        }
        
        public void Hide()
        {
            if (!canBeHidden)
            {
                return;
            }
            
            isRevealed = false;
            
            foreach (Renderer renderer in chestRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
        }
        
        private void Show()
        {
            foreach (Renderer renderer in chestRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
        }
        
        public bool IsOpen()
        {
            return isOpen;
        }
        
        public bool IsLocked()
        {
            return isLocked;
        }
        
        public bool IsHidden()
        {
            return isHidden;
        }
        
        public void Unlock()
        {
            isLocked = false;
        }
        
        public void Lock()
        {
            isLocked = true;
        }
        
        public List<LootItem> GetLoot()
        {
            return awardedLoot;
        }
        
        public void AddLootItem(LootItem item)
        {
            LootItem[] newLoot = new LootItem[possibleLoot.Length + 1];
            possibleLoot.CopyTo(newLoot, 0);
            newLoot[newLoot.Length - 1] = item;
            possibleLoot = newLoot;
        }
        
        public event System.Action<List<LootItem>> OnChestOpened;
        public event System.Action OnRevealed;
    }
}
