using UnityEngine;
using UnityEngine.UIElements;
using System;

public class PrestigePanelController : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement prestigeRoot;

    // Header elements
    private Label prestigePointsAmount;
    private Label totalPrestigesCount;

    // Prestige action elements
    private Label prestigeRequirement;
    private Label prestigeGainPreview;
    private Button prestigeBtn;

    // Upgrade elements
    private UpgradeUIElement[] upgradeElements;

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
        UpdateUpgrades();
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
                prestigeRequirement.style.color = new Color(231f/255f, 76f/255f, 60f/255f); // Red
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

    void SwitchToPanel(string panelName)
    {
        if (panelName == "dimensions")
        {
            if (prestigeRoot != null)
                prestigeRoot.style.display = DisplayStyle.None;
            if (dimensionsRoot != null)
                dimensionsRoot.style.display = DisplayStyle.Flex;
        }
        else if (panelName == "prestige")
        {
            if (dimensionsRoot != null)
                dimensionsRoot.style.display = DisplayStyle.None;
            if (prestigeRoot != null)
                prestigeRoot.style.display = DisplayStyle.Flex;
        }
        else if (panelName == "options")
        {
            // TODO: Implement options panel
            Debug.Log("Options panel not yet implemented");
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
}
