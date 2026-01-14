using UnityEngine;
using UnityEngine.UIElements;

public class ShopPanelController : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;

    // UI Elements
    private Label premiumCurrencyLabel;

    // Shop Item 1
    private VisualElement shopItem1;
    private Button shopItem1BuyBtn;
    private Label shopItem1PriceLabel;

    // Shop Item 2
    private VisualElement shopItem2;
    private Button shopItem2BuyBtn;
    private Label shopItem2PriceLabel;

    // Shop Item 3
    private VisualElement shopItem3;
    private Button shopItem3BuyBtn;
    private Label shopItem3PriceLabel;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("[ShopPanelController] UIDocument not found!");
            return;
        }

        root = uiDocument.rootVisualElement;
        CacheUIElements();
        RegisterCallbacks();
        UpdateShopDisplay();
    }

    void Update()
    {
        if (ShopManager.Instance == null) return;

        // Update premium currency display
        if (premiumCurrencyLabel != null)
        {
            premiumCurrencyLabel.text = ShopManager.Instance.premiumCurrency.ToString();
        }

        // Update shop items display (to handle save/load)
        UpdateShopDisplay();
    }

    void CacheUIElements()
    {
        var shopRoot = root.Q<VisualElement>("shop-root");
        if (shopRoot == null)
        {
            Debug.LogError("[ShopPanelController] shop-root not found!");
            return;
        }

        // Premium currency
        premiumCurrencyLabel = shopRoot.Q<Label>("PremiumCurrencyAmount");

        // Shop items
        shopItem1 = shopRoot.Q<VisualElement>("ShopItem1");
        shopItem1BuyBtn = shopRoot.Q<Button>("ShopItem1BuyBtn");
        shopItem1PriceLabel = shopRoot.Q<Label>("ShopItem1Price");

        shopItem2 = shopRoot.Q<VisualElement>("ShopItem2");
        shopItem2BuyBtn = shopRoot.Q<Button>("ShopItem2BuyBtn");
        shopItem2PriceLabel = shopRoot.Q<Label>("ShopItem2Price");

        shopItem3 = shopRoot.Q<VisualElement>("ShopItem3");
        shopItem3BuyBtn = shopRoot.Q<Button>("ShopItem3BuyBtn");
        shopItem3PriceLabel = shopRoot.Q<Label>("ShopItem3Price");

        if (premiumCurrencyLabel == null) Debug.LogError("[ShopPanelController] PremiumCurrencyAmount not found!");
        if (shopItem1BuyBtn == null) Debug.LogError("[ShopPanelController] ShopItem1BuyBtn not found!");
        if (shopItem1PriceLabel == null) Debug.LogError("[ShopPanelController] ShopItem1Price not found!");
        if (shopItem2BuyBtn == null) Debug.LogError("[ShopPanelController] ShopItem2BuyBtn not found!");
        if (shopItem2PriceLabel == null) Debug.LogError("[ShopPanelController] ShopItem2Price not found!");
        if (shopItem3BuyBtn == null) Debug.LogError("[ShopPanelController] ShopItem3BuyBtn not found!");
        if (shopItem3PriceLabel == null) Debug.LogError("[ShopPanelController] ShopItem3Price not found!");
    }

    void RegisterCallbacks()
    {
        if (shopItem1BuyBtn != null)
        {
            shopItem1BuyBtn.clicked += () => OnBuyButtonClicked(ShopManager.ShopItem.Boost_Dim1to4);
        }

        if (shopItem2BuyBtn != null)
        {
            shopItem2BuyBtn.clicked += () => OnBuyButtonClicked(ShopManager.ShopItem.Boost_Dim5to8);
        }

        if (shopItem3BuyBtn != null)
        {
            shopItem3BuyBtn.clicked += () => OnBuyButtonClicked(ShopManager.ShopItem.Boost_AllDimensions);
        }
    }

    void OnBuyButtonClicked(ShopManager.ShopItem item)
    {
        if (ShopManager.Instance == null)
        {
            Debug.LogError("[ShopPanelController] ShopManager.Instance is null!");
            return;
        }

        bool success = ShopManager.Instance.BuyItem(item);

        if (success)
        {
            int level = ShopManager.Instance.GetItemLevel(item);
            Debug.Log($"[ShopPanelController] Successfully purchased {item} level {level}!");
            UpdateShopDisplay();

            // Save the game after purchase
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }
        }
        else
        {
            Debug.Log($"[ShopPanelController] Failed to purchase {item} - not enough currency");
        }
    }

    void UpdateShopDisplay()
    {
        if (ShopManager.Instance == null) return;

        UpdateItemDisplay(ShopManager.ShopItem.Boost_Dim1to4, shopItem1, shopItem1BuyBtn, shopItem1PriceLabel);
        UpdateItemDisplay(ShopManager.ShopItem.Boost_Dim5to8, shopItem2, shopItem2BuyBtn, shopItem2PriceLabel);
        UpdateItemDisplay(ShopManager.ShopItem.Boost_AllDimensions, shopItem3, shopItem3BuyBtn, shopItem3PriceLabel);
    }

    void UpdateItemDisplay(ShopManager.ShopItem item, VisualElement card, Button button, Label priceLabel)
    {
        if (card == null || button == null) return;

        int currentLevel = ShopManager.Instance.GetItemLevel(item);
        int nextPrice = ShopManager.Instance.GetItemPrice(item);
        bool canAfford = ShopManager.Instance.premiumCurrency >= nextPrice;

        // Update button text
        if (currentLevel == 0)
        {
            button.text = "BUY";
        }
        else
        {
            button.text = $"BUY (Lv.{currentLevel} → {currentLevel + 1})";
        }

        // Update price label
        if (priceLabel != null)
        {
            priceLabel.text = $"{nextPrice}";
        }

        // Enable/disable button based on affordability
        button.SetEnabled(canAfford);

        // Update button style based on affordability
        if (canAfford)
        {
            button.RemoveFromClassList("shop-buy-button--purchased");
            button.AddToClassList("shop-buy-button");
        }
        else
        {
            button.RemoveFromClassList("shop-buy-button");
            button.AddToClassList("shop-buy-button--purchased");
        }

        // Update card style based on ownership
        if (currentLevel > 0)
        {
            card.AddToClassList("shop-item-card--owned");
        }
        else
        {
            card.RemoveFromClassList("shop-item-card--owned");
        }
    }
}
