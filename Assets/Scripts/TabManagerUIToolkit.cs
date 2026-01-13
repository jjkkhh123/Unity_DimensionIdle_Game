using UnityEngine;
using UnityEngine.UIElements;

public class TabManagerUIToolkit : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;

    // Panels
    private VisualElement dimensionsRoot;
    private VisualElement prestigeRoot;
    private VisualElement optionsRoot;

    // Tab buttons (from each panel's menu)
    private Button[] dimensionsButtons = new Button[3];
    private Button[] prestigeButtons = new Button[3];
    private Button[] optionsButtons = new Button[3];

    private Color activeColor = new Color(0.2f, 0.6f, 0.86f, 1f);
    private Color inactiveColor = new Color(0.16f, 0.5f, 0.73f, 0.15f);

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("[TabManagerUIToolkit] UIDocument not found!");
            return;
        }

        root = uiDocument.rootVisualElement;
        CacheUIElements();
        RegisterCallbacks();

        // Show dimensions panel by default
        ShowPanel("Dimensions");
    }

    void CacheUIElements()
    {
        // Cache panels
        dimensionsRoot = root.Q<VisualElement>("root");
        prestigeRoot = root.Q<VisualElement>("prestige-root");
        optionsRoot = root.Q<VisualElement>("options-root");

        if (dimensionsRoot == null) Debug.LogError("[TabManagerUIToolkit] root (dimensions) not found!");
        if (prestigeRoot == null) Debug.LogError("[TabManagerUIToolkit] prestige-root not found!");
        if (optionsRoot == null) Debug.LogError("[TabManagerUIToolkit] options-root not found!");

        // Cache buttons from dimensions panel menu
        var dimensionsMenu = dimensionsRoot?.Q<VisualElement>("menu");
        if (dimensionsMenu != null)
        {
            dimensionsButtons[0] = dimensionsMenu.Q<Button>("DimensionsBtn");
            dimensionsButtons[1] = dimensionsMenu.Q<Button>("PrestigeBtn");
            dimensionsButtons[2] = dimensionsMenu.Q<Button>("OptionBtn");
        }

        // Cache buttons from prestige panel menu
        var prestigeMenu = prestigeRoot?.Q<VisualElement>("menu");
        if (prestigeMenu != null)
        {
            prestigeButtons[0] = prestigeMenu.Q<Button>("DimensionsBtn");
            prestigeButtons[1] = prestigeMenu.Q<Button>("PrestigeBtn");
            prestigeButtons[2] = prestigeMenu.Q<Button>("OptionBtn");
        }

        // Cache buttons from options panel menu
        var optionsMenu = optionsRoot?.Q<VisualElement>("menu");
        if (optionsMenu != null)
        {
            optionsButtons[0] = optionsMenu.Q<Button>("DimensionsBtn");
            optionsButtons[1] = optionsMenu.Q<Button>("PrestigeBtn");
            optionsButtons[2] = optionsMenu.Q<Button>("OptionMenuBtn");
        }
    }

    void RegisterCallbacks()
    {
        // Register callbacks for all dimension menu buttons
        for (int i = 0; i < dimensionsButtons.Length; i++)
        {
            if (dimensionsButtons[i] != null)
            {
                int index = i;
                dimensionsButtons[i].clicked += () => OnTabClicked(index);
            }
        }

        // Register callbacks for all prestige menu buttons
        for (int i = 0; i < prestigeButtons.Length; i++)
        {
            if (prestigeButtons[i] != null)
            {
                int index = i;
                prestigeButtons[i].clicked += () => OnTabClicked(index);
            }
        }

        // Register callbacks for all options menu buttons
        for (int i = 0; i < optionsButtons.Length; i++)
        {
            if (optionsButtons[i] != null)
            {
                int index = i;
                optionsButtons[i].clicked += () => OnTabClicked(index);
            }
        }
    }

    void OnTabClicked(int index)
    {
        string panelName = "";
        switch (index)
        {
            case 0: panelName = "Dimensions"; break;
            case 1: panelName = "Prestige"; break;
            case 2: panelName = "Options"; break;
        }

        if (!string.IsNullOrEmpty(panelName))
        {
            ShowPanel(panelName);
        }
    }

    public void ShowPanel(string panelName)
    {
        Debug.Log($"[TabManagerUIToolkit] Switching to panel: {panelName}");

        // Show/hide panels
        if (dimensionsRoot != null)
        {
            dimensionsRoot.style.display = (panelName == "Dimensions") ? DisplayStyle.Flex : DisplayStyle.None;
            Debug.Log($"[TabManagerUIToolkit] Dimensions panel: {dimensionsRoot.style.display.value}");
        }

        if (prestigeRoot != null)
        {
            prestigeRoot.style.display = (panelName == "Prestige") ? DisplayStyle.Flex : DisplayStyle.None;
            Debug.Log($"[TabManagerUIToolkit] Prestige panel: {prestigeRoot.style.display.value}");
        }

        if (optionsRoot != null)
        {
            optionsRoot.style.display = (panelName == "Options") ? DisplayStyle.Flex : DisplayStyle.None;
            Debug.Log($"[TabManagerUIToolkit] Options panel: {optionsRoot.style.display.value}");
        }

        // Update button colors
        UpdateButtonColors(panelName);
    }

    void UpdateButtonColors(string activePanelName)
    {
        int activeIndex = activePanelName switch
        {
            "Dimensions" => 0,
            "Prestige" => 1,
            "Options" => 2,
            _ => -1
        };

        // Update dimensions menu buttons
        for (int i = 0; i < dimensionsButtons.Length; i++)
        {
            if (dimensionsButtons[i] != null)
            {
                dimensionsButtons[i].style.backgroundColor = (i == activeIndex) ? activeColor : inactiveColor;
            }
        }

        // Update prestige menu buttons
        for (int i = 0; i < prestigeButtons.Length; i++)
        {
            if (prestigeButtons[i] != null)
            {
                prestigeButtons[i].style.backgroundColor = (i == activeIndex) ? activeColor : inactiveColor;
            }
        }

        // Update options menu buttons
        for (int i = 0; i < optionsButtons.Length; i++)
        {
            if (optionsButtons[i] != null)
            {
                optionsButtons[i].style.backgroundColor = (i == activeIndex) ? activeColor : inactiveColor;
            }
        }
    }
}
