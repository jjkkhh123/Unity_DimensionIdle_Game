using System;
using UnityEngine;
using UnityEngine.UIElements;

public class DimensionsPanelController : MonoBehaviour
{
    // Buy Mode Enum
    public enum BuyMode
    {
        BuyOne,
        UntilTen
    }

    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement dimensionsRoot;
    private VisualElement prestigeRoot;
    private TabManagerUIToolkit tabManager;

    // Antimatter Display
    private Label antimatterAmount;

    // Buy Mode
    private Button changeBuyModeBtn;
    private BuyMode currentBuyMode = BuyMode.BuyOne;

    // Tickspeed Panel
    private Label tickspeedMultiplier;
    private Label tickspeedCost;
    private Button tickspeedBuyBtn;

    // DimBoost Panel
    private Label dimBoostCount;
    private Label dimBoostRequirement;
    private Button dimBoostBtn;

    // Dimension Elements (8 dimensions)
    private DimensionUIElement[] dimensionElements = new DimensionUIElement[8];

    // Bottom Menu
    private Button dimensionsBtn;
    private Button prestigeBtn;
    private Button optionBtn;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("[DimensionsPanelController] UIDocument not found!");
            return;
        }

        root = uiDocument.rootVisualElement;

        // Get TabManager
        tabManager = FindFirstObjectByType<TabManagerUIToolkit>();
        if (tabManager == null)
        {
            Debug.LogWarning("[DimensionsPanelController] TabManagerUIToolkit not found!");
        }

        // Cache panel roots
        dimensionsRoot = root.Q<VisualElement>("root");
        prestigeRoot = root.Q<VisualElement>("prestige-root");

        if (dimensionsRoot == null)
        {
            Debug.LogError("[DimensionsPanelController] Dimensions root not found!");
        }

        if (prestigeRoot == null)
        {
            Debug.LogError("[DimensionsPanelController] Prestige root not found!");
        }

        CacheUIElements();
        RegisterButtonCallbacks();
    }

    void CacheUIElements()
    {
        // Antimatter
        antimatterAmount = dimensionsRoot.Q<Label>("AntimatterAmount");

        // Buy Mode Button
        changeBuyModeBtn = dimensionsRoot.Q<Button>("ChangeBuyModeBtn");

        // Tickspeed
        tickspeedMultiplier = dimensionsRoot.Q<Label>("TickspeedMultiplier");
        tickspeedCost = dimensionsRoot.Q<Label>("TickspeedCost");
        tickspeedBuyBtn = dimensionsRoot.Q<Button>("TickspeedBuyBtn");

        // DimBoost
        dimBoostCount = dimensionsRoot.Q<Label>("DimBoostCount");
        dimBoostRequirement = dimensionsRoot.Q<Label>("DimBoostRequirement");
        dimBoostBtn = dimensionsRoot.Q<Button>("DimBoostBtn");

        // Dimensions (1-8)
        for (int i = 0; i < 8; i++)
        {
            int dimensionIndex = i + 1;
            dimensionElements[i] = new DimensionUIElement
            {
                title = dimensionsRoot.Q<Label>($"Dimension{dimensionIndex}Title"),
                multiplier = dimensionsRoot.Q<Label>($"Dimension{dimensionIndex}Multiplier"),
                amount = dimensionsRoot.Q<Label>($"Dimension{dimensionIndex}Amount"),
                perSec = dimensionsRoot.Q<Label>($"Dimension{dimensionIndex}PerSec"),
                buyBtn = dimensionsRoot.Q<Button>($"Dimension{dimensionIndex}BuyBtn"),
                progressBg = dimensionsRoot.Q<VisualElement>($"Dimension{dimensionIndex}ProgressBg"),
                progressOwned = dimensionsRoot.Q<VisualElement>($"Dimension{dimensionIndex}ProgressOwned"),
                progressAffordable = dimensionsRoot.Q<VisualElement>($"Dimension{dimensionIndex}ProgressAffordable")
            };
        }

        // Bottom Menu (from dimensions panel)
        dimensionsBtn = dimensionsRoot.Q<Button>("DimensionsBtn");
        prestigeBtn = dimensionsRoot.Q<Button>("PrestigeBtn");
        optionBtn = dimensionsRoot.Q<Button>("OptionBtn");
    }

    void RegisterButtonCallbacks()
    {
        // Buy Mode
        if (changeBuyModeBtn != null)
        {
            changeBuyModeBtn.clicked += OnChangeBuyModeClicked;
        }

        // Tickspeed
        if (tickspeedBuyBtn != null)
        {
            tickspeedBuyBtn.clicked += OnTickspeedBuyClicked;
        }

        // DimBoost
        if (dimBoostBtn != null)
        {
            dimBoostBtn.clicked += OnDimBoostClicked;
        }

        // Dimensions
        for (int i = 0; i < 8; i++)
        {
            int index = i; // Capture for closure
            DimensionUIElement elem = dimensionElements[i];

            if (elem.buyBtn != null)
            {
                elem.buyBtn.clicked += () => OnDimensionBuy(index);
            }
        }

        // Bottom Menu
        if (prestigeBtn != null)
        {
            prestigeBtn.clicked += OnPrestigeClicked;
        }

        if (optionBtn != null)
        {
            optionBtn.clicked += OnOptionClicked;
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

        UpdateBuyModeButton();
        UpdateAntimatterDisplay();
        UpdateTickspeedPanel();
        UpdateDimBoostPanel();
        UpdateDimensions();
    }

    void UpdateBuyModeButton()
    {
        if (changeBuyModeBtn != null)
        {
            changeBuyModeBtn.text = currentBuyMode == BuyMode.BuyOne ? "Buy 1" : "Until 10";
        }
    }

    void UpdateAntimatterDisplay()
    {
        if (antimatterAmount != null)
        {
            BigDouble perSecond = BigDouble.Zero;
            if (GameManager.Instance.dimensions.Count > 0)
            {
                perSecond = GameManager.Instance.dimensions[0].GetProduction();

                if (TickSpeedManager.Instance != null)
                {
                    double tickspeedMultiplier = TickSpeedManager.Instance.GetTickspeedMultiplier();
                    perSecond = perSecond * new BigDouble(tickspeedMultiplier);
                }
            }

            antimatterAmount.text = $"{GameManager.Instance.GetAntimatterString()}\n({perSecond}/sec)";
        }
    }

    void UpdateTickspeedPanel()
    {
        if (TickSpeedManager.Instance == null)
            return;

        if (tickspeedMultiplier != null)
        {
            double multiplier = TickSpeedManager.Instance.GetTickspeedMultiplier();
            tickspeedMultiplier.text = $"x{FormatMultiplier(multiplier)}";
        }

        if (tickspeedCost != null)
        {
            BigDouble cost = TickSpeedManager.Instance.GetCurrentPrice();
            tickspeedCost.text = $"Cost: {cost}";
        }

        if (tickspeedBuyBtn != null)
        {
            bool canBuy = TickSpeedManager.Instance.CanBuyTickspeed();
            tickspeedBuyBtn.SetEnabled(canBuy);
            tickspeedBuyBtn.style.opacity = canBuy ? 1.0f : 0.6f;
        }
    }

    void UpdateDimBoostPanel()
    {
        if (DimBoostManager.Instance == null || GameManager.Instance == null)
            return;

        if (dimBoostCount != null)
        {
            int boostCount = DimBoostManager.Instance.dimBoosts;
            string benefitText = GetBoostBenefitText(boostCount);
            dimBoostCount.text = $"{boostCount} Boosts {benefitText}";
        }

        if (dimBoostRequirement != null)
        {
            int highestTier = DimBoostManager.Instance.GetHighestUnlockedTier();
            int required = DimBoostManager.Instance.GetRequiredAmount();

            string requirementText;
            if (highestTier < 8)
            {
                int nextTier = DimBoostManager.Instance.GetNextUnlockTier();
                Dimension highestDim = GameManager.Instance.dimensions[highestTier - 1];
                requirementText = $"Req: {required} Dim {highestTier} ({highestDim.bought}/{required}) → Unlock Dim {nextTier}";
            }
            else
            {
                Dimension dim8 = GameManager.Instance.dimensions[7];
                requirementText = $"Req: {required} Dim 8 ({dim8.bought}/{required})";
            }

            dimBoostRequirement.text = requirementText;
        }

        if (dimBoostBtn != null)
        {
            bool canBoost = DimBoostManager.Instance.CanDimBoost();
            dimBoostBtn.SetEnabled(canBoost);
            dimBoostBtn.style.opacity = canBoost ? 1.0f : 0.6f;
        }
    }

    string GetBoostBenefitText(int currentBoosts)
    {
        int nextBoostLevel = currentBoosts + 1;

        if (nextBoostLevel > 8)
        {
            return "(All Dimensions x2)";
        }
        else
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("(");

            for (int i = 1; i <= nextBoostLevel; i++)
            {
                if (i > 1)
                    sb.Append(", ");
                sb.Append($"Dim{i}");
            }

            sb.Append(" x2)");
            return sb.ToString();
        }
    }

    void UpdateDimensions()
    {
        for (int i = 0; i < 8; i++)
        {
            Dimension dim = GameManager.Instance.dimensions[i];
            DimensionUIElement ui = dimensionElements[i];

            if (ui.buyBtn == null)
                continue;

            bool isLocked = !dim.unlocked;

            if (isLocked)
            {
                // Locked state
                ui.buyBtn.SetEnabled(false);
                ui.buyBtn.style.opacity = 0.4f;

                if (ui.amount != null)
                    ui.amount.text = "Locked";

                if (ui.multiplier != null)
                    ui.multiplier.text = "";

                if (ui.perSec != null)
                    ui.perSec.text = "";

                ui.buyBtn.text = "Locked";
            }
            else
            {
                // Unlocked state
                ui.buyBtn.SetEnabled(true);

                // Update amount
                if (ui.amount != null)
                    ui.amount.text = dim.amount.ToString();

                // Update multiplier
                if (ui.multiplier != null)
                {
                    // Prestige multiplier
                    double prestigeMultiplier = 1.0;
                    if (PrestigeManager.Instance != null)
                    {
                        prestigeMultiplier = PrestigeManager.Instance.GetDimensionMultiplier(dim.tier);
                    }

                    // Bulk multiplier (10개 구매마다)
                    double bulkMultiplier = 2.0;
                    if (PrestigeManager.Instance != null)
                    {
                        bulkMultiplier += PrestigeManager.Instance.GetBulkBonusIncrease();
                    }
                    int tier10Count = dim.bought / 10;
                    BigDouble boughtBonus = BigDouble.Pow(new BigDouble(bulkMultiplier), tier10Count);

                    // Shop multiplier
                    double shopMultiplier = 1.0;
                    if (ShopManager.Instance != null)
                    {
                        shopMultiplier = ShopManager.Instance.GetBoostMultiplier(dim.tier);
                    }

                    BigDouble totalMultiplier = new BigDouble(prestigeMultiplier) * dim.multiplier * boughtBonus * new BigDouble(shopMultiplier);
                    ui.multiplier.text = $"x{FormatMultiplier(totalMultiplier)}";
                }

                // Update production per second (growth rate for all dimensions)
                if (ui.perSec != null)
                {
                    // Next dimension (i+1) produces current dimension (i)
                    if (i + 1 < GameManager.Instance.dimensions.Count)
                    {
                        Dimension nextDim = GameManager.Instance.dimensions[i + 1];

                        if (nextDim.unlocked && dim.amount > BigDouble.Zero)
                        {
                            BigDouble nextProduction = nextDim.GetProduction();

                            // Apply tickspeed multiplier
                            if (TickSpeedManager.Instance != null)
                            {
                                double tickMultiplier = TickSpeedManager.Instance.GetTickspeedMultiplier();
                                nextProduction = nextProduction * new BigDouble(tickMultiplier);
                            }

                            // Growth rate = (production / current amount) * 100
                            BigDouble growthRate = (nextProduction / dim.amount) * new BigDouble(100);
                            ui.perSec.text = $"(+{growthRate}%/s)";
                        }
                        else
                        {
                            ui.perSec.text = "(+0%/s)";
                        }
                    }
                    else
                    {
                        // Dimension 8 has no next dimension
                        ui.perSec.text = "(+0%/s)";
                    }
                }

                // Update buy button based on current buy mode
                // AD풍: GetCurrentPrice() 사용
                BigDouble cost = dim.GetCurrentPrice();
                bool canAfford = GameManager.Instance.antimatter >= cost;

                if (currentBuyMode == BuyMode.BuyOne)
                {
                    ui.buyBtn.text = $"Buy 1\n{cost}";
                }
                else // UntilTen
                {
                    // AD풍: Until 10 표시 (구매 가능 수량에 맞춰 가격 계산)
                    int remaining = dim.GetRemainingUntil10();
                    BigDouble currency = GameManager.Instance.antimatter;

                    // 실제로 구매 가능한 수량 계산
                    int affordable = CalculateAffordableUntil10(dim, currency);

                    if (affordable >= remaining)
                    {
                        // 전부 살 수 있음
                        BigDouble costUntil10 = dim.GetCostUntil10();
                        ui.buyBtn.text = $"Until 10 ({remaining})\n{costUntil10}";
                        canAfford = true;
                    }
                    else if (affordable > 0)
                    {
                        // 일부만 살 수 있음
                        BigDouble partialCost = CalculateCostForAffordable(dim, affordable);
                        ui.buyBtn.text = $"Until 10 ({affordable}/{remaining})\n{partialCost}";
                        canAfford = true;
                    }
                    else
                    {
                        // 1개도 못 삼
                        ui.buyBtn.text = $"Until 10 ({remaining})\n{cost}";
                        canAfford = false;
                    }
                }

                ui.buyBtn.style.opacity = canAfford ? 1.0f : 0.6f;

                // Update progress bars
                UpdateProgressBar(ui, dim);
            }
        }
    }

    void UpdateProgressBar(DimensionUIElement ui, Dimension dim)
    {
        if (ui.progressBg == null || ui.progressOwned == null || ui.progressAffordable == null)
            return;

        // Get the parent container
        VisualElement progressContainer = ui.progressBg.parent;
        if (progressContainer == null)
            return;

        // Only show progress bar in UntilTen mode
        if (currentBuyMode != BuyMode.UntilTen)
        {
            progressContainer.style.display = DisplayStyle.None;
            return;
        }

        progressContainer.style.display = DisplayStyle.Flex;

        // Calculate owned and affordable amounts
        int currentBought = dim.bought;
        int ownedInCurrentTen = currentBought % 10; // 0-9

        // AD풍: Until 10까지 남은 개수와 구매 가능 개수 계산
        int remaining = dim.GetRemainingUntil10();
        BigDouble currency = GameManager.Instance.antimatter;

        // 구매 가능한 개수 시뮬레이션
        int canAfford = 0;
        BigDouble tempCost = BigDouble.Zero;
        int tempBought = dim.bought;

        for (int i = 0; i < remaining; i++)
        {
            int sets = tempBought / 10;
            BigDouble setCost = dim.baseCost * BigDouble.Pow(dim.costIncreasePer10, sets);
            BigDouble singleCost = setCost / new BigDouble(10);

            if (tempCost + singleCost <= currency)
            {
                tempCost = tempCost + singleCost;
                tempBought++;
                canAfford++;
            }
            else
            {
                break;
            }
        }

        // Calculate percentages
        float ownedPercent = ownedInCurrentTen / 10f;
        float affordablePercent = canAfford / 10f;

        // Update background: darker if can't afford any, lighter if can afford at least 1
        if (canAfford > 0)
        {
            ui.progressBg.style.backgroundColor = new Color(0.31f, 0.31f, 0.31f, 0.6f);
        }
        else
        {
            ui.progressBg.style.backgroundColor = new Color(0.31f, 0.31f, 0.31f, 0.5f);
        }

        // Update owned bar (green) - vibrant green with glow
        ui.progressOwned.style.width = Length.Percent(ownedPercent * 100f);
        ui.progressOwned.style.left = 0;

        // Update affordable bar (yellow), positioned after owned
        ui.progressAffordable.style.width = Length.Percent(affordablePercent * 100f);
        ui.progressAffordable.style.left = Length.Percent(ownedPercent * 100f);
    }

    int CalculateBuyUntilTen(Dimension dim)
    {
        int currentAmount = dim.bought;
        int remainder = currentAmount % 10;
        int amountToBuy = remainder == 0 ? 10 : (10 - remainder);
        return amountToBuy;
    }

    /// <summary>
    /// Until 10까지 구매 가능한 수량 계산
    /// </summary>
    int CalculateAffordableUntil10(Dimension dim, BigDouble currency)
    {
        int remaining = dim.GetRemainingUntil10();
        int affordable = 0;
        BigDouble totalCost = BigDouble.Zero;
        int tempBought = dim.bought;

        for (int i = 0; i < remaining; i++)
        {
            int sets = tempBought / 10;
            BigDouble setCost = dim.baseCost * BigDouble.Pow(dim.costIncreasePer10, sets);
            BigDouble singleCost = setCost / new BigDouble(10);

            if (totalCost + singleCost <= currency)
            {
                totalCost = totalCost + singleCost;
                tempBought++;
                affordable++;
            }
            else
            {
                break;
            }
        }

        return affordable;
    }

    /// <summary>
    /// 특정 수량 구매 시 총 비용 계산
    /// </summary>
    BigDouble CalculateCostForAffordable(Dimension dim, int count)
    {
        BigDouble totalCost = BigDouble.Zero;
        int tempBought = dim.bought;

        for (int i = 0; i < count; i++)
        {
            int sets = tempBought / 10;
            BigDouble setCost = dim.baseCost * BigDouble.Pow(dim.costIncreasePer10, sets);
            BigDouble singleCost = setCost / new BigDouble(10);
            totalCost = totalCost + singleCost;
            tempBought++;
        }

        return totalCost;
    }

    /// <summary>
    /// 배율 포맷: 정수면 소수점 없이, 아니면 소수점 2자리
    /// </summary>
    string FormatMultiplier(double value)
    {
        if (value >= 1e6)
        {
            // 지수 표기
            int exp = (int)Math.Floor(Math.Log10(value));
            double mantissa = value / Math.Pow(10, exp);
            if (Math.Abs(mantissa - Math.Round(mantissa)) < 0.001)
                return $"{mantissa:F0}e{exp}";
            return $"{mantissa:F2}e{exp}";
        }
        else
        {
            // 일반 숫자: 소수점 2자리까지 표시
            if (Math.Abs(value - Math.Round(value)) < 0.001)
                return value.ToString("F0"); // 정수면 소수점 생략
            return value.ToString("F2"); // 소수점 있으면 2자리
        }
    }

    /// <summary>
    /// 배율 포맷 (BigDouble 오버로드)
    /// </summary>
    string FormatMultiplier(BigDouble value)
    {
        return value.ToString();
    }

    // Button Event Handlers
    void OnChangeBuyModeClicked()
    {
        // Toggle between BuyOne and UntilTen
        currentBuyMode = currentBuyMode == BuyMode.BuyOne ? BuyMode.UntilTen : BuyMode.BuyOne;
    }

    void OnTickspeedBuyClicked()
    {
        if (TickSpeedManager.Instance != null)
        {
            // Until 10 모드이고 벌크 구매 해금 시 MAX 구매
            if (currentBuyMode == BuyMode.UntilTen && TickSpeedManager.Instance.bulkBuyUnlocked)
            {
                TickSpeedManager.Instance.BuyTickspeedMax();
            }
            else
            {
                TickSpeedManager.Instance.BuyTickspeed();
            }
        }
    }

    void OnDimBoostClicked()
    {
        if (DimBoostManager.Instance != null && DimBoostManager.Instance.CanDimBoost())
        {
            DimBoostManager.Instance.DoDimBoost();
        }
    }

    void OnDimensionBuy(int index)
    {
        if (GameManager.Instance == null)
            return;

        int tier = index + 1;
        Dimension dim = GameManager.Instance.dimensions[index];

        if (!dim.unlocked)
            return;

        if (currentBuyMode == BuyMode.BuyOne)
        {
            // Buy 1
            if (GameManager.Instance.CanBuyDimension(tier, 1))
            {
                GameManager.Instance.BuyDimension(tier, 1);
            }
        }
        else // UntilTen
        {
            // AD풍: Until 10 구매 (새 메서드 사용)
            GameManager.Instance.BuyDimensionUntil10(tier);
        }
    }

    void OnPrestigeClicked()
    {
        if (tabManager != null)
        {
            tabManager.ShowPanel("Prestige");
        }
        else
        {
            Debug.LogError("[DimensionsPanelController] TabManager not found - cannot switch panels");
        }
    }

    void OnOptionClicked()
    {
        if (tabManager != null)
        {
            tabManager.ShowPanel("Options");
        }
        else
        {
            Debug.LogError("[DimensionsPanelController] TabManager not found - cannot switch panels");
        }
    }

    void OnDestroy()
    {
        // Unregister callbacks
        if (changeBuyModeBtn != null)
            changeBuyModeBtn.clicked -= OnChangeBuyModeClicked;

        if (tickspeedBuyBtn != null)
            tickspeedBuyBtn.clicked -= OnTickspeedBuyClicked;

        if (dimBoostBtn != null)
            dimBoostBtn.clicked -= OnDimBoostClicked;

        for (int i = 0; i < 8; i++)
        {
            int index = i;
            DimensionUIElement elem = dimensionElements[i];

            if (elem.buyBtn != null)
                elem.buyBtn.clicked -= () => OnDimensionBuy(index);
        }

        if (prestigeBtn != null)
            prestigeBtn.clicked -= OnPrestigeClicked;

        if (optionBtn != null)
            optionBtn.clicked -= OnOptionClicked;
    }

    // Helper class for dimension UI elements
    private class DimensionUIElement
    {
        public Label title;
        public Label multiplier;
        public Label amount;
        public Label perSec;
        public Button buyBtn;
        public VisualElement progressBg;
        public VisualElement progressOwned;
        public VisualElement progressAffordable;
    }
}
