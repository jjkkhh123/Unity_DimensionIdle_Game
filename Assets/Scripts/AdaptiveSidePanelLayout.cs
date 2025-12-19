using UnityEngine;

public class AdaptiveSidePanelLayout : MonoBehaviour
{
    void Start()
    {
        if (PlatformDetector.Instance == null || !PlatformDetector.IsMobile)
        {
            Debug.Log("[AdaptiveSidePanelLayout] PC mode or PlatformDetector not found, using default layout.");
            return;
        }

        PlatformUIConfig config = PlatformDetector.CurrentConfig;

        // Mobile layout:
        // - Tickspeed: Top center (below antimatter text)
        // - DimBoost: Left column, below Dimension 4
        // - Prestige: Right column, below Dimension 8
        RepositionPanel("TickspeedContainer", config.tickspeedPanelPosition, config.sidePanelSize);
        RepositionPanel("DimBoostContainer", config.dimBoostPanelPosition, config.sidePanelSize);
        RepositionPanel("PrestigeContainer", config.prestigePanelPosition, config.sidePanelSize);
    }

    void RepositionPanel(string panelName, Vector3 position, Vector2 size)
    {
        Transform panel = transform.Find(panelName);
        if (panel == null)
        {
            Debug.LogWarning($"[AdaptiveSidePanelLayout] {panelName} not found!");
            return;
        }

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Debug.Log($"[AdaptiveSidePanelLayout] {panelName} repositioned to {position}, size: {size}");
    }
}
