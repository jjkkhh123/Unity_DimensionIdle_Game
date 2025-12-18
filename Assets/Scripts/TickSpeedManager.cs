using UnityEngine;

public class TickSpeedManager : MonoBehaviour
{
    public static TickSpeedManager Instance { get; private set; }

    public int tickspeedLevel = 0;
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
    }
}
