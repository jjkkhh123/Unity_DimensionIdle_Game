using UnityEngine;

public class AdaptiveOptionsLayout : MonoBehaviour
{
    void Start()
    {
        if (PlatformDetector.Instance == null || !PlatformDetector.IsMobile)
        {
            Debug.Log("[AdaptiveOptionsLayout] PC mode or PlatformDetector not found, using default layout.");
            return;
        }

        // Mobile: Adjust Options panel layout
        Debug.Log("[AdaptiveOptionsLayout] Applying mobile layout to Options panel");

        // Find elements
        Transform title = transform.Find("OptionsTitle");
        Transform helpButton = transform.Find("HelpButton");
        Transform helpPanel = transform.Find("HelpPanel");
        Transform exportButton = transform.Find("ExportButton");
        Transform importButton = transform.Find("ImportButton");
        Transform resetButton = transform.Find("ResetButton");
        Transform quitButton = transform.Find("QuitButton");
        Transform statusText = transform.Find("StatusText");

        // Adjust positions and sizes for mobile
        if (title != null)
        {
            RectTransform rt = title.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 850);  // Higher position
            Debug.Log("[AdaptiveOptionsLayout] Title repositioned");
        }

        if (helpButton != null)
        {
            RectTransform rt = helpButton.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 730);
            rt.sizeDelta = new Vector2(900, 80);  // Wider button
            Debug.Log("[AdaptiveOptionsLayout] Help button resized to 900x80");
        }

        if (exportButton != null)
        {
            RectTransform rt = exportButton.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 600);
            rt.sizeDelta = new Vector2(900, 80);
            Debug.Log("[AdaptiveOptionsLayout] Export button adjusted");
        }

        if (importButton != null)
        {
            RectTransform rt = importButton.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 500);
            rt.sizeDelta = new Vector2(900, 80);
            Debug.Log("[AdaptiveOptionsLayout] Import button adjusted");
        }

        if (resetButton != null)
        {
            RectTransform rt = resetButton.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 400);
            rt.sizeDelta = new Vector2(900, 80);
            Debug.Log("[AdaptiveOptionsLayout] Reset button adjusted");
        }

        if (quitButton != null)
        {
            RectTransform rt = quitButton.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 300);
            rt.sizeDelta = new Vector2(900, 80);
            Debug.Log("[AdaptiveOptionsLayout] Quit button adjusted");
        }

        if (statusText != null)
        {
            RectTransform rt = statusText.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 150);
            rt.sizeDelta = new Vector2(900, 120);
            Debug.Log("[AdaptiveOptionsLayout] Status text adjusted");
        }

        // Adjust Help Panel for mobile
        if (helpPanel != null)
        {
            RectTransform rt = helpPanel.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1000, 600);  // Larger help panel
            rt.anchoredPosition = new Vector2(0, 400);
            Debug.Log("[AdaptiveOptionsLayout] Help panel resized to 1000x600");
        }
    }
}
