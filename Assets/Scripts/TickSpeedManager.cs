using UnityEngine;

public class TickSpeedManager : MonoBehaviour
{
    public static TickSpeedManager Instance { get; private set; }

    public int tickspeedLevel = 0;
    public bool bulkBuyUnlocked = false; // 마일스톤 5회로 해금
    private BigDouble basePrice = new BigDouble(100);
    private const double PRICE_MULTIPLIER = 10.0;
    private const double SPEED_MULTIPLIER_PER_LEVEL = 1.1;

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
        }
    }

    public BigDouble GetCurrentPrice()
    {
        return basePrice * BigDouble.Pow(new BigDouble(PRICE_MULTIPLIER), tickspeedLevel);
    }

    public bool CanBuyTickspeed()
    {
        if (GameManager.Instance == null)
            return false;

        return GameManager.Instance.antimatter >= GetCurrentPrice();
    }

    public void BuyTickspeed()
    {
        if (!CanBuyTickspeed())
        {
            return;
        }

        BigDouble price = GetCurrentPrice();
        GameManager.Instance.antimatter = GameManager.Instance.antimatter - price;
        tickspeedLevel++;
    }

    public double GetTickspeedMultiplier()
    {
        // 기본 배수
        double baseMultiplier = 1.1;

        // 프레스티지 업그레이드로 배수 증가
        if (PrestigeManager.Instance != null)
        {
            baseMultiplier += PrestigeManager.Instance.GetTickspeedBoost();
        }

        if (tickspeedLevel == 0)
            return 1.0;

        return System.Math.Pow(baseMultiplier, tickspeedLevel);
    }

    public void Reset()
    {
        tickspeedLevel = 0;
        // bulkBuyUnlocked은 마일스톤으로 영구 해금이므로 리셋하지 않음
    }

    // ===== 벌크 구매 기능 (마일스톤 5회로 해금) =====

    public int CalculateMaxAffordable()
    {
        if (GameManager.Instance == null)
            return 0;

        BigDouble currency = GameManager.Instance.antimatter;
        BigDouble totalCost = BigDouble.Zero;
        int tempLevel = tickspeedLevel;
        int count = 0;

        while (true)
        {
            BigDouble nextCost = basePrice * BigDouble.Pow(new BigDouble(PRICE_MULTIPLIER), tempLevel);
            if (totalCost + nextCost > currency)
                break;

            totalCost = totalCost + nextCost;
            tempLevel++;
            count++;

            // 무한 루프 방지
            if (count > 1000)
                break;
        }

        return count;
    }

    public BigDouble CalculateBulkCost(int count)
    {
        BigDouble totalCost = BigDouble.Zero;
        int tempLevel = tickspeedLevel;

        for (int i = 0; i < count; i++)
        {
            BigDouble cost = basePrice * BigDouble.Pow(new BigDouble(PRICE_MULTIPLIER), tempLevel);
            totalCost = totalCost + cost;
            tempLevel++;
        }

        return totalCost;
    }

    public void BuyTickspeedMax()
    {
        if (!bulkBuyUnlocked || GameManager.Instance == null)
            return;

        int maxAffordable = CalculateMaxAffordable();
        if (maxAffordable <= 0)
            return;

        BigDouble totalCost = CalculateBulkCost(maxAffordable);
        GameManager.Instance.antimatter = GameManager.Instance.antimatter - totalCost;
        tickspeedLevel += maxAffordable;

        Debug.Log($"[Tickspeed] Bulk bought {maxAffordable} levels. New level: {tickspeedLevel}");
    }
}
