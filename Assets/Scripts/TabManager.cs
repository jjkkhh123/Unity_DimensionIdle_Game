using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    public GameObject dimensionsPanel;
    public GameObject prestigePanel;
    public GameObject optionsPanel;

    public Button dimensionsButton;
    public Button prestigeButton;
    public Button optionsButton;

    private Color activeColor = new Color(0.3f, 0.6f, 0.9f, 1f);
    private Color inactiveColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    void Start()
    {
        if (dimensionsButton != null)
            dimensionsButton.onClick.AddListener(() => ShowPanel("Dimensions"));

        if (prestigeButton != null)
            prestigeButton.onClick.AddListener(() => ShowPanel("Prestige"));

        if (optionsButton != null)
            optionsButton.onClick.AddListener(() => ShowPanel("Options"));

        ShowPanel("Dimensions");
    }

    public void ShowPanel(string panelName)
    {
        Debug.Log($"[TabManager] Switching to panel: {panelName}");

        if (dimensionsPanel != null)
        {
            dimensionsPanel.SetActive(panelName == "Dimensions");
            Debug.Log($"[TabManager] Dimensions panel: {dimensionsPanel.activeSelf}");
        }

        if (prestigePanel != null)
        {
            prestigePanel.SetActive(panelName == "Prestige");
            Debug.Log($"[TabManager] Prestige panel: {prestigePanel.activeSelf}");
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(panelName == "Options");
            Debug.Log($"[TabManager] Options panel: {optionsPanel.activeSelf}");
        }

        UpdateButtonColors(panelName);
    }

    void UpdateButtonColors(string activePanelName)
    {
        if (dimensionsButton != null)
        {
            Image img = dimensionsButton.GetComponent<Image>();
            if (img != null)
                img.color = (activePanelName == "Dimensions") ? activeColor : inactiveColor;
        }

        if (prestigeButton != null)
        {
            Image img = prestigeButton.GetComponent<Image>();
            if (img != null)
                img.color = (activePanelName == "Prestige") ? activeColor : inactiveColor;
        }

        if (optionsButton != null)
        {
            Image img = optionsButton.GetComponent<Image>();
            if (img != null)
                img.color = (activePanelName == "Options") ? activeColor : inactiveColor;
        }
    }
}
