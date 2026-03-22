using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class CraftingSystem : MonoBehaviour
    {
        [Header("Crafting Settings")]
        [SerializeField] private bool requireWorkbench = false;
        [SerializeField] private CraftingStation currentStation;
        
        [Header("Recipe Database")]
        [SerializeField] private CraftingRecipe[] availableRecipes;
        
        [Header("Skill Requirements")]
        [SerializeField] private bool useSkillRequirements = true;
        [SerializeField] private float baseCraftingTime = 5f;
        
        [Header("Visual")]
        [SerializeField] private GameObject craftingUI;
        [SerializeField] private ParticleSystem craftingEffect;
        
        private InventorySystem inventory;
        private PlayerStats playerStats;
        private bool isCrafting = false;
        private float craftingProgress = 0f;
        private CraftingRecipe currentRecipe;
        private List<string> unlockedRecipes = new List<string>();
        
        public enum CraftingStation
        {
            None,
            Workbench,
            AlchemyTable,
            Forge,
            AllomancerBench
        }
        
        [System.Serializable]
        public class CraftingRecipe
        {
            public string recipeName;
            public CraftingStation requiredStation = CraftingStation.None;
            public List<Ingredient> ingredients = new List<Ingredient>();
            public string resultItemName;
            public int resultQuantity = 1;
            public float craftingTime = 5f;
            public int requiredSkillLevel = 1;
            public string skillType = "Crafting";
        }
        
        [System.Serializable]
        public class Ingredient
        {
            public string itemName;
            public int quantity = 1;
        }
        
        private void Awake()
        {
            inventory = GetComponent<InventorySystem>();
            playerStats = GetComponent<PlayerStats>();
        }
        
        private void Start()
        {
            if (craftingUI != null)
            {
                craftingUI.SetActive(false);
            }
            
            InitializeUnlockedRecipes();
        }
        
        private void InitializeUnlockedRecipes()
        {
            foreach (CraftingRecipe recipe in availableRecipes)
            {
                if (recipe.requiredSkillLevel == 1)
                {
                    unlockedRecipes.Add(recipe.recipeName);
                }
            }
        }
        
        private void Update()
        {
            if (isCrafting)
            {
                UpdateCraftingProgress();
            }
        }
        
        public void OpenCraftingMenu(CraftingStation station)
        {
            if (requireWorkbench && station == CraftingStation.None)
            {
                return;
            }
            
            currentStation = station;
            isCrafting = true;
            
            if (craftingUI != null)
            {
                craftingUI.SetActive(true);
            }
            
            if (OnCraftingMenuOpened != null)
            {
                OnCraftingMenuOpened(station);
            }
        }
        
        public void CloseCraftingMenu()
        {
            isCrafting = false;
            currentStation = CraftingStation.None;
            
            if (craftingUI != null)
            {
                craftingUI.SetActive(false);
            }
            
            if (OnCraftingMenuClosed != null)
            {
                OnCraftingMenuClosed();
            }
        }
        
        public List<CraftingRecipe> GetAvailableRecipes()
        {
            List<CraftingRecipe> available = new List<CraftingRecipe>();
            
            foreach (CraftingRecipe recipe in availableRecipes)
            {
                if (recipe.requiredStation == CraftingStation.None || recipe.requiredStation == currentStation)
                {
                    if (unlockedRecipes.Contains(recipe.recipeName))
                    {
                        available.Add(recipe);
                    }
                }
            }
            
            return available;
        }
        
        public bool CanCraft(CraftingRecipe recipe)
        {
            if (recipe.requiredStation != CraftingStation.None && recipe.requiredStation != currentStation)
            {
                return false;
            }
            
            if (useSkillRequirements)
            {
                float skillLevel = GetSkillLevel(recipe.skillType);
                if (skillLevel < recipe.requiredSkillLevel)
                {
                    return false;
                }
            }
            
            foreach (Ingredient ingredient in recipe.ingredients)
            {
                if (!inventory.HasItem(ingredient.itemName, ingredient.quantity))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public void StartCrafting(CraftingRecipe recipe)
        {
            if (!CanCraft(recipe))
            {
                return;
            }
            
            currentRecipe = recipe;
            
            foreach (Ingredient ingredient in recipe.ingredients)
            {
                inventory.RemoveItem(ingredient.itemName, ingredient.quantity);
            }
            
            craftingProgress = 0f;
            
            if (craftingEffect != null)
            {
                craftingEffect.Play();
            }
            
            if (OnCraftingStarted != null)
            {
                OnCraftingStarted(recipe);
            }
        }
        
        private void UpdateCraftingProgress()
        {
            float craftingTime = currentRecipe.craftingTime * baseCraftingTime / 5f;
            craftingProgress += Time.deltaTime / craftingTime;
            
            if (OnCraftingProgressUpdated != null)
            {
                OnCraftingProgressUpdated(craftingProgress);
            }
            
            if (craftingProgress >= 1f)
            {
                CompleteCrafting();
            }
        }
        
        private void CompleteCrafting()
        {
            if (currentRecipe == null)
            {
                return;
            }
            
            inventory.AddItem(currentRecipe.resultItemName, currentRecipe.resultQuantity);
            
            if (useSkillRequirements)
            {
                GrantCraftingExperience(currentRecipe.skillType, currentRecipe.requiredSkillLevel * 10);
            }
            
            if (craftingEffect != null)
            {
                craftingEffect.Stop();
            }
            
            if (OnCraftingCompleted != null)
            {
                OnCraftingCompleted(currentRecipe);
            }
            
            currentRecipe = null;
            craftingProgress = 0f;
        }
        
        public void CancelCrafting()
        {
            if (currentRecipe == null || craftingProgress <= 0f)
            {
                return;
            }
            
            float refundPercentage = 1f - craftingProgress;
            
            foreach (Ingredient ingredient in currentRecipe.ingredients)
            {
                int refundAmount = Mathf.RoundToInt(ingredient.quantity * refundPercentage);
                if (refundAmount > 0)
                {
                    inventory.AddItem(ingredient.itemName, refundAmount);
                }
            }
            
            if (craftingEffect != null)
            {
                craftingEffect.Stop();
            }
            
            currentRecipe = null;
            craftingProgress = 0f;
            
            if (OnCraftingCancelled != null)
            {
                OnCraftingCancelled();
            }
        }
        
        private float GetSkillLevel(string skillType)
        {
            if (playerStats != null)
            {
                return playerStats.GetSkillLevel(skillType);
            }
            
            return 1f;
        }
        
        private void GrantCraftingExperience(string skillType, float amount)
        {
            if (playerStats != null)
            {
                playerStats.AddSkillExperience(skillType, amount);
                CheckForNewRecipes();
            }
        }
        
        private void CheckForNewRecipes()
        {
            foreach (CraftingRecipe recipe in availableRecipes)
            {
                if (!unlockedRecipes.Contains(recipe.recipeName))
                {
                    float skillLevel = GetSkillLevel(recipe.skillType);
                    if (skillLevel >= recipe.requiredSkillLevel)
                    {
                        unlockedRecipes.Add(recipe.recipeName);
                        
                        if (OnRecipeUnlocked != null)
                        {
                            OnRecipeUnlocked(recipe);
                        }
                    }
                }
            }
        }
        
        public bool IsCrafting()
        {
            return isCrafting && currentRecipe != null;
        }
        
        public float GetCraftingProgress()
        {
            return craftingProgress;
        }
        
        public CraftingRecipe GetCurrentRecipe()
        {
            return currentRecipe;
        }
        
        public CraftingStation GetCurrentStation()
        {
            return currentStation;
        }
        
        public void AddRecipe(CraftingRecipe recipe)
        {
            CraftingRecipe[] newRecipes = new CraftingRecipe[availableRecipes.Length + 1];
            availableRecipes.CopyTo(newRecipes, 0);
            newRecipes[newRecipes.Length - 1] = recipe;
            availableRecipes = newRecipes;
        }
        
        public void UnlockRecipe(string recipeName)
        {
            if (!unlockedRecipes.Contains(recipeName))
            {
                unlockedRecipes.Add(recipeName);
            }
        }
        
        public bool IsRecipeUnlocked(string recipeName)
        {
            return unlockedRecipes.Contains(recipeName);
        }
        
        public event System.Action<CraftingStation> OnCraftingMenuOpened;
        public event System.Action OnCraftingMenuClosed;
        public event System.Action<CraftingRecipe> OnCraftingStarted;
        public event System.Action<float> OnCraftingProgressUpdated;
        public event System.Action<CraftingRecipe> OnCraftingCompleted;
        public event System.Action OnCraftingCancelled;
        public event System.Action<CraftingRecipe> OnRecipeUnlocked;
    }
}
