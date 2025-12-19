using UnityEngine;

public class PlatformDetector : MonoBehaviour
{
    public static PlatformDetector Instance { get; private set; }

    [Header("Configuration")]
    public PlatformUIConfig pcConfig;
    public PlatformUIConfig mobileConfig;

    [Header("Editor Testing")]
    [Tooltip("Manual: Use simulateMobile checkbox | Auto: Detect from Game View resolution")]
    public bool useManualOverride = false;
    [Tooltip("Only used when useManualOverride is true")]
    public bool simulateMobile = false;

    public static bool IsMobile { get; private set; }
    public static PlatformUIConfig CurrentConfig { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        DetectPlatform();
    }

    void DetectPlatform()
    {
        #if UNITY_EDITOR
        if (useManualOverride)
        {
            // Manual mode: Use checkbox
            IsMobile = simulateMobile;
            Debug.Log($"[PlatformDetector] Manual override: {(IsMobile ? "Mobile" : "PC")}");
        }
        else
        {
            // Auto mode: Detect from Game View resolution
            IsMobile = Screen.height > Screen.width;
            Debug.Log($"[PlatformDetector] Auto-detected from Game View: {Screen.width}x{Screen.height} → {(IsMobile ? "Mobile (Portrait)" : "PC (Landscape)")}");
        }
        #else
        // Build: Detect from platform and screen orientation
        IsMobile = Application.isMobilePlatform || Screen.height > Screen.width;
        Debug.Log($"[PlatformDetector] Build detected: {(IsMobile ? "Mobile" : "PC")}");
        #endif

        CurrentConfig = IsMobile ? mobileConfig : pcConfig;
        Debug.Log($"[PlatformDetector] Using config: {CurrentConfig.referenceResolution}");
    }
}
