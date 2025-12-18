using System;
using System.Collections.Generic;
using UnityEngine;

public class PrestigeManager : MonoBehaviour
{
    public static PrestigeManager Instance { get; private set; }

    public int prestigePoints = 0;
    public int totalPrestiges = 0;

    public Dictionary<string, PrestigeUpgrade> upgrades = new Dictionary<string, PrestigeUpgrade>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUpgrades();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeUpgrades()
    {
        // 틱스피드 계수 증가 (1포인트당 0.01 증가)
        upgrades["tickspeed_boost"] = new PrestigeUpgrade(
            "tickspeed_boost",
            "Tickspeed Power",
            "Increases tickspeed multiplier by 0.01 per upgrade",
            1
        );

        // 차원별 생산량 2배 (차원마다 다른 가격)
        int[] dimCosts = { 1, 2, 3, 5, 7, 8, 9, 10 };
        double[] dimPriceScaling = { 1.10, 1.15, 1.15, 1.20, 1.25, 1.25, 1.25, 1.25 };
        for (int i = 1; i <= 8; i++)
        {
            upgrades[$"dim{i}_mult"] = new PrestigeUpgrade(
                $"dim{i}_mult",
                $"Dimension {i} Boost",
                $"Multiplies Dimension {i} production by 2x per upgrade",
                dimCosts[i - 1],
                999,
                dimPriceScaling[i - 1]
            );
        }

        // 10개 구매 보너스 증가
        upgrades["bulk_bonus"] = new PrestigeUpgrade(
            "bulk_bonus",
            "Bulk Purchase Bonus",
            "Increases the x10 purchase bonus by 0.05 per upgrade (base: 2.00x)",
            2
        );
    }

    public bool CanPrestige()
    {
        if (GameManager.Instance == null)
            return false;

        return CalculatePrestigePointsGained() > 0;
    }

    public int CalculatePrestigePointsGained()
    {
        if (GameManager.Instance == null)
            return 0;

        BigDouble antimatter = GameManager.Instance.antimatter;

        // 1e10 = 1 포인트, 1e20 = 2 포인트...
        if (antimatter < new BigDouble(1e10))
            return 0;

        // exponent / 10을 소수점 버림
        int points = (antimatter.exponent + (int)Math.Log10(antimatter.mantissa)) / 10;
        return points;
    }

    public void DoPrestige()
    {
        if (!CanPrestige())
        {
            Debug.Log("Cannot prestige yet!");
            return;
        }

        int pointsGained = CalculatePrestigePointsGained();
        prestigePoints += pointsGained;
        totalPrestiges++;

        Debug.Log($"Prestige! Gained {pointsGained} prestige points. Total: {prestigePoints}");

        ResetDimensions();
    }

    private void ResetDimensions()
    {
        GameManager.Instance.antimatter = new BigDouble(10);

        foreach (Dimension dim in GameManager.Instance.dimensions)
        {
            dim.Reset();
        }

        GameManager.Instance.dimensions[0].unlocked = true;
        GameManager.Instance.dimensions[1].unlocked = true;

        GameManager.Instance.infinityReached = false;

        // Tickspeed 초기화
        if (TickSpeedManager.Instance != null)
        {
            TickSpeedManager.Instance.Reset();
        }

        // DimBoost 초기화
        if (DimBoostManager.Instance != null)
        {
            DimBoostManager.Instance.Reset();
        }
    }

    public bool CanBuyUpgrade(string upgradeId)
    {
        if (!upgrades.ContainsKey(upgradeId))
            return false;

        PrestigeUpgrade upgrade = upgrades[upgradeId];
        return upgrade.CanAfford(prestigePoints);
    }

    public void BuyUpgrade(string upgradeId)
    {
        if (!CanBuyUpgrade(upgradeId))
        {
            Debug.Log($"Cannot afford upgrade: {upgradeId}");
            return;
        }

        PrestigeUpgrade upgrade = upgrades[upgradeId];
        int cost = upgrade.GetNextCost();

        prestigePoints -= cost;
        upgrade.Purchase();

        Debug.Log($"Purchased {upgrade.name} (Level {upgrade.level})");
    }

    // 업그레이드 효과 계산
    public double GetTickspeedBoost()
    {
        if (!upgrades.ContainsKey("tickspeed_boost"))
            return 0;

        return upgrades["tickspeed_boost"].level * 0.01;
    }

    public double GetDimensionMultiplier(int tier)
    {
        if (tier < 1 || tier > 8)
            return 1.0;

        string key = $"dim{tier}_mult";
        if (!upgrades.ContainsKey(key))
            return 1.0;

        int level = upgrades[key].level;
        return Math.Pow(2.0, level);
    }

    public double GetBulkBonusIncrease()
    {
        if (!upgrades.ContainsKey("bulk_bonus"))
            return 0;

        return upgrades["bulk_bonus"].level * 0.05;
    }

    public string GetPrestigeInfo()
    {
        if (!CanPrestige())
        {
            BigDouble requirement = new BigDouble(1e10);
            return $"Prestige available at {requirement}\nCurrent: {GameManager.Instance.antimatter}";
        }

        int pointsGained = CalculatePrestigePointsGained();
        return $"Prestige: +{pointsGained} points (Total: {prestigePoints})";
    }
}
