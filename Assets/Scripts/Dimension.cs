using System;
using UnityEngine;

[System.Serializable]
public class Dimension
{
    public int tier;
    public BigDouble amount;
    public BigDouble basePrice;
    public BigDouble currentPrice;
    public int bought;
    public bool unlocked;
    public BigDouble multiplier;

    private const double PRICE_INCREASE_PER_PURCHASE = 1.20;
    private const double PRICE_INCREASE_PER_10 = 5.0;

    public Dimension(int tier, double basePrice)
    {
        this.tier = tier;
        this.amount = BigDouble.Zero;
        this.basePrice = new BigDouble(basePrice);
        this.currentPrice = this.basePrice;
        this.bought = 0;
        this.unlocked = (tier <= 2);
        this.multiplier = BigDouble.One;
    }

    public Dimension(int tier, BigDouble basePrice)
    {
        this.tier = tier;
        this.amount = BigDouble.Zero;
        this.basePrice = basePrice;
        this.currentPrice = this.basePrice;
        this.bought = 0;
        this.unlocked = (tier <= 2);
        this.multiplier = BigDouble.One;
    }

    public BigDouble GetProduction()
    {
        // 10개 구매마다 생산량 보너스 (기본 2배 + 프레스티지 업그레이드)
        double bulkMultiplier = 2.0;
        if (PrestigeManager.Instance != null)
        {
            bulkMultiplier += PrestigeManager.Instance.GetBulkBonusIncrease();
        }

        int tier10Count = bought / 10;
        BigDouble boughtBonus = BigDouble.Pow(new BigDouble(bulkMultiplier), tier10Count);

        // 프레스티지 차원 배수 적용
        double prestigeMultiplier = 1.0;
        if (PrestigeManager.Instance != null)
        {
            prestigeMultiplier = PrestigeManager.Instance.GetDimensionMultiplier(tier);
        }

        return amount * multiplier * boughtBonus * new BigDouble(prestigeMultiplier);
    }

    public void Buy(int count = 1)
    {
        amount = amount + new BigDouble(count);
        UpdatePrice(count);
        bought += count;
    }

    public void BuyMax(BigDouble currency, out int amountBought, out BigDouble totalCost)
    {
        amountBought = 0;
        totalCost = BigDouble.Zero;

        BigDouble remaining = currency;

        while (remaining >= currentPrice && amountBought < 1000)
        {
            totalCost = totalCost + currentPrice;
            remaining = remaining - currentPrice;

            UpdatePrice(1);
            bought++;
            amountBought++;
        }

        if (amountBought > 0)
        {
            amount = amount + new BigDouble(amountBought);
        }
    }

    private void UpdatePrice(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int justBought = bought + i + 1;

            currentPrice = currentPrice * new BigDouble(PRICE_INCREASE_PER_PURCHASE);

            // 10, 20, 30... 번째 구매 후 다음 가격에 증가하는 배수 적용
            // 10개: 5배, 20개: 5.75배, 30개: 6.6125배 (15%씩 증가)
            if (justBought % 10 == 0 && justBought > 0)
            {
                int tier10Index = (justBought / 10) - 1; // 0-based index
                double scalingMultiplier = PRICE_INCREASE_PER_10 * System.Math.Pow(1.15, tier10Index);
                currentPrice = currentPrice * new BigDouble(scalingMultiplier);
            }
        }
    }

    public void ApplyPrestigeMultiplier(BigDouble multiplier)
    {
        this.multiplier = this.multiplier * multiplier;
    }

    public void Reset()
    {
        amount = BigDouble.Zero;
        bought = 0;
        currentPrice = basePrice;
        multiplier = BigDouble.One; // DimBoost 배수 초기화

        if (tier > 2)
        {
            unlocked = false;
        }
    }

    public void CheckUnlock(Dimension previousDimension)
    {
        if (!unlocked && previousDimension != null)
        {
            if (previousDimension.bought >= 40)
            {
                unlocked = true;
                Debug.Log($"Dimension {tier} unlocked!");
            }
        }
    }
}
