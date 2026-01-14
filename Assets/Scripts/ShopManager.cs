using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    // Premium currency
    public int premiumCurrency = 1000; // Start with some currency for testing

    // Shop items enum
    public enum ShopItem
    {
        Boost_Dim1to4,
        Boost_Dim5to8,
        Boost_AllDimensions
    }

    // Item levels (구매 횟수)
    private Dictionary<ShopItem, int> itemLevels = new Dictionary<ShopItem, int>();

    // Base prices
    private Dictionary<ShopItem, int> basePrices = new Dictionary<ShopItem, int>
    {
        { ShopItem.Boost_Dim1to4, 100 },
        { ShopItem.Boost_Dim5to8, 200 },
        { ShopItem.Boost_AllDimensions, 500 }
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeItemLevels();
    }

    void InitializeItemLevels()
    {
        // Initialize all items at level 0
        itemLevels[ShopItem.Boost_Dim1to4] = 0;
        itemLevels[ShopItem.Boost_Dim5to8] = 0;
        itemLevels[ShopItem.Boost_AllDimensions] = 0;
    }

    public bool BuyItem(ShopItem item)
    {
        int price = GetItemPrice(item);

        // Check if enough currency
        if (premiumCurrency < price)
        {
            Debug.LogWarning($"[ShopManager] Not enough premium currency! Need {price}, have {premiumCurrency}");
            return false;
        }

        // Purchase item
        premiumCurrency -= price;

        // Increase level
        if (!itemLevels.ContainsKey(item))
        {
            itemLevels[item] = 0;
        }
        itemLevels[item]++;

        Debug.Log($"[ShopManager] Purchased {item} level {itemLevels[item]} for {price} premium currency. Remaining: {premiumCurrency}");
        return true;
    }

    public int GetItemLevel(ShopItem item)
    {
        return itemLevels.ContainsKey(item) ? itemLevels[item] : 0;
    }

    public int GetItemPrice(ShopItem item)
    {
        if (!basePrices.ContainsKey(item)) return 0;

        int basePrice = basePrices[item];
        int currentLevel = GetItemLevel(item);
        int nextLevel = currentLevel + 1;

        // 가격 공식: basePrice + 100 * (level - 1) * level / 2
        int price = basePrice + (100 * (nextLevel - 1) * nextLevel / 2);
        return price;
    }

    public string GetItemName(ShopItem item)
    {
        return item switch
        {
            ShopItem.Boost_Dim1to4 => "2x Production: Dimensions 1-4",
            ShopItem.Boost_Dim5to8 => "2x Production: Dimensions 5-8",
            ShopItem.Boost_AllDimensions => "2x Production: All Dimensions",
            _ => "Unknown Item"
        };
    }

    public string GetItemDescription(ShopItem item)
    {
        return item switch
        {
            ShopItem.Boost_Dim1to4 => "Permanent 2x multiplier for dimensions 1-4 (stacks with prestige & DimBoost)",
            ShopItem.Boost_Dim5to8 => "Permanent 2x multiplier for dimensions 5-8 (stacks with prestige & DimBoost)",
            ShopItem.Boost_AllDimensions => "Permanent 2x multiplier for ALL dimensions 1-8 (stacks with prestige & DimBoost)",
            _ => ""
        };
    }

    // Get the boost multiplier for a specific dimension tier
    public double GetBoostMultiplier(int dimensionTier)
    {
        double multiplier = 1.0;

        // Check Boost_Dim1to4
        if (dimensionTier >= 1 && dimensionTier <= 4)
        {
            int level = GetItemLevel(ShopItem.Boost_Dim1to4);
            if (level > 0)
            {
                multiplier *= System.Math.Pow(2.0, level);
            }
        }

        // Check Boost_Dim5to8
        if (dimensionTier >= 5 && dimensionTier <= 8)
        {
            int level = GetItemLevel(ShopItem.Boost_Dim5to8);
            if (level > 0)
            {
                multiplier *= System.Math.Pow(2.0, level);
            }
        }

        // Check Boost_AllDimensions
        int allLevel = GetItemLevel(ShopItem.Boost_AllDimensions);
        if (allLevel > 0)
        {
            multiplier *= System.Math.Pow(2.0, allLevel);
        }

        return multiplier;
    }

    // For save/load
    public Dictionary<ShopItem, int> GetItemLevels()
    {
        return new Dictionary<ShopItem, int>(itemLevels);
    }

    public void SetItemLevels(Dictionary<ShopItem, int> levels)
    {
        itemLevels = new Dictionary<ShopItem, int>(levels);
    }
}
