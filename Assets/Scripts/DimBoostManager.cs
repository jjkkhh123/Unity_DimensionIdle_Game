using UnityEngine;

public class DimBoostManager : MonoBehaviour
{
    public static DimBoostManager Instance { get; private set; }

    public int dimBoosts = 0;
    private const int REQUIRED_AMOUNT = 20; // 현재 최고 차원 20개 구매 필요

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

    public bool CanDimBoost()
    {
        if (GameManager.Instance == null)
            return false;

        int highestUnlockedTier = GetHighestUnlockedTier();

        // 최소 3차원까지 해금되어야 함
        if (highestUnlockedTier < 2)
            return false;

        // 8차원이 아직 해금 안됐으면 기존 로직
        if (highestUnlockedTier < 8)
        {
            Dimension highestDim = GameManager.Instance.dimensions[highestUnlockedTier - 1];
            return highestDim.bought >= REQUIRED_AMOUNT;
        }
        else // 8차원 해금됨 - 계속 DimBoost 가능
        {
            Dimension dim8 = GameManager.Instance.dimensions[7];
            int required = GetRequiredAmount();
            return dim8.bought >= required;
        }
    }

    public int GetNextUnlockTier()
    {
        int highestUnlockedTier = GetHighestUnlockedTier();
        return highestUnlockedTier + 1;
    }

    public int GetRequiredAmount()
    {
        int highestUnlockedTier = GetHighestUnlockedTier();

        // 8차원이 아직 해금 안됐으면 고정 20개
        if (highestUnlockedTier < 8)
        {
            return REQUIRED_AMOUNT;
        }
        else // 8차원 해금됨 - 20, 40, 60, 80...
        {
            if (GameManager.Instance == null)
                return REQUIRED_AMOUNT;

            Dimension dim8 = GameManager.Instance.dimensions[7];
            int currentBought = dim8.bought;

            // 다음 20의 배수 계산
            // 0~19 → 20, 20~39 → 40, 40~59 → 60...
            return ((currentBought / REQUIRED_AMOUNT) + 1) * REQUIRED_AMOUNT;
        }
    }

    public int GetHighestUnlockedTier()
    {
        if (GameManager.Instance == null)
            return 2;

        for (int i = GameManager.Instance.dimensions.Count - 1; i >= 0; i--)
        {
            if (GameManager.Instance.dimensions[i].unlocked)
            {
                return i + 1;
            }
        }
        return 2; // 최소 2차원
    }

    public void DoDimBoost()
    {
        if (!CanDimBoost())
            return;

        int nextTier = GetNextUnlockTier();

        // 안티매터 초기화
        GameManager.Instance.antimatter = new BigDouble(10);

        // 모든 차원 리셋
        for (int i = 0; i < GameManager.Instance.dimensions.Count; i++)
        {
            GameManager.Instance.dimensions[i].Reset();
        }

        // 1-2차원 다시 해금
        GameManager.Instance.dimensions[0].unlocked = true;
        GameManager.Instance.dimensions[1].unlocked = true;

        // 이전까지 해금했던 차원들 다시 해금
        for (int i = 2; i < nextTier && i < GameManager.Instance.dimensions.Count; i++)
        {
            GameManager.Instance.dimensions[i].unlocked = true;
        }

        // 다음 차원 해금
        if (nextTier <= 8)
        {
            GameManager.Instance.dimensions[nextTier - 1].unlocked = true;
        }

        // 1차원부터 (dimBoosts+1)차원까지 x2 배수 적용
        // 1회: 1차원만, 2회: 1~2차원, 3회: 1~3차원, ...
        int boostCount = dimBoosts + 1;
        for (int i = 0; i < boostCount && i < GameManager.Instance.dimensions.Count; i++)
        {
            GameManager.Instance.dimensions[i].ApplyPrestigeMultiplier(new BigDouble(2.0));
        }

        dimBoosts++;

        // Infinity 상태 리셋
        GameManager.Instance.infinityReached = false;

        // Tickspeed도 리셋
        if (TickSpeedManager.Instance != null)
        {
            TickSpeedManager.Instance.Reset();
        }
    }

    public void Reset()
    {
        dimBoosts = 0;
    }
}
