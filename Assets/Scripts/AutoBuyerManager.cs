using UnityEngine;

public class AutoBuyerManager : MonoBehaviour
{
    public static AutoBuyerManager Instance { get; private set; }

    // 구매 모드
    public enum BuyMode { Single, Bulk }

    // 해금 상태 (차원 1-8)
    public bool[] dimensionAutoBuyersUnlocked = new bool[8];

    // 활성화 상태
    public bool[] dimensionAutoBuyersEnabled = new bool[8];

    // 구매 모드 설정
    public BuyMode[] dimensionBuyModes = new BuyMode[8];

    // 속도 업그레이드
    public int speedUpgradeLevel = 0;
    public const int MAX_SPEED_LEVEL = 10;
    private const float BASE_INTERVAL = 1.0f; // 기본 1초
    private const float INTERVAL_REDUCTION = 0.1f; // 레벨당 0.1초 감소
    private const float MIN_INTERVAL = 0.1f; // 최소 0.1초

    // 속도 업그레이드 비용
    private int[] speedUpgradeCosts = { 5, 10, 15, 25, 40, 60, 90, 130, 180, 250 };

    // 자동 구매 타이머
    private float autoBuyTimer = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAutoBuyers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAutoBuyers()
    {
        for (int i = 0; i < 8; i++)
        {
            dimensionAutoBuyersUnlocked[i] = false;
            dimensionAutoBuyersEnabled[i] = false;
            dimensionBuyModes[i] = BuyMode.Single;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null)
            return;

        autoBuyTimer += Time.deltaTime;

        float interval = GetAutoBuyInterval();
        if (autoBuyTimer >= interval)
        {
            autoBuyTimer = 0f;
            ExecuteAutoBuyers();
        }
    }

    void ExecuteAutoBuyers()
    {
        for (int i = 0; i < 8; i++)
        {
            if (!dimensionAutoBuyersUnlocked[i] || !dimensionAutoBuyersEnabled[i])
                continue;

            int tier = i + 1;
            Dimension dim = GameManager.Instance.dimensions[i];

            if (!dim.unlocked)
                continue;

            if (dimensionBuyModes[i] == BuyMode.Single)
            {
                // 단일 구매
                if (GameManager.Instance.CanBuyDimension(tier, 1))
                {
                    GameManager.Instance.BuyDimension(tier, 1);
                }
            }
            else
            {
                // 벌크 구매 (Until 10)
                GameManager.Instance.BuyDimensionUntil10(tier);
            }
        }
    }

    public float GetAutoBuyInterval()
    {
        float interval = BASE_INTERVAL - (speedUpgradeLevel * INTERVAL_REDUCTION);
        return Mathf.Max(interval, MIN_INTERVAL);
    }

    public void UnlockAutoBuyer(int dimensionIndex)
    {
        if (dimensionIndex >= 0 && dimensionIndex < 8)
        {
            dimensionAutoBuyersUnlocked[dimensionIndex] = true;
            Debug.Log($"[AutoBuyer] Dimension {dimensionIndex + 1} autobuyer unlocked!");
        }
    }

    public void ToggleAutoBuyer(int dimensionIndex)
    {
        if (dimensionIndex >= 0 && dimensionIndex < 8 && dimensionAutoBuyersUnlocked[dimensionIndex])
        {
            dimensionAutoBuyersEnabled[dimensionIndex] = !dimensionAutoBuyersEnabled[dimensionIndex];
            Debug.Log($"[AutoBuyer] Dimension {dimensionIndex + 1} autobuyer: {(dimensionAutoBuyersEnabled[dimensionIndex] ? "ON" : "OFF")}");
        }
    }

    public void SetAutoBuyerEnabled(int dimensionIndex, bool enabled)
    {
        if (dimensionIndex >= 0 && dimensionIndex < 8 && dimensionAutoBuyersUnlocked[dimensionIndex])
        {
            dimensionAutoBuyersEnabled[dimensionIndex] = enabled;
        }
    }

    public void SetBuyMode(int dimensionIndex, BuyMode mode)
    {
        if (dimensionIndex >= 0 && dimensionIndex < 8)
        {
            dimensionBuyModes[dimensionIndex] = mode;
            Debug.Log($"[AutoBuyer] Dimension {dimensionIndex + 1} buy mode: {mode}");
        }
    }

    public bool CanUpgradeSpeed()
    {
        if (speedUpgradeLevel >= MAX_SPEED_LEVEL)
            return false;

        if (PrestigeManager.Instance == null)
            return false;

        return PrestigeManager.Instance.prestigePoints >= GetSpeedUpgradeCost();
    }

    public int GetSpeedUpgradeCost()
    {
        if (speedUpgradeLevel >= MAX_SPEED_LEVEL)
            return 0;

        return speedUpgradeCosts[speedUpgradeLevel];
    }

    public void UpgradeSpeed()
    {
        if (!CanUpgradeSpeed())
            return;

        int cost = GetSpeedUpgradeCost();
        PrestigeManager.Instance.prestigePoints -= cost;
        speedUpgradeLevel++;

        Debug.Log($"[AutoBuyer] Speed upgraded to level {speedUpgradeLevel}. Interval: {GetAutoBuyInterval():F1}s");
    }

    public float GetNextInterval()
    {
        if (speedUpgradeLevel >= MAX_SPEED_LEVEL)
            return GetAutoBuyInterval();

        float nextInterval = BASE_INTERVAL - ((speedUpgradeLevel + 1) * INTERVAL_REDUCTION);
        return Mathf.Max(nextInterval, MIN_INTERVAL);
    }

    public void Reset()
    {
        // 프레스티지 시에는 해금 상태 유지, 활성화만 초기화
        for (int i = 0; i < 8; i++)
        {
            dimensionAutoBuyersEnabled[i] = false;
        }
        autoBuyTimer = 0f;
    }
}
