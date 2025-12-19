using UnityEngine;
using TMPro;

public class AdaptiveTextScaler : MonoBehaviour
{
    [SerializeField] private bool scaleChildren = true;

    void Start()
    {
        if (PlatformDetector.Instance == null)
        {
            Debug.LogWarning("[AdaptiveTextScaler] PlatformDetector not found, skipping text scaling.");
            return;
        }

        float scale = PlatformDetector.CurrentConfig.fontScaleMultiplier;

        if (scale == 1.0f)
        {
            // PC mode, no scaling needed
            return;
        }

        TextMeshProUGUI[] texts = scaleChildren ?
            GetComponentsInChildren<TextMeshProUGUI>(true) :
            new[] { GetComponent<TextMeshProUGUI>() };

        int scaledCount = 0;
        foreach (var text in texts)
        {
            if (text != null)
            {
                text.fontSize = text.fontSize * scale;
                scaledCount++;
            }
        }

        Debug.Log($"[AdaptiveTextScaler] Scaled {scaledCount} text elements by {scale}x in {gameObject.name}");
    }
}
