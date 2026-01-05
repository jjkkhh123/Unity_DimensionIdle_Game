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
            tickspeedMultiplier.text = $"x{multiplier:F2}";
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
                    double prestigeMultiplier = 1.0;
                    if (PrestigeManager.Instance != null)
                    {
                        prestigeMultiplier = PrestigeManager.Instance.GetDimensionMultiplier(dim.tier);
                    }

                    BigDouble totalMultiplier = new BigDouble(prestigeMultiplier) * dim.multiplier;
                    ui.multiplier.text = $"x{totalMultiplier.ToDouble():F2}";
                }

                // Update production per second
                if (ui.perSec != null)
                {
                    BigDouble production = dim.GetProduction();

                    // Apply tickspeed multiplier
                    if (TickSpeedManager.Instance != null)
                    {
                        double tickMultiplier = TickSpeedManager.Instance.GetTickspeedMultiplier();
                        production = production * new BigDouble(tickMultiplier);
                    }

                    ui.perSec.text = $"(+{production}/s)";
                }

                // Update buy button based on current buy mode
                BigDouble cost = dim.currentPrice;
                bool canAfford = GameManager.Instance.antimatter >= cost;

                if (currentBuyMode == BuyMode.BuyOne)
                {
                    ui.buyBtn.text = $"Buy 1\n{cost}";
                }
                else // UntilTen
                {
                    int buyAmount = CalculateBuyUntilTen(dim);
                    ui.buyBtn.text = buyAmount > 0 ? $"Buy {buyAmount}\n{cost}" : $"Until 10\n{cost}";
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

        // Calculate how many can afford until next 10
        int nextTen = ((currentBought / 10) + 1) * 10;
        int toBuy = nextTen - currentBought;
        int canAfford = 0;

        // Simulate purchases to see how many we can afford
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
            TickSpeedManager.Instance.BuyTickspeed();
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
            // Buy until next multiple of 10
            int amountToBuy = CalculateBuyUntilTen(dim);

            // Try to buy the calculated amount
            for (int i = 0; i < amountToBuy; i++)
            {
                if (GameManager.Instance.CanBuyDimension(tier, 1))
                {
                    GameManager.Instance.BuyDimension(tier, 1);
                }
                else
                {
                    break; // Stop if we can't afford more
                }
            }
        }
    }

    void OnPrestigeClicked()
    {
        if (dimensionsRoot != null && prestigeRoot != null)
        {
            dimensionsRoot.style.display = DisplayStyle.None;
            prestigeRoot.style.display = DisplayStyle.Flex;
            Debug.Log("[DimensionsPanelController] Switched to Prestige panel");
        }
        else
        {
            Debug.LogError("[DimensionsPanelController] Cannot switch panels - root elements not found");
        }
    }

    void OnOptionClicked()
    {
        // TODO: Switch to Options tab
        // For now, you can integrate with TabManager or handle tab switching here
        Debug.Log("Options button clicked - Tab switching not yet implemented");
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
