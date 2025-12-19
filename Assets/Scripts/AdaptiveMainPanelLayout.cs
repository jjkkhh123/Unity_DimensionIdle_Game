using UnityEngine;

public class AdaptiveMainPanelLayout : MonoBehaviour
{
    void Start()
    {
        if (PlatformDetector.Instance == null || !PlatformDetector.IsMobile)
        {
            Debug.Log("[AdaptiveMainPanelLayout] PC mode or PlatformDetector not found, using default layout.");
            return;
        }

        // Mobile: Center the antimatter texts and move them to top
        Transform antimatterText = transform.Find("AntimatterText");
        Transform antimatterPerSecText = transform.Find("AntimatterPerSecondText");

        if (antimatterText != null)
        {
            RectTransform rt = antimatterText.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 870);  // 850 → 870 (더 위로)
            Debug.Log("[AdaptiveMainPanelLayout] AntimatterText repositioned to (0, 870)");
        }

        if (antimatterPerSecText != null)
        {
            RectTransform rt = antimatterPerSecText.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 815);  // 770 → 815 (더 위로, Tickspeed와 간격 확보)
            Debug.Log("[AdaptiveMainPanelLayout] AntimatterPerSecondText repositioned to (0, 815)");
        }
    }
}
