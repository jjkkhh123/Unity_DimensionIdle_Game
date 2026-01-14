using System;
using UnityEngine;
using UnityEngine.UIElements;

public class OfflinePanelController : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;

    // Time Bar
    private Label storedTimeLabel;
    private Label maxTimeLabel;
    private ProgressBar offlineTimeProgress;
    private Label effectiveTimeLabel;

    // Upgrades
    private Label maxTimeUpgradeDesc;
    private Label maxTimeUpgradeCost;
    private Button maxTimeUpgradeBtn;

    private Label efficiencyUpgradeDesc;
    private Label efficiencyUpgradeCost;
    private Button efficiencyUpgradeBtn;

    // Boost Controls
    private TextField multiplierInput;
    private Label boostDurationPreview;
    private Button startBoostBtn;
    private Button stopBoostBtn;
    private VisualElement boostActiveStatus;
    private Label boostActiveLabel;
    private Label boostRemainingLabel;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("[OfflinePanelController] UIDocument not found!");
            return;
        }

        root = uiDocument.rootVisualElement;
        CacheUIElements();
        RegisterCallbacks();
        UpdateOfflineDisplay();
    }

    void Update()
    {
        if (OfflineManager.Instance == null) return;

        UpdateOfflineDisplay();
    }

    void CacheUIElements()
    {
        var offlineRoot = root.Q<VisualElement>("offline-root");
        if (offlineRoot == null)
        {
            Debug.LogError("[OfflinePanelController] offline-root not found!");
            return;
        }

        // Time Bar
        storedTimeLabel = offlineRoot.Q<Label>("StoredTimeLabel");
        maxTimeLabel = offlineRoot.Q<Label>("MaxTimeLabel");
        offlineTimeProgress = offlineRoot.Q<ProgressBar>("OfflineTimeProgress");
        effectiveTimeLabel = offlineRoot.Q<Label>("EffectiveTimeLabel");

        // Upgrades
        maxTimeUpgradeDesc = offlineRoot.Q<Label>("MaxTimeUpgradeDesc");
        maxTimeUpgradeCost = offlineRoot.Q<Label>("MaxTimeUpgradeCost");
        maxTimeUpgradeBtn = offlineRoot.Q<Button>("MaxTimeUpgradeBtn");

        efficiencyUpgradeDesc = offlineRoot.Q<Label>("EfficiencyUpgradeDesc");
        efficiencyUpgradeCost = offlineRoot.Q<Label>("EfficiencyUpgradeCost");
        efficiencyUpgradeBtn = offlineRoot.Q<Button>("EfficiencyUpgradeBtn");

        // Boost Controls
        multiplierInput = offlineRoot.Q<TextField>("MultiplierInput");
        boostDurationPreview = offlineRoot.Q<Label>("BoostDurationPreview");
        startBoostBtn = offlineRoot.Q<Button>("StartBoostBtn");
        stopBoostBtn = offlineRoot.Q<Button>("StopBoostBtn");
        boostActiveStatus = offlineRoot.Q<VisualElement>("BoostActiveStatus");
        boostActiveLabel = offlineRoot.Q<Label>("BoostActiveLabel");
        boostRemainingLabel = offlineRoot.Q<Label>("BoostRemainingLabel");

        // Error checks
        if (storedTimeLabel == null) Debug.LogError("[OfflinePanelController] StoredTimeLabel not found!");
        if (maxTimeLabel == null) Debug.LogError("[OfflinePanelController] MaxTimeLabel not found!");
        if (offlineTimeProgress == null) Debug.LogError("[OfflinePanelController] OfflineTimeProgress not found!");
    }

    void RegisterCallbacks()
    {
        if (maxTimeUpgradeBtn != null)
        {
            maxTimeUpgradeBtn.clicked += OnMaxTimeUpgradeClicked;
        }

        if (efficiencyUpgradeBtn != null)
        {
            efficiencyUpgradeBtn.clicked += OnEfficiencyUpgradeClicked;
        }

        if (multiplierInput != null)
        {
            multiplierInput.RegisterValueChangedCallback(OnMultiplierChanged);
        }

        if (startBoostBtn != null)
        {
            startBoostBtn.clicked += OnStartBoostClicked;
        }

        if (stopBoostBtn != null)
        {
            stopBoostBtn.clicked += OnStopBoostClicked;
        }
    }

    void UpdateOfflineDisplay()
    {
        if (OfflineManager.Instance == null) return;

        // Update time bar
        UpdateTimeBar();

        // Update upgrades
        UpdateUpgrades();

        // Update boost controls
        UpdateBoostControls();
    }

    void UpdateTimeBar()
    {
        if (OfflineManager.Instance == null) return;

        double stored = OfflineManager.Instance.storedOfflineTime;
        double max = OfflineManager.Instance.maxOfflineTime;
        double efficiency = OfflineManager.Instance.efficiencyRatio;

        // Format time
        string storedStr = FormatTime(stored);
        string maxStr = FormatTime(max);

        if (storedTimeLabel != null)
        {
            storedTimeLabel.text = $"Stored: {storedStr}";
        }

        if (maxTimeLabel != null)
        {
            maxTimeLabel.text = $"Cap: {maxStr}";
        }

        if (offlineTimeProgress != null)
        {
            offlineTimeProgress.value = (float)((stored / max) * 100.0);
        }

        if (effectiveTimeLabel != null)
        {
            effectiveTimeLabel.text = $"Effective time at {efficiency * 100:F0}% efficiency";
        }
    }

    void UpdateUpgrades()
    {
        if (OfflineManager.Instance == null) return;

        // Max Time Upgrade
        int currentMaxHours = (int)(OfflineManager.Instance.maxOfflineTime / 3600);
        int nextMaxHours = currentMaxHours + 6;
        double maxTimeCost = OfflineManager.Instance.GetMaxTimeUpgradeCost();
        bool canUpgradeMaxTime = OfflineManager.Instance.CanUpgradeMaxTime();

        if (maxTimeUpgradeDesc != null)
        {
            maxTimeUpgradeDesc.text = $"{currentMaxHours}h → {nextMaxHours}h";
        }

        if (maxTimeUpgradeCost != null)
        {
            maxTimeUpgradeCost.text = FormatTime(maxTimeCost);
        }

        if (maxTimeUpgradeBtn != null)
        {
            maxTimeUpgradeBtn.SetEnabled(canUpgradeMaxTime);
        }

        // Efficiency Upgrade
        double currentEfficiency = OfflineManager.Instance.efficiencyRatio;
        double nextEfficiency = currentEfficiency + 0.05;
        double efficiencyCost = OfflineManager.Instance.GetEfficiencyUpgradeCost();
        bool canUpgradeEfficiency = OfflineManager.Instance.CanUpgradeEfficiency();
        int efficiencyLevel = OfflineManager.Instance.efficiencyUpgradeLevel;

        if (efficiencyUpgradeDesc != null)
        {
            if (efficiencyLevel >= 10)
            {
                efficiencyUpgradeDesc.text = $"{currentEfficiency * 100:F0}% (MAX)";
            }
            else
            {
                efficiencyUpgradeDesc.text = $"{currentEfficiency * 100:F0}% → {nextEfficiency * 100:F0}%";
            }
        }

        if (efficiencyUpgradeCost != null)
        {
            if (efficiencyLevel >= 10)
            {
                efficiencyUpgradeCost.text = "MAX";
            }
            else
            {
                efficiencyUpgradeCost.text = FormatTime(efficiencyCost);
            }
        }

        if (efficiencyUpgradeBtn != null)
        {
            efficiencyUpgradeBtn.SetEnabled(canUpgradeEfficiency);
        }
    }

    void UpdateBoostControls()
    {
        if (OfflineManager.Instance == null) return;

        bool isActive = OfflineManager.Instance.isBoostActive;

        // Update duration preview
        UpdateBoostPreview();

        // Show/hide buttons based on boost state
        if (startBoostBtn != null)
        {
            startBoostBtn.style.display = isActive ? DisplayStyle.None : DisplayStyle.Flex;
        }

        if (stopBoostBtn != null)
        {
            stopBoostBtn.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (boostActiveStatus != null)
        {
            boostActiveStatus.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // Update active status labels
        if (isActive)
        {
            double multiplier = OfflineManager.Instance.currentMultiplier;
            double remaining = OfflineManager.Instance.boostRemainingTime;

            if (boostActiveLabel != null)
            {
                boostActiveLabel.text = $"Boost x{multiplier:F0} ACTIVE";
            }

            if (boostRemainingLabel != null)
            {
                boostRemainingLabel.text = $"Time remaining: {FormatTime(remaining)}";
            }
        }
    }

    void UpdateBoostPreview()
    {
        if (OfflineManager.Instance == null || boostDurationPreview == null) return;

        if (multiplierInput == null || string.IsNullOrEmpty(multiplierInput.value))
        {
            boostDurationPreview.text = "Boost duration: -- min";
            return;
        }

        if (double.TryParse(multiplierInput.value, out double multiplier))
        {
            multiplier = Mathf.Clamp((float)multiplier, 1, 20);
            double duration = OfflineManager.Instance.CalculateBoostDuration(multiplier);
            string durationStr = FormatTime(duration);
            boostDurationPreview.text = $"Boost duration: {durationStr}";
        }
        else
        {
            boostDurationPreview.text = "Boost duration: -- min";
        }
    }

    void OnMaxTimeUpgradeClicked()
    {
        if (OfflineManager.Instance == null) return;
        OfflineManager.Instance.UpgradeMaxTime();
        UpdateOfflineDisplay();
    }

    void OnEfficiencyUpgradeClicked()
    {
        if (OfflineManager.Instance == null) return;
        OfflineManager.Instance.UpgradeEfficiency();
        UpdateOfflineDisplay();
    }

    void OnMultiplierChanged(ChangeEvent<string> evt)
    {
        UpdateBoostPreview();
    }

    void OnStartBoostClicked()
    {
        if (OfflineManager.Instance == null || multiplierInput == null) return;

        if (double.TryParse(multiplierInput.value, out double multiplier))
        {
            multiplier = Mathf.Clamp((float)multiplier, 1, 20);
            OfflineManager.Instance.StartBoost(multiplier);
            UpdateBoostControls();
        }
        else
        {
            Debug.LogWarning("[OfflinePanelController] Invalid multiplier input");
        }
    }

    void OnStopBoostClicked()
    {
        if (OfflineManager.Instance == null) return;
        OfflineManager.Instance.StopBoost();
        UpdateBoostControls();
    }

    string FormatTime(double seconds)
    {
        int hours = (int)(seconds / 3600);
        int minutes = (int)((seconds % 3600) / 60);
        int secs = (int)(seconds % 60);

        if (hours > 0)
        {
            return $"{hours:D2}:{minutes:D2}:{secs:D2}";
        }
        else
        {
            return $"{minutes:D2}:{secs:D2}";
        }
    }
}
