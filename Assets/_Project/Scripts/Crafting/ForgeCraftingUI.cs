/* ForgeCraftingUI.cs
 *
 * UI panel for the alloy forge crafting system.
 * Shows available recipes, required ingredients, and crafting results.
 *
 * Opened by AlloyForge.OnInteract(), closed by pressing Escape or
 * clicking the close button.
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ForgeCraftingUI : MonoBehaviour
{
    public static ForgeCraftingUI Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject panel;
    public Transform recipeListContent;
    public GameObject recipeButtonPrefab;
    public TextMeshProUGUI recipeNameText;
    public TextMeshProUGUI recipeDescriptionText;
    public TextMeshProUGUI ingredientListText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI purityText;
    public Button craftButton;
    public Button closeButton;
    public TextMeshProUGUI forgeNameText;
    public TextMeshProUGUI fuelText;

    [Header("Result Feedback")]
    public TextMeshProUGUI feedbackText;
    public float feedbackDuration = 3f;

    private AlloyForge currentForge;
    private AlloyRecipe selectedRecipe;
    private Inventory playerInventory;
    private float feedbackTimer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (panel != null) panel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (craftButton != null) craftButton.onClick.AddListener(OnCraftPressed);
    }

    void Update()
    {
        if (panel != null && panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();

        // Fade feedback text
        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.unscaledDeltaTime;
            if (feedbackTimer <= 0f && feedbackText != null)
                feedbackText.text = "";
        }
    }

    public void Open(AlloyForge forge)
    {
        currentForge = forge;
        playerInventory = FindObjectOfType<Inventory>();

        if (panel != null) panel.SetActive(true);
        if (forgeNameText != null) forgeNameText.text = forge.forgeName;

        UpdateFuelDisplay();
        PopulateRecipeList();
        ClearSelection();
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        if (currentForge != null) currentForge.CloseForge();
        currentForge = null;
        selectedRecipe = null;
    }

    void PopulateRecipeList()
    {
        if (recipeListContent == null || currentForge == null) return;

        // Clear existing buttons
        foreach (Transform child in recipeListContent)
            Destroy(child.gameObject);

        // Create a button for each recipe
        foreach (var recipe in currentForge.recipes)
        {
            if (recipeButtonPrefab != null)
            {
                var btnObj = Instantiate(recipeButtonPrefab, recipeListContent);
                var btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = recipe.recipeName;

                var btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    var r = recipe; // capture for lambda
                    btn.onClick.AddListener(() => SelectRecipe(r));
                }

                // Grey out if missing ingredients
                bool canCraft = CanCraftRecipe(recipe);
                if (!canCraft)
                {
                    var colors = btn.colors;
                    colors.normalColor = new Color(0.3f, 0.3f, 0.3f);
                    btn.colors = colors;
                }
            }
        }
    }

    void SelectRecipe(AlloyRecipe recipe)
    {
        selectedRecipe = recipe;

        if (recipeNameText != null)
            recipeNameText.text = recipe.recipeName;

        if (recipeDescriptionText != null)
            recipeDescriptionText.text = recipe.loreNote;

        // Build ingredient list
        if (ingredientListText != null)
        {
            string ingredients = "";
            foreach (var ing in recipe.ingredients)
            {
                bool has = playerInventory != null && playerInventory.HasItem(ing.itemName, ing.amount);
                string color = has ? "#B8A890" : "#CC4444";
                int owned = playerInventory != null ? playerInventory.GetItemCount(ing.itemName) : 0;
                ingredients += $"<color={color}>{ing.itemName}: {owned}/{ing.amount}</color>\n";
            }
            ingredientListText.text = ingredients;
        }

        if (resultText != null)
            resultText.text = $"Result: {recipe.resultAmount}x {recipe.resultItemName}";

        if (purityText != null)
            purityText.text = $"Expected Purity: ~{recipe.basePurity:P0}";

        // Enable/disable craft button
        if (craftButton != null)
            craftButton.interactable = CanCraftRecipe(recipe);
    }

    void ClearSelection()
    {
        selectedRecipe = null;
        if (recipeNameText != null) recipeNameText.text = "Select a recipe";
        if (recipeDescriptionText != null) recipeDescriptionText.text = "";
        if (ingredientListText != null) ingredientListText.text = "";
        if (resultText != null) resultText.text = "";
        if (purityText != null) purityText.text = "";
        if (craftButton != null) craftButton.interactable = false;
    }

    void OnCraftPressed()
    {
        if (selectedRecipe == null || currentForge == null || playerInventory == null) return;

        CraftingResult result = currentForge.TryCraft(selectedRecipe, playerInventory);

        if (feedbackText != null)
        {
            feedbackText.text = result.message;
            feedbackText.color = result.success ? new Color(0.5f, 0.9f, 0.5f) : new Color(0.9f, 0.4f, 0.4f);
            feedbackTimer = feedbackDuration;
        }

        if (result.success)
        {
            // Refresh displays
            UpdateFuelDisplay();
            SelectRecipe(selectedRecipe); // refresh ingredient counts
            PopulateRecipeList(); // refresh grey-out states

            NotificationSystem.Instance?.ShowNotification(
                $"Crafted {result.resultAmount}x {result.resultName} ({result.purity:P0} purity)");
        }
    }

    bool CanCraftRecipe(AlloyRecipe recipe)
    {
        if (playerInventory == null) return false;
        foreach (var ing in recipe.ingredients)
        {
            if (!playerInventory.HasItem(ing.itemName, ing.amount))
                return false;
        }
        return true;
    }

    void UpdateFuelDisplay()
    {
        if (fuelText != null && currentForge != null)
        {
            if (currentForge.requiresFuel)
                fuelText.text = $"Fuel: {currentForge.fuelRemaining}";
            else
                fuelText.text = "Fuel: Unlimited";
        }
    }
}
