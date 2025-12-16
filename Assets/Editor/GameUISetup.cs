using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class GameUISetup : EditorWindow
{
    [MenuItem("Tools/Setup Antimatter Game UI")]
    static void SetupUI()
    {
        if (EditorUtility.DisplayDialog("Setup Game UI",
            "This will create all UI elements for the game. Continue?",
            "Yes", "Cancel"))
        {
            CreateGameUI();
        }
    }

    static void CreateGameUI()
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

        // UIManager 찾기 또는 생성
        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        GameObject uiManagerObj;
        if (uiManager == null)
        {
            uiManagerObj = new GameObject("UIManager");
            uiManagerObj.transform.SetParent(canvas.transform, false);
            uiManager = uiManagerObj.AddComponent<UIManager>();
        }

        // MainPanel 생성
        GameObject mainPanel = CreateMainPanel(canvas.transform);

        // Dimension Buttons 생성
        DimensionButton[] dimensionButtons = CreateDimensionButtons(canvas.transform);

        // Prestige Panel 생성
        (Button prestigeButton, TextMeshProUGUI prestigeButtonText, TextMeshProUGUI prestigeInfoText) = CreatePrestigePanel(canvas.transform);

        // Infinity Panel 생성
        (GameObject infinityPanel, TextMeshProUGUI infinityText) = CreateInfinityPanel(canvas.transform);

        // UIManager 연결
        uiManager.antimatterText = mainPanel.transform.Find("AntimatterText").GetComponent<TextMeshProUGUI>();
        uiManager.antimatterPerSecondText = mainPanel.transform.Find("AntimatterPerSecondText").GetComponent<TextMeshProUGUI>();
        uiManager.dimensionButtons = dimensionButtons;
        uiManager.prestigeButton = prestigeButton;
        uiManager.prestigeButtonText = prestigeButtonText;
        uiManager.prestigeInfoText = prestigeInfoText;
        uiManager.infinityPanel = infinityPanel;
        uiManager.infinityText = infinityText;

        EditorUtility.SetDirty(uiManager);

        Debug.Log("Game UI Setup Complete!");
    }

    static GameObject CreateMainPanel(Transform parent)
    {
        GameObject mainPanel = new GameObject("MainPanel");
        RectTransform rt = mainPanel.AddComponent<RectTransform>();
        rt.SetParent(parent, false);

        // AntimatterText (중앙 상단)
        GameObject antimatterTextObj = new GameObject("AntimatterText");
        RectTransform atmRT = antimatterTextObj.AddComponent<RectTransform>();
        atmRT.SetParent(mainPanel.transform, false);
        atmRT.anchorMin = new Vector2(0.5f, 1);
        atmRT.anchorMax = new Vector2(0.5f, 1);
        atmRT.pivot = new Vector2(0.5f, 1);
        atmRT.anchoredPosition = new Vector2(0, -20);
        atmRT.sizeDelta = new Vector2(800, 60);

        TextMeshProUGUI antimatterText = antimatterTextObj.AddComponent<TextMeshProUGUI>();
        antimatterText.text = "Antimatter: 10";
        antimatterText.fontSize = 40;
        antimatterText.alignment = TextAlignmentOptions.Center;

        // AntimatterPerSecondText (중앙, 안티매터 아래)
        GameObject perSecTextObj = new GameObject("AntimatterPerSecondText");
        RectTransform psRT = perSecTextObj.AddComponent<RectTransform>();
        psRT.SetParent(mainPanel.transform, false);
        psRT.anchorMin = new Vector2(0.5f, 1);
        psRT.anchorMax = new Vector2(0.5f, 1);
        psRT.pivot = new Vector2(0.5f, 1);
        psRT.anchoredPosition = new Vector2(0, -70);
        psRT.sizeDelta = new Vector2(600, 40);

        TextMeshProUGUI perSecText = perSecTextObj.AddComponent<TextMeshProUGUI>();
        perSecText.text = "/sec: 0";
        perSecText.fontSize = 28;
        perSecText.alignment = TextAlignmentOptions.Center;

        return mainPanel;
    }

    static DimensionButton[] CreateDimensionButtons(Transform parent)
    {
        DimensionButton[] buttons = new DimensionButton[8];

        for (int i = 0; i < 8; i++)
        {
            buttons[i] = CreateSingleDimensionButton(parent, i + 1);
        }

        return buttons;
    }

    static DimensionButton CreateSingleDimensionButton(Transform parent, int tier)
    {
        // Calculate position
        int column = (tier - 1) / 4;
        int row = (tier - 1) % 4;
        float xPos = -500 + (column * 400);
        float yPos = 200 - (row * 140);

        GameObject buttonObj = new GameObject($"Dimension{tier}Panel");
        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(350, 120);
        rt.anchoredPosition = new Vector2(xPos, yPos);

        // Add Image for background
        Image bgImage = buttonObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.3f, 1f);

        // NameText
        TextMeshProUGUI nameText = CreateTextChild(buttonObj.transform, "NameText", $"Dimension {tier}", 28, new Vector2(-120, 35));

        // AmountText
        TextMeshProUGUI amountText = CreateTextChild(buttonObj.transform, "AmountText", "Owned: 0", 18, new Vector2(-120, 5));

        // ProductionText
        string prodText = tier == 1 ? "Antimatter/s: 0" : $"Dim {tier - 1}/s: 0";
        TextMeshProUGUI productionText = CreateTextChild(buttonObj.transform, "ProductionText", prodText, 18, new Vector2(-120, -20));

        // PriceText
        TextMeshProUGUI priceText = CreateTextChild(buttonObj.transform, "PriceText", "Price: 0", 18, new Vector2(-120, -45));

        // MultiplierText (오른쪽 정렬)
        GameObject multiplierTextObj = new GameObject("MultiplierText");
        RectTransform multRT = multiplierTextObj.AddComponent<RectTransform>();
        multRT.SetParent(buttonObj.transform, false);
        multRT.anchorMin = new Vector2(1, 0.5f);
        multRT.anchorMax = new Vector2(1, 0.5f);
        multRT.pivot = new Vector2(1, 0.5f);
        multRT.anchoredPosition = new Vector2(-10, 20);
        multRT.sizeDelta = new Vector2(100, 30);

        TextMeshProUGUI multiplierText = multiplierTextObj.AddComponent<TextMeshProUGUI>();
        multiplierText.text = "x1.00";
        multiplierText.fontSize = 22;
        multiplierText.alignment = TextAlignmentOptions.Right;
        multiplierText.color = new Color(1f, 0.8f, 0f, 1f);

        // BuyButton (오른쪽 하단)
        GameObject buyButtonObj = new GameObject("BuyButton");
        RectTransform buyRT = buyButtonObj.AddComponent<RectTransform>();
        buyRT.SetParent(buttonObj.transform, false);
        buyRT.anchorMin = new Vector2(1, 0);
        buyRT.anchorMax = new Vector2(1, 0);
        buyRT.pivot = new Vector2(1, 0);
        buyRT.anchoredPosition = new Vector2(-10, 10);
        buyRT.sizeDelta = new Vector2(120, 30);

        Image buyBgImage = buyButtonObj.AddComponent<Image>();
        buyBgImage.color = new Color(0.3f, 0.5f, 0.8f, 1f);
        Button buyButton = buyButtonObj.AddComponent<Button>();

        GameObject buyTextObj = new GameObject("Text");
        RectTransform buyTextRT = buyTextObj.AddComponent<RectTransform>();
        buyTextRT.SetParent(buyButtonObj.transform, false);
        buyTextRT.anchorMin = Vector2.zero;
        buyTextRT.anchorMax = Vector2.one;
        buyTextRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI buyText = buyTextObj.AddComponent<TextMeshProUGUI>();
        buyText.text = "Buy x1";
        buyText.fontSize = 16;
        buyText.alignment = TextAlignmentOptions.Center;
        buyText.color = Color.white;

        // BuyMaxButton (오른쪽 하단, BuyButton 위)
        GameObject buyMaxButtonObj = new GameObject("BuyMaxButton");
        RectTransform buyMaxRT = buyMaxButtonObj.AddComponent<RectTransform>();
        buyMaxRT.SetParent(buttonObj.transform, false);
        buyMaxRT.anchorMin = new Vector2(1, 0);
        buyMaxRT.anchorMax = new Vector2(1, 0);
        buyMaxRT.pivot = new Vector2(1, 0);
        buyMaxRT.anchoredPosition = new Vector2(-10, 45);
        buyMaxRT.sizeDelta = new Vector2(120, 30);

        Image buyMaxBgImage = buyMaxButtonObj.AddComponent<Image>();
        buyMaxBgImage.color = new Color(0.3f, 0.5f, 0.8f, 1f);
        Button buyMaxButton = buyMaxButtonObj.AddComponent<Button>();

        GameObject buyMaxTextObj = new GameObject("Text");
        RectTransform buyMaxTextRT = buyMaxTextObj.AddComponent<RectTransform>();
        buyMaxTextRT.SetParent(buyMaxButtonObj.transform, false);
        buyMaxTextRT.anchorMin = Vector2.zero;
        buyMaxTextRT.anchorMax = Vector2.one;
        buyMaxTextRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI buyMaxText = buyMaxTextObj.AddComponent<TextMeshProUGUI>();
        buyMaxText.text = "Buy Max";
        buyMaxText.fontSize = 16;
        buyMaxText.alignment = TextAlignmentOptions.Center;
        buyMaxText.color = Color.white;

        // LockedPanel
        GameObject lockedPanel = CreateLockedPanel(buttonObj.transform);

        // Add DimensionButton component
        DimensionButton dimButton = buttonObj.AddComponent<DimensionButton>();
        dimButton.dimensionTier = tier;
        dimButton.buyButton = buyButton;
        dimButton.buyMaxButton = buyMaxButton;
        dimButton.dimensionNameText = nameText;
        dimButton.amountText = amountText;
        dimButton.multiplierText = multiplierText;
        dimButton.productionText = productionText;
        dimButton.priceText = priceText;
        dimButton.lockedPanel = lockedPanel;

        EditorUtility.SetDirty(dimButton);

        return dimButton;
    }

    static TextMeshProUGUI CreateTextChild(Transform parent, string name, string text, float fontSize, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);

        // 왼쪽 정렬을 위한 anchor 설정
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);

        // 버튼 왼쪽 기준으로 위치 설정 (10픽셀 여백)
        rt.anchoredPosition = new Vector2(10, position.y);
        rt.sizeDelta = new Vector2(240, 30);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = Color.white;

        return tmp;
    }

    static Button CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);
        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.5f, 0.8f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        // Button Text
        GameObject textObj = new GameObject("Text");
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(buttonObj.transform, false);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 16;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        return button;
    }

    static GameObject CreateLockedPanel(Transform parent)
    {
        GameObject lockedPanel = new GameObject("LockedPanel");
        RectTransform rt = lockedPanel.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Image image = lockedPanel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.8f);

        // Locked Text
        GameObject textObj = new GameObject("Text");
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(lockedPanel.transform, false);
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "LOCKED";
        tmp.fontSize = 32;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.red;

        lockedPanel.SetActive(false);

        return lockedPanel;
    }

    static (Button, TextMeshProUGUI, TextMeshProUGUI) CreatePrestigePanel(Transform parent)
    {
        // Prestige Button (우측 상단)
        GameObject prestigeButtonObj = new GameObject("PrestigeButton");
        RectTransform rt = prestigeButtonObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -120);
        rt.sizeDelta = new Vector2(300, 60);

        Image image = prestigeButtonObj.AddComponent<Image>();
        image.color = new Color(0.8f, 0.3f, 0.3f, 1f);

        Button button = prestigeButtonObj.AddComponent<Button>();

        // Button Text
        GameObject textObj = new GameObject("Text");
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(prestigeButtonObj.transform, false);
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Prestige (Locked)";
        buttonText.fontSize = 24;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;

        // Prestige Info Text (프레스티지 버튼 아래)
        GameObject infoTextObj = new GameObject("PrestigeInfoText");
        RectTransform infoRT = infoTextObj.AddComponent<RectTransform>();
        infoRT.SetParent(parent, false);
        infoRT.anchorMin = new Vector2(1, 1);
        infoRT.anchorMax = new Vector2(1, 1);
        infoRT.pivot = new Vector2(1, 1);
        infoRT.anchoredPosition = new Vector2(-20, -190);
        infoRT.sizeDelta = new Vector2(300, 60);

        TextMeshProUGUI infoText = infoTextObj.AddComponent<TextMeshProUGUI>();
        infoText.text = "Prestige Points: 0\nTotal Prestiges: 0";
        infoText.fontSize = 20;
        infoText.alignment = TextAlignmentOptions.Center;
        infoText.color = Color.white;

        return (button, buttonText, infoText);
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

        // Infinity Text
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
        tmp.color = new Color(1f, 0.84f, 0f, 1f); // Gold color

        infinityPanel.SetActive(false);

        return (infinityPanel, tmp);
    }
}
