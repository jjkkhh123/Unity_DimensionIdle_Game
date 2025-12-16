using System;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public BigDouble antimatter;
    public DimensionSaveData[] dimensions;
    public int prestigePoints;
    public int totalPrestiges;
    public bool infinityReached;
    public int tickspeedLevel;
    public PrestigeUpgradeSaveData[] prestigeUpgrades;
    public string saveTime;
}

[System.Serializable]
public class PrestigeUpgradeSaveData
{
    public string id;
    public int level;

    public PrestigeUpgradeSaveData(string id, int level)
    {
        this.id = id;
        this.level = level;
    }
}

[System.Serializable]
public class DimensionSaveData
{
    public int tier;
    public BigDouble amount;
    public BigDouble currentPrice;
    public int bought;
    public bool unlocked;
    public BigDouble multiplier;

    public DimensionSaveData(Dimension dim)
    {
        tier = dim.tier;
        amount = dim.amount;
        currentPrice = dim.currentPrice;
        bought = dim.bought;
        unlocked = dim.unlocked;
        multiplier = dim.multiplier;
    }

    public void ApplyToDimension(Dimension dim)
    {
        dim.amount = amount;
        dim.currentPrice = currentPrice;
        dim.bought = bought;
        dim.unlocked = unlocked;
        dim.multiplier = multiplier;
    }
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string saveFilePath;
    private const float AUTO_SAVE_INTERVAL = 30f;
    private float autoSaveTimer = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        saveFilePath = Path.Combine(Application.persistentDataPath, "antimatter_save.json");

        // 씬 로드 이벤트 구독
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // 씬 로드 이벤트 구독 해제
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"[SaveManager] Scene loaded: {scene.name}, starting delayed load");
        StartCoroutine(LoadGameDelayed());
    }

    void Start()
    {
        StartCoroutine(LoadGameDelayed());
    }

    System.Collections.IEnumerator LoadGameDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        LoadGame();
    }

    void Update()
    {
        autoSaveTimer += Time.deltaTime;

        if (autoSaveTimer >= AUTO_SAVE_INTERVAL)
        {
            autoSaveTimer = 0f;
            SaveGame();
        }
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveGame();
        }
    }

    public void SaveGame()
    {
        if (GameManager.Instance == null || PrestigeManager.Instance == null)
        {
            return;
        }

        GameSaveData saveData = new GameSaveData
        {
            antimatter = GameManager.Instance.antimatter,
            dimensions = new DimensionSaveData[GameManager.Instance.dimensions.Count],
            prestigePoints = PrestigeManager.Instance.prestigePoints,
            totalPrestiges = PrestigeManager.Instance.totalPrestiges,
            infinityReached = GameManager.Instance.infinityReached,
            tickspeedLevel = TickSpeedManager.Instance != null ? TickSpeedManager.Instance.tickspeedLevel : 0,
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        for (int i = 0; i < GameManager.Instance.dimensions.Count; i++)
        {
            saveData.dimensions[i] = new DimensionSaveData(GameManager.Instance.dimensions[i]);
        }

        // 프레스티지 업그레이드 저장
        if (PrestigeManager.Instance != null && PrestigeManager.Instance.upgrades != null)
        {
            saveData.prestigeUpgrades = new PrestigeUpgradeSaveData[PrestigeManager.Instance.upgrades.Count];
            int index = 0;
            foreach (var kvp in PrestigeManager.Instance.upgrades)
            {
                saveData.prestigeUpgrades[index] = new PrestigeUpgradeSaveData(kvp.Key, kvp.Value.level);
                index++;
            }
        }

        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Game saved to {saveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("No save file found. Starting new game.");
            return;
        }

        if (GameManager.Instance == null || PrestigeManager.Instance == null)
        {
            Debug.LogWarning("Cannot load: Managers not initialized");
            return;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

            GameManager.Instance.antimatter = saveData.antimatter;
            GameManager.Instance.infinityReached = saveData.infinityReached;

            for (int i = 0; i < saveData.dimensions.Length && i < GameManager.Instance.dimensions.Count; i++)
            {
                saveData.dimensions[i].ApplyToDimension(GameManager.Instance.dimensions[i]);
            }

            PrestigeManager.Instance.prestigePoints = saveData.prestigePoints;
            PrestigeManager.Instance.totalPrestiges = saveData.totalPrestiges;

            if (TickSpeedManager.Instance != null)
            {
                TickSpeedManager.Instance.tickspeedLevel = saveData.tickspeedLevel;
            }

            // 프레스티지 업그레이드 로드
            if (PrestigeManager.Instance != null && saveData.prestigeUpgrades != null)
            {
                foreach (var upgradeSave in saveData.prestigeUpgrades)
                {
                    if (PrestigeManager.Instance.upgrades.ContainsKey(upgradeSave.id))
                    {
                        PrestigeManager.Instance.upgrades[upgradeSave.id].level = upgradeSave.level;
                    }
                }
            }

            Debug.Log($"Game loaded from {saveFilePath} (Saved: {saveData.saveTime})");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Save file deleted.");
        }
    }

    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }
}
