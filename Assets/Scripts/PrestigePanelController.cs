using UnityEngine;
using UnityEngine.UIElements;
using System;

public class PrestigePanelController : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement prestigeRoot;
    private TabManagerUIToolkit tabManager;

    // Header elements
    private Label prestigePointsAmount;
    private Label totalPrestigesCount;

    // Prestige action elements
    private Label prestigeRequirement;
    private Label prestigeGainPreview;
    private Button prestigeBtn;

    // Upgrade elements
    private UpgradeUIElement[] upgradeElements;

    // Subtab elements
    private Button upgradesTabBtn;
    private Button milestonesTabBtn;
    private VisualElement upgradesScroll;
    private VisualElement milestonesScroll;
    private bool showingMilestones = false;

    // Milestone UI elements
    private MilestoneUIElement[] milestoneElements;

    // Autobuyer UI elements
    private AutoBuyerUIElement[] autoBuyerElements;
    private Label speedUpgradeEffect;
    private Label speedUpgradeCost;
    private Button speedUpgradeBtn;

    // Bottom menu buttons
    private Button dimensionsBtn;
    private Button prestigeMenuBtn;
    private Button optionBtn;

    // Reference to DimensionsPanel root
    private VisualElement dimensionsRoot;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("[PrestigePanelController] UIDocument not found!");
            return;
        }

        root = uiDocument.rootVisualElement;

        // Get TabManager
        tabManager = GetComponent<TabManagerUIToolkit>();
        if (tabManager == null)
        {
            Debug.LogWarning("[PrestigePanelController] TabManagerUIToolkit not found!");
        }

        CacheUIElements();
        RegisterButtonCallbacks();
    }

    void CacheUIElements()
    {
        // Get panel roots
        prestigeRoot = root.Q<VisualElement>("prestige-root");
        dimensionsRoot = root.Q<VisualElement>("root");

        if (prestigeRoot == null)
        {
            Debug.LogError("[PrestigePanelController] prestige-root not found!");
            return;
        }

        // Header
        prestigePointsAmount = prestigeRoot.Q<Label>("PrestigePointsAmount");
        totalPrestigesCount = prestigeRoot.Q<Label>("TotalPrestigesCount");

        // Prestige action
        prestigeRequirement = prestigeRoot.Q<Label>("PrestigeRequirement");
        prestigeGainPreview = prestigeRoot.Q<Label>("PrestigeGainPreview");
        prestigeBtn = prestigeRoot.Q<Button>("PrestigeBtn");

        // Initialize upgrade elements array
        upgradeElements = new UpgradeUIElement[10];

        // Tickspeed upgrade
        upgradeElements[0] = new UpgradeUIElement
        {
            id = "tickspeed_boost",
            container = prestigeRoot.Q<VisualElement>("TickspeedUpgrade"),
            nameLabel = prestigeRoot.Q<Label>("TickspeedUpgradeName"),
            levelLabel = prestigeRoot.Q<Label>("TickspeedUpgradeLevel"),
            descriptionLabel = prestigeRoot.Q<Label>("TickspeedUpgradeDesc"),
            effectLabel = prestigeRoot.Q<Label>("TickspeedUpgradeEffect"),
            costLabel = prestigeRoot.Q<Label>("TickspeedUpgradeCost"),
            buyButton = prestigeRoot.Q<Button>("TickspeedUpgradeBuyBtn")
        };

        // Dimension 1-8 upgrades
        for (int i = 1; i <= 8; i++)
        {
            upgradeElements[i] = new UpgradeUIElement
            {
                id = $"dim{i}_mult",
                container = prestigeRoot.Q<VisualElement>($"Dim{i}Upgrade"),
                nameLabel = prestigeRoot.Q<Label>($"Dim{i}UpgradeName"),
                levelLabel = prestigeRoot.Q<Label>($"Dim{i}UpgradeLevel"),
                descriptionLabel = prestigeRoot.Q<Label>($"Dim{i}UpgradeDesc"),
                effectLabel = prestigeRoot.Q<Label>($"Dim{i}UpgradeEffect"),
                costLabel = prestigeRoot.Q<Label>($"Dim{i}UpgradeCost"),
                buyButton = prestigeRoot.Q<Button>($"Dim{i}UpgradeBuyBtn")
            };
        }

        // Bulk bonus upgrade
        upgradeElements[9] = new UpgradeUIElement
        {
            id = "bulk_bonus",
            container = prestigeRoot.Q<VisualElement>("BulkBonusUpgrade"),
            nameLabel = prestigeRoot.Q<Label>("BulkBonusUpgradeName"),
            levelLabel = prestigeRoot.Q<Label>("BulkBonusUpgradeLevel"),
            descriptionLabel = prestigeRoot.Q<Label>("BulkBonusUpgradeDesc"),
            effectLabel = prestigeRoot.Q<Label>("BulkBonusUpgradeEffect"),
            costLabel = prestigeRoot.Q<Label>("BulkBonusUpgradeCost"),
            buyButton = prestigeRoot.Q<Button>("BulkBonusUpgradeBuyBtn")
        };

        // Subtab buttons
        upgradesTabBtn = prestigeRoot.Q<Button>("UpgradesTabBtn");
        milestonesTabBtn = prestigeRoot.Q<Button>("MilestonesTabBtn");
        upgradesScroll = prestigeRoot.Q<VisualElement>("upgrades-scroll");
        milestonesScroll = prestigeRoot.Q<VisualElement>("milestones-scroll");

        // Milestone UI elements (4 milestones)
        milestoneElements = new MilestoneUIElement[4];
        for (int i = 0; i < 4; i++)
        {
            int index = i + 1;
            milestoneElements[i] = new MilestoneUIElement
            {
                container = prestigeRoot.Q<VisualElement>($"Milestone{index}"),
                requirementLabel = prestigeRoot.Q<Label>($"Milestone{index}Requirement"),
                statusLabel = prestigeRoot.Q<Label>($"Milestone{index}Status"),
                descLabel = prestigeRoot.Q<Label>($"Milestone{index}Desc"),
                progressBar = prestigeRoot.Q<ProgressBar>($"Milestone{index}Progress")
            };
        }

        // Autobuyer UI elements (2 autobuyers for now)
        autoBuyerElements = new AutoBuyerUIElement[2];
        for (int i = 0; i < 2; i++)
        {
            int index = i + 1;
            autoBuyerElements[i] = new AutoBuyerUIElement
            {
                container = prestigeRoot.Q<VisualElement>($"AutoBuyer{index}"),
                toggleBtn = prestigeRoot.Q<Button>($"AutoBuyer{index}Toggle"),
                modeBtn = prestigeRoot.Q<Button>($"AutoBuyer{index}Mode"),
                statusLabel = prestigeRoot.Q<Label>($"AutoBuyer{index}Status")
            };
        }

        // Speed upgrade
        speedUpgradeEffect = prestigeRoot.Q<Label>("SpeedUpgradeEffect");
        speedUpgradeCost = prestigeRoot.Q<Label>("SpeedUpgradeCost");
        speedUpgradeBtn = prestigeRoot.Q<Button>("SpeedUpgradeBtn");

        // Bottom menu
        dimensionsBtn = prestigeRoot.Q<Button>("DimensionsBtn");
        prestigeMenuBtn = prestigeRoot.Q<Button>("PrestigeMenuBtn");
        optionBtn = prestigeRoot.Q<Button>("OptionBtn");
    }

    void RegisterButtonCallbacks()
    {
        // Prestige button
        if (prestigeBtn != null)
        {
            prestigeBtn.clicked += OnPrestigeClicked;
        }

        // Upgrade buttons
        for (int i = 0; i < upgradeElements.Length; i++)
        {
            if (upgradeElements[i].buyButton != null)
            {
                int index = i; // Capture for closure
                upgradeElements[i].buyButton.clicked += () => OnUpgradeBuyClicked(upgradeElements[index].id);
            }
        }

        // Subtab buttons
        if (upgradesTabBtn != null)
        {
            upgradesTabBtn.clicked += () => ShowUpgradesTab();
        }

        if (milestonesTabBtn != null)
        {
            milestonesTabBtn.clicked += () => ShowMilestonesTab();
        }

        // Autobuyer buttons
        for (int i = 0; i < autoBuyerElements.Length; i++)
        {
            int index = i;
            if (autoBuyerElements[i].toggleBtn != null)
            {
                autoBuyerElements[i].toggleBtn.clicked += () => OnAutoBuyerToggle(index);
            }
            if (autoBuyerElements[i].modeBtn != null)
            {
                autoBuyerElements[i].modeBtn.clicked += () => OnAutoBuyerModeToggle(index);
            }
        }

        // Speed upgrade button
        if (speedUpgradeBtn != null)
        {
            speedUpgradeBtn.clicked += OnSpeedUpgradeClicked;
        }

        // Bottom menu
        if (dimensionsBtn != null)
        {
            dimensionsBtn.clicked += () => SwitchToPanel("dimensions");
        }

        if (prestigeMenuBtn != null)
        {
            prestigeMenuBtn.clicked += () => SwitchToPanel("prestige");
        }

        if (optionBtn != null)
        {
            optionBtn.clicked += () => SwitchToPanel("options");
        }
    }

    void Update()
    {
        // Only update if prestige panel is visible
        if (prestigeRoot != null && prestigeRoot.style.display == DisplayStyle.Flex)
        {
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (PrestigeManager.Instance == null)
            return;

        UpdateHeader();
        UpdatePrestigeAction();

        if (showingMilestones)
        {
            UpdateMilestones();
            UpdateAutoBuyers();
        }
        else
        {
            UpdateUpgrades();
        }
    }

    void UpdateHeader()
    {
        if (prestigePointsAmount != null)
        {
            prestigePointsAmount.text = PrestigeManager.Instance.prestigePoints.ToString();
        }

        if (totalPrestigesCount != null)
        {
            totalPrestigesCount.text = PrestigeManager.Instance.totalPrestiges.ToString();
        }
    }

    void UpdatePrestigeAction()
    {
        if (GameManager.Instance == null || PrestigeManager.Instance == null)
            return;

        // Update requirement text
        if (prestigeRequirement != null)
        {
            BigDouble currentAntimatter = GameManager.Instance.antimatter;
            BigDouble requirement = new BigDouble(1e10);

            if (currentAntimatter >= requirement)
            {
                prestigeRequirement.text = $"Requirement: Met ({currentAntimatter})";
                prestigeRequirement.style.color = new Color(46f/255f, 204f/255f, 113f/255f); // Green
            }
            else
            {
                prestigeRequirement.text = $"Requirement: {requirement} antimatter (Current: {currentAntimatter})";
                prestigeRequirement.style.color = new Color(255f/255f, 150f/255f, 130f/255f); // Bright Coral
            }
        }

        // Update gain preview
        if (prestigeGainPreview != null)
        {
            int gainAmount = PrestigeManager.Instance.CalculatePrestigePointsGained();
            prestigeGainPreview.text = $"You will gain: {gainAmount} PP";

            if (gainAmount > 0)
            {
                prestigeGainPreview.style.color = new Color(241f/255f, 196f/255f, 15f/255f); // Yellow
            }
            else
            {
                prestigeGainPreview.style.color = new Color(0.7f, 0.7f, 0.7f); // Gray
            }
        }

        // Update prestige button
        if (prestigeBtn != null)
        {
            bool canPrestige = PrestigeManager.Instance.CanPrestige();
            prestigeBtn.SetEnabled(canPrestige);
            prestigeBtn.style.opacity = canPrestige ? 1.0f : 0.5f;
        }
    }

    void UpdateUpgrades()
    {
        if (PrestigeManager.Instance == null)
            return;

        for (int i = 0; i < upgradeElements.Length; i++)
        {
            UpgradeUIElement elem = upgradeElements[i];
            if (elem.container == null || !PrestigeManager.Instance.upgrades.ContainsKey(elem.id))
                continue;

            PrestigeUpgrade upgrade = PrestigeManager.Instance.upgrades[elem.id];

            // Update level
            if (elem.levelLabel != null)
            {
                elem.levelLabel.text = $"Level: {upgrade.level}";
            }

            // Update effect
            if (elem.effectLabel != null)
            {
                string effectText = GetUpgradeEffectText(elem.id, upgrade);
                elem.effectLabel.text = effectText;
            }

            // Update cost
            if (elem.costLabel != null)
            {
                int cost = upgrade.GetNextCost();
                if (upgrade.level >= upgrade.maxLevel)
                {
                    elem.costLabel.text = "MAX LEVEL";
                    elem.costLabel.style.color = new Color(46f/255f, 204f/255f, 113f/255f); // Green
                }
                else
                {
                    elem.costLabel.text = $"Cost: {cost} PP";
                    elem.costLabel.style.color = Color.white;
                }
            }

            // Update buy button
            if (elem.buyButton != null)
            {
                bool canBuy = PrestigeManager.Instance.CanBuyUpgrade(elem.id);
                bool isMaxLevel = upgrade.level >= upgrade.maxLevel;

                elem.buyButton.SetEnabled(canBuy && !isMaxLevel);
                elem.buyButton.style.opacity = (canBuy && !isMaxLevel) ? 1.0f : 0.5f;

                if (isMaxLevel)
                {
                    elem.buyButton.text = "MAX";
                }
                else
                {
                    elem.buyButton.text = "BUY";
                }
            }
        }
    }

    string GetUpgradeEffectText(string upgradeId, PrestigeUpgrade upgrade)
    {
        switch (upgradeId)
        {
            case "tickspeed_boost":
                double tickBoost = upgrade.level * 0.01;
                return $"Effect: +{tickBoost:F2} tickspeed";

            case "dim1_mult":
            case "dim2_mult":
            case "dim3_mult":
            case "dim4_mult":
            case "dim5_mult":
            case "dim6_mult":
            case "dim7_mult":
            case "dim8_mult":
                double multiplier = Math.Pow(2.0, upgrade.level);
                return $"Effect: x{multiplier:F2}";

            case "bulk_bonus":
                double baseBonus = 2.0;
                double currentBonus = baseBonus + (upgrade.level * 0.05);
                double nextBonus = baseBonus + ((upgrade.level + 1) * 0.05);
                return $"Effect: {currentBonus:F2}x → {nextBonus:F2}x";

            default:
                return "Effect: Unknown";
        }
    }

    void OnPrestigeClicked()
    {
        if (PrestigeManager.Instance == null || !PrestigeManager.Instance.CanPrestige())
            return;

        // Show confirmation dialog (simple version for now)
        int pointsToGain = PrestigeManager.Instance.CalculatePrestigePointsGained();

        Debug.Log($"[Prestige] You will gain {pointsToGain} Prestige Points and reset all progress.");

        // TODO: Add proper confirmation dialog UI
        // For now, just execute prestige
        PrestigeManager.Instance.DoPrestige();

        Debug.Log($"[Prestige] Complete! Total PP: {PrestigeManager.Instance.prestigePoints}");
    }

    void OnUpgradeBuyClicked(string upgradeId)
    {
        if (PrestigeManager.Instance == null)
            return;

        if (PrestigeManager.Instance.CanBuyUpgrade(upgradeId))
        {
            PrestigeManager.Instance.BuyUpgrade(upgradeId);
            Debug.Log($"[Prestige] Purchased upgrade: {upgradeId}");
        }
        else
        {
            Debug.Log($"[Prestige] Cannot afford upgrade: {upgradeId}");
        }
    }

    // ===== Subtab Methods =====

    void ShowUpgradesTab()
    {
        showingMilestones = false;

        if (upgradesScroll != null)
            upgradesScroll.style.display = DisplayStyle.Flex;
        if (milestonesScroll != null)
            milestonesScroll.style.display = DisplayStyle.None;

        // Update tab button styles
        if (upgradesTabBtn != null)
            upgradesTabBtn.AddToClassList("prestige-subtab-btn-active");
        if (milestonesTabBtn != null)
            milestonesTabBtn.RemoveFromClassList("prestige-subtab-btn-active");
    }

    void ShowMilestonesTab()
    {
        showingMilestones = true;

        if (upgradesScroll != null)
            upgradesScroll.style.display = DisplayStyle.None;
        if (milestonesScroll != null)
            milestonesScroll.style.display = DisplayStyle.Flex;

        // Update tab button styles
        if (upgradesTabBtn != null)
            upgradesTabBtn.RemoveFromClassList("prestige-subtab-btn-active");
        if (milestonesTabBtn != null)
            milestonesTabBtn.AddToClassList("prestige-subtab-btn-active");
    }

    // ===== Milestone Methods =====

    void UpdateMilestones()
    {
        if (PrestigeManager.Instance == null || milestoneElements == null)
            return;

        int totalPrestiges = PrestigeManager.Instance.totalPrestiges;

        for (int i = 0; i < milestoneElements.Length && i < PrestigeManager.Instance.milestones.Count; i++)
        {
            MilestoneUIElement ui = milestoneElements[i];
            Milestone milestone = PrestigeManager.Instance.milestones[i];

            if (ui.container == null)
                continue;

            bool isUnlocked = milestone.isUnlocked;

            // Update status
            if (ui.statusLabel != null)
            {
                ui.statusLabel.text = isUnlocked ? "UNLOCKED" : "LOCKED";
                if (isUnlocked)
                    ui.statusLabel.AddToClassList("milestone-status-unlocked");
                else
                    ui.statusLabel.RemoveFromClassList("milestone-status-unlocked");
            }

            // Update card style
            if (isUnlocked)
            {
                ui.container.RemoveFromClassList("milestone-card-locked");
                ui.container.AddToClassList("milestone-card-unlocked");
            }
            else
            {
                ui.container.AddToClassList("milestone-card-locked");
                ui.container.RemoveFromClassList("milestone-card-unlocked");
            }

            // Update progress bar
            if (ui.progressBar != null)
            {
                float progress = isUnlocked ? 100f : (totalPrestiges / (float)milestone.requiredPrestiges * 100f);
                ui.progressBar.value = Mathf.Min(progress, 100f);
            }
        }
    }

    // ===== Autobuyer Methods =====

    void UpdateAutoBuyers()
    {
        if (AutoBuyerManager.Instance == null || autoBuyerElements == null)
            return;

        for (int i = 0; i < autoBuyerElements.Length; i++)
        {
            AutoBuyerUIElement ui = autoBuyerElements[i];
            if (ui.container == null)
                continue;

            bool isUnlocked = AutoBuyerManager.Instance.dimensionAutoBuyersUnlocked[i];
            bool isEnabled = AutoBuyerManager.Instance.dimensionAutoBuyersEnabled[i];
            AutoBuyerManager.BuyMode mode = AutoBuyerManager.Instance.dimensionBuyModes[i];

            // Update locked status
            if (isUnlocked)
            {
                ui.container.RemoveFromClassList("autobuyer-locked");
                if (ui.statusLabel != null)
                    ui.statusLabel.text = "";
            }
            else
            {
                ui.container.AddToClassList("autobuyer-locked");
                if (ui.statusLabel != null)
                    ui.statusLabel.text = "(Locked)";
            }

            // Update toggle button
            if (ui.toggleBtn != null)
            {
                ui.toggleBtn.text = isEnabled ? "ON" : "OFF";
                ui.toggleBtn.SetEnabled(isUnlocked);
                if (isEnabled)
                    ui.toggleBtn.AddToClassList("autobuyer-toggle-on");
                else
                    ui.toggleBtn.RemoveFromClassList("autobuyer-toggle-on");
            }

            // Update mode button
            if (ui.modeBtn != null)
            {
                ui.modeBtn.text = mode == AutoBuyerManager.BuyMode.Single ? "Single" : "Bulk";
                ui.modeBtn.SetEnabled(isUnlocked);
            }
        }

        // Update speed upgrade
        if (speedUpgradeEffect != null)
        {
            float currentInterval = AutoBuyerManager.Instance.GetAutoBuyInterval();
            float nextInterval = AutoBuyerManager.Instance.GetNextInterval();
            int level = AutoBuyerManager.Instance.speedUpgradeLevel;

            if (level >= AutoBuyerManager.MAX_SPEED_LEVEL)
            {
                speedUpgradeEffect.text = $"Interval: {currentInterval:F1}s (MAX)";
            }
            else
            {
                speedUpgradeEffect.text = $"Interval: {currentInterval:F1}s → {nextInterval:F1}s";
            }
        }

        if (speedUpgradeCost != null)
        {
            int level = AutoBuyerManager.Instance.speedUpgradeLevel;
            if (level >= AutoBuyerManager.MAX_SPEED_LEVEL)
            {
                speedUpgradeCost.text = "MAX LEVEL";
            }
            else
            {
                int cost = AutoBuyerManager.Instance.GetSpeedUpgradeCost();
                speedUpgradeCost.text = $"Cost: {cost} PP";
            }
        }

        if (speedUpgradeBtn != null)
        {
            bool canUpgrade = AutoBuyerManager.Instance.CanUpgradeSpeed();
            speedUpgradeBtn.SetEnabled(canUpgrade);
            speedUpgradeBtn.style.opacity = canUpgrade ? 1.0f : 0.5f;

            if (AutoBuyerManager.Instance.speedUpgradeLevel >= AutoBuyerManager.MAX_SPEED_LEVEL)
            {
                speedUpgradeBtn.text = "MAX";
            }
            else
            {
                speedUpgradeBtn.text = "UPGRADE";
            }
        }
    }

    void OnAutoBuyerToggle(int index)
    {
        if (AutoBuyerManager.Instance == null)
            return;

        AutoBuyerManager.Instance.ToggleAutoBuyer(index);
    }

    void OnAutoBuyerModeToggle(int index)
    {
        if (AutoBuyerManager.Instance == null)
            return;

        // Toggle between Single and Bulk
        AutoBuyerManager.BuyMode currentMode = AutoBuyerManager.Instance.dimensionBuyModes[index];
        AutoBuyerManager.BuyMode newMode = currentMode == AutoBuyerManager.BuyMode.Single
            ? AutoBuyerManager.BuyMode.Bulk
            : AutoBuyerManager.BuyMode.Single;

        AutoBuyerManager.Instance.SetBuyMode(index, newMode);
    }

    void OnSpeedUpgradeClicked()
    {
        if (AutoBuyerManager.Instance == null)
            return;

        AutoBuyerManager.Instance.UpgradeSpeed();
    }

    void SwitchToPanel(string panelName)
    {
        if (tabManager != null)
        {
            // Capitalize first letter for TabManager
            string capitalizedName = char.ToUpper(panelName[0]) + panelName.Substring(1);
            tabManager.ShowPanel(capitalizedName);
        }
        else
        {
            Debug.LogError("[PrestigePanelController] TabManager not found - cannot switch panels");
        }
    }

    void OnDestroy()
    {
        // Unregister callbacks
        if (prestigeBtn != null)
            prestigeBtn.clicked -= OnPrestigeClicked;

        for (int i = 0; i < upgradeElements.Length; i++)
        {
            if (upgradeElements[i].buyButton != null)
            {
                string id = upgradeElements[i].id;
                upgradeElements[i].buyButton.clicked -= () => OnUpgradeBuyClicked(id);
            }
        }

        if (dimensionsBtn != null)
            dimensionsBtn.clicked -= () => SwitchToPanel("dimensions");

        if (prestigeMenuBtn != null)
            prestigeMenuBtn.clicked -= () => SwitchToPanel("prestige");

        if (optionBtn != null)
            optionBtn.clicked -= () => SwitchToPanel("options");
    }

    // Helper class for upgrade UI elements
    private class UpgradeUIElement
    {
        public string id;
        public VisualElement container;
        public Label nameLabel;
        public Label levelLabel;
        public Label descriptionLabel;
        public Label effectLabel;
        public Label costLabel;
        public Button buyButton;
    }

    // Helper class for milestone UI elements
    private class MilestoneUIElement
    {
        public VisualElement container;
        public Label requirementLabel;
        public Label statusLabel;
        public Label descLabel;
        public ProgressBar progressBar;
    }

    // Helper class for autobuyer UI elements
    private class AutoBuyerUIElement
    {
        public VisualElement container;
        public Button toggleBtn;
        public Button modeBtn;
        public Label statusLabel;
    }
}
