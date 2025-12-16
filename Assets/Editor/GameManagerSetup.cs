using UnityEngine;
using UnityEditor;
using System.IO;

public class GameManagerSetup : EditorWindow
{
    [MenuItem("Tools/Setup Game Managers")]
    static void SetupManagers()
    {
        if (EditorUtility.DisplayDialog("Setup Game Managers",
            "This will create GameManager with all necessary components. Continue?",
            "Yes", "Cancel"))
        {
            CreateGameManagers();
        }
    }

    [MenuItem("Tools/Clear Save Data")]
    static void ClearSaveData()
    {
        if (EditorUtility.DisplayDialog("Clear Save Data",
            "This will DELETE all saved game data. Are you sure?",
            "Yes, Delete", "Cancel"))
        {
            string saveFilePath = Path.Combine(UnityEngine.Application.persistentDataPath, "antimatter_save.json");

            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                Debug.Log("Save data deleted successfully!");
                EditorUtility.DisplayDialog("Success", "Save data has been deleted!", "OK");
            }
            else
            {
                Debug.Log("No save file found.");
                EditorUtility.DisplayDialog("Info", "No save file found.", "OK");
            }
        }
    }

    static void CreateGameManagers()
    {
        // GameManager 찾기 또는 생성
        GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gameManager = gmObj.AddComponent<GameManager>();
            Debug.Log("GameManager created!");
        }

        // PrestigeManager 추가
        PrestigeManager prestigeManager = Object.FindFirstObjectByType<PrestigeManager>();
        if (prestigeManager == null)
        {
            GameObject gmObj = gameManager.gameObject;
            gmObj.AddComponent<PrestigeManager>();
            Debug.Log("PrestigeManager added!");
        }

        // SaveManager 추가
        SaveManager saveManager = Object.FindFirstObjectByType<SaveManager>();
        if (saveManager == null)
        {
            GameObject gmObj = gameManager.gameObject;
            gmObj.AddComponent<SaveManager>();
            Debug.Log("SaveManager added!");
        }

        // TickSpeedManager 추가
        TickSpeedManager tickSpeedManager = Object.FindFirstObjectByType<TickSpeedManager>();
        if (tickSpeedManager == null)
        {
            GameObject gmObj = gameManager.gameObject;
            gmObj.AddComponent<TickSpeedManager>();
            Debug.Log("TickSpeedManager added!");
        }

        Debug.Log("Game Managers Setup Complete!");
    }
}
