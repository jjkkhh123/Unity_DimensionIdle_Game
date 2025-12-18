using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main Display")]
    public TextMeshProUGUI antimatterText;
    public TextMeshProUGUI antimatterPerSecondText;

    [Header("Dimension Buttons")]
    public DimensionButton[] dimensionButtons;

    [Header("Prestige")]
    public Button prestigeButton;
    public TextMeshProUGUI prestigeButtonText;
    public TextMeshProUGUI prestigeInfoText;

    [Header("Tickspeed")]
    public Button tickspeedButton;
    public TextMeshProUGUI tickspeedButtonText;
    public TextMeshProUGUI tickspeedInfoText;

    [Header("Dimension Boost")]
    public Button dimBoostButton;
    public TextMeshProUGUI dimBoostButtonText;
    public TextMeshProUGUI dimBoostInfoText;

    [Header("Infinity")]
    public GameObject infinityPanel;
    public TextMeshProUGUI infinityText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (infinityPanel != null)
            infinityPanel.SetActive(false);

        if (prestigeButton != null)
            prestigeButton.onClick.AddListener(OnPrestigeButtonClicked);

        if (tickspeedButton != null)
        {
            tickspeedButton.onClick.AddListener(OnTickspeedButtonClicked);

            // 틱스피드 버튼에 꾹 누르기 기능 추가
            ButtonHoldHandler tickspeedHold = tickspeedButton.gameObject.AddComponent<ButtonHoldHandler>();
            tickspeedHold.holdDelay = 0.4f;
            tickspeedHold.initialInterval = 0.12f;
            tickspeedHold.minInterval = 0.02f;
            tickspeedHold.acceleration = 0.90f;
            tickspeedHold.onHoldClick.AddListener(OnTickspeedButtonClicked);
        }

        if (dimBoostButton != null)
        {
            dimBoostButton.onClick.AddListener(OnDimBoostButtonClicked);
        }
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (GameManager.Instance == null)
            return;

        UpdateAntimatterDisplay();

        UpdateDimensionButtons();

        UpdatePrestigeButton();

        UpdateTickspeedButton();

        UpdateDimBoostButton();

        UpdateInfinityPanel();
    }

    void UpdateAntimatterDisplay()
    {
        if (antimatterText != null)
        {
            antimatterText.text = $"Antimatter: {GameManager.Instance.GetAntimatterString()}";
        }

        if (antimatterPerSecondText != null)
        {
            BigDouble perSecond = BigDouble.Zero;
            if (GameManager.Instance.dimensions.Count > 0)
            {
                perSecond = GameManager.Instance.dimensions[0].GetProduction();

                // 틱스피드 배수 적용
                if (TickSpeedManager.Instance != null)
                {
                    double tickspeedMultiplier = TickSpeedManager.Instance.GetTickspeedMultiplier();
                    perSecond = perSecond * new BigDouble(tickspeedMultiplier);
                }
            }
            antimatterPerSecondText.text = $"/sec: {perSecond}";
        }
    }

    void UpdateDimensionButtons()
    {
        if (dimensionButtons == null)
            return;

        for (int i = 0; i < dimensionButtons.Length; i++)
        {
            if (dimensionButtons[i] != null)
            {
                dimensionButtons[i].UpdateButton();
            }
        }
    }

    void UpdatePrestigeButton()
    {
        if (PrestigeManager.Instance == null || GameManager.Instance == null)
            return;

        bool canPrestige = PrestigeManager.Instance.CanPrestige();

        if (prestigeButton != null)
        {
            prestigeButton.interactable = canPrestige;
        }

        if (prestigeButtonText != null)
        {
            if (canPrestige)
            {
                int points = PrestigeManager.Instance.CalculatePrestigePointsGained();
                prestigeButtonText.text = $"Prestige (+{points} points)";
            }
            else
            {
                prestigeButtonText.text = "Prestige (Locked)";
            }
        }

        if (prestigeInfoText != null)
        {
            string statusText = "";

            if (canPrestige)
            {
                int points = PrestigeManager.Instance.CalculatePrestigePointsGained();
                statusText = $"Ready! Will gain +{points} point(s)\n\n";
            }
            else
            {
                BigDouble requirement = new BigDouble(1e10);
                statusText = $"Requires: {requirement} Antimatter\n\n";
            }

            statusText += $"Prestige Points: {PrestigeManager.Instance.prestigePoints}\n";
            statusText += $"Total Prestiges: {PrestigeManager.Instance.totalPrestiges}\n\n";
            statusText += $"Formula: 1e10 = 1 point\n";
            statusText += $"(exponent / 10)\n\n";
            statusText += $"Spend points in Prestige tab!";

            prestigeInfoText.text = statusText;
        }
    }

    private bool infinityPanelDismissed = false;

    void UpdateInfinityPanel()
    {
        if (infinityPanel != null && GameManager.Instance != null)
        {
            // 무한 상태가 아니면 패널 숨기고 플래그 리셋
            if (!GameManager.Instance.infinityReached)
            {
                if (infinityPanel.activeSelf)
                {
                    infinityPanel.SetActive(false);
                }
                infinityPanelDismissed = false;
            }
            // 무한 도달 시 패널 표시 (사용자가 닫지 않았다면)
            else if (!infinityPanelDismissed)
            {
                if (!infinityPanel.activeSelf)
                {
                    infinityPanel.SetActive(true);

                    if (infinityText != null)
                    {
                        infinityText.text = "INFINITY REACHED!\n\nCongratulations!\n\n(Production stopped)\n\nSpend antimatter to resume production\n\nClick anywhere to continue";
                    }
                }

                // 패널이 활성화되어 있고 클릭하면 닫기
                if (infinityPanel.activeSelf && Input.GetMouseButtonDown(0))
                {
                    infinityPanel.SetActive(false);
                    infinityPanelDismissed = true;
                }
            }
        }
    }

    void OnPrestigeButtonClicked()
    {
        if (PrestigeManager.Instance != null)
        {
            PrestigeManager.Instance.DoPrestige();
        }
    }

    void UpdateTickspeedButton()
    {
        if (TickSpeedManager.Instance == null)
            return;

        bool canBuy = TickSpeedManager.Instance.CanBuyTickspeed();

        if (tickspeedButton != null)
        {
            tickspeedButton.interactable = canBuy;
        }

        if (tickspeedButtonText != null)
        {
            BigDouble price = TickSpeedManager.Instance.GetCurrentPrice();
            tickspeedButtonText.text = $"Upgrade Tickspeed\nCost: {price}";
        }

        if (tickspeedInfoText != null)
        {
            int level = TickSpeedManager.Instance.tickspeedLevel;
            double multiplier = TickSpeedManager.Instance.GetTickspeedMultiplier();

            // 프레스티지 업그레이드에 따른 기본 배율 계산
            double baseMultiplier = 1.1;
            if (PrestigeManager.Instance != null)
                baseMultiplier += PrestigeManager.Instance.GetTickspeedBoost();

            tickspeedInfoText.text = $"Tickspeed Level: {level}\nCurrent Multiplier: x{multiplier:F2}\n\nIncreases production speed\nMultiplies by x{baseMultiplier:F2} per upgrade";
        }
    }

    void OnTickspeedButtonClicked()
    {
        if (TickSpeedManager.Instance != null)
        {
            TickSpeedManager.Instance.BuyTickspeed();
        }
    }

    void UpdateDimBoostButton()
    {
        if (DimBoostManager.Instance == null || GameManager.Instance == null)
            return;

        bool canDimBoost = DimBoostManager.Instance.CanDimBoost();

        if (dimBoostButton != null)
        {
            dimBoostButton.interactable = canDimBoost;
        }

        if (dimBoostButtonText != null)
        {
            int highestTier = DimBoostManager.Instance.GetHighestUnlockedTier();

            if (canDimBoost)
            {
                if (highestTier < 8)
                {
                    int nextTier = DimBoostManager.Instance.GetNextUnlockTier();
                    dimBoostButtonText.text = $"Dimension Enhance\n(Unlock {nextTier}th Dimension)";
                }
                else
                {
                    // 8차원 이미 해금됨
                    dimBoostButtonText.text = $"Dimension Enhance\n(Boost All Dimensions)";
                }
            }
            else
            {
                dimBoostButtonText.text = "Dimension Enhance\n(Locked)";
            }
        }

        if (dimBoostInfoText != null)
        {
            int highestTier = DimBoostManager.Instance.GetHighestUnlockedTier();
            int required = DimBoostManager.Instance.GetRequiredAmount();
            int nextTier = DimBoostManager.Instance.GetNextUnlockTier();

            string statusText = "";

            // 조건 표시
            if (highestTier >= 3)
            {
                if (highestTier < 8)
                {
                    // 8차원 해금 전
                    Dimension highestDim = GameManager.Instance.dimensions[highestTier - 1];
                    statusText += $"Requires: {required} {highestTier}th Dimensions\n";
                    statusText += $"Current: {highestDim.bought}\n\n";
                }
                else
                {
                    // 8차원 해금 후
                    Dimension dim8 = GameManager.Instance.dimensions[7];
                    statusText += $"Requires: {required} 8th Dimensions\n";
                    statusText += $"Current: {dim8.bought}\n\n";
                }
            }

            statusText += $"Total Dim Boosts: {DimBoostManager.Instance.dimBoosts}\n\n";
            statusText += "Effect:\n";
            statusText += $"• Reset Antimatter & Dimensions\n";

            // 8차원 해금 전에만 "Unlock next Dimension" 표시
            if (highestTier < 8)
            {
                statusText += $"• Unlock next Dimension\n";
            }

            // 배수 표시
            int nextBoostCount = DimBoostManager.Instance.dimBoosts + 1;
            if (nextBoostCount == 1)
            {
                statusText += $"• 1st Dimension ×2.0";
            }
            else if (nextBoostCount >= 8)
            {
                statusText += $"• All Dimensions ×2.0";
            }
            else
            {
                statusText += $"• Dimensions 1-{nextBoostCount} ×2.0 each";
            }

            dimBoostInfoText.text = statusText;
        }
    }

    void OnDimBoostButtonClicked()
    {
        if (DimBoostManager.Instance != null)
        {
            DimBoostManager.Instance.DoDimBoost();
        }
    }
}
