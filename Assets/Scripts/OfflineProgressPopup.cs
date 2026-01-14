using System;
using UnityEngine;
using UnityEngine.UIElements;

public class OfflineProgressPopup : MonoBehaviour
{
    public static OfflineProgressPopup Instance { get; private set; }

    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement popupOverlay;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            root = uiDocument.rootVisualElement;
        }
    }

    public void ShowOfflineProgress(double offlineSeconds, double accumulatedSeconds, double totalStoredSeconds)
    {
        if (root == null) return;

        // Remove existing popup if any
        if (popupOverlay != null)
        {
            root.Remove(popupOverlay);
        }

        // Create popup overlay
        popupOverlay = new VisualElement();
        popupOverlay.style.position = Position.Absolute;
        popupOverlay.style.top = 0;
        popupOverlay.style.left = 0;
        popupOverlay.style.width = Length.Percent(100);
        popupOverlay.style.height = Length.Percent(100);
        popupOverlay.style.backgroundColor = new Color(0, 0, 0, 0.7f);
        popupOverlay.style.alignItems = Align.Center;
        popupOverlay.style.justifyContent = Justify.Center;

        // Create popup container
        VisualElement popup = new VisualElement();
        popup.style.backgroundColor = new Color(0.17f, 0.24f, 0.31f, 1f); // #2c3e50
        popup.style.borderTopLeftRadius = 15;
        popup.style.borderTopRightRadius = 15;
        popup.style.borderBottomLeftRadius = 15;
        popup.style.borderBottomRightRadius = 15;
        popup.style.paddingTop = 30;
        popup.style.paddingBottom = 30;
        popup.style.paddingLeft = 40;
        popup.style.paddingRight = 40;
        popup.style.width = 500;
        popup.style.borderTopWidth = 3;
        popup.style.borderBottomWidth = 3;
        popup.style.borderLeftWidth = 3;
        popup.style.borderRightWidth = 3;
        popup.style.borderTopColor = new Color(0.61f, 0.35f, 0.71f, 1f); // #9b59b6
        popup.style.borderBottomColor = new Color(0.61f, 0.35f, 0.71f, 1f);
        popup.style.borderLeftColor = new Color(0.61f, 0.35f, 0.71f, 1f);
        popup.style.borderRightColor = new Color(0.61f, 0.35f, 0.71f, 1f);

        // Title
        Label title = new Label("Welcome Back!");
        title.style.fontSize = 32;
        title.style.color = new Color(0.95f, 0.77f, 0.06f, 1f); // #f1c40f
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.unityTextAlign = TextAnchor.MiddleCenter;
        title.style.marginBottom = 20;
        popup.Add(title);

        // Offline time info
        Label offlineLabel = new Label("You were offline for:");
        offlineLabel.style.fontSize = 18;
        offlineLabel.style.color = new Color(1f, 1f, 1f, 0.8f);
        offlineLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        offlineLabel.style.marginBottom = 5;
        popup.Add(offlineLabel);

        Label offlineTime = new Label(FormatTime(offlineSeconds));
        offlineTime.style.fontSize = 28;
        offlineTime.style.color = new Color(0.61f, 0.35f, 0.71f, 1f); // #9b59b6
        offlineTime.style.unityFontStyleAndWeight = FontStyle.Bold;
        offlineTime.style.unityTextAlign = TextAnchor.MiddleCenter;
        offlineTime.style.marginBottom = 20;
        popup.Add(offlineTime);

        // Accumulated time info
        double efficiency = OfflineManager.Instance != null ? OfflineManager.Instance.efficiencyRatio : 0.5;
        Label accumulatedLabel = new Label($"Time accumulated ({efficiency * 100:F0}% efficiency):");
        accumulatedLabel.style.fontSize = 18;
        accumulatedLabel.style.color = new Color(1f, 1f, 1f, 0.8f);
        accumulatedLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        accumulatedLabel.style.marginBottom = 5;
        popup.Add(accumulatedLabel);

        Label accumulatedTime = new Label(FormatTime(accumulatedSeconds));
        accumulatedTime.style.fontSize = 28;
        accumulatedTime.style.color = new Color(0.2f, 0.6f, 0.86f, 1f); // #3498db
        accumulatedTime.style.unityFontStyleAndWeight = FontStyle.Bold;
        accumulatedTime.style.unityTextAlign = TextAnchor.MiddleCenter;
        accumulatedTime.style.marginBottom = 20;
        popup.Add(accumulatedTime);

        // Divider
        VisualElement divider = new VisualElement();
        divider.style.height = 2;
        divider.style.backgroundColor = new Color(1f, 1f, 1f, 0.2f);
        divider.style.marginTop = 10;
        divider.style.marginBottom = 15;
        popup.Add(divider);

        // Total stored time
        Label totalLabel = new Label("Total stored time:");
        totalLabel.style.fontSize = 16;
        totalLabel.style.color = new Color(1f, 1f, 1f, 0.6f);
        totalLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        totalLabel.style.marginBottom = 5;
        popup.Add(totalLabel);

        Label totalTime = new Label(FormatTime(totalStoredSeconds));
        totalTime.style.fontSize = 22;
        totalTime.style.color = new Color(0.95f, 0.77f, 0.06f, 1f); // #f1c40f
        totalTime.style.unityFontStyleAndWeight = FontStyle.Bold;
        totalTime.style.unityTextAlign = TextAnchor.MiddleCenter;
        totalTime.style.marginBottom = 25;
        popup.Add(totalTime);

        // Close button
        Button closeButton = new Button(() => ClosePopup());
        closeButton.text = "CONTINUE";
        closeButton.style.height = 45;
        closeButton.style.fontSize = 20;
        closeButton.style.backgroundColor = new Color(0.61f, 0.35f, 0.71f, 1f); // #9b59b6
        closeButton.style.color = Color.white;
        closeButton.style.unityFontStyleAndWeight = FontStyle.Bold;
        closeButton.style.borderTopLeftRadius = 8;
        closeButton.style.borderTopRightRadius = 8;
        closeButton.style.borderBottomLeftRadius = 8;
        closeButton.style.borderBottomRightRadius = 8;
        closeButton.style.borderTopWidth = 0;
        closeButton.style.borderBottomWidth = 0;
        closeButton.style.borderLeftWidth = 0;
        closeButton.style.borderRightWidth = 0;
        popup.Add(closeButton);

        popupOverlay.Add(popup);
        root.Add(popupOverlay);
    }

    void ClosePopup()
    {
        if (popupOverlay != null && root != null)
        {
            root.Remove(popupOverlay);
            popupOverlay = null;
        }
    }

    string FormatTime(double seconds)
    {
        int days = (int)(seconds / 86400);
        int hours = (int)((seconds % 86400) / 3600);
        int minutes = (int)((seconds % 3600) / 60);
        int secs = (int)(seconds % 60);

        if (days > 0)
        {
            return $"{days}d {hours:D2}h {minutes:D2}m";
        }
        else if (hours > 0)
        {
            return $"{hours:D2}h {minutes:D2}m {secs:D2}s";
        }
        else
        {
            return $"{minutes:D2}m {secs:D2}s";
        }
    }
}
