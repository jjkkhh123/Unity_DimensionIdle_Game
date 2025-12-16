using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BigDouble antimatter;
    public List<Dimension> dimensions;
    public bool infinityReached = false;

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

        InitializeGame();
    }

    void InitializeGame()
    {
        antimatter = new BigDouble(10);
        dimensions = new List<Dimension>();

        dimensions.Add(new Dimension(1, 10));
        dimensions.Add(new Dimension(2, 1000));
        dimensions.Add(new Dimension(3, 1e10));
        dimensions.Add(new Dimension(4, 1e30));
        dimensions.Add(new Dimension(5, new BigDouble(1, 100)));
        dimensions.Add(new Dimension(6, new BigDouble(1, 200)));
        dimensions.Add(new Dimension(7, new BigDouble(1, 300)));
        dimensions.Add(new Dimension(8, new BigDouble(1, 400)));

        dimensions[0].unlocked = true;
        dimensions[1].unlocked = true;
    }

    void Update()
    {
        if (infinityReached)
            return;

        float deltaTime = Time.deltaTime;

        ProduceDimensions(deltaTime);

        CheckDimensionUnlocks();

        CheckInfinity();
    }

    void ProduceDimensions(float deltaTime)
    {
        // Tickspeed 적용
        double tickspeedMultiplier = 1.0;
        if (TickSpeedManager.Instance != null)
        {
            tickspeedMultiplier = TickSpeedManager.Instance.GetTickspeedMultiplier();
        }

        double effectiveDeltaTime = deltaTime * tickspeedMultiplier;

        for (int i = dimensions.Count - 1; i >= 0; i--)
        {
            Dimension dim = dimensions[i];

            if (!dim.unlocked || dim.amount.mantissa == 0)
                continue;

            BigDouble production = dim.GetProduction() * new BigDouble(effectiveDeltaTime);

            if (i == 0)
            {
                antimatter = antimatter + production;
            }
            else
            {
                dimensions[i - 1].amount = dimensions[i - 1].amount + production;
            }
        }
    }

    void CheckDimensionUnlocks()
    {
        for (int i = 2; i < dimensions.Count; i++)
        {
            if (!dimensions[i].unlocked)
            {
                dimensions[i].CheckUnlock(dimensions[i - 1]);
            }
        }
    }

    void CheckInfinity()
    {
        if (antimatter >= BigDouble.Infinity)
        {
            infinityReached = true;
            Debug.Log("Infinity Reached!");
        }
    }

    public bool CanBuyDimension(int tier, int count = 1)
    {
        if (tier < 1 || tier > dimensions.Count)
            return false;

        Dimension dim = dimensions[tier - 1];

        if (!dim.unlocked)
            return false;

        BigDouble totalCost = dim.currentPrice;

        if (count > 1)
        {
            BigDouble tempPrice = dim.currentPrice;
            totalCost = BigDouble.Zero;

            for (int i = 0; i < count; i++)
            {
                totalCost = totalCost + tempPrice;
                tempPrice = tempPrice * new BigDouble(1.15);

                int justBought = dim.bought + i + 1;
                if (justBought % 10 == 0 && justBought > 0)
                {
                    tempPrice = tempPrice * new BigDouble(5.0);
                }
            }
        }

        return antimatter >= totalCost;
    }

    public void BuyDimension(int tier, int count = 1)
    {
        if (!CanBuyDimension(tier, count))
        {
            Debug.Log($"Cannot buy Dimension {tier}");
            return;
        }

        Dimension dim = dimensions[tier - 1];

        BigDouble totalCost = dim.currentPrice;

        if (count > 1)
        {
            BigDouble tempPrice = dim.currentPrice;
            totalCost = BigDouble.Zero;

            for (int i = 0; i < count; i++)
            {
                totalCost = totalCost + tempPrice;
                tempPrice = tempPrice * new BigDouble(1.15);

                int justBought = dim.bought + i + 1;
                if (justBought % 10 == 0 && justBought > 0)
                {
                    tempPrice = tempPrice * new BigDouble(5.0);
                }
            }
        }

        antimatter = antimatter - totalCost;
        dim.Buy(count);

        Debug.Log($"Bought {count}x Dimension {tier}. Cost: {totalCost}");
    }

    public void BuyMaxDimension(int tier)
    {
        if (tier < 1 || tier > dimensions.Count)
            return;

        Dimension dim = dimensions[tier - 1];

        if (!dim.unlocked)
            return;

        int amountBought;
        BigDouble totalCost;

        dim.BuyMax(antimatter, out amountBought, out totalCost);

        if (amountBought > 0)
        {
            antimatter = antimatter - totalCost;
            Debug.Log($"Bought {amountBought}x Dimension {tier}. Total Cost: {totalCost}");
        }
    }

    public string GetAntimatterString()
    {
        return antimatter.ToString();
    }

    public string GetDimensionAmountString(int tier)
    {
        if (tier < 1 || tier > dimensions.Count)
            return "0";

        return dimensions[tier - 1].amount.ToString();
    }

    public string GetDimensionPriceString(int tier)
    {
        if (tier < 1 || tier > dimensions.Count)
            return "0";

        return dimensions[tier - 1].currentPrice.ToString();
    }

    public string GetDimensionProductionString(int tier)
    {
        if (tier < 1 || tier > dimensions.Count)
            return "0";

        return dimensions[tier - 1].GetProduction().ToString();
    }

    public bool IsDimensionUnlocked(int tier)
    {
        if (tier < 1 || tier > dimensions.Count)
            return false;

        return dimensions[tier - 1].unlocked;
    }
}
