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

    // Shop data
    public int premiumCurrency;
    public ShopItemSaveData[] shopItems;

    // Offline data
    public double storedOfflineTime;
    public int maxTimeUpgradeLevel;
    public int efficiencyUpgradeLevel;

    // Milestone/Autobuyer data
    public bool tickspeedBulkUnlocked;
    public bool[] autoBuyersUnlocked;
    public bool[] autoBuyersEnabled;
    public int[] autoBuyerModes; // 0 = Single, 1 = Bulk
    public int autoBuyerSpeedLevel;
}

[System.Serializable]
public class ShopItemSaveData
{
    public string itemId;
    public int level;

    public ShopItemSaveData(string id, int itemLevel)
    {
        itemId = id;
        level = itemLevel;
    }
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
    public BigDouble currentPrice;  // 레거시 호환용 (AD풍에서는 bought 기반 계산)
    public int bought;
    public bool unlocked;
    public BigDouble multiplier;

    public DimensionSaveData(Dimension dim)
    {
        tier = dim.tier;
        amount = dim.amount;
        // AD풍: GetCurrentPrice() 사용
        currentPrice = dim.GetCurrentPrice();
        bought = dim.bought;
        unlocked = dim.unlocked;
        multiplier = dim.multiplier;
    }

    public void ApplyToDimension(Dimension dim)
    {
        dim.amount = amount;
        // AD풍: currentPrice는 bought 기반으로 자동 계산되므로 로드하지 않음
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

        // Shop 데이터 저장
        if (ShopManager.Instance != null)
        {
            saveData.premiumCurrency = ShopManager.Instance.premiumCurrency;

            var itemLevels = ShopManager.Instance.GetItemLevels();
            saveData.shopItems = new ShopItemSaveData[itemLevels.Count];
            int index = 0;
            foreach (var kvp in itemLevels)
            {
                saveData.shopItems[index] = new ShopItemSaveData(kvp.Key.ToString(), kvp.Value);
                index++;
            }
        }

        // Offline 데이터 저장
        if (OfflineManager.Instance != null)
        {
            saveData.storedOfflineTime = OfflineManager.Instance.storedOfflineTime;
            saveData.maxTimeUpgradeLevel = OfflineManager.Instance.maxTimeUpgradeLevel;
            saveData.efficiencyUpgradeLevel = OfflineManager.Instance.efficiencyUpgradeLevel;
        }

        // Milestone/Autobuyer 데이터 저장
        if (TickSpeedManager.Instance != null)
        {
            saveData.tickspeedBulkUnlocked = TickSpeedManager.Instance.bulkBuyUnlocked;
        }

        if (AutoBuyerManager.Instance != null)
        {
            saveData.autoBuyersUnlocked = (bool[])AutoBuyerManager.Instance.dimensionAutoBuyersUnlocked.Clone();
            saveData.autoBuyersEnabled = (bool[])AutoBuyerManager.Instance.dimensionAutoBuyersEnabled.Clone();
            saveData.autoBuyerModes = new int[8];
            for (int i = 0; i < 8; i++)
            {
                saveData.autoBuyerModes[i] = (int)AutoBuyerManager.Instance.dimensionBuyModes[i];
            }
            saveData.autoBuyerSpeedLevel = AutoBuyerManager.Instance.speedUpgradeLevel;
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

            // Shop 데이터 로드
            if (ShopManager.Instance != null && saveData.shopItems != null)
            {
                ShopManager.Instance.premiumCurrency = saveData.premiumCurrency;

                var itemLevels = new System.Collections.Generic.Dictionary<ShopManager.ShopItem, int>();
                foreach (var itemSave in saveData.shopItems)
                {
                    if (System.Enum.TryParse<ShopManager.ShopItem>(itemSave.itemId, out var item))
                    {
                        itemLevels[item] = itemSave.level;
                    }
                }
                ShopManager.Instance.SetItemLevels(itemLevels);
            }

            // Offline 데이터 로드
            if (OfflineManager.Instance != null)
            {
                OfflineManager.Instance.maxTimeUpgradeLevel = saveData.maxTimeUpgradeLevel;
                OfflineManager.Instance.efficiencyUpgradeLevel = saveData.efficiencyUpgradeLevel;

                // Recalculate max time and efficiency based on upgrade levels
                OfflineManager.Instance.maxOfflineTime = 86400 + (saveData.maxTimeUpgradeLevel * 21600);
                OfflineManager.Instance.efficiencyRatio = 0.5 + (saveData.efficiencyUpgradeLevel * 0.05);

                // Calculate offline time based on save time
                OfflineManager.Instance.storedOfflineTime = saveData.storedOfflineTime;
                double previousStored = saveData.storedOfflineTime;

                if (!string.IsNullOrEmpty(saveData.saveTime))
                {
                    if (System.DateTime.TryParse(saveData.saveTime, out System.DateTime lastSaveTime))
                    {
                        System.DateTime now = System.DateTime.Now;
                        double offlineSeconds = (now - lastSaveTime).TotalSeconds;

                        if (offlineSeconds > 0)
                        {
                            OfflineManager.Instance.AccumulateOfflineTime(offlineSeconds);
                            double newStored = OfflineManager.Instance.storedOfflineTime;
                            double accumulated = newStored - previousStored;

                            Debug.Log($"[SaveManager] Offline time calculated from save: {offlineSeconds / 3600:F2}h");

                            // Show popup if offline for more than 1 minute
                            if (offlineSeconds >= 60 && OfflineProgressPopup.Instance != null)
                            {
                                OfflineProgressPopup.Instance.ShowOfflineProgress(offlineSeconds, accumulated, newStored);
                            }
                        }
                    }
                }
            }

            // Milestone/Autobuyer 데이터 로드
            if (TickSpeedManager.Instance != null)
            {
                TickSpeedManager.Instance.bulkBuyUnlocked = saveData.tickspeedBulkUnlocked;
            }

            if (AutoBuyerManager.Instance != null)
            {
                if (saveData.autoBuyersUnlocked != null)
                {
                    for (int i = 0; i < saveData.autoBuyersUnlocked.Length && i < 8; i++)
                    {
                        AutoBuyerManager.Instance.dimensionAutoBuyersUnlocked[i] = saveData.autoBuyersUnlocked[i];
                    }
                }
                if (saveData.autoBuyersEnabled != null)
                {
                    for (int i = 0; i < saveData.autoBuyersEnabled.Length && i < 8; i++)
                    {
                        AutoBuyerManager.Instance.dimensionAutoBuyersEnabled[i] = saveData.autoBuyersEnabled[i];
                    }
                }
                if (saveData.autoBuyerModes != null)
                {
                    for (int i = 0; i < saveData.autoBuyerModes.Length && i < 8; i++)
                    {
                        AutoBuyerManager.Instance.dimensionBuyModes[i] = (AutoBuyerManager.BuyMode)saveData.autoBuyerModes[i];
                    }
                }
                AutoBuyerManager.Instance.speedUpgradeLevel = saveData.autoBuyerSpeedLevel;
            }

            // 로드 완료 후 마일스톤 체크 (totalPrestiges 기반으로 해금)
            if (PrestigeManager.Instance != null)
            {
                PrestigeManager.Instance.CheckMilestones();
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

    // Export save as Base64 encoded string
    public string ExportSaveToString()
    {
        SaveGame(); // Save current state first

        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("No save file to export");
            return null;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            string base64 = Convert.ToBase64String(bytes);
            Debug.Log("Save exported to string successfully");
            return base64;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to export save: {e.Message}");
            return null;
        }
    }

    // Import save from Base64 encoded string
    public bool ImportSaveFromString(string saveString)
    {
        if (string.IsNullOrEmpty(saveString))
        {
            Debug.LogWarning("Empty save string");
            return false;
        }

        try
        {
            // Decode Base64 to JSON
            byte[] bytes = Convert.FromBase64String(saveString.Trim());
            string json = System.Text.Encoding.UTF8.GetString(bytes);

            // Validate by attempting to parse
            GameSaveData testData = JsonUtility.FromJson<GameSaveData>(json);
            if (testData == null)
            {
                Debug.LogError("Invalid save data format");
                return false;
            }

            // Save to file
            File.WriteAllText(saveFilePath, json);
            Debug.Log("Save imported successfully");
            return true;
        }
        catch (FormatException)
        {
            Debug.LogError("Invalid Base64 format");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to import save: {e.Message}");
            return false;
        }
    }
}
