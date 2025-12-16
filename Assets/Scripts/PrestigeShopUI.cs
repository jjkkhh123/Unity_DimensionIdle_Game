using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PrestigeShopUI : MonoBehaviour
{
    public TextMeshProUGUI pointsText;
    public Transform upgradeContainer;
    public GameObject upgradeButtonPrefab;

    private Dictionary<string, GameObject> upgradeButtons = new Dictionary<string, GameObject>();
    private bool buttonsCreated = false;

    void Start()
    {
        // 코루틴으로 지연 생성
        StartCoroutine(CreateUpgradeButtonsDelayed());
    }

    System.Collections.IEnumerator CreateUpgradeButtonsDelayed()
    {
        yield return new UnityEngine.WaitForSeconds(0.2f);
        CreateUpgradeButtons();
    }

    void Update()
    {
        // 버튼이 아직 생성되지 않았으면 다시 시도
        if (!buttonsCreated && PrestigeManager.Instance != null && upgradeContainer != null)
        {
            CreateUpgradeButtons();
        }

        UpdateUI();
    }

    void CreateUpgradeButtons()
    {
        if (PrestigeManager.Instance == null || upgradeContainer == null)
        {
            Debug.LogWarning("[PrestigeShopUI] Cannot create buttons - Manager or Container is null");
            return;
        }

        if (buttonsCreated)
            return;

        Debug.Log($"[PrestigeShopUI] Creating {PrestigeManager.Instance.upgrades.Count} upgrade buttons");

        // 먼저 bulk_bonus를 우측 상단에 배치
        if (PrestigeManager.Instance.upgrades.ContainsKey("bulk_bonus"))
        {
            var bulkBonus = PrestigeManager.Instance.upgrades["bulk_bonus"];
            GameObject bulkBonusBtn = CreateUpgradeButton(bulkBonus, 450, 320);
            upgradeButtons["bulk_bonus"] = bulkBonusBtn;
        }

        // 나머지 업그레이드들을 그리드로 배치
        int index = 0;
        foreach (var kvp in PrestigeManager.Instance.upgrades)
        {
            if (kvp.Key == "bulk_bonus")
                continue;  // 이미 배치함

            int col = index / 5;  // 0 또는 1 (2열)
            int row = index % 5;  // 0~4 (5행)

            float xPos = -450 + (col * 900);  // -450 (왼쪽), 450 (오른쪽)

            // 우측 열은 bulk_bonus 아래부터 시작
            float yPos;
            if (col == 1)
                yPos = 160 - (row * 160);  // 두 번째 행부터
            else
                yPos = 320 - (row * 160);  // 최상단부터

            GameObject btnObj = CreateUpgradeButton(kvp.Value, xPos, yPos);
            upgradeButtons[kvp.Key] = btnObj;

            index++;
        }

        buttonsCreated = true;
        Debug.Log($"[PrestigeShopUI] Successfully created buttons with bulk_bonus at top-right");
    }

    GameObject CreateUpgradeButton(PrestigeUpgrade upgrade, float xPos, float yPos)
    {
        GameObject btnContainer = new GameObject($"Upgrade_{upgrade.id}");
        RectTransform containerRT = btnContainer.AddComponent<RectTransform>();
        containerRT.SetParent(upgradeContainer, false);
        containerRT.anchorMin = new Vector2(0.5f, 0.5f);  // 중앙 기준
        containerRT.anchorMax = new Vector2(0.5f, 0.5f);
        containerRT.pivot = new Vector2(0.5f, 0.5f);
        containerRT.anchoredPosition = new Vector2(xPos, yPos);
        containerRT.sizeDelta = new Vector2(850, 140);  // 박스 크기

        Image bg = btnContainer.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.3f, 1f);  // 배경색

        // Name
        GameObject nameObj = new GameObject("Name");
        RectTransform nameRT = nameObj.AddComponent<RectTransform>();
        nameRT.SetParent(btnContainer.transform, false);
        nameRT.anchorMin = new Vector2(0, 0.5f);
        nameRT.anchorMax = new Vector2(0, 0.5f);
        nameRT.pivot = new Vector2(0, 0.5f);
        nameRT.anchoredPosition = new Vector2(20, 15);
        nameRT.sizeDelta = new Vector2(600, 40);

        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = upgrade.name;
        nameText.fontSize = 24;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.color = Color.white;
        nameText.fontStyle = TMPro.FontStyles.Bold;

        // Description
        GameObject descObj = new GameObject("Description");
        RectTransform descRT = descObj.AddComponent<RectTransform>();
        descRT.SetParent(btnContainer.transform, false);
        descRT.anchorMin = new Vector2(0, 0.5f);
        descRT.anchorMax = new Vector2(0, 0.5f);
        descRT.pivot = new Vector2(0, 0.5f);
        descRT.anchoredPosition = new Vector2(20, -20);
        descRT.sizeDelta = new Vector2(600, 30);

        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = upgrade.description;
        descText.fontSize = 16;
        descText.alignment = TextAlignmentOptions.Left;
        descText.color = new Color(0.8f, 0.8f, 0.8f, 1f);

        // Level Display
        GameObject levelObj = new GameObject("Level");
        RectTransform levelRT = levelObj.AddComponent<RectTransform>();
        levelRT.SetParent(btnContainer.transform, false);
        levelRT.anchorMin = new Vector2(0.5f, 0.5f);
        levelRT.anchorMax = new Vector2(0.5f, 0.5f);
        levelRT.pivot = new Vector2(0.5f, 0.5f);
        levelRT.anchoredPosition = new Vector2(100, 0);
        levelRT.sizeDelta = new Vector2(200, 50);

        TextMeshProUGUI levelText = levelObj.AddComponent<TextMeshProUGUI>();
        levelText.text = $"Level: {upgrade.level}";
        levelText.fontSize = 22;
        levelText.alignment = TextAlignmentOptions.Center;
        levelText.color = new Color(1f, 0.9f, 0.3f, 1f);

        // Buy Button
        GameObject buyBtnObj = new GameObject("BuyButton");
        RectTransform buyRT = buyBtnObj.AddComponent<RectTransform>();
        buyRT.SetParent(btnContainer.transform, false);
        buyRT.anchorMin = new Vector2(1, 0.5f);
        buyRT.anchorMax = new Vector2(1, 0.5f);
        buyRT.pivot = new Vector2(1, 0.5f);
        buyRT.anchoredPosition = new Vector2(-20, 0);
        buyRT.sizeDelta = new Vector2(200, 60);

        Image buyBg = buyBtnObj.AddComponent<Image>();
        buyBg.color = new Color(0.3f, 0.6f, 0.3f, 1f);

        Button buyButton = buyBtnObj.AddComponent<Button>();
        buyButton.onClick.AddListener(() => OnBuyUpgrade(upgrade.id));

        GameObject buyTextObj = new GameObject("Text");
        RectTransform buyTextRT = buyTextObj.AddComponent<RectTransform>();
        buyTextRT.SetParent(buyBtnObj.transform, false);
        buyTextRT.anchorMin = Vector2.zero;
        buyTextRT.anchorMax = Vector2.one;
        buyTextRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI buyText = buyTextObj.AddComponent<TextMeshProUGUI>();
        buyText.text = $"Buy ({upgrade.cost} PP)";
        buyText.fontSize = 20;
        buyText.alignment = TextAlignmentOptions.Center;
        buyText.color = Color.white;

        return btnContainer;
    }

    void UpdateUI()
    {
        if (PrestigeManager.Instance == null)
            return;

        // Update points display
        if (pointsText != null)
        {
            pointsText.text = $"Prestige Points: {PrestigeManager.Instance.prestigePoints}";
        }

        // 버튼이 생성되지 않았으면 UI 업데이트 스킵
        if (!buttonsCreated)
            return;

        // Update upgrade buttons
        foreach (var kvp in PrestigeManager.Instance.upgrades)
        {
            if (!upgradeButtons.ContainsKey(kvp.Key))
                continue;

            GameObject btnObj = upgradeButtons[kvp.Key];
            PrestigeUpgrade upgrade = kvp.Value;

            // Update level text
            TextMeshProUGUI levelText = btnObj.transform.Find("Level").GetComponent<TextMeshProUGUI>();
            if (levelText != null)
            {
                levelText.text = $"Level: {upgrade.level}";
            }

            // Update buy button
            Button buyButton = btnObj.transform.Find("BuyButton").GetComponent<Button>();
            if (buyButton != null)
            {
                bool canAfford = PrestigeManager.Instance.CanBuyUpgrade(kvp.Key);
                buyButton.interactable = canAfford;

                Image buyBg = buyButton.GetComponent<Image>();
                if (buyBg != null)
                {
                    buyBg.color = canAfford ? new Color(0.3f, 0.6f, 0.3f, 1f) : new Color(0.4f, 0.4f, 0.4f, 1f);
                }
            }
        }
    }

    void OnBuyUpgrade(string upgradeId)
    {
        if (PrestigeManager.Instance != null)
        {
            PrestigeManager.Instance.BuyUpgrade(upgradeId);
        }
    }
}
