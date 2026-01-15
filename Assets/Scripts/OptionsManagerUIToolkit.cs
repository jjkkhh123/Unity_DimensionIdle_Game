using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Collections;

public class OptionsManagerUIToolkit : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;

    // Options panel elements
    private VisualElement optionsRoot;
    private Button exportSaveBtn;
    private Button importSaveBtn;
    private Button resetGameBtn;
    private Button quitGameBtn;
    private Button helpBtn;
    private Button tutorialBtn;
    private Label statusText;

    // Help panel (will be created dynamically)
    private VisualElement helpPanel;
    private bool isHelpVisible = false;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("[OptionsManagerUIToolkit] UIDocument not found!");
            return;
        }

        root = uiDocument.rootVisualElement;
        CacheUIElements();
        RegisterCallbacks();
    }

    void CacheUIElements()
    {
        optionsRoot = root.Q<VisualElement>("options-root");

        exportSaveBtn = root.Q<Button>("ExportSaveBtn");
        importSaveBtn = root.Q<Button>("ImportSaveBtn");
        resetGameBtn = root.Q<Button>("ResetGameBtn");
        quitGameBtn = root.Q<Button>("QuitGameBtn");
        helpBtn = root.Q<Button>("HelpBtn");
        tutorialBtn = root.Q<Button>("TutorialBtn");
        statusText = root.Q<Label>("OptionsStatusText");

        if (optionsRoot == null) Debug.LogError("[OptionsManagerUIToolkit] options-root not found!");
        if (exportSaveBtn == null) Debug.LogError("[OptionsManagerUIToolkit] ExportSaveBtn not found!");
        if (importSaveBtn == null) Debug.LogError("[OptionsManagerUIToolkit] ImportSaveBtn not found!");
        if (resetGameBtn == null) Debug.LogError("[OptionsManagerUIToolkit] ResetGameBtn not found!");
        if (quitGameBtn == null) Debug.LogError("[OptionsManagerUIToolkit] QuitGameBtn not found!");
        if (helpBtn == null) Debug.LogError("[OptionsManagerUIToolkit] HelpBtn not found!");
        if (tutorialBtn == null) Debug.LogError("[OptionsManagerUIToolkit] TutorialBtn not found!");
        if (statusText == null) Debug.LogError("[OptionsManagerUIToolkit] OptionsStatusText not found!");
    }

    void RegisterCallbacks()
    {
        if (exportSaveBtn != null)
            exportSaveBtn.clicked += ExportSave;

        if (importSaveBtn != null)
            importSaveBtn.clicked += ImportSave;

        if (resetGameBtn != null)
            resetGameBtn.clicked += ResetGame;

        if (quitGameBtn != null)
            quitGameBtn.clicked += QuitGame;

        if (helpBtn != null)
            helpBtn.clicked += ToggleHelp;

        if (tutorialBtn != null)
            tutorialBtn.clicked += ShowTutorial;
    }

    void ShowTutorial()
    {
        if (TutorialPopup.Instance != null)
        {
            TutorialPopup.Instance.ShowTutorial();
        }
        else
        {
            Debug.LogWarning("[OptionsManagerUIToolkit] TutorialPopup.Instance is null!");
        }
    }

    void ToggleHelp()
    {
        isHelpVisible = !isHelpVisible;

        if (isHelpVisible)
        {
            if (helpPanel == null)
            {
                CreateHelpPanel();
            }
            helpPanel.style.display = DisplayStyle.Flex;
            if (helpBtn != null)
                helpBtn.text = "HIDE HELP";
        }
        else
        {
            if (helpPanel != null)
                helpPanel.style.display = DisplayStyle.None;
            if (helpBtn != null)
                helpBtn.text = "SHOW HELP";
        }
    }

    void CreateHelpPanel()
    {
        helpPanel = new VisualElement();
        helpPanel.name = "help-panel";
        helpPanel.style.position = Position.Absolute;
        helpPanel.style.left = 0;
        helpPanel.style.top = 0;
        helpPanel.style.right = 0;
        helpPanel.style.bottom = 0;
        helpPanel.style.backgroundColor = new Color(0, 0, 0, 0.8f);
        helpPanel.style.alignItems = Align.Center;
        helpPanel.style.justifyContent = Justify.Center;

        // Content container
        var content = new VisualElement();
        content.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        content.style.borderTopLeftRadius = 12;
        content.style.borderTopRightRadius = 12;
        content.style.borderBottomLeftRadius = 12;
        content.style.borderBottomRightRadius = 12;
        content.style.paddingTop = 20;
        content.style.paddingBottom = 20;
        content.style.paddingLeft = 20;
        content.style.paddingRight = 20;
        content.style.width = new Length(80, LengthUnit.Percent);
        content.style.maxWidth = 800;

        // Title
        var title = new Label("Game Help");
        title.style.fontSize = 32;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.color = Color.white;
        title.style.unityTextAlign = TextAnchor.MiddleCenter;
        title.style.marginBottom = 20;
        content.Add(title);

        // Help sections
        AddHelpSection(content, "Dimensions", "Purchase dimensions to produce antimatter. Each dimension produces the one below it. Dimension 1 produces antimatter directly.");
        AddHelpSection(content, "Dimension Enhance", "Reset your progress but unlock new dimensions and boost production. Requires purchasing a certain amount of your highest dimension.");
        AddHelpSection(content, "Tickspeed", "Increases the speed at which all dimensions produce. Can be upgraded multiple times for exponential growth.");
        AddHelpSection(content, "Prestige", "Reset all progress to gain Prestige Points. Use these points to purchase permanent upgrades that persist through resets.");

        // Close button
        var closeBtn = new Button(() => ToggleHelp());
        closeBtn.text = "CLOSE";
        closeBtn.style.marginTop = 20;
        closeBtn.style.height = 50;
        closeBtn.style.fontSize = 20;
        closeBtn.style.backgroundColor = new Color(0.3f, 0.7f, 0.3f, 1f);
        closeBtn.style.color = Color.white;
        closeBtn.style.borderTopLeftRadius = 10;
        closeBtn.style.borderTopRightRadius = 10;
        closeBtn.style.borderBottomLeftRadius = 10;
        closeBtn.style.borderBottomRightRadius = 10;
        content.Add(closeBtn);

        helpPanel.Add(content);
        root.Add(helpPanel);
    }

    void AddHelpSection(VisualElement parent, string title, string description)
    {
        var section = new VisualElement();
        section.style.marginBottom = 15;
        section.style.paddingTop = 10;
        section.style.paddingBottom = 10;
        section.style.paddingLeft = 10;
        section.style.paddingRight = 10;
        section.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
        section.style.borderTopLeftRadius = 8;
        section.style.borderTopRightRadius = 8;
        section.style.borderBottomLeftRadius = 8;
        section.style.borderBottomRightRadius = 8;

        var titleLabel = new Label(title);
        titleLabel.style.fontSize = 24;
        titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        titleLabel.style.color = new Color(0.2f, 0.6f, 0.86f, 1f);
        titleLabel.style.marginBottom = 5;
        section.Add(titleLabel);

        var descLabel = new Label(description);
        descLabel.style.fontSize = 16;
        descLabel.style.color = new Color(1f, 1f, 1f, 0.8f);
        descLabel.style.whiteSpace = WhiteSpace.Normal;
        section.Add(descLabel);

        parent.Add(section);
    }

    void ExportSave()
    {
        Debug.Log("[ExportSave] Function called");
        if (SaveManager.Instance != null)
        {
            string saveString = SaveManager.Instance.ExportSaveToString();

            if (!string.IsNullOrEmpty(saveString))
            {
                // Copy to clipboard
                GUIUtility.systemCopyBuffer = saveString;

                // Show popup with save string
                ShowExportPopup(saveString);

                Debug.Log("[ExportSave] Save exported successfully");
            }
            else
            {
                ShowStatus("Export failed! No save data.");
                Debug.LogError("[ExportSave] Failed to export save string");
            }
        }
        else
        {
            Debug.LogError("[ExportSave] SaveManager.Instance is null!");
        }
    }

    void ImportSave()
    {
        Debug.Log("[ImportSave] Opening import dialog");
        ShowImportPopup();
    }

    void ResetGame()
    {
        Debug.Log("[ResetGame] Starting reset coroutine");
        StartCoroutine(ResetGameWithConfirmation());
    }

    IEnumerator ResetGameWithConfirmation()
    {
        Debug.Log("[ResetGame] Coroutine started");
        bool? confirmed = null;
        Debug.Log("[ResetGame] Showing confirmation dialog");
        ShowConfirmDialog("Reset Game?", "This will DELETE all save data and restart the game. Are you sure?", (result) => {
            Debug.Log($"[ResetGame] Callback invoked with result: {result}");
            confirmed = result;
        });

        Debug.Log("[ResetGame] Waiting for user confirmation...");
        yield return new WaitUntil(() => confirmed.HasValue);

        Debug.Log($"[ResetGame] Confirmation received: {confirmed}");

        if (confirmed == false)
        {
            ShowStatus("Reset cancelled.");
            Debug.Log("[ResetGame] User cancelled reset");
            yield break;
        }

        if (SaveManager.Instance != null)
        {
            Debug.Log("[ResetGame] Deleting save data...");
            SaveManager.Instance.DeleteSave();
            ShowStatus("Game reset! Reloading...");
            Debug.Log("[ResetGame] Save deleted! Destroying all managers...");

            if (GameManager.Instance != null)
            {
                Destroy(GameManager.Instance.gameObject);
                Debug.Log("[ResetGame] GameManager destroyed");
            }
            if (PrestigeManager.Instance != null)
            {
                Destroy(PrestigeManager.Instance.gameObject);
                Debug.Log("[ResetGame] PrestigeManager destroyed");
            }
            if (TickSpeedManager.Instance != null)
            {
                Destroy(TickSpeedManager.Instance.gameObject);
                Debug.Log("[ResetGame] TickSpeedManager destroyed");
            }
            if (SaveManager.Instance != null)
            {
                Destroy(SaveManager.Instance.gameObject);
                Debug.Log("[ResetGame] SaveManager destroyed");
            }

            yield return new WaitForSeconds(0.1f);

            Debug.Log("[ResetGame] Calling LoadScene...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
        }
        else
        {
            Debug.LogError("[ResetGame] SaveManager.Instance is null!");
        }
    }

    void QuitGame()
    {
        Debug.Log("[QuitGame] Quit button clicked");
        StartCoroutine(QuitGameWithConfirmation());
    }

    IEnumerator QuitGameWithConfirmation()
    {
        Debug.Log("[QuitGame] Showing confirmation dialog");
        bool? confirmed = null;
        ShowConfirmDialog("Quit Game?", "Are you sure you want to quit?", (result) => {
            Debug.Log($"[QuitGame] Callback invoked with result: {result}");
            confirmed = result;
        });

        yield return new WaitUntil(() => confirmed.HasValue);

        Debug.Log($"[QuitGame] Confirmation received: {confirmed}");

        if (confirmed == false)
        {
            ShowStatus("Quit cancelled.");
            Debug.Log("[QuitGame] User cancelled quit");
            yield break;
        }

        Debug.Log("[QuitGame] Quitting application...");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            CancelInvoke("ClearStatus");
            Invoke("ClearStatus", 5f);
        }
    }

    void ClearStatus()
    {
        if (statusText != null)
            statusText.text = "";
    }

    void ShowConfirmDialog(string title, string message, System.Action<bool> callback)
    {
        Debug.Log($"[ShowConfirmDialog] Creating dialog: {title}");

        // Create overlay
        var overlay = new VisualElement();
        overlay.name = "confirm-dialog-overlay";
        overlay.style.position = Position.Absolute;
        overlay.style.left = 0;
        overlay.style.top = 0;
        overlay.style.right = 0;
        overlay.style.bottom = 0;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.8f);
        overlay.style.alignItems = Align.Center;
        overlay.style.justifyContent = Justify.Center;

        // Dialog panel
        var panel = new VisualElement();
        panel.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        panel.style.borderTopLeftRadius = 12;
        panel.style.borderTopRightRadius = 12;
        panel.style.borderBottomLeftRadius = 12;
        panel.style.borderBottomRightRadius = 12;
        panel.style.paddingTop = 20;
        panel.style.paddingBottom = 20;
        panel.style.paddingLeft = 30;
        panel.style.paddingRight = 30;
        panel.style.width = 500;
        panel.style.minHeight = 250;

        // Title
        var titleLabel = new Label(title);
        titleLabel.style.fontSize = 32;
        titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        titleLabel.style.color = Color.white;
        titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        titleLabel.style.marginBottom = 20;
        panel.Add(titleLabel);

        // Message
        var messageLabel = new Label(message);
        messageLabel.style.fontSize = 20;
        messageLabel.style.color = Color.white;
        messageLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        messageLabel.style.whiteSpace = WhiteSpace.Normal;
        messageLabel.style.marginBottom = 30;
        panel.Add(messageLabel);

        // Button container
        var buttonContainer = new VisualElement();
        buttonContainer.style.flexDirection = FlexDirection.Row;
        buttonContainer.style.justifyContent = Justify.Center;
        panel.Add(buttonContainer);

        // Yes button
        var yesBtn = new Button(() => {
            Debug.Log("[ShowConfirmDialog] YES button clicked");
            callback(true);
            Debug.Log("[ShowConfirmDialog] Destroying dialog");
            root.Remove(overlay);
        });
        yesBtn.text = "YES";
        yesBtn.style.width = 120;
        yesBtn.style.height = 40;
        yesBtn.style.fontSize = 24;
        yesBtn.style.marginRight = 20;
        yesBtn.style.backgroundColor = new Color(0.3f, 0.7f, 0.3f, 1f);
        yesBtn.style.color = Color.white;
        yesBtn.style.borderTopLeftRadius = 8;
        yesBtn.style.borderTopRightRadius = 8;
        yesBtn.style.borderBottomLeftRadius = 8;
        yesBtn.style.borderBottomRightRadius = 8;
        buttonContainer.Add(yesBtn);

        // No button
        var noBtn = new Button(() => {
            Debug.Log("[ShowConfirmDialog] NO button clicked");
            callback(false);
            Debug.Log("[ShowConfirmDialog] Destroying dialog");
            root.Remove(overlay);
        });
        noBtn.text = "NO";
        noBtn.style.width = 120;
        noBtn.style.height = 40;
        noBtn.style.fontSize = 24;
        noBtn.style.backgroundColor = new Color(0.7f, 0.3f, 0.3f, 1f);
        noBtn.style.color = Color.white;
        noBtn.style.borderTopLeftRadius = 8;
        noBtn.style.borderTopRightRadius = 8;
        noBtn.style.borderBottomLeftRadius = 8;
        noBtn.style.borderBottomRightRadius = 8;
        buttonContainer.Add(noBtn);

        overlay.Add(panel);
        root.Add(overlay);

        Debug.Log("[ShowConfirmDialog] Dialog created successfully");
    }

    void ShowExportPopup(string saveString)
    {
        Debug.Log("[ShowExportPopup] Creating export popup");

        // Create overlay
        var overlay = new VisualElement();
        overlay.name = "export-popup-overlay";
        overlay.style.position = Position.Absolute;
        overlay.style.left = 0;
        overlay.style.top = 0;
        overlay.style.right = 0;
        overlay.style.bottom = 0;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.8f);
        overlay.style.alignItems = Align.Center;
        overlay.style.justifyContent = Justify.Center;

        // Dialog panel
        var panel = new VisualElement();
        panel.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        panel.style.borderTopLeftRadius = 12;
        panel.style.borderTopRightRadius = 12;
        panel.style.borderBottomLeftRadius = 12;
        panel.style.borderBottomRightRadius = 12;
        panel.style.paddingTop = 20;
        panel.style.paddingBottom = 20;
        panel.style.paddingLeft = 30;
        panel.style.paddingRight = 30;
        panel.style.width = new Length(80, LengthUnit.Percent);
        panel.style.maxWidth = 900;
        panel.style.maxHeight = new Length(80, LengthUnit.Percent);

        // Title
        var titleLabel = new Label("Save Exported!");
        titleLabel.style.fontSize = 32;
        titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        titleLabel.style.color = Color.white;
        titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        titleLabel.style.marginBottom = 10;
        panel.Add(titleLabel);

        // Info message
        var infoLabel = new Label("Save string copied to clipboard! You can also manually copy it below:");
        infoLabel.style.fontSize = 18;
        infoLabel.style.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        infoLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        infoLabel.style.whiteSpace = WhiteSpace.Normal;
        infoLabel.style.marginBottom = 15;
        panel.Add(infoLabel);

        // Text field with save string
        var textField = new TextField();
        textField.isReadOnly = true;
        textField.multiline = true;
        textField.value = saveString;
        textField.style.height = 200;
        textField.style.marginBottom = 20;
        textField.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        textField.style.color = Color.white;
        textField.style.fontSize = 14;
        textField.style.whiteSpace = WhiteSpace.Normal;
        textField.style.unityTextAlign = TextAnchor.UpperLeft;
        panel.Add(textField);

        // Button container
        var buttonContainer = new VisualElement();
        buttonContainer.style.flexDirection = FlexDirection.Row;
        buttonContainer.style.justifyContent = Justify.Center;
        panel.Add(buttonContainer);

        // Copy button
        var copyBtn = new Button(() => {
            GUIUtility.systemCopyBuffer = saveString;
            ShowStatus("Copied to clipboard!");
            Debug.Log("[ShowExportPopup] Copied to clipboard");
        });
        copyBtn.text = "COPY";
        copyBtn.style.width = 120;
        copyBtn.style.height = 40;
        copyBtn.style.fontSize = 20;
        copyBtn.style.marginRight = 10;
        copyBtn.style.backgroundColor = new Color(0.2f, 0.6f, 0.86f, 1f);
        copyBtn.style.color = Color.white;
        copyBtn.style.borderTopLeftRadius = 8;
        copyBtn.style.borderTopRightRadius = 8;
        copyBtn.style.borderBottomLeftRadius = 8;
        copyBtn.style.borderBottomRightRadius = 8;
        buttonContainer.Add(copyBtn);

        // Close button
        var closeBtn = new Button(() => {
            Debug.Log("[ShowExportPopup] Close button clicked");
            root.Remove(overlay);
        });
        closeBtn.text = "CLOSE";
        closeBtn.style.width = 120;
        closeBtn.style.height = 40;
        closeBtn.style.fontSize = 20;
        closeBtn.style.backgroundColor = new Color(0.3f, 0.7f, 0.3f, 1f);
        closeBtn.style.color = Color.white;
        closeBtn.style.borderTopLeftRadius = 8;
        closeBtn.style.borderTopRightRadius = 8;
        closeBtn.style.borderBottomLeftRadius = 8;
        closeBtn.style.borderBottomRightRadius = 8;
        buttonContainer.Add(closeBtn);

        overlay.Add(panel);
        root.Add(overlay);

        Debug.Log("[ShowExportPopup] Export popup created successfully");
    }

    void ShowImportPopup()
    {
        Debug.Log("[ShowImportPopup] Creating import popup");

        // Create overlay
        var overlay = new VisualElement();
        overlay.name = "import-popup-overlay";
        overlay.style.position = Position.Absolute;
        overlay.style.left = 0;
        overlay.style.top = 0;
        overlay.style.right = 0;
        overlay.style.bottom = 0;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.8f);
        overlay.style.alignItems = Align.Center;
        overlay.style.justifyContent = Justify.Center;

        // Dialog panel
        var panel = new VisualElement();
        panel.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        panel.style.borderTopLeftRadius = 12;
        panel.style.borderTopRightRadius = 12;
        panel.style.borderBottomLeftRadius = 12;
        panel.style.borderBottomRightRadius = 12;
        panel.style.paddingTop = 20;
        panel.style.paddingBottom = 20;
        panel.style.paddingLeft = 30;
        panel.style.paddingRight = 30;
        panel.style.width = new Length(80, LengthUnit.Percent);
        panel.style.maxWidth = 900;
        panel.style.maxHeight = new Length(80, LengthUnit.Percent);

        // Title
        var titleLabel = new Label("Import Save");
        titleLabel.style.fontSize = 32;
        titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        titleLabel.style.color = Color.white;
        titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        titleLabel.style.marginBottom = 10;
        panel.Add(titleLabel);

        // Info message
        var infoLabel = new Label("Paste your save string below:");
        infoLabel.style.fontSize = 18;
        infoLabel.style.color = new Color(1f, 1f, 1f, 0.8f);
        infoLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        infoLabel.style.whiteSpace = WhiteSpace.Normal;
        infoLabel.style.marginBottom = 15;
        panel.Add(infoLabel);

        // Text field for input
        var textField = new TextField();
        textField.multiline = true;
        textField.value = "";
        textField.style.height = 200;
        textField.style.marginBottom = 20;
        textField.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        textField.style.color = Color.white;
        textField.style.fontSize = 14;
        textField.style.whiteSpace = WhiteSpace.Normal;
        textField.style.unityTextAlign = TextAnchor.UpperLeft;
        panel.Add(textField);

        // Warning message
        var warningLabel = new Label("⚠ This will overwrite your current save!");
        warningLabel.style.fontSize = 16;
        warningLabel.style.color = new Color(1f, 0.5f, 0f, 1f);
        warningLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        warningLabel.style.marginBottom = 15;
        panel.Add(warningLabel);

        // Button container
        var buttonContainer = new VisualElement();
        buttonContainer.style.flexDirection = FlexDirection.Row;
        buttonContainer.style.justifyContent = Justify.Center;
        panel.Add(buttonContainer);

        // Import button
        var importBtn = new Button(() => {
            Debug.Log("[ShowImportPopup] Import button clicked");
            string saveString = textField.value;

            if (string.IsNullOrEmpty(saveString))
            {
                ShowStatus("Please paste a save string!");
                return;
            }

            if (SaveManager.Instance != null)
            {
                bool success = SaveManager.Instance.ImportSaveFromString(saveString);

                if (success)
                {
                    root.Remove(overlay);
                    ShowStatus("Import successful! Reloading...");
                    Debug.Log("[ShowImportPopup] Import successful, reloading scene");
                    StartCoroutine(ReloadSceneAfterDelay());
                }
                else
                {
                    ShowStatus("Import failed! Invalid save string.");
                    Debug.LogError("[ShowImportPopup] Import failed");
                }
            }
            else
            {
                ShowStatus("SaveManager not found!");
                Debug.LogError("[ShowImportPopup] SaveManager.Instance is null");
            }
        });
        importBtn.text = "IMPORT";
        importBtn.style.width = 120;
        importBtn.style.height = 40;
        importBtn.style.fontSize = 20;
        importBtn.style.marginRight = 10;
        importBtn.style.backgroundColor = new Color(0.2f, 0.6f, 0.86f, 1f);
        importBtn.style.color = Color.white;
        importBtn.style.borderTopLeftRadius = 8;
        importBtn.style.borderTopRightRadius = 8;
        importBtn.style.borderBottomLeftRadius = 8;
        importBtn.style.borderBottomRightRadius = 8;
        buttonContainer.Add(importBtn);

        // Cancel button
        var cancelBtn = new Button(() => {
            Debug.Log("[ShowImportPopup] Cancel button clicked");
            root.Remove(overlay);
            ShowStatus("Import cancelled.");
        });
        cancelBtn.text = "CANCEL";
        cancelBtn.style.width = 120;
        cancelBtn.style.height = 40;
        cancelBtn.style.fontSize = 20;
        cancelBtn.style.backgroundColor = new Color(0.7f, 0.3f, 0.3f, 1f);
        cancelBtn.style.color = Color.white;
        cancelBtn.style.borderTopLeftRadius = 8;
        cancelBtn.style.borderTopRightRadius = 8;
        cancelBtn.style.borderBottomLeftRadius = 8;
        cancelBtn.style.borderBottomRightRadius = 8;
        buttonContainer.Add(cancelBtn);

        overlay.Add(panel);
        root.Add(overlay);

        Debug.Log("[ShowImportPopup] Import popup created successfully");
    }

    IEnumerator ReloadSceneAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("[ReloadSceneAfterDelay] Reloading scene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
}
