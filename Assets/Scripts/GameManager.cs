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
        dimensions.Add(new Dimension(4, new BigDouble(1, 40)));
        dimensions.Add(new Dimension(5, new BigDouble(1, 80)));
        dimensions.Add(new Dimension(6, new BigDouble(1, 130)));
        dimensions.Add(new Dimension(7, new BigDouble(1, 200)));
        dimensions.Add(new Dimension(8, new BigDouble(1, 260)));

        dimensions[0].unlocked = true;
        dimensions[1].unlocked = true;
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        CheckInfinity();

        // 반물질이 무한 미만일 때만 생산 (무한 이상이면 생산 중단)
        if (antimatter < BigDouble.Infinity)
        {
            ProduceDimensions(deltaTime);
        }

        CheckDimensionUnlocks();
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
        // Dimension Enhance를 통해서만 차원 해금
        // 기존 자동 해금 로직 제거됨
    }

    void CheckInfinity()
    {
        // 무한 도달 체크
        if (antimatter >= BigDouble.Infinity && !infinityReached)
        {
            infinityReached = true;
        }
        // 무한 미만으로 떨어지면 상태 리셋
        else if (antimatter < BigDouble.Infinity && infinityReached)
        {
            infinityReached = false;
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
                tempPrice = tempPrice * new BigDouble(1.20);

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
                tempPrice = tempPrice * new BigDouble(1.20);

                int justBought = dim.bought + i + 1;
                if (justBought % 10 == 0 && justBought > 0)
                {
                    tempPrice = tempPrice * new BigDouble(5.0);
                }
            }
        }

        antimatter = antimatter - totalCost;
        dim.Buy(count);
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
