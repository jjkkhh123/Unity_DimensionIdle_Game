using System;

[System.Serializable]
public class PrestigeUpgrade
{
    public string id;
    public string name;
    public string description;
    public int cost;
    public int level;
    public int maxLevel;
    public double priceScaling; // 가격 증가율 (1.25 = 25% 증가)

    public PrestigeUpgrade(string id, string name, string description, int cost, int maxLevel = 999, double priceScaling = 1.25)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.cost = cost;
        this.level = 0;
        this.maxLevel = maxLevel;
        this.priceScaling = priceScaling;
    }

    public int GetTotalCost()
    {
        // 누적 비용: cost * level
        return cost * level;
    }

    public int GetNextCost()
    {
        if (level >= maxLevel)
            return int.MaxValue;

        // 레벨이 올라갈수록 priceScaling 비율로 비싸짐 (소수점 버림)
        double multiplier = Math.Pow(priceScaling, level);
        return (int)(cost * multiplier);
    }

    public bool CanAfford(int points)
    {
        return points >= GetNextCost() && level < maxLevel;
    }

    public void Purchase()
    {
        if (level < maxLevel)
            level++;
    }
}
