/* ShopUI.cs
 *
 * UI panel for buying and selling at shops.
 * Shows shop inventory, player inventory, prices, and transaction buttons.
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject panel;
    public Transform shopItemsContent;
    public Transform playerItemsContent;
    public GameObject itemButtonPrefab;

    [Header("Info Display")]
    public TextMeshProUGUI shopNameText;
    public TextMeshProUGUI playerBoxingsText;
    public TextMeshProUGUI selectedItemNameText;
    public TextMeshProUGUI selectedItemDescText;
    public TextMeshProUGUI selectedItemPriceText;
    public Image selectedItemIcon;

    [Header("Buttons")]
    public Button buyButton;
    public Button sellButton;
    public Button closeButton;

    [Header("Feedback")]
    public TextMeshProUGUI feedbackText;

    private ShopSystem currentShop;
    private Inventory playerInventory;
    private int playerBoxings;
    private ShopItem selectedShopItem;
    private string selectedPlayerItem;
    private bool isBuying = true;
    private float feedbackTimer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (panel != null) panel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (buyButton != null) buyButton.onClick.AddListener(OnBuyPressed);
        if (sellButton != null) sellButton.onClick.AddListener(OnSellPressed);
    }

    void Update()
    {
        if (panel != null && panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();

        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.unscaledDeltaTime;
            if (feedbackTimer <= 0f && feedbackText != null)
                feedbackText.text = "";
        }
    }

    public void Open(ShopSystem shop)
    {
        currentShop = shop;
        playerInventory = FindObjectOfType<Inventory>();
        playerBoxings = PlayerPrefs.GetInt("PlayerBoxings", 100); // temp storage

        if (panel != null) panel.SetActive(true);
        if (shopNameText != null) shopNameText.text = shop.shopName;

        RefreshShopInventory();
        RefreshPlayerInventory();
        UpdateBoxingsDisplay();
        ClearSelection();

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetState(CursorManager.CursorState.Menu);
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        PlayerPrefs.SetInt("PlayerBoxings", playerBoxings);
        currentShop = null;

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetState(CursorManager.CursorState.Gameplay);
    }

    void RefreshShopInventory()
    {
        if (shopItemsContent == null || currentShop == null) return;

        foreach (Transform child in shopItemsContent)
            Destroy(child.gameObject);

        foreach (var item in currentShop.inventory)
        {
            if (item.stock <= 0) continue;

            var btn = CreateItemButton(shopItemsContent, item.itemName,
                $"{currentShop.GetBuyPrice(item)} box", item.stock);

            var shopItem = item; // capture
            btn.onClick.AddListener(() => SelectShopItem(shopItem));
        }
    }

    void RefreshPlayerInventory()
    {
        if (playerItemsContent == null || playerInventory == null) return;

        foreach (Transform child in playerItemsContent)
            Destroy(child.gameObject);

        // This would iterate player inventory — simplified for now
    }

    Button CreateItemButton(Transform parent, string name, string price, int stock)
    {
        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        var layout = btnObj.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.padding = new RectOffset(8, 8, 4, 4);

        var bgImg = btnObj.AddComponent<Image>();
        bgImg.color = new Color(0.12f, 0.12f, 0.15f);

        var btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.18f, 0.18f, 0.25f);
        colors.pressedColor = new Color(0.1f, 0.15f, 0.25f);
        btn.colors = colors;

        var le = btnObj.AddComponent<LayoutElement>();
        le.preferredHeight = 35f;

        // Name
        var nameObj = new GameObject("Name");
        nameObj.transform.SetParent(btnObj.transform, false);
        var nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
        nameTMP.text = name;
        nameTMP.fontSize = 14;
        nameTMP.color = new Color(0.85f, 0.8f, 0.7f);
        var nameLE = nameObj.AddComponent<LayoutElement>();
        nameLE.preferredWidth = 180f;

        // Price
        var priceObj = new GameObject("Price");
        priceObj.transform.SetParent(btnObj.transform, false);
        var priceTMP = priceObj.AddComponent<TextMeshProUGUI>();
        priceTMP.text = price;
        priceTMP.fontSize = 13;
        priceTMP.color = new Color(0.9f, 0.75f, 0.3f);
        priceTMP.alignment = TextAlignmentOptions.MidlineRight;
        var priceLE = priceObj.AddComponent<LayoutElement>();
        priceLE.preferredWidth = 80f;

        // Stock
        var stockObj = new GameObject("Stock");
        stockObj.transform.SetParent(btnObj.transform, false);
        var stockTMP = stockObj.AddComponent<TextMeshProUGUI>();
        stockTMP.text = $"x{stock}";
        stockTMP.fontSize = 12;
        stockTMP.color = new Color(0.6f, 0.6f, 0.6f);
        stockTMP.alignment = TextAlignmentOptions.MidlineRight;
        var stockLE = stockObj.AddComponent<LayoutElement>();
        stockLE.preferredWidth = 40f;

        return btn;
    }

    void SelectShopItem(ShopItem item)
    {
        selectedShopItem = item;
        selectedPlayerItem = null;
        isBuying = true;

        if (selectedItemNameText != null) selectedItemNameText.text = item.itemName;
        if (selectedItemDescText != null) selectedItemDescText.text = item.description ?? "";
        if (selectedItemPriceText != null)
            selectedItemPriceText.text = $"Price: {currentShop.GetBuyPrice(item)} crowns";
        if (selectedItemIcon != null && item.icon != null)
            selectedItemIcon.sprite = item.icon;

        if (buyButton != null) buyButton.interactable = playerBoxings >= currentShop.GetBuyPrice(item);
    }

    void ClearSelection()
    {
        selectedShopItem = null;
        selectedPlayerItem = null;
        if (selectedItemNameText != null) selectedItemNameText.text = "Select an item";
        if (selectedItemDescText != null) selectedItemDescText.text = "";
        if (selectedItemPriceText != null) selectedItemPriceText.text = "";
        if (buyButton != null) buyButton.interactable = false;
        if (sellButton != null) sellButton.interactable = false;
    }

    void OnBuyPressed()
    {
        if (selectedShopItem == null || currentShop == null || playerInventory == null) return;

        var result = currentShop.TryBuy(selectedShopItem, playerInventory, ref playerBoxings);
        ShowFeedback(result.message, result.success);

        if (result.success)
        {
            UpdateBoxingsDisplay();
            RefreshShopInventory();
            SelectShopItem(selectedShopItem); // refresh price display
        }
    }

    void OnSellPressed()
    {
        if (string.IsNullOrEmpty(selectedPlayerItem) || currentShop == null || playerInventory == null) return;

        var result = currentShop.TrySell(selectedPlayerItem, 1, playerInventory, ref playerBoxings);
        ShowFeedback(result.message, result.success);

        if (result.success)
        {
            UpdateBoxingsDisplay();
            RefreshPlayerInventory();
        }
    }

    void UpdateBoxingsDisplay()
    {
        if (playerBoxingsText != null)
            playerBoxingsText.text = $"Crowns: {playerBoxings}";
    }

    void ShowFeedback(string message, bool success)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = success ? new Color(0.5f, 0.9f, 0.5f) : new Color(0.9f, 0.4f, 0.4f);
            feedbackTimer = 3f;
        }
    }
}
