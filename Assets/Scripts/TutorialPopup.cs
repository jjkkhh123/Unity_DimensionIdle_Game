using UnityEngine;
using UnityEngine.UIElements;

public class TutorialPopup : MonoBehaviour
{
    public static TutorialPopup Instance { get; private set; }

    private const string FIRST_TIME_KEY = "FirstTimePlayed";

    private UIDocument uiDocument;
    private VisualElement root;

    // UI Elements
    private VisualElement tutorialOverlay;
    private Label pageIndicator;
    private Label titleLabel;
    private Label line1Label;
    private Label line2Label;
    private Label line3Label;
    private Button backBtn;
    private Button skipBtn;
    private Button nextBtn;
    private VisualElement dotsContainer;

    private int currentPage = 0;

    // Tutorial content - each page has: Title, Line1, Line2, Line3
    private readonly string[][] tutorialPages = new string[][]
    {
        // Page 1: Welcome & Goal
        new string[]
        {
            "Welcome to Antimatter Dimensions!",
            "Your goal is to produce Antimatter until you reach <color=#f1c40f>Infinity (1.79e308)</color>.",
            "Click on dimensions to buy them and start producing antimatter!",
            ""
        },
        // Page 2: Dimensions
        new string[]
        {
            "Dimensions",
            "There are <color=#3498db>8 Dimensions</color> in total.",
            "Each dimension produces the one below it.",
            "<color=#9b59b6>Dimension 8 → 7 → 6 → ... → 1 → Antimatter</color>"
        },
        // Page 3: Tickspeed & DimBoost
        new string[]
        {
            "Tickspeed & Dimension Boost",
            "<color=#3498db>Tickspeed</color>: Increases ALL dimension production speed.",
            "<color=#9b59b6>Dimension Boost</color>: Resets progress but unlocks new dimensions.",
            "You need 20 of your highest dimension to boost!"
        },
        // Page 4: Prestige
        new string[]
        {
            "Prestige System",
            "When you reach <color=#f1c40f>1e10 Antimatter</color>, you can Prestige!",
            "Prestige resets dimensions but gives you <color=#e74c3c>Prestige Points</color>.",
            "Use PP to buy permanent upgrades!"
        },
        // Page 5: Tips
        new string[]
        {
            "Tips for Success",
            "Hold buttons to buy continuously.",
            "Check the <color=#f1c40f>Shop</color> and <color=#9b59b6>Offline</color> tabs for bonuses!",
            "Don't be afraid to Prestige - it speeds up progress!"
        }
    };

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
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("[TutorialPopup] UIDocument not found!");
            return;
        }

        root = uiDocument.rootVisualElement;
        CacheUIElements();
        RegisterCallbacks();

        // Check if first time playing
        if (PlayerPrefs.GetInt(FIRST_TIME_KEY, 0) == 0)
        {
            Invoke(nameof(ShowTutorial), 0.5f);
        }
    }

    void CacheUIElements()
    {
        tutorialOverlay = root.Q<VisualElement>("tutorial-overlay");
        pageIndicator = root.Q<Label>("TutorialPageIndicator");
        titleLabel = root.Q<Label>("TutorialTitle");
        line1Label = root.Q<Label>("TutorialLine1");
        line2Label = root.Q<Label>("TutorialLine2");
        line3Label = root.Q<Label>("TutorialLine3");
        backBtn = root.Q<Button>("TutorialBackBtn");
        skipBtn = root.Q<Button>("TutorialSkipBtn");
        nextBtn = root.Q<Button>("TutorialNextBtn");
        dotsContainer = root.Q<VisualElement>("TutorialDots");

        // Enable rich text for content labels
        if (line1Label != null) line1Label.enableRichText = true;
        if (line2Label != null) line2Label.enableRichText = true;
        if (line3Label != null) line3Label.enableRichText = true;

        if (tutorialOverlay == null) Debug.LogError("[TutorialPopup] tutorial-overlay not found!");
        if (pageIndicator == null) Debug.LogError("[TutorialPopup] TutorialPageIndicator not found!");
        if (titleLabel == null) Debug.LogError("[TutorialPopup] TutorialTitle not found!");
    }

    void RegisterCallbacks()
    {
        if (backBtn != null)
            backBtn.clicked += PreviousPage;

        if (skipBtn != null)
            skipBtn.clicked += CloseTutorial;

        if (nextBtn != null)
            nextBtn.clicked += OnNextClicked;
    }

    void OnNextClicked()
    {
        if (currentPage == tutorialPages.Length - 1)
        {
            CloseTutorial();
        }
        else
        {
            NextPage();
        }
    }

    public void ShowTutorial()
    {
        if (tutorialOverlay == null) return;

        currentPage = 0;
        UpdatePage();
        tutorialOverlay.style.display = DisplayStyle.Flex;
    }

    void UpdatePage()
    {
        if (currentPage < 0 || currentPage >= tutorialPages.Length) return;

        string[] content = tutorialPages[currentPage];

        // Update text
        if (pageIndicator != null)
            pageIndicator.text = $"{currentPage + 1} / {tutorialPages.Length}";

        if (titleLabel != null)
            titleLabel.text = content[0];

        if (line1Label != null)
            line1Label.text = content[1];

        if (line2Label != null)
            line2Label.text = content[2];

        if (line3Label != null)
            line3Label.text = content[3];

        // Update back button visibility
        if (backBtn != null)
            backBtn.style.visibility = currentPage == 0 ? Visibility.Hidden : Visibility.Visible;

        // Update next button text and style
        if (nextBtn != null)
        {
            bool isLastPage = currentPage == tutorialPages.Length - 1;
            nextBtn.text = isLastPage ? "START!" : "NEXT >";

            // Change color for last page
            nextBtn.RemoveFromClassList("popup-btn-primary");
            nextBtn.RemoveFromClassList("popup-btn-success");
            nextBtn.AddToClassList(isLastPage ? "popup-btn-success" : "popup-btn-primary");
        }

        // Update dots
        UpdateDots();
    }

    void UpdateDots()
    {
        if (dotsContainer == null) return;

        for (int i = 0; i < tutorialPages.Length; i++)
        {
            var dot = dotsContainer.Q<VisualElement>($"Dot{i}");
            if (dot != null)
            {
                dot.RemoveFromClassList("popup-dot-active");
                if (i == currentPage)
                {
                    dot.AddToClassList("popup-dot-active");
                }
            }
        }
    }

    void NextPage()
    {
        if (currentPage < tutorialPages.Length - 1)
        {
            currentPage++;
            UpdatePage();
        }
    }

    void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
        }
    }

    void CloseTutorial()
    {
        // Mark as played
        PlayerPrefs.SetInt(FIRST_TIME_KEY, 1);
        PlayerPrefs.Save();

        if (tutorialOverlay != null)
        {
            tutorialOverlay.style.display = DisplayStyle.None;
        }
    }

    // Public method to reset tutorial (for testing)
    public void ResetTutorial()
    {
        PlayerPrefs.SetInt(FIRST_TIME_KEY, 0);
        PlayerPrefs.Save();
        Debug.Log("[TutorialPopup] Tutorial reset. Will show on next start.");
    }
}
