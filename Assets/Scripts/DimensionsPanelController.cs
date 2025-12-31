using UnityEngine;
using UnityEngine.UIElements;

public class DimensionsPanelController : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;

    // Antimatter Display
    private Label antimatterAmount;

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
        CacheUIElements();
        RegisterButtonCallbacks();
    }

    void CacheUIElements()
    {
        // Antimatter
        antimatterAmount = root.Q<Label>("AntimatterAmount");

        // Tickspeed
        tickspeedMultiplier = root.Q<Label>("TickspeedMultiplier");
        tickspeedCost = root.Q<Label>("TickspeedCost");
        tickspeedBuyBtn = root.Q<Button>("TickspeedBuyBtn");

        // DimBoost
        dimBoostCount = root.Q<Label>("DimBoostCount");
        dimBoostRequirement = root.Q<Label>("DimBoostRequirement");
        dimBoostBtn = root.Q<Button>("DimBoostBtn");

        // Dimensions (1-8)
        for (int i = 0; i < 8; i++)
        {
            int dimensionIndex = i + 1;
            dimensionElements[i] = new DimensionUIElement
            {
                title = root.Q<Label>($"Dimension{dimensionIndex}Title"),
                multiplier = root.Q<Label>($"Dimension{dimensionIndex}Multiplier"),
                amount = root.Q<Label>($"Dimension{dimensionIndex}Amount"),
                perSec = root.Q<Label>($"Dimension{dimensionIndex}PerSec"),
                buyBtn = root.Q<Button>($"Dimension{dimensionIndex}BuyBtn"),
                maxBtn = root.Q<Button>($"Dimension{dimensionIndex}MaxBtn")
            };
        }

        // Bottom Menu
        dimensionsBtn = root.Q<Button>("DimensionsBtn");
        prestigeBtn = root.Q<Button>("PrestigeBtn");
        optionBtn = root.Q<Button>("OptionBtn");
    }

    void RegisterButtonCallbacks()
    {
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

            if (elem.maxBtn != null)
            {
                elem.maxBtn.clicked += () => OnDimensionBuyMax(index);
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

        UpdateAntimatterDisplay();
        UpdateTickspeedPanel();
        UpdateDimBoostPanel();
        UpdateDimensions();
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

            if (ui.buyBtn == null || ui.maxBtn == null)
                continue;

            bool isLocked = !dim.unlocked;

            if (isLocked)
            {
                // Locked state
                ui.buyBtn.SetEnabled(false);
                ui.maxBtn.SetEnabled(false);
                ui.buyBtn.style.opacity = 0.4f;
                ui.maxBtn.style.opacity = 0.4f;

                if (ui.amount != null)
                    ui.amount.text = "Locked";

                if (ui.multiplier != null)
                    ui.multiplier.text = "";

                if (ui.perSec != null)
                    ui.perSec.text = "";

                ui.buyBtn.text = "Locked";
                ui.maxBtn.text = "Locked";
            }
            else
            {
                // Unlocked state
                ui.buyBtn.SetEnabled(true);
                ui.maxBtn.SetEnabled(true);

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

                // Update buy button
                BigDouble cost = dim.currentPrice;
                bool canAfford = GameManager.Instance.antimatter >= cost;

                ui.buyBtn.text = $"Buy 1\n{cost}";
                ui.buyBtn.style.opacity = canAfford ? 1.0f : 0.6f;

                // Update max button
                ui.maxBtn.text = "Buy Max";
                ui.maxBtn.style.opacity = canAfford ? 1.0f : 0.6f;
            }
        }
    }

    // Button Event Handlers
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

        if (dim.unlocked && GameManager.Instance.CanBuyDimension(tier, 1))
        {
            GameManager.Instance.BuyDimension(tier, 1);
        }
    }

    void OnDimensionBuyMax(int index)
    {
        if (GameManager.Instance == null)
            return;

        int tier = index + 1;
        Dimension dim = GameManager.Instance.dimensions[index];

        if (dim.unlocked)
        {
            GameManager.Instance.BuyMaxDimension(tier);
        }
    }

    void OnPrestigeClicked()
    {
        // TODO: Switch to Prestige tab
        // For now, you can integrate with TabManager or handle tab switching here
        Debug.Log("Prestige button clicked - Tab switching not yet implemented");
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

            if (elem.maxBtn != null)
                elem.maxBtn.clicked -= () => OnDimensionBuyMax(index);
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
        public Button maxBtn;
    }
}
