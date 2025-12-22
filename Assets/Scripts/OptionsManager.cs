using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class OptionsManager : MonoBehaviour
{
    [Header("Help")]
    public Button helpButton;
    public GameObject helpPanel;
    public TextMeshProUGUI helpButtonText;

    [Header("Save/Load")]
    public Button exportButton;
    public Button importButton;
    public Button resetButton;
    public Button quitButton;
    public TextMeshProUGUI statusText;

    private bool isHelpVisible = false;

    void Start()
    {
        if (helpButton != null)
            helpButton.onClick.AddListener(ToggleHelp);

        if (exportButton != null)
            exportButton.onClick.AddListener(ExportSave);

        if (importButton != null)
            importButton.onClick.AddListener(ImportSave);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    void ToggleHelp()
    {
        isHelpVisible = !isHelpVisible;
        if (helpPanel != null)
        {
            helpPanel.SetActive(isHelpVisible);

            // Help 패널을 화면 최상단(맨 앞)에 배치
            if (isHelpVisible)
            {
                helpPanel.transform.SetAsLastSibling();
            }
        }

        if (helpButtonText != null)
            helpButtonText.text = isHelpVisible ? "Hide Help" : "Show Help";
    }

    void ExportSave()
    {
        Debug.Log("[ExportSave] Function called");
        if (SaveManager.Instance != null)
        {
            Debug.Log("[ExportSave] SaveManager exists, calling SaveGame");
            SaveManager.Instance.SaveGame();

            string saveFilePath = Path.Combine(Application.persistentDataPath, "antimatter_save.json");
            string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            string exportPath = Path.Combine(desktopPath, "antimatter_save_export.json");

            Debug.Log($"[ExportSave] Save file path: {saveFilePath}");
            Debug.Log($"[ExportSave] Export path: {exportPath}");
            Debug.Log($"[ExportSave] File exists: {File.Exists(saveFilePath)}");

            if (File.Exists(saveFilePath))
            {
                try
                {
                    File.Copy(saveFilePath, exportPath, true);
                    ShowStatus($"Exported to Desktop!\n{exportPath}");
                    Debug.Log($"[ExportSave] SUCCESS: Save exported to: {exportPath}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[ExportSave] ERROR copying file: {e.Message}");
                    ShowStatus($"Export failed: {e.Message}");
                }
            }
            else
            {
                ShowStatus("No save file found!");
                Debug.LogWarning("[ExportSave] No save file found");
            }
        }
        else
        {
            Debug.LogError("[ExportSave] SaveManager.Instance is null!");
        }
    }

    void ImportSave()
    {
        Debug.Log("[ImportSave] Starting import coroutine");
        StartCoroutine(ImportSaveWithConfirmation());
    }

    System.Collections.IEnumerator ImportSaveWithConfirmation()
    {
        Debug.Log("[ImportSave] Coroutine started");
        // 확인 대화창 표시
        bool? confirmed = null;
        Debug.Log("[ImportSave] Showing confirmation dialog");
        ShowConfirmDialog("Import Save?", "This will load the save file from Desktop and restart the game. Continue?", (result) => {
            Debug.Log($"[ImportSave] Callback invoked with result: {result}");
            confirmed = result;
        });

        Debug.Log("[ImportSave] Waiting for user confirmation...");
        yield return new WaitUntil(() => confirmed.HasValue);

        Debug.Log($"[ImportSave] Confirmation received: {confirmed}");

        if (confirmed == false)
        {
            ShowStatus("Import cancelled.");
            Debug.Log("[ImportSave] User cancelled import");
            yield break;
        }

        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string importPath = Path.Combine(desktopPath, "antimatter_save_export.json");
        string saveFilePath = Path.Combine(Application.persistentDataPath, "antimatter_save.json");

        Debug.Log($"[ImportSave] Import path: {importPath}");
        Debug.Log($"[ImportSave] Save path: {saveFilePath}");
        Debug.Log($"[ImportSave] File exists: {File.Exists(importPath)}");

        if (File.Exists(importPath))
        {
            bool copySuccess = false;
            try
            {
                File.Copy(importPath, saveFilePath, true);
                Debug.Log("[ImportSave] File copied successfully!");
                copySuccess = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ImportSave] ERROR: {e.Message}");
                ShowStatus($"Import failed: {e.Message}");
            }

            if (copySuccess)
            {
                ShowStatus("Import successful! Reloading...");
                Debug.Log("[ImportSave] Waiting 1 second before reload...");

                yield return new WaitForSeconds(1f);

                // 씬 재로드
                Debug.Log("[ImportSave] Calling LoadScene...");
                UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
            }
        }
        else
        {
            ShowStatus($"No export file found on Desktop!\nLooking for: antimatter_save_export.json");
            Debug.LogWarning("[ImportSave] Export file not found on desktop");
        }
    }

    void ResetGame()
    {
        Debug.Log("[ResetGame] Starting reset coroutine");
        StartCoroutine(ResetGameWithConfirmation());
    }

    System.Collections.IEnumerator ResetGameWithConfirmation()
    {
        Debug.Log("[ResetGame] Coroutine started");
        // 확인 대화창 표시
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

            // 모든 매니저 파괴 (완전히 새로 시작하기 위해)
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

            // 씬 재로드
            Debug.Log("[ResetGame] Calling LoadScene...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
        }
        else
        {
            Debug.LogError("[ResetGame] SaveManager.Instance is null!");
        }
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
        // Canvas 찾기
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[ShowConfirmDialog] Canvas not found!");
            return;
        }
        Debug.Log("[ShowConfirmDialog] Canvas found, creating dialog UI");

        // 확인 대화창 패널 생성
        GameObject dialogObj = new GameObject("ConfirmDialog");
        RectTransform dialogRT = dialogObj.AddComponent<RectTransform>();
        dialogRT.SetParent(canvas.transform, false);
        dialogRT.anchorMin = Vector2.zero;
        dialogRT.anchorMax = Vector2.one;
        dialogRT.sizeDelta = Vector2.zero;

        // 반투명 배경
        Image bgImage = dialogObj.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);

        // 대화창 패널
        GameObject panelObj = new GameObject("Panel");
        RectTransform panelRT = panelObj.AddComponent<RectTransform>();
        panelRT.SetParent(dialogObj.transform, false);
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(500, 250);

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // 제목 텍스트
        GameObject titleObj = new GameObject("Title");
        RectTransform titleRT = titleObj.AddComponent<RectTransform>();
        titleRT.SetParent(panelObj.transform, false);
        titleRT.anchorMin = new Vector2(0.5f, 1);
        titleRT.anchorMax = new Vector2(0.5f, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.anchoredPosition = new Vector2(0, -20);
        titleRT.sizeDelta = new Vector2(450, 50);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = title;
        titleText.fontSize = 32;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // 메시지 텍스트
        GameObject messageObj = new GameObject("Message");
        RectTransform messageRT = messageObj.AddComponent<RectTransform>();
        messageRT.SetParent(panelObj.transform, false);
        messageRT.anchorMin = new Vector2(0.5f, 0.5f);
        messageRT.anchorMax = new Vector2(0.5f, 0.5f);
        messageRT.pivot = new Vector2(0.5f, 0.5f);
        messageRT.anchoredPosition = new Vector2(0, 10);
        messageRT.sizeDelta = new Vector2(450, 100);

        TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
        messageText.text = message;
        messageText.fontSize = 20;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.color = Color.white;

        // Yes 버튼
        GameObject yesButtonObj = new GameObject("YesButton");
        RectTransform yesRT = yesButtonObj.AddComponent<RectTransform>();
        yesRT.SetParent(panelObj.transform, false);
        yesRT.anchorMin = new Vector2(0.5f, 0);
        yesRT.anchorMax = new Vector2(0.5f, 0);
        yesRT.pivot = new Vector2(0.5f, 0);
        yesRT.anchoredPosition = new Vector2(-80, 20);
        yesRT.sizeDelta = new Vector2(120, 40);

        Image yesImage = yesButtonObj.AddComponent<Image>();
        yesImage.color = new Color(0.3f, 0.7f, 0.3f, 1f);
        Button yesButton = yesButtonObj.AddComponent<Button>();

        GameObject yesTextObj = new GameObject("Text");
        RectTransform yesTextRT = yesTextObj.AddComponent<RectTransform>();
        yesTextRT.SetParent(yesButtonObj.transform, false);
        yesTextRT.anchorMin = Vector2.zero;
        yesTextRT.anchorMax = Vector2.one;
        yesTextRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI yesText = yesTextObj.AddComponent<TextMeshProUGUI>();
        yesText.text = "YES";
        yesText.fontSize = 24;
        yesText.alignment = TextAlignmentOptions.Center;
        yesText.color = Color.white;

        yesButton.onClick.AddListener(() => {
            Debug.Log("[ShowConfirmDialog] YES button clicked");
            callback(true);
            Debug.Log("[ShowConfirmDialog] Destroying dialog");
            Destroy(dialogObj);
        });

        // No 버튼
        GameObject noButtonObj = new GameObject("NoButton");
        RectTransform noRT = noButtonObj.AddComponent<RectTransform>();
        noRT.SetParent(panelObj.transform, false);
        noRT.anchorMin = new Vector2(0.5f, 0);
        noRT.anchorMax = new Vector2(0.5f, 0);
        noRT.pivot = new Vector2(0.5f, 0);
        noRT.anchoredPosition = new Vector2(80, 20);
        noRT.sizeDelta = new Vector2(120, 40);

        Image noImage = noButtonObj.AddComponent<Image>();
        noImage.color = new Color(0.7f, 0.3f, 0.3f, 1f);
        Button noButton = noButtonObj.AddComponent<Button>();

        GameObject noTextObj = new GameObject("Text");
        RectTransform noTextRT = noTextObj.AddComponent<RectTransform>();
        noTextRT.SetParent(noButtonObj.transform, false);
        noTextRT.anchorMin = Vector2.zero;
        noTextRT.anchorMax = Vector2.one;
        noTextRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI noText = noTextObj.AddComponent<TextMeshProUGUI>();
        noText.text = "NO";
        noText.fontSize = 24;
        noText.alignment = TextAlignmentOptions.Center;
        noText.color = Color.white;

        noButton.onClick.AddListener(() => {
            Debug.Log("[ShowConfirmDialog] NO button clicked");
            callback(false);
            Debug.Log("[ShowConfirmDialog] Destroying dialog");
            Destroy(dialogObj);
        });

        Debug.Log("[ShowConfirmDialog] Dialog created successfully");
    }

    void QuitGame()
    {
        Debug.Log("[QuitGame] Quit button clicked");
        StartCoroutine(QuitGameWithConfirmation());
    }

    System.Collections.IEnumerator QuitGameWithConfirmation()
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

        // 빌드에서는 Application.Quit() 사용
        // Unity 에디터에서는 작동하지 않음
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
