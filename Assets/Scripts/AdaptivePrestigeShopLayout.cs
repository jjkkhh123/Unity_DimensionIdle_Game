using UnityEngine;

public class AdaptivePrestigeShopLayout : MonoBehaviour
{
    void Start()
    {
        if (PlatformDetector.Instance == null || !PlatformDetector.IsMobile)
        {
            Debug.Log("[AdaptivePrestigeShopLayout] PC mode or PlatformDetector not found, using default layout.");
            return;
        }

        // Mobile: Adjust Prestige Shop panel layout
        Debug.Log("[AdaptivePrestigeShopLayout] Applying mobile layout to Prestige Shop");

        // Find UpgradeContainer
        Transform upgradeContainer = transform.Find("UpgradeContainer");
        if (upgradeContainer != null)
        {
            RectTransform rt = upgradeContainer.GetComponent<RectTransform>();
            // Mobile: 1열 레이아웃을 위한 더 큰 세로 공간
            // 11개 업그레이드 x 160px = 1760px
            rt.sizeDelta = new Vector2(1000, 1800);
            rt.anchoredPosition = new Vector2(0, -200);  // 더 아래로 시작
            Debug.Log("[AdaptivePrestigeShopLayout] UpgradeContainer resized to 1000x1800 for mobile");
        }

        // Find Title
        Transform title = transform.Find("Title");
        if (title != null)
        {
            RectTransform rt = title.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -40);
            Debug.Log("[AdaptivePrestigeShopLayout] Title adjusted for mobile");
        }

        // Find PointsText
        Transform pointsText = transform.Find("PointsText");
        if (pointsText != null)
        {
            RectTransform rt = pointsText.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -110);
            Debug.Log("[AdaptivePrestigeShopLayout] PointsText adjusted for mobile");
        }
    }
}
