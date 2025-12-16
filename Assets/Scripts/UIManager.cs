using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main Display")]
    public TextMeshProUGUI antimatterText;
    public TextMeshProUGUI antimatterPerSecondText;

    [Header("Dimension Buttons")]
    public DimensionButton[] dimensionButtons;

    [Header("Prestige")]
    public Button prestigeButton;
    public TextMeshProUGUI prestigeButtonText;
    public TextMeshProUGUI prestigeInfoText;

    [Header("Tickspeed")]
    public Button tickspeedButton;
    public TextMeshProUGUI tickspeedButtonText;
    public TextMeshProUGUI tickspeedInfoText;

    [Header("Infinity")]
    public GameObject infinityPanel;
    public TextMeshProUGUI infinityText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (infinityPanel != null)
            infinityPanel.SetActive(false);

        if (prestigeButton != null)
            prestigeButton.onClick.AddListener(OnPrestigeButtonClicked);

        if (tickspeedButton != null)
            tickspeedButton.onClick.AddListener(OnTickspeedButtonClicked);
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

        UpdateDimensionButtons();

        UpdatePrestigeButton();

        UpdateTickspeedButton();

        UpdateInfinityPanel();
    }

    void UpdateAntimatterDisplay()
    {
        if (antimatterText != null)
        {
            antimatterText.text = $"Antimatter: {GameManager.Instance.GetAntimatterString()}";
        }

        if (antimatterPerSecondText != null)
        {
            BigDouble perSecond = BigDouble.Zero;
            if (GameManager.Instance.dimensions.Count > 0)
            {
                perSecond = GameManager.Instance.dimensions[0].GetProduction();
            }
            antimatterPerSecondText.text = $"/sec: {perSecond}";
        }
    }

    void UpdateDimensionButtons()
    {
        if (dimensionButtons == null)
            return;

        for (int i = 0; i < dimensionButtons.Length; i++)
        {
            if (dimensionButtons[i] != null)
            {
                dimensionButtons[i].UpdateButton();
            }
        }
    }

    void UpdatePrestigeButton()
    {
        if (PrestigeManager.Instance == null || GameManager.Instance == null)
            return;

        bool canPrestige = PrestigeManager.Instance.CanPrestige();

        if (prestigeButton != null)
        {
            prestigeButton.interactable = canPrestige;
        }

        if (prestigeButtonText != null)
        {
            if (canPrestige)
            {
                int points = PrestigeManager.Instance.CalculatePrestigePointsGained();
                prestigeButtonText.text = $"Prestige (+{points} points)";
            }
            else
            {
                prestigeButtonText.text = "Prestige (Locked)";
            }
        }

        if (prestigeInfoText != null)
        {
            string statusText = "";

            if (canPrestige)
            {
                int points = PrestigeManager.Instance.CalculatePrestigePointsGained();
                statusText = $"Ready! Will gain +{points} point(s)\n\n";
            }
            else
            {
                BigDouble requirement = new BigDouble(1e10);
                statusText = $"Requires: {requirement} Antimatter\n\n";
            }

            statusText += $"Prestige Points: {PrestigeManager.Instance.prestigePoints}\n";
            statusText += $"Total Prestiges: {PrestigeManager.Instance.totalPrestiges}\n\n";
            statusText += $"Formula: 1e10 = 1 point\n";
            statusText += $"(exponent / 10)\n\n";
            statusText += $"Spend points in Prestige tab!";

            prestigeInfoText.text = statusText;
        }
    }

    void UpdateInfinityPanel()
    {
        if (infinityPanel != null && GameManager.Instance != null)
        {
            if (GameManager.Instance.infinityReached && !infinityPanel.activeSelf)
            {
                infinityPanel.SetActive(true);

                if (infinityText != null)
                {
                    infinityText.text = "INFINITY REACHED!\n\nCongratulations!\n\nMore content coming soon...";
                }
            }
        }
    }

    void OnPrestigeButtonClicked()
    {
        if (PrestigeManager.Instance != null)
        {
            PrestigeManager.Instance.DoPrestige();
        }
    }

    void UpdateTickspeedButton()
    {
        if (TickSpeedManager.Instance == null)
            return;

        bool canBuy = TickSpeedManager.Instance.CanBuyTickspeed();

        if (tickspeedButton != null)
        {
            tickspeedButton.interactable = canBuy;
        }

        if (tickspeedButtonText != null)
        {
            BigDouble price = TickSpeedManager.Instance.GetCurrentPrice();
            tickspeedButtonText.text = $"Upgrade Tickspeed\nCost: {price}";
        }

        if (tickspeedInfoText != null)
        {
            int level = TickSpeedManager.Instance.tickspeedLevel;
            double multiplier = TickSpeedManager.Instance.GetTickspeedMultiplier();

            // 프레스티지 업그레이드에 따른 기본 배율 계산
            double baseMultiplier = 1.1;
            if (PrestigeManager.Instance != null)
                baseMultiplier += PrestigeManager.Instance.GetTickspeedBoost();

            tickspeedInfoText.text = $"Tickspeed Level: {level}\nCurrent Multiplier: x{multiplier:F2}\n\nIncreases production speed\nMultiplies by x{baseMultiplier:F2} per upgrade";
        }
    }

    void OnTickspeedButtonClicked()
    {
        if (TickSpeedManager.Instance != null)
        {
            TickSpeedManager.Instance.BuyTickspeed();
        }
    }
}
