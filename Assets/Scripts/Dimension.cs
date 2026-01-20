using System;
using UnityEngine;

[System.Serializable]
public class Dimension
{
    public int tier;
    public BigDouble amount;
    public BigDouble baseCost;           // AD풍: 기본 가격
    public BigDouble costIncreasePer10;  // AD풍: 10개 세트당 가격 배율
    public int bought;
    public bool unlocked;
    public BigDouble multiplier;

    // AD풍 기본 가격 테이블 (차원별)
    // Antimatter Dimensions 스타일: 고차원일수록 기하급수적 증가
    private static readonly double[] BASE_COSTS = {
        10,      // 1차원
        1e3,     // 2차원
        1e10,    // 3차원
        1e20,    // 4차원
        1e35,    // 5차원
        1e60,    // 6차원
        1e80,    // 7차원
        1e100    // 8차원
    };

    // AD풍 10개 세트당 가격 증가 배율 (차원별)
    // 고차원일수록 세트 배율이 더 큼
    private static readonly double[] COST_INCREASE_PER_10 = {
        1e3,     // 1차원: 10개마다 ×1,000
        1e4,     // 2차원: 10개마다 ×10,000
        1e5,     // 3차원: 10개마다 ×100,000
        1e6,     // 4차원: 10개마다 ×1,000,000
        1e7,     // 5차원: 10개마다 ×10,000,000
        1e8,     // 6차원: 10개마다 ×100,000,000
        1e9,     // 7차원: 10개마다 ×1,000,000,000
        1e10     // 8차원: 10개마다 ×10,000,000,000
    };

    public Dimension(int tier, double basePrice)
    {
        this.tier = tier;
        this.amount = BigDouble.Zero;

        // AD풍: 테이블에서 가격 설정 (tier는 1-based)
        int index = Math.Clamp(tier - 1, 0, BASE_COSTS.Length - 1);
        this.baseCost = new BigDouble(BASE_COSTS[index]);
        this.costIncreasePer10 = new BigDouble(COST_INCREASE_PER_10[index]);

        this.bought = 0;
        this.unlocked = (tier <= 2);
        this.multiplier = BigDouble.One;
    }

    public Dimension(int tier, BigDouble basePrice)
    {
        this.tier = tier;
        this.amount = BigDouble.Zero;

        // AD풍: 테이블에서 가격 설정 (tier는 1-based)
        int index = Math.Clamp(tier - 1, 0, BASE_COSTS.Length - 1);
        this.baseCost = new BigDouble(BASE_COSTS[index]);
        this.costIncreasePer10 = new BigDouble(COST_INCREASE_PER_10[index]);

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

        // Shop 배수 적용
        double shopMultiplier = 1.0;
        if (ShopManager.Instance != null)
        {
            shopMultiplier = ShopManager.Instance.GetBoostMultiplier(tier);
        }

        return amount * multiplier * boughtBonus * new BigDouble(prestigeMultiplier) * new BigDouble(shopMultiplier);
    }

    // ========== AD풍 가격 계산 메서드 ==========

    /// <summary>
    /// 현재 세트 번호 (0, 1, 2, ... = 0~9개, 10~19개, 20~29개...)
    /// </summary>
    public int GetCurrentSet()
    {
        return bought / 10;
    }

    /// <summary>
    /// 현재 세트의 가격 (세트 전체 가격)
    /// setCost = baseCost × costIncreasePer10^sets
    /// </summary>
    public BigDouble GetSetCost()
    {
        int sets = GetCurrentSet();
        return baseCost * BigDouble.Pow(costIncreasePer10, sets);
    }

    /// <summary>
    /// 현재 세트에서 개별 구매 가격 (세트 가격 / 10)
    /// </summary>
    public BigDouble GetSingleCost()
    {
        return GetSetCost() / new BigDouble(10);
    }

    /// <summary>
    /// 현재 구매 가격 (UI 표시용, GetSingleCost와 동일)
    /// </summary>
    public BigDouble GetCurrentPrice()
    {
        return GetSingleCost();
    }

    /// <summary>
    /// 다음 10개 세트까지 남은 개수
    /// </summary>
    public int GetRemainingUntil10()
    {
        return 10 - (bought % 10);
    }

    /// <summary>
    /// "Until 10" 구매 시 총 비용 계산
    /// 현재 세트의 남은 개수만큼의 비용
    /// </summary>
    public BigDouble GetCostUntil10()
    {
        int remaining = GetRemainingUntil10();
        BigDouble singleCost = GetSingleCost();
        return singleCost * new BigDouble(remaining);
    }

    /// <summary>
    /// 특정 개수 구매 시 총 비용 계산 (세트 경계 고려)
    /// </summary>
    public BigDouble GetCostForCount(int count)
    {
        if (count <= 0) return BigDouble.Zero;

        BigDouble totalCost = BigDouble.Zero;
        int tempBought = bought;

        for (int i = 0; i < count; i++)
        {
            int sets = tempBought / 10;
            BigDouble setCost = baseCost * BigDouble.Pow(costIncreasePer10, sets);
            BigDouble singleCost = setCost / new BigDouble(10);
            totalCost = totalCost + singleCost;
            tempBought++;
        }

        return totalCost;
    }

    /// <summary>
    /// 주어진 화폐로 구매 가능한 최대 개수 계산
    /// </summary>
    public int GetMaxAffordable(BigDouble currency)
    {
        if (currency <= BigDouble.Zero) return 0;

        int maxBuyable = 0;
        BigDouble totalCost = BigDouble.Zero;
        int tempBought = bought;

        // 최대 1000개까지 시뮬레이션 (성능 제한)
        while (maxBuyable < 1000)
        {
            int sets = tempBought / 10;
            BigDouble setCost = baseCost * BigDouble.Pow(costIncreasePer10, sets);
            BigDouble singleCost = setCost / new BigDouble(10);

            if (totalCost + singleCost > currency)
                break;

            totalCost = totalCost + singleCost;
            tempBought++;
            maxBuyable++;
        }

        return maxBuyable;
    }

    // ========== 구매 메서드 ==========

    public void Buy(int count = 1)
    {
        amount = amount + new BigDouble(count);
        bought += count;
        // AD풍: 가격은 bought 기반으로 동적 계산되므로 별도 업데이트 불필요
    }

    public void BuyMax(BigDouble currency, out int amountBought, out BigDouble totalCost)
    {
        amountBought = 0;
        totalCost = BigDouble.Zero;

        BigDouble remaining = currency;

        // 최대 1000개까지 구매 (성능 제한)
        while (amountBought < 1000)
        {
            BigDouble singleCost = GetSingleCost();

            if (remaining < singleCost)
                break;

            totalCost = totalCost + singleCost;
            remaining = remaining - singleCost;
            bought++;
            amountBought++;
        }

        if (amountBought > 0)
        {
            amount = amount + new BigDouble(amountBought);
        }
    }

    /// <summary>
    /// "Until 10" 구매: 다음 10개 세트까지 구매
    /// </summary>
    public void BuyUntil10(BigDouble currency, out int amountBought, out BigDouble totalCost)
    {
        int remaining = GetRemainingUntil10();
        totalCost = GetCostUntil10();

        if (currency >= totalCost)
        {
            amountBought = remaining;
            amount = amount + new BigDouble(remaining);
            bought += remaining;
        }
        else
        {
            // 부족하면 살 수 있는 만큼만
            BuyMax(currency, out amountBought, out totalCost);
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
        // AD풍: 가격은 bought 기반으로 동적 계산되므로 별도 리셋 불필요
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
