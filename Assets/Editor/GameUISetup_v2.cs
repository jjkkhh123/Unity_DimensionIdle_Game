using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class GameUISetup_v2 : EditorWindow
{
    [MenuItem("Tools/Setup Game UI with Tabs")]
    static void SetupUIWithTabs()
    {
        if (EditorUtility.DisplayDialog("Setup Game UI with Tabs",
            "This will create UI with Dimensions and Options tabs. Continue?",
            "Yes", "Cancel"))
        {
            CreateGameUIWithTabs();
        }
    }

    static void CreateGameUIWithTabs()
    {
        // Canvas 찾기 또는 생성
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem 생성
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        // Canvas Scaler 설정
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Content Area 생성 (상단~하단 메뉴바 위까지)
        GameObject contentArea = new GameObject("ContentArea");
        RectTransform contentRT = contentArea.AddComponent<RectTransform>();
        contentRT.SetParent(canvas.transform, false);
        contentRT.anchorMin = new Vector2(0, 0.08f);
        contentRT.anchorMax = Vector2.one;
        contentRT.sizeDelta = Vector2.zero;

        // Dimensions Panel 생성
        GameObject dimensionsPanel = CreateDimensionsPanel(contentArea.transform);

        // Options Panel 생성
        GameObject optionsPanel = CreateOptionsPanel(contentArea.transform);

        // Prestige Panel 생성
        GameObject prestigePanel = CreatePrestigeShopPanel(contentArea.transform);

        // 하단 TabBar 생성
        (Button dimensionsBtn, Button prestigeBtn, Button optionsBtn) = CreateTabBar(canvas.transform);

        // Infinity Panel 생성 (전체 화면 덮는 패널)
        (GameObject infinityPanel, TextMeshProUGUI infinityText) = CreateInfinityPanel(canvas.transform);

        // TabManager 생성
        GameObject tabManagerObj = new GameObject("TabManager");
        tabManagerObj.transform.SetParent(canvas.transform, false);
        TabManager tabManager = tabManagerObj.AddComponent<TabManager>();
        tabManager.dimensionsPanel = dimensionsPanel;
        tabManager.prestigePanel = prestigePanel;
        tabManager.optionsPanel = optionsPanel;
        tabManager.dimensionsButton = dimensionsBtn;
        tabManager.prestigeButton = prestigeBtn;
        tabManager.optionsButton = optionsBtn;

        // UIManager 찾기 또는 생성 및 연결
        SetupUIManager(canvas, dimensionsPanel, infinityPanel, infinityText);

        // OptionsManager 추가
        OptionsManager optionsManager = optionsPanel.AddComponent<OptionsManager>();
        ConnectOptionsManager(optionsManager, optionsPanel);

        // PrestigeShopUI 추가
        PrestigeShopUI prestigeShopUI = prestigePanel.AddComponent<PrestigeShopUI>();
        ConnectPrestigeShopUI(prestigeShopUI, prestigePanel);

        Debug.Log("Game UI with Tabs Setup Complete!");
    }

    static GameObject CreateDimensionsPanel(Transform parent)
    {
        GameObject panel = new GameObject("DimensionsPanel");
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // MainPanel (안티매터 표시)
        GameObject mainPanel = new GameObject("MainPanel");
        RectTransform mainRT = mainPanel.AddComponent<RectTransform>();
        mainRT.SetParent(panel.transform, false);

        // AntimatterText
        CreateCenteredText(mainPanel.transform, "AntimatterText", "Antimatter: 10", 50, new Vector2(600, 420), new Vector2(1000, 70));

        // AntimatterPerSecondText
        CreateCenteredText(mainPanel.transform, "AntimatterPerSecondText", "/sec: 0", 38, new Vector2(600, 300), new Vector2(800, 50));

        // Dimension Buttons (8개)
        for (int i = 1; i <= 8; i++)
        {
            CreateDimensionButton(panel.transform, i);
        }

        // Prestige Panel
        CreatePrestigePanel(panel.transform);

        // Tickspeed Panel
        CreateTickspeedPanel(panel.transform);

        return panel;
    }

    static GameObject CreatePrestigeShopPanel(Transform parent)
    {
        GameObject panel = new GameObject("PrestigePanel");
        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.SetParent(parent, false);
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.sizeDelta = Vector2.zero;

        // Title
        GameObject titleObj = new GameObject("Title");
        RectTransform titleRT = titleObj.AddComponent<RectTransform>();
        titleRT.SetParent(panel.transform, false);
        titleRT.anchorMin = new Vector2(0.5f, 1);
        titleRT.anchorMax = new Vector2(0.5f, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.anchoredPosition = new Vector2(0, -20);
        titleRT.sizeDelta = new Vector2(800, 60);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "PRESTIGE UPGRADES";
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = TMPro.FontStyles.Bold;
        titleText.color = new Color(1f, 0.8f, 0.3f, 1f);

        // Points Display
        GameObject pointsObj = new GameObject("PointsText");
        RectTransform pointsRT = pointsObj.AddComponent<RectTransform>();
        pointsRT.SetParent(panel.transform, false);
        pointsRT.anchorMin = new Vector2(0.5f, 1);
        pointsRT.anchorMax = new Vector2(0.5f, 1);
        pointsRT.pivot = new Vector2(0.5f, 1);
        pointsRT.anchoredPosition = new Vector2(0, -90);
        pointsRT.sizeDelta = new Vector2(600, 50);
        TextMeshProUGUI pointsText = pointsObj.AddComponent<TextMeshProUGUI>();
        pointsText.text = "Prestige Points: 0";
        pointsText.fontSize = 28;
        pointsText.alignment = TextAlignmentOptions.Center;
        pointsText.color = new Color(0.3f, 1f, 0.3f, 1f);

        // Upgrade Container (2열 5행 그리드)
        GameObject upgradeContainer = new GameObject("UpgradeContainer");
        RectTransform containerRT = upgradeContainer.AddComponent<RectTransform>();
        containerRT.SetParent(panel.transform, false);
        containerRT.anchorMin = new Vector2(0.5f, 0.5f);
        containerRT.anchorMax = new Vector2(0.5f, 0.5f);
        containerRT.pivot = new Vector2(0.5f, 0.5f);
        containerRT.anchoredPosition = new Vector2(0, -50);
        containerRT.sizeDelta = new Vector2(1800, 900);

        panel.SetActive(false);
        return panel;
    }

    static GameObject CreateOptionsPanel(Transform parent)
    {
        GameObject panel = new GameObject("OptionsPanel");
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // Title
        CreateCenteredText(panel.transform, "OptionsTitle", "OPTIONS", 48, new Vector2(0, 400), new Vector2(600, 80));

        // Save/Load Section
        CreateButton(panel.transform, "ExportButton", "Export Save to Desktop", new Vector2(0, 100), new Vector2(400, 50));
        CreateButton(panel.transform, "ImportButton", "Import Save from Desktop", new Vector2(0, 30), new Vector2(400, 50));
        CreateButton(panel.transform, "ResetButton", "Reset Game", new Vector2(0, -40), new Vector2(400, 50));

        // Status Text
        CreateCenteredText(panel.transform, "StatusText", "", 20, new Vector2(0, -120), new Vector2(600, 100));

        panel.SetActive(false);

        return panel;
    }

    static (Button, Button, Button) CreateTabBar(Transform parent)
    {
        GameObject tabBar = new GameObject("TabBar");
        RectTransform tabBarRT = tabBar.AddComponent<RectTransform>();
        tabBarRT.SetParent(parent, false);
        tabBarRT.anchorMin = new Vector2(0, 0);
        tabBarRT.anchorMax = new Vector2(1, 0.08f);
        tabBarRT.sizeDelta = Vector2.zero;
        Image tabBarBg = tabBar.AddComponent<Image>();
        tabBarBg.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        Button dimensionsBtn = CreateTabButton(tabBar.transform, "DimensionsButton", "DIMENSIONS", -300);
        Button prestigeBtn = CreateTabButton(tabBar.transform, "PrestigeButton", "PRESTIGE", 0);
        Button optionsBtn = CreateTabButton(tabBar.transform, "OptionsButton", "OPTIONS", 300);

        return (dimensionsBtn, prestigeBtn, optionsBtn);
    }

    static Button CreateTabButton(Transform parent, string name, string text, float xOffset)
    {
        GameObject btnObj = new GameObject(name);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(xOffset, 0);
        rt.sizeDelta = new Vector2(300, 60);

        Image image = btnObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Button button = btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(btnObj.transform, false);
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return button;
    }

    static void CreateDimensionButton(Transform parent, int tier)
    {
        int column = (tier - 1) / 4;  // 0 또는 1
        int row = (tier - 1) % 4;     // 0, 1, 2, 3
        float xPos = column == 0 ? -600 : 0;
        float yPos = 350 - (row * 225);

        GameObject buttonObj = new GameObject($"Dimension{tier}Panel");
        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(550, 200);
        rt.anchoredPosition = new Vector2(xPos, yPos);

        Image bgImage = buttonObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.3f, 1f);

        // Texts (왼쪽 정렬)
        CreateDimensionText(buttonObj.transform, "NameText", $"Dimension {tier}", 32, new Vector2(10, 75), new Vector2(240, 30), TextAlignmentOptions.Left);
        CreateDimensionText(buttonObj.transform, "AmountText", "Bought: 0 | Owned: 0", 24, new Vector2(10, 35), new Vector2(240, 30), TextAlignmentOptions.Left);
        string prodText = tier == 1 ? "Antimatter/s: 0" : $"Dim {tier - 1}/s: 0";
        CreateDimensionText(buttonObj.transform, "ProductionText", prodText, 24, new Vector2(10, 5), new Vector2(240, 30), TextAlignmentOptions.Left);
        CreateDimensionText(buttonObj.transform, "PriceText", "Price: 0", 24, new Vector2(10, -25), new Vector2(240, 30), TextAlignmentOptions.Left);

        // MultiplierText (오른쪽 정렬)
        CreateDimensionText(buttonObj.transform, "MultiplierText", "x1.00", 24, new Vector2(-10, 70), new Vector2(100, 30), TextAlignmentOptions.Right, true);

        // Buttons (오른쪽 하단)
        CreateDimensionButton_Buy(buttonObj.transform, "BuyButton", "Buy x1", new Vector2(-10, 10));
        Image[] progressBars = CreateDimensionButton_BuyMax(buttonObj.transform, "BuyMaxButton", "Until x10", new Vector2(-10, 65));

        // Locked Panel
        CreateLockedPanel(buttonObj.transform);

        // DimensionButton Component
        DimensionButton dimButton = buttonObj.AddComponent<DimensionButton>();
        dimButton.dimensionTier = tier;
        dimButton.buyButton = buttonObj.transform.Find("BuyButton").GetComponent<Button>();
        dimButton.buyMaxButton = buttonObj.transform.Find("BuyMaxButton").GetComponent<Button>();
        dimButton.dimensionNameText = buttonObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        dimButton.amountText = buttonObj.transform.Find("AmountText").GetComponent<TextMeshProUGUI>();
        dimButton.multiplierText = buttonObj.transform.Find("MultiplierText").GetComponent<TextMeshProUGUI>();
        dimButton.productionText = buttonObj.transform.Find("ProductionText").GetComponent<TextMeshProUGUI>();
        dimButton.priceText = buttonObj.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();
        dimButton.lockedPanel = buttonObj.transform.Find("LockedPanel").gameObject;
        dimButton.unlockRequirementText = buttonObj.transform.Find("LockedPanel/UnlockRequirementText").GetComponent<TextMeshProUGUI>();
        dimButton.buyMaxProgressBars = progressBars;

        EditorUtility.SetDirty(dimButton);
    }

    static void CreatePrestigePanel(Transform parent)
    {
        // Prestige Container
        GameObject containerObj = new GameObject("PrestigeContainer");
        RectTransform containerRT = containerObj.AddComponent<RectTransform>();
        containerRT.SetParent(parent, false);
        containerRT.anchorMin = new Vector2(0.5f, 0.5f);
        containerRT.anchorMax = new Vector2(0.5f, 0.5f);
        containerRT.pivot = new Vector2(0.5f, 0.5f);
        containerRT.anchoredPosition = new Vector2(775, -300);
        containerRT.sizeDelta = new Vector2(300, 300);

        Image containerBg = containerObj.AddComponent<Image>();
        containerBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        // Title
        GameObject titleObj = new GameObject("Title");
        RectTransform titleRT = titleObj.AddComponent<RectTransform>();
        titleRT.SetParent(containerObj.transform, false);
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.anchoredPosition = new Vector2(0, -10);
        titleRT.sizeDelta = new Vector2(-20, 30);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "PRESTIGE";
        titleText.fontSize = 24;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = TMPro.FontStyles.Bold;
        titleText.color = new Color(1f, 0.7f, 0.7f, 1f);

        // Prestige Button
        GameObject btnObj = new GameObject("PrestigeButton");
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.SetParent(containerObj.transform, false);
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -50);
        rt.sizeDelta = new Vector2(280, 50);

        Image image = btnObj.AddComponent<Image>();
        image.color = new Color(0.8f, 0.3f, 0.3f, 1f);
        Button button = btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(btnObj.transform, false);
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Prestige (Locked)";
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        // Info Text
        GameObject infoObj = new GameObject("PrestigeInfoText");
        RectTransform infoRT = infoObj.AddComponent<RectTransform>();
        infoRT.SetParent(containerObj.transform, false);
        infoRT.anchorMin = new Vector2(0, 0);
        infoRT.anchorMax = new Vector2(1, 0);
        infoRT.pivot = new Vector2(0.5f, 0);
        infoRT.anchoredPosition = new Vector2(0, 10);
        infoRT.sizeDelta = new Vector2(-20, 160);

        TextMeshProUGUI infoText = infoObj.AddComponent<TextMeshProUGUI>();
        infoText.text = "Prestige Points: 0\nTotal Prestiges: 0";
        infoText.fontSize = 16;
        infoText.alignment = TextAlignmentOptions.Top;
        infoText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
    }

    static void CreateTickspeedPanel(Transform parent)
    {
        // Tickspeed Container
        GameObject containerObj = new GameObject("TickspeedContainer");
        RectTransform containerRT = containerObj.AddComponent<RectTransform>();
        containerRT.SetParent(parent, false);
        containerRT.anchorMin = new Vector2(0.5f, 0.5f);
        containerRT.anchorMax = new Vector2(0.5f, 0.5f);
        containerRT.pivot = new Vector2(0.5f, 0.5f);
        containerRT.anchoredPosition = new Vector2(450, -300);
        containerRT.sizeDelta = new Vector2(300, 300);

        Image containerBg = containerObj.AddComponent<Image>();
        containerBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        // Title
        GameObject titleObj = new GameObject("Title");
        RectTransform titleRT = titleObj.AddComponent<RectTransform>();
        titleRT.SetParent(containerObj.transform, false);
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.anchoredPosition = new Vector2(0, -10);
        titleRT.sizeDelta = new Vector2(-20, 30);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "TICKSPEED";
        titleText.fontSize = 24;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = TMPro.FontStyles.Bold;
        titleText.color = new Color(0.7f, 1f, 0.7f, 1f);

        // Tickspeed Button
        GameObject btnObj = new GameObject("TickspeedButton");
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.SetParent(containerObj.transform, false);
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -50);
        rt.sizeDelta = new Vector2(280, 50);

        Image image = btnObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.7f, 0.3f, 1f);
        Button button = btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(btnObj.transform, false);
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Upgrade Tickspeed\nCost: 10";
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        // Info Text
        GameObject infoObj = new GameObject("TickspeedInfoText");
        RectTransform infoRT = infoObj.AddComponent<RectTransform>();
        infoRT.SetParent(containerObj.transform, false);
        infoRT.anchorMin = new Vector2(0, 0);
        infoRT.anchorMax = new Vector2(1, 0);
        infoRT.pivot = new Vector2(0.5f, 0);
        infoRT.anchoredPosition = new Vector2(0, 10);
        infoRT.sizeDelta = new Vector2(-20, 120);

        TextMeshProUGUI infoText = infoObj.AddComponent<TextMeshProUGUI>();
        infoText.text = "Tickspeed Level: 0\nCurrent Multiplier: x1.00";
        infoText.fontSize = 16;
        infoText.alignment = TextAlignmentOptions.Top;
        infoText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
    }

    static (GameObject, TextMeshProUGUI) CreateInfinityPanel(Transform parent)
    {
        GameObject infinityPanel = new GameObject("InfinityPanel");
        RectTransform rt = infinityPanel.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Image image = infinityPanel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.9f);

        GameObject textObj = new GameObject("InfinityText");
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(infinityPanel.transform, false);
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "INFINITY REACHED!\n\nCongratulations!\n\nMore content coming soon...";
        tmp.fontSize = 72;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.84f, 0f, 1f);

        infinityPanel.SetActive(false);

        return (infinityPanel, tmp);
    }

    static void SetupUIManager(Canvas canvas, GameObject dimensionsPanel, GameObject infinityPanel, TextMeshProUGUI infinityText)
    {
        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager == null)
        {
            GameObject uiManagerObj = new GameObject("UIManager");
            uiManagerObj.transform.SetParent(canvas.transform, false);
            uiManager = uiManagerObj.AddComponent<UIManager>();
        }

        GameObject mainPanel = dimensionsPanel.transform.Find("MainPanel").gameObject;
        uiManager.antimatterText = mainPanel.transform.Find("AntimatterText").GetComponent<TextMeshProUGUI>();
        uiManager.antimatterPerSecondText = mainPanel.transform.Find("AntimatterPerSecondText").GetComponent<TextMeshProUGUI>();

        DimensionButton[] buttons = dimensionsPanel.GetComponentsInChildren<DimensionButton>();
        uiManager.dimensionButtons = buttons;

        uiManager.prestigeButton = dimensionsPanel.transform.Find("PrestigeContainer/PrestigeButton").GetComponent<Button>();
        uiManager.prestigeButtonText = dimensionsPanel.transform.Find("PrestigeContainer/PrestigeButton/Text").GetComponent<TextMeshProUGUI>();
        uiManager.prestigeInfoText = dimensionsPanel.transform.Find("PrestigeContainer/PrestigeInfoText").GetComponent<TextMeshProUGUI>();

        uiManager.tickspeedButton = dimensionsPanel.transform.Find("TickspeedContainer/TickspeedButton").GetComponent<Button>();
        uiManager.tickspeedButtonText = dimensionsPanel.transform.Find("TickspeedContainer/TickspeedButton/Text").GetComponent<TextMeshProUGUI>();
        uiManager.tickspeedInfoText = dimensionsPanel.transform.Find("TickspeedContainer/TickspeedInfoText").GetComponent<TextMeshProUGUI>();

        uiManager.infinityPanel = infinityPanel;
        uiManager.infinityText = infinityText;

        EditorUtility.SetDirty(uiManager);
    }

    static void ConnectOptionsManager(OptionsManager optionsManager, GameObject optionsPanel)
    {
        optionsManager.exportButton = optionsPanel.transform.Find("ExportButton").GetComponent<Button>();
        optionsManager.importButton = optionsPanel.transform.Find("ImportButton").GetComponent<Button>();
        optionsManager.resetButton = optionsPanel.transform.Find("ResetButton").GetComponent<Button>();
        optionsManager.statusText = optionsPanel.transform.Find("StatusText").GetComponent<TextMeshProUGUI>();

        EditorUtility.SetDirty(optionsManager);
    }

    static void ConnectPrestigeShopUI(PrestigeShopUI prestigeShopUI, GameObject prestigePanel)
    {
        prestigeShopUI.pointsText = prestigePanel.transform.Find("PointsText").GetComponent<TextMeshProUGUI>();
        prestigeShopUI.upgradeContainer = prestigePanel.transform.Find("UpgradeContainer");

        EditorUtility.SetDirty(prestigeShopUI);
    }

    // Helper methods
    static void CreateCenteredText(Transform parent, string name, string text, float fontSize, Vector2 position, Vector2 size)
    {
        GameObject textObj = new GameObject(name);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    static void CreateLeftAlignedText(Transform parent, string name, string text, float fontSize, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = new Vector2(10, position.y);
        rt.sizeDelta = new Vector2(240, 30);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = Color.white;
    }

    static void CreateRightAlignedText(Transform parent, string name, string text, float fontSize, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot = new Vector2(1, 0.5f);
        rt.anchoredPosition = new Vector2(-10, position.y);
        rt.sizeDelta = new Vector2(100, 30);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.color = new Color(1f, 0.8f, 0f, 1f);
    }

    static void CreateSmallButton(Transform parent, string name, string text, Vector2 position)
    {
        GameObject btnObj = new GameObject(name);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(120, 30);

        Image image = btnObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.5f, 0.8f, 1f);
        Button button = btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(btnObj.transform, false);
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 16;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    static Image[] CreateProgressButton(Transform parent, string name, string text, Vector2 position)
    {
        GameObject btnObj = new GameObject(name);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(120, 30);

        Image image = btnObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.5f, 0.8f, 1f);
        Button button = btnObj.AddComponent<Button>();

        // Progress bars container
        GameObject progressContainer = new GameObject("ProgressBars");
        RectTransform progressRT = progressContainer.AddComponent<RectTransform>();
        progressRT.SetParent(btnObj.transform, false);
        progressRT.anchorMin = new Vector2(0, 0);
        progressRT.anchorMax = new Vector2(1, 0);
        progressRT.pivot = new Vector2(0.5f, 0);
        progressRT.anchoredPosition = new Vector2(0, 2);
        progressRT.sizeDelta = new Vector2(-10, 4);

        Image[] progressBars = new Image[10];
        float barWidth = 10f;
        float spacing = 1f;

        for (int i = 0; i < 10; i++)
        {
            GameObject barObj = new GameObject($"Bar{i}");
            RectTransform barRT = barObj.AddComponent<RectTransform>();
            barRT.SetParent(progressContainer.transform, false);
            barRT.anchorMin = new Vector2(0, 0);
            barRT.anchorMax = new Vector2(0, 1);
            barRT.pivot = new Vector2(0, 0.5f);
            barRT.anchoredPosition = new Vector2(i * (barWidth + spacing), 0);
            barRT.sizeDelta = new Vector2(barWidth, 0);

            Image barImage = barObj.AddComponent<Image>();
            barImage.color = new Color(0.5f, 0.5f, 0.5f, 0.25f);
            progressBars[i] = barImage;
        }

        GameObject textObj = new GameObject("Text");
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(btnObj.transform, false);
        textRT.anchorMin = new Vector2(0, 0.2f);
        textRT.anchorMax = new Vector2(1, 1);
        textRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 16;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return progressBars;
    }

    static Button CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image image = btnObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.5f, 0.8f, 1f);
        Button button = btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(btnObj.transform, false);
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return button;
    }

    static void CreateLockedPanel(Transform parent)
    {
        GameObject lockedPanel = new GameObject("LockedPanel");
        RectTransform rt = lockedPanel.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Image image = lockedPanel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.85f);

        GameObject textObj = new GameObject("UnlockRequirementText");
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(lockedPanel.transform, false);
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "LOCKED\n\nRequires:\n40 Previous Dimension";
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.7f, 0.7f, 1f);

        lockedPanel.SetActive(false);
    }

    // 디멘션 전용 텍스트 생성 함수
    static void CreateDimensionText(Transform parent, string name, string text, float fontSize, Vector2 position, Vector2 size, TextAlignmentOptions alignment, bool isRightAligned = false)
    {
        GameObject textObj = new GameObject(name);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);

        if (isRightAligned)
        {
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
        }
        else
        {
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
        }

        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = isRightAligned ? new Color(1f, 0.8f, 0f, 1f) : Color.white;
    }

    // 디멘션 Buy 버튼 생성
    static void CreateDimensionButton_Buy(Transform parent, string name, string text, Vector2 position)
    {
        GameObject btnObj = new GameObject(name);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(300, 50);

        Image image = btnObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.5f, 0.8f, 1f);
        Button button = btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(btnObj.transform, false);
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    // 디멘션 Buy Max 버튼 생성 (프로그레스 바 포함)
    static Image[] CreateDimensionButton_BuyMax(Transform parent, string name, string text, Vector2 position)
    {
        GameObject btnObj = new GameObject(name);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(300, 50);

        Image image = btnObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.5f, 0.8f, 1f);
        Button button = btnObj.AddComponent<Button>();

        // Progress bars container - 더 길고 크게
        GameObject progressContainer = new GameObject("ProgressBars");
        RectTransform progressRT = progressContainer.AddComponent<RectTransform>();
        progressRT.SetParent(btnObj.transform, false);
        progressRT.anchorMin = new Vector2(0, 0);
        progressRT.anchorMax = new Vector2(1, 0);
        progressRT.pivot = new Vector2(0.5f, 0);
        progressRT.anchoredPosition = new Vector2(0, 3);
        progressRT.sizeDelta = new Vector2(-20, 6);  // 높이 4 -> 6

        Image[] progressBars = new Image[10];
        float barWidth = 26f;  // 10 -> 26 (더 넓게)
        float spacing = 2f;    // 1 -> 2

        for (int i = 0; i < 10; i++)
        {
            GameObject barObj = new GameObject($"Bar{i}");
            RectTransform barRT = barObj.AddComponent<RectTransform>();
            barRT.SetParent(progressContainer.transform, false);
            barRT.anchorMin = new Vector2(0, 0);
            barRT.anchorMax = new Vector2(0, 1);
            barRT.pivot = new Vector2(0, 0.5f);
            barRT.anchoredPosition = new Vector2(i * (barWidth + spacing), 0);
            barRT.sizeDelta = new Vector2(barWidth, 0);

            Image barImage = barObj.AddComponent<Image>();
            barImage.color = new Color(0.5f, 0.5f, 0.5f, 0.25f);
            progressBars[i] = barImage;
        }

        GameObject textObj = new GameObject("Text");
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(btnObj.transform, false);
        textRT.anchorMin = new Vector2(0, 0.15f);
        textRT.anchorMax = new Vector2(1, 1);
        textRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return progressBars;
    }
}
