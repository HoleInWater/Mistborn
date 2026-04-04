/* AlloyForge.cs
 *
 * Metal alloy crafting system for Ashwalker.
 *
 * Lore: Metallurgic metals must be in precise alloy ratios to work.
 * Wrong ratios cause sickness or death. The exact compositions are:
 *   Steel  = Iron + Carbon (precise ratio)
 *   Pewter = Tin + Lead
 *   Brass  = Copper + Zinc
 *   Bronze = Copper + Tin
 *   Electrum = Gold + Silver
 *   Bendalloy = Cadmium + Lead + Tin + Bismuth
 *   Duralumin = Aluminum + Copper
 *   Nicrosil = Chromium + Nickel + Silicon
 *
 * The player can craft alloys at forges found throughout Cinderhold.
 * Pure metals (Iron, Tin, Zinc, Copper, Gold, Cadmium, Aluminum, Chromium)
 * are found or purchased. Alloys must be crafted.
 *
 * Impure alloys (wrong ratios) cause damage when burned — implemented
 * via MetalVialSystem.purity.
 */

using UnityEngine;
using System.Collections.Generic;

public class AlloyForge : MonoBehaviour, IInteractable
{
    public static AlloyForge ActiveForge { get; private set; }

    [Header("Forge Settings")]
    public string forgeName = "Lowborn Forge";
    public float qualityBonus = 0f;  // 0 = standard, 0.1 = good, 0.2 = master forge
    public bool requiresFuel = true;
    public int fuelRemaining = 10;

    [Header("Recipes")]
    public List<AlloyRecipe> recipes = new List<AlloyRecipe>();

    [Header("References")]
    public Transform craftingPosition;
    public ParticleSystem forgeFireVFX;

    private bool isActive;

    void Start()
    {
        if (recipes.Count == 0)
            PopulateDefaultRecipes();
    }

    // ── IInteractable ────────────────────────────────────────────────────────

    public string GetInteractPrompt() => $"Press [F] to use {forgeName}";
    public bool CanInteract() => !isActive && (!requiresFuel || fuelRemaining > 0);

    public void OnInteract(GameObject interactor)
    {
        if (!CanInteract()) return;
        isActive = true;
        ActiveForge = this;

        // Show crafting UI
        ForgeCraftingUI.Instance?.Open(this);

        // Cursor for UI
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetState(CursorManager.CursorState.Menu);
    }

    public void CloseForge()
    {
        isActive = false;
        ActiveForge = null;
        ForgeCraftingUI.Instance?.Close();

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetState(CursorManager.CursorState.Gameplay);
    }

    // ── Crafting ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempt to craft an alloy. Returns the result with purity.
    /// </summary>
    public CraftingResult TryCraft(AlloyRecipe recipe, Inventory playerInventory)
    {
        if (!CanInteract())
            return new CraftingResult { success = false, message = "Forge is out of fuel." };

        // Check ingredients
        foreach (var ingredient in recipe.ingredients)
        {
            if (!playerInventory.HasItem(ingredient.itemName, ingredient.amount))
                return new CraftingResult
                {
                    success = false,
                    message = $"Missing {ingredient.amount}x {ingredient.itemName}"
                };
        }

        // Consume ingredients
        foreach (var ingredient in recipe.ingredients)
            playerInventory.RemoveItem(ingredient.itemName, ingredient.amount);

        // Calculate purity
        float purity = CalculatePurity(recipe);

        // Consume fuel
        if (requiresFuel) fuelRemaining--;

        // Add result to inventory
        playerInventory.AddItem(recipe.resultItemName, recipe.resultAmount);

        // VFX
        if (forgeFireVFX != null) forgeFireVFX.Play();
        SoundManager.Instance?.PlayImpactSound();

        return new CraftingResult
        {
            success = true,
            purity = purity,
            resultName = recipe.resultItemName,
            resultAmount = recipe.resultAmount,
            message = $"Crafted {recipe.resultAmount}x {recipe.resultItemName} (Purity: {purity:P0})"
        };
    }

    float CalculatePurity(AlloyRecipe recipe)
    {
        // Base purity from recipe difficulty
        float basePurity = recipe.basePurity;

        // Forge quality bonus
        basePurity += qualityBonus;

        // Skill bonus (if MetallurgicSkillTree exists)
        if (MetallurgicSkillTree.Instance != null)
        {
            float craftSkill = MetallurgicSkillTree.Instance.GetSkillValue("Craft_Alloys");
            basePurity += craftSkill * 0.05f;
        }

        // Random variation (±5%)
        basePurity += Random.Range(-0.05f, 0.05f);

        return Mathf.Clamp01(basePurity);
    }

    // ── Default Recipes ──────────────────────────────────────────────────────

    void PopulateDefaultRecipes()
    {
        recipes = new List<AlloyRecipe>
        {
            new AlloyRecipe
            {
                recipeName = "Steel Alloy",
                resultItemName = "Steel Flakes",
                resultAmount = 5,
                basePurity = 0.85f,
                ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { itemName = "Iron Ore", amount = 3 },
                    new RecipeIngredient { itemName = "Carbon", amount = 1 }
                },
                loreNote = "Steel must be precisely alloyed or it won't respond to Metallurgic Pushing."
            },

            new AlloyRecipe
            {
                recipeName = "Pewter Alloy",
                resultItemName = "Pewter Beads",
                resultAmount = 5,
                basePurity = 0.80f,
                ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { itemName = "Tin Ingot", amount = 2 },
                    new RecipeIngredient { itemName = "Lead Ingot", amount = 2 }
                },
                loreNote = "Pewter's ratio must be exact — too much lead and it becomes toxic even when burned."
            },

            new AlloyRecipe
            {
                recipeName = "Brass Alloy",
                resultItemName = "Brass Shavings",
                resultAmount = 5,
                basePurity = 0.85f,
                ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { itemName = "Copper Nugget", amount = 2 },
                    new RecipeIngredient { itemName = "Zinc Dust", amount = 2 }
                },
                loreNote = "Brass is the Queller's metal — precise ratios allow emotional dampening."
            },

            new AlloyRecipe
            {
                recipeName = "Bronze Alloy",
                resultItemName = "Bronze Filings",
                resultAmount = 5,
                basePurity = 0.85f,
                ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { itemName = "Copper Nugget", amount = 2 },
                    new RecipeIngredient { itemName = "Tin Ingot", amount = 1 }
                },
                loreNote = "Bronze lets a Seeker detect Metallurgic pulses — the alloy ratio determines sensitivity."
            },

            new AlloyRecipe
            {
                recipeName = "Electrum Alloy",
                resultItemName = "Electrum Flakes",
                resultAmount = 3,
                basePurity = 0.75f,
                ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { itemName = "Gold Nugget", amount = 2 },
                    new RecipeIngredient { itemName = "Silver Ingot", amount = 1 }
                },
                loreNote = "Electrum — the poor man's Oraculum. Hard to alloy correctly, but it counters future-sight."
            },

            new AlloyRecipe
            {
                recipeName = "Duralumin Alloy",
                resultItemName = "Duralumin Beads",
                resultAmount = 3,
                basePurity = 0.70f,
                ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { itemName = "Aluminum Ingot", amount = 2 },
                    new RecipeIngredient { itemName = "Copper Nugget", amount = 1 }
                },
                loreNote = "Duralumin is tricky — aluminum is extremely rare in the Ashen Dominion. The Ashen King controls all aluminum supply."
            },

            new AlloyRecipe
            {
                recipeName = "Bendalloy Alloy",
                resultItemName = "Bendalloy Shavings",
                resultAmount = 2,
                basePurity = 0.65f,
                ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { itemName = "Cadmium Ingot", amount = 1 },
                    new RecipeIngredient { itemName = "Lead Ingot", amount = 1 },
                    new RecipeIngredient { itemName = "Tin Ingot", amount = 1 },
                    new RecipeIngredient { itemName = "Bismuth Crystal", amount = 1 }
                },
                loreNote = "Bendalloy is the rarest alloy — four components, precise ratios. Extremely expensive."
            },

            new AlloyRecipe
            {
                recipeName = "Nicrosil Alloy",
                resultItemName = "Nicrosil Beads",
                resultAmount = 2,
                basePurity = 0.65f,
                ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { itemName = "Chromium Dust", amount = 1 },
                    new RecipeIngredient { itemName = "Nickel Ingot", amount = 1 },
                    new RecipeIngredient { itemName = "Silicon Shard", amount = 1 }
                },
                loreNote = "Nicrosil is a modern discovery — most Era 1 Metallurgists don't know it exists."
            },

            // ── Metal Vial Crafting ──────────────────────────────────────────

            new AlloyRecipe
            {
                recipeName = "Steel Vial",
                resultItemName = "Steel Vial",
                resultAmount = 1,
                basePurity = 0.90f,
                ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { itemName = "Steel Flakes", amount = 3 },
                    new RecipeIngredient { itemName = "Alcohol Solution", amount = 1 },
                    new RecipeIngredient { itemName = "Glass Vial", amount = 1 }
                },
                loreNote = "Metal vials suspend fine metal shavings in alcohol for safe ingestion."
            },

            new AlloyRecipe
            {
                recipeName = "Pewter Vial",
                resultItemName = "Pewter Vial",
                resultAmount = 1,
                basePurity = 0.90f,
                ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { itemName = "Pewter Beads", amount = 3 },
                    new RecipeIngredient { itemName = "Alcohol Solution", amount = 1 },
                    new RecipeIngredient { itemName = "Glass Vial", amount = 1 }
                },
                loreNote = "Pewter vials are the most commonly consumed — Thugs burn through them quickly."
            },

            new AlloyRecipe
            {
                recipeName = "Full Ashwalker Vial",
                resultItemName = "Ashwalker Vial",
                resultAmount = 1,
                basePurity = 0.85f,
                ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { itemName = "Steel Flakes", amount = 1 },
                    new RecipeIngredient { itemName = "Iron Filings", amount = 1 },
                    new RecipeIngredient { itemName = "Pewter Beads", amount = 1 },
                    new RecipeIngredient { itemName = "Tin Flakes", amount = 1 },
                    new RecipeIngredient { itemName = "Zinc Dust", amount = 1 },
                    new RecipeIngredient { itemName = "Brass Shavings", amount = 1 },
                    new RecipeIngredient { itemName = "Copper Nugget", amount = 1 },
                    new RecipeIngredient { itemName = "Bronze Filings", amount = 1 },
                    new RecipeIngredient { itemName = "Alcohol Solution", amount = 1 },
                    new RecipeIngredient { itemName = "Glass Vial", amount = 1 }
                },
                loreNote = "A full Ashwalker vial contains all eight basic metals. Expensive and rare — only a Ashwalker can use it."
            },
        };
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// DATA STRUCTURES
// ═══════════════════════════════════════════════════════════════════════════

[System.Serializable]
public class AlloyRecipe
{
    public string recipeName;
    public string resultItemName;
    public int resultAmount;
    public float basePurity;
    public List<RecipeIngredient> ingredients;
    [TextArea(1, 3)] public string loreNote;
}

[System.Serializable]
public class RecipeIngredient
{
    public string itemName;
    public int amount;
}

public struct CraftingResult
{
    public bool success;
    public float purity;
    public string resultName;
    public int resultAmount;
    public string message;
}
