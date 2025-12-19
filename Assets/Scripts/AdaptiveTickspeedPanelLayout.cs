using UnityEngine;

public class AdaptiveTickspeedPanelLayout : MonoBehaviour
{
    void Start()
    {
        if (PlatformDetector.Instance == null || !PlatformDetector.IsMobile)
        {
            Debug.Log("[AdaptiveTickspeedPanelLayout] PC mode or PlatformDetector not found, using default layout.");
            return;
        }

        // Mobile: Adjust internal layout to fill the panel
        // Panel is 500×240px (vs 300×300px on PC)

        Transform title = transform.Find("Title");
        Transform button = transform.Find("TickspeedButton");
        Transform infoText = transform.Find("TickspeedInfoText");

        if (title != null)
        {
            RectTransform rt = title.GetComponent<RectTransform>();
            // Full width with minimal margin: -20 → -10
            rt.sizeDelta = new Vector2(-10, 30);
            rt.anchoredPosition = new Vector2(0, -5);
            Debug.Log("[AdaptiveTickspeedPanelLayout] Title resized with minimal margin");
        }

        if (button != null)
        {
            RectTransform rt = button.GetComponent<RectTransform>();
            // Full width button with minimal margin: 280 → 490, height: 50
            rt.sizeDelta = new Vector2(490, 50);
            rt.anchoredPosition = new Vector2(0, -40);
            Debug.Log("[AdaptiveTickspeedPanelLayout] TickspeedButton resized to 490×50 at (0, -40)");
        }

        if (infoText != null)
        {
            RectTransform rt = infoText.GetComponent<RectTransform>();
            // Fill remaining space: 240 - 5 - 30 - 50 - 5 = 150px
            rt.sizeDelta = new Vector2(-10, 145);
            rt.anchoredPosition = new Vector2(0, 5);
            Debug.Log("[AdaptiveTickspeedPanelLayout] TickspeedInfoText resized to fill panel (height 145)");
        }
    }
}
