using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using TMPro;

public class AdaptiveUISetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Adaptive UI System")]
    static void SetupAdaptiveUI()
    {
        Debug.Log("[AdaptiveUISetup] Starting Adaptive UI setup...");

        // Step 1: Create Config Assets
        CreateConfigAssets();

        // Step 2: Set Script Execution Order
        SetScriptExecutionOrder();

        // Step 3: Attach Components to Scene Objects
        AttachAdaptiveComponents();

        Debug.Log("[AdaptiveUISetup] ✅ Adaptive UI setup complete!");
        EditorUtility.DisplayDialog("Adaptive UI Setup", "Adaptive UI system has been set up successfully!\n\n" +
            "Created:\n" +
            "- PC_Config asset\n" +
            "- Mobile_Config asset\n" +
            "- Script execution order\n" +
            "- Adaptive components on scene objects\n" +
            "  (DimensionLayout, SidePanelLayout, TextScaler, MainPanelLayout,\n" +
            "   PrestigePanelLayout, DimBoostPanelLayout, TickspeedPanelLayout)\n\n" +
            "Testing:\n" +
            "- Auto mode (default): Change Game View resolution to Portrait/Landscape\n" +
            "- Manual mode: Enable 'Use Manual Override' and toggle 'Simulate Mobile'", "OK");
    }

    static void CreateConfigAssets()
    {
        Debug.Log("[AdaptiveUISetup] Creating Config assets...");

        // Create Resources/PlatformConfigs folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/PlatformConfigs"))
            AssetDatabase.CreateFolder("Assets/Resources", "PlatformConfigs");

        // Create PC Config
        PlatformUIConfig pcConfig = ScriptableObject.CreateInstance<PlatformUIConfig>();
        pcConfig.referenceResolution = new Vector2(1920, 1080);
        pcConfig.matchWidthOrHeight = 0.5f;
        pcConfig.fontScaleMultiplier = 1.0f;
        pcConfig.buttonSizeMultiplier = 1.0f;
        pcConfig.dimensionButtonSize = new Vector2(550, 200);
        pcConfig.dimensionButtonColumnPositions = new Vector2[] { new Vector2(-600, 0), new Vector2(0, 0) };
        pcConfig.dimensionButtonRowSpacing = 225f;
        pcConfig.sidePanelSize = new Vector2(300, 300);
        pcConfig.prestigePanelPosition = new Vector3(775, -300, 0);
        pcConfig.tickspeedPanelPosition = new Vector3(450, -300, 0);
        pcConfig.dimBoostPanelPosition = new Vector3(450, 25, 0);

        AssetDatabase.CreateAsset(pcConfig, "Assets/Resources/PlatformConfigs/PC_Config.asset");
        Debug.Log("[AdaptiveUISetup] Created PC_Config.asset");

        // Create Mobile Config
        // ContentArea height ~1767 (92% of 1920), center at ~883.5 from bottom
        // Available range: -883 to +883 from ContentArea center
        // 목표: 세로 공간을 최대한 활용하여 화면 꽉 채우기
        PlatformUIConfig mobileConfig = ScriptableObject.CreateInstance<PlatformUIConfig>();
        mobileConfig.referenceResolution = new Vector2(1080, 1920);
        mobileConfig.matchWidthOrHeight = 0.0f;
        mobileConfig.fontScaleMultiplier = 1.05f;  // 1.0 → 1.05 (약간의 폰트 확대로 가독성 향상)
        mobileConfig.buttonSizeMultiplier = 1.3f;
        mobileConfig.dimensionButtonSize = new Vector2(500, 270);  // 250 → 270 (높이 20px 추가 증가)
        mobileConfig.dimensionButtonColumnPositions = new Vector2[] { new Vector2(-270, 0), new Vector2(270, 0) };
        mobileConfig.dimensionButtonRowSpacing = 260f;  // 240 → 260 (간격 20px 추가 증가)
        mobileConfig.sidePanelSize = new Vector2(500, 240);  // 220 → 240 (높이 20px 추가 증가)
        mobileConfig.tickspeedPanelPosition = new Vector3(0, 640, 0);      // 680 → 640 (/sec와 간격 확보)
        mobileConfig.dimBoostPanelPosition = new Vector3(-270, -690, 0);   // -600 → -690 (더 아래로)
        mobileConfig.prestigePanelPosition = new Vector3(270, -690, 0);    // -600 → -690 (더 아래로)

        AssetDatabase.CreateAsset(mobileConfig, "Assets/Resources/PlatformConfigs/Mobile_Config.asset");
        Debug.Log("[AdaptiveUISetup] Created Mobile_Config.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void SetScriptExecutionOrder()
    {
        Debug.Log("[AdaptiveUISetup] Setting script execution order...");

        SetExecutionOrder("PlatformDetector", -100);
        SetExecutionOrder("AdaptiveCanvasScaler", -50);

        Debug.Log("[AdaptiveUISetup] Script execution order set: PlatformDetector=-100, AdaptiveCanvasScaler=-50");
    }

    static void SetExecutionOrder(string scriptName, int executionOrder)
    {
        foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
        {
            if (monoScript.name == scriptName)
            {
                if (MonoImporter.GetExecutionOrder(monoScript) != executionOrder)
                {
                    MonoImporter.SetExecutionOrder(monoScript, executionOrder);
                }
                return;
            }
        }
    }

    static void AttachAdaptiveComponents()
    {
        Debug.Log("[AdaptiveUISetup] Attaching adaptive components to scene objects...");

        Scene scene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = scene.GetRootGameObjects();

        GameObject gameManager = FindObjectByName(rootObjects, "GameManager");
        GameObject canvas = FindObjectByName(rootObjects, "Canvas");

        if (gameManager == null)
        {
            Debug.LogError("[AdaptiveUISetup] GameManager not found in scene!");
            return;
        }

        if (canvas == null)
        {
            Debug.LogError("[AdaptiveUISetup] Canvas not found in scene!");
            return;
        }

        // Attach PlatformDetector to GameManager
        PlatformDetector detector = gameManager.GetComponent<PlatformDetector>();
        if (detector == null)
        {
            detector = gameManager.AddComponent<PlatformDetector>();
            Debug.Log("[AdaptiveUISetup] Added PlatformDetector to GameManager");
        }

        // Load and assign config assets
        PlatformUIConfig pcConfig = AssetDatabase.LoadAssetAtPath<PlatformUIConfig>("Assets/Resources/PlatformConfigs/PC_Config.asset");
        PlatformUIConfig mobileConfig = AssetDatabase.LoadAssetAtPath<PlatformUIConfig>("Assets/Resources/PlatformConfigs/Mobile_Config.asset");

        detector.pcConfig = pcConfig;
        detector.mobileConfig = mobileConfig;
        detector.useManualOverride = false;  // Auto-detect from Game View by default
        detector.simulateMobile = false;
        EditorUtility.SetDirty(detector);
        Debug.Log("[AdaptiveUISetup] Assigned configs to PlatformDetector");

        // Attach AdaptiveCanvasScaler to Canvas
        if (canvas.GetComponent<AdaptiveCanvasScaler>() == null)
        {
            canvas.AddComponent<AdaptiveCanvasScaler>();
            Debug.Log("[AdaptiveUISetup] Added AdaptiveCanvasScaler to Canvas");
        }

        // Find UI elements
        Transform contentArea = FindChildRecursive(canvas.transform, "ContentArea");
        if (contentArea == null)
        {
            Debug.LogError("[AdaptiveUISetup] ContentArea not found!");
            return;
        }

        Transform dimensionsPanel = FindChildRecursive(contentArea, "DimensionsPanel");
        Transform mainPanel = FindChildRecursive(contentArea, "MainPanel");
        Transform tabBar = FindChildRecursive(canvas.transform, "TabBar");

        // Attach to DimensionsPanel
        if (dimensionsPanel != null)
        {
            if (dimensionsPanel.GetComponent<AdaptiveDimensionLayout>() == null)
            {
                dimensionsPanel.gameObject.AddComponent<AdaptiveDimensionLayout>();
                Debug.Log("[AdaptiveUISetup] Added AdaptiveDimensionLayout to DimensionsPanel");
            }

            if (dimensionsPanel.GetComponent<AdaptiveSidePanelLayout>() == null)
            {
                dimensionsPanel.gameObject.AddComponent<AdaptiveSidePanelLayout>();
                Debug.Log("[AdaptiveUISetup] Added AdaptiveSidePanelLayout to DimensionsPanel");
            }

            // Add AdaptiveTextScaler to each Dimension panel
            for (int i = 1; i <= 8; i++)
            {
                Transform dimPanel = dimensionsPanel.Find($"Dimension{i}Panel");
                if (dimPanel != null && dimPanel.GetComponent<AdaptiveTextScaler>() == null)
                {
                    AdaptiveTextScaler scaler = dimPanel.gameObject.AddComponent<AdaptiveTextScaler>();
                    // Use reflection to set private field
                    var field = typeof(AdaptiveTextScaler).GetField("scaleChildren", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                        field.SetValue(scaler, true);
                    EditorUtility.SetDirty(dimPanel.gameObject);
                    Debug.Log($"[AdaptiveUISetup] Added AdaptiveTextScaler to Dimension{i}Panel");
                }
            }
        }

        // Attach to MainPanel
        if (mainPanel != null)
        {
            if (mainPanel.GetComponent<AdaptiveTextScaler>() == null)
            {
                AdaptiveTextScaler scaler = mainPanel.gameObject.AddComponent<AdaptiveTextScaler>();
                var field = typeof(AdaptiveTextScaler).GetField("scaleChildren", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                    field.SetValue(scaler, true);
                Debug.Log("[AdaptiveUISetup] Added AdaptiveTextScaler to MainPanel");
            }

            if (mainPanel.GetComponent<AdaptiveMainPanelLayout>() == null)
            {
                mainPanel.gameObject.AddComponent<AdaptiveMainPanelLayout>();
                Debug.Log("[AdaptiveUISetup] Added AdaptiveMainPanelLayout to MainPanel");
            }

            EditorUtility.SetDirty(mainPanel.gameObject);
        }

        // Attach to PrestigeContainer
        if (dimensionsPanel != null)
        {
            Transform prestigeContainer = FindChildRecursive(dimensionsPanel, "PrestigeContainer");
            if (prestigeContainer != null && prestigeContainer.GetComponent<AdaptivePrestigePanelLayout>() == null)
            {
                prestigeContainer.gameObject.AddComponent<AdaptivePrestigePanelLayout>();
                EditorUtility.SetDirty(prestigeContainer.gameObject);
                Debug.Log("[AdaptiveUISetup] Added AdaptivePrestigePanelLayout to PrestigeContainer");
            }

            Transform dimBoostContainer = FindChildRecursive(dimensionsPanel, "DimBoostContainer");
            if (dimBoostContainer != null && dimBoostContainer.GetComponent<AdaptiveDimBoostPanelLayout>() == null)
            {
                dimBoostContainer.gameObject.AddComponent<AdaptiveDimBoostPanelLayout>();
                EditorUtility.SetDirty(dimBoostContainer.gameObject);
                Debug.Log("[AdaptiveUISetup] Added AdaptiveDimBoostPanelLayout to DimBoostContainer");
            }

            Transform tickspeedContainer = FindChildRecursive(dimensionsPanel, "TickspeedContainer");
            if (tickspeedContainer != null && tickspeedContainer.GetComponent<AdaptiveTickspeedPanelLayout>() == null)
            {
                tickspeedContainer.gameObject.AddComponent<AdaptiveTickspeedPanelLayout>();
                EditorUtility.SetDirty(tickspeedContainer.gameObject);
                Debug.Log("[AdaptiveUISetup] Added AdaptiveTickspeedPanelLayout to TickspeedContainer");
            }
        }

        // Attach to TabBar
        if (tabBar != null && tabBar.GetComponent<AdaptiveTextScaler>() == null)
        {
            AdaptiveTextScaler scaler = tabBar.gameObject.AddComponent<AdaptiveTextScaler>();
            var field = typeof(AdaptiveTextScaler).GetField("scaleChildren", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(scaler, true);
            EditorUtility.SetDirty(tabBar.gameObject);
            Debug.Log("[AdaptiveUISetup] Added AdaptiveTextScaler to TabBar");
        }

        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("[AdaptiveUISetup] Scene saved");
    }

    static GameObject FindObjectByName(GameObject[] rootObjects, string name)
    {
        foreach (GameObject obj in rootObjects)
        {
            if (obj.name == name)
                return obj;
        }
        return null;
    }

    static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}
