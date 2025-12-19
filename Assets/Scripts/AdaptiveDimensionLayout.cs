using UnityEngine;

public class AdaptiveDimensionLayout : MonoBehaviour
{
    void Start()
    {
        if (PlatformDetector.Instance == null || !PlatformDetector.IsMobile)
        {
            Debug.Log("[AdaptiveDimensionLayout] PC mode or PlatformDetector not found, using default layout.");
            return;
        }

        PlatformUIConfig config = PlatformDetector.CurrentConfig;

        // Find all 8 dimension button panels
        // Mobile layout: Left column (1-4), Right column (5-8)
        for (int i = 1; i <= 8; i++)
        {
            Transform dimPanel = transform.Find($"Dimension{i}Panel");
            if (dimPanel == null)
            {
                Debug.LogWarning($"[AdaptiveDimensionLayout] Dimension{i}Panel not found!");
                continue;
            }

            RectTransform rt = dimPanel.GetComponent<RectTransform>();

            // Determine column and row
            int col = (i <= 4) ? 0 : 1; // Left column: 1-4, Right column: 5-8
            int row = (i <= 4) ? (i - 1) : (i - 5); // Row 0-3 for each column

            Vector2 colPosition = config.dimensionButtonColumnPositions[col];
            float yPos = 420 - (row * config.dimensionButtonRowSpacing);  // 350 → 420 (더 위에서 시작)

            rt.anchoredPosition = new Vector2(colPosition.x, yPos);
            rt.sizeDelta = config.dimensionButtonSize;

            Debug.Log($"[AdaptiveDimensionLayout] Dimension {i} repositioned to ({colPosition.x}, {yPos}), size: {config.dimensionButtonSize}");
        }
    }
}
