using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class AdaptiveCanvasScaler : MonoBehaviour
{
    void Awake()
    {
        if (PlatformDetector.Instance == null)
        {
            Debug.LogError("[AdaptiveCanvasScaler] PlatformDetector not found!");
            return;
        }

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        PlatformUIConfig config = PlatformDetector.CurrentConfig;

        scaler.referenceResolution = config.referenceResolution;
        scaler.matchWidthOrHeight = config.matchWidthOrHeight;

        Debug.Log($"[AdaptiveCanvasScaler] Canvas scaled to: {config.referenceResolution}, matchWidthOrHeight: {config.matchWidthOrHeight}");
    }
}
