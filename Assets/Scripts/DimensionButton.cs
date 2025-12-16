using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DimensionButton : MonoBehaviour
{
    [Header("Dimension Settings")]
    public int dimensionTier = 1;

    [Header("UI References")]
    public Button buyButton;
    public Button buyMaxButton;
    public TextMeshProUGUI dimensionNameText;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI productionText;
    public TextMeshProUGUI priceText;
    public GameObject lockedPanel;
    public TextMeshProUGUI unlockRequirementText;
    public Image[] buyMaxProgressBars;

    void Start()
    {
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        if (buyMaxButton != null)
        {
            buyMaxButton.onClick.AddListener(OnBuyMaxButtonClicked);
        }

        if (dimensionNameText != null)
        {
            dimensionNameText.text = $"Dimension {dimensionTier}";
        }
    }

    public void UpdateButton()
    {
        if (GameManager.Instance == null)
            return;

        bool unlocked = GameManager.Instance.IsDimensionUnlocked(dimensionTier);

        if (lockedPanel != null)
        {
            lockedPanel.SetActive(!unlocked);
        }

        if (!unlocked)
        {
            if (buyButton != null) buyButton.interactable = false;
            if (buyMaxButton != null) buyMaxButton.interactable = false;
            UpdateUnlockRequirement();
            return;
        }

        UpdateAmountText();
        UpdateMultiplierText();
        UpdateProductionText();
        UpdatePriceText();
        UpdateBuyButtonState();
        UpdateBuyMaxProgress();
    }

    void UpdateUnlockRequirement()
    {
        if (unlockRequirementText == null || dimensionTier <= 2)
            return;

        if (GameManager.Instance == null)
            return;

        // 이전 차원 확인
        int prevTier = dimensionTier - 1;
        Dimension prevDim = GameManager.Instance.dimensions[prevTier - 1];

        int current = prevDim.bought;
        int required = 40;

        unlockRequirementText.text = $"LOCKED\n\nRequires:\n{required} Dimension {prevTier}\n\nCurrent: {current}/{required}";
    }

    void UpdateAmountText()
    {
        if (amountText != null)
        {
            Dimension dim = GameManager.Instance.dimensions[dimensionTier - 1];
            string amount = GameManager.Instance.GetDimensionAmountString(dimensionTier);
            amountText.text = $"Bought: {dim.bought} | Owned: {amount}";
        }
    }

    void UpdateMultiplierText()
    {
        if (multiplierText != null && PrestigeManager.Instance != null)
        {
            double multiplier = PrestigeManager.Instance.GetDimensionMultiplier(dimensionTier);
            multiplierText.text = $"x{multiplier:F2}";
        }
    }

    void UpdateProductionText()
    {
        if (productionText != null)
        {
            string production = GameManager.Instance.GetDimensionProductionString(dimensionTier);

            if (dimensionTier == 1)
            {
                productionText.text = $"Antimatter/s: {production}";
            }
            else
            {
                productionText.text = $"Dim {dimensionTier - 1}/s: {production}";
            }
        }
    }

    void UpdatePriceText()
    {
        if (priceText != null)
        {
            string price = GameManager.Instance.GetDimensionPriceString(dimensionTier);
            priceText.text = $"Price: {price}";
        }
    }

    void UpdateBuyButtonState()
    {
        bool canBuy = GameManager.Instance.CanBuyDimension(dimensionTier, 1);

        if (buyButton != null)
        {
            buyButton.interactable = canBuy;
        }

        if (buyMaxButton != null)
        {
            buyMaxButton.interactable = canBuy;
        }
    }

    void UpdateBuyMaxProgress()
    {
        if (buyMaxProgressBars == null || buyMaxProgressBars.Length != 10)
            return;

        Dimension dim = GameManager.Instance.dimensions[dimensionTier - 1];
        int currentBought = dim.bought;
        int inCurrentTen = currentBought % 10; // 현재 10개 중 몇 개 구매했는지 (0-9)

        // 다음 10의 배수까지 몇 개 살 수 있는지 계산
        int nextTen = ((currentBought / 10) + 1) * 10;
        int toBuy = nextTen - currentBought;
        int canAfford = 0;

        // 몇 개까지 살 수 있는지 계산
        Dimension tempDim = new Dimension(dim.tier, dim.basePrice);
        tempDim.amount = dim.amount;
        tempDim.currentPrice = dim.currentPrice;
        tempDim.bought = dim.bought;
        tempDim.unlocked = dim.unlocked;
        tempDim.multiplier = dim.multiplier;

        BigDouble tempCurrency = GameManager.Instance.antimatter;
        for (int i = 0; i < toBuy; i++)
        {
            if (tempCurrency >= tempDim.currentPrice)
            {
                tempCurrency = tempCurrency - tempDim.currentPrice;
                tempDim.Buy(1);
                canAfford++;
            }
            else
            {
                break;
            }
        }

        // 10개 칸 색상 업데이트
        for (int i = 0; i < 10; i++)
        {
            if (buyMaxProgressBars[i] == null) continue;

            if (i < inCurrentTen)
            {
                // 이미 구매한 칸 - 초록색 불투명
                buyMaxProgressBars[i].color = new Color(0.2f, 0.8f, 0.2f, 1f);
            }
            else if (i < inCurrentTen + canAfford)
            {
                // 구매 가능한 칸 - 노란색 반투명
                buyMaxProgressBars[i].color = new Color(1f, 0.9f, 0.2f, 0.7f);
            }
            else
            {
                // 구매 불가능한 칸 - 회색 매우 연한
                buyMaxProgressBars[i].color = new Color(0.5f, 0.5f, 0.5f, 0.25f);
            }
        }
    }

    void OnBuyButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BuyDimension(dimensionTier, 1);
        }
    }

    void OnBuyMaxButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            // 다음 10의 배수까지 구매
            Dimension dim = GameManager.Instance.dimensions[dimensionTier - 1];
            int currentBought = dim.bought;
            int nextTen = ((currentBought / 10) + 1) * 10;
            int toBuy = nextTen - currentBought;

            // 최대 toBuy개 구매 시도
            for (int i = 0; i < toBuy; i++)
            {
                if (!GameManager.Instance.CanBuyDimension(dimensionTier, 1))
                    break;

                GameManager.Instance.BuyDimension(dimensionTier, 1);
            }
        }
    }
}
