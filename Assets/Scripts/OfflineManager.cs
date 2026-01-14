using System;
using UnityEngine;

public class OfflineManager : MonoBehaviour
{
    public static OfflineManager Instance { get; private set; }

    // Storage
    public double storedOfflineTime = 0;      // seconds
    public double maxOfflineTime = 86400;     // 24 hours (업그레이드 가능)
    public double efficiencyRatio = 0.5;      // 50% (업그레이드 가능)

    // Upgrades
    public int maxTimeUpgradeLevel = 0;
    public int efficiencyUpgradeLevel = 0;

    // Boost
    public bool isBoostActive = false;
    public double currentMultiplier = 1.0;
    public double boostRemainingTime = 0;

    // Last exit time tracking
    private DateTime lastExitTime;

    // Upgrade costs (in seconds)
    private const double MAX_TIME_BASE_COST = 72000;      // 20 hours
    private const double MAX_TIME_COST_INCREASE = 21600;  // 6 hours per level
    private const double EFFICIENCY_BASE_COST = 43200;    // 12 hours
    private const double EFFICIENCY_COST_INCREASE = 21600; // 6 hours per level
    private const int EFFICIENCY_MAX_LEVEL = 10;

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
    }

    void Start()
    {
        // Offline time calculation is handled by SaveManager.LoadGame()
        // to use the save file's saveTime instead of PlayerPrefs
    }

    void Update()
    {
        if (isBoostActive)
        {
            // Consume stored time and decrease boost remaining time
            double deltaTime = Time.deltaTime;
            double consumptionRate = currentMultiplier / efficiencyRatio;
            double consumed = deltaTime * consumptionRate;

            storedOfflineTime -= consumed;
            boostRemainingTime -= deltaTime;

            // Stop boost if time runs out
            if (storedOfflineTime <= 0 || boostRemainingTime <= 0)
            {
                storedOfflineTime = Math.Max(0, storedOfflineTime);
                StopBoost();
            }
        }

        // Test cheats (only in Unity Editor)
        #if UNITY_EDITOR
        TestCheats();
        #endif
    }

    #if UNITY_EDITOR
    void TestCheats()
    {
        // F5: Add 1 hour of offline time
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("[OfflineManager CHEAT] F5 KEY PRESSED!");
            AccumulateOfflineTime(3600);
            Debug.LogWarning($"[OfflineManager CHEAT] Added 1 hour. Total: {storedOfflineTime / 3600:F2}h");
        }

        // F6: Add 6 hours of offline time
        if (Input.GetKeyDown(KeyCode.F6))
        {
            Debug.Log("[OfflineManager CHEAT] F6 KEY PRESSED!");
            AccumulateOfflineTime(21600);
            Debug.LogWarning($"[OfflineManager CHEAT] Added 6 hours. Total: {storedOfflineTime / 3600:F2}h");
        }

        // F7: Add 24 hours of offline time
        if (Input.GetKeyDown(KeyCode.F7))
        {
            Debug.Log("[OfflineManager CHEAT] F7 KEY PRESSED!");
            AccumulateOfflineTime(86400);
            Debug.LogWarning($"[OfflineManager CHEAT] Added 24 hours. Total: {storedOfflineTime / 3600:F2}h");
        }

        // F8: Simulate offline popup (save & reload)
        if (Input.GetKeyDown(KeyCode.F8))
        {
            Debug.Log("[OfflineManager CHEAT] F8 KEY PRESSED!");
            TestOfflinePopup();
        }

        // F9: Debug status
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.LogWarning("=== OFFLINE MANAGER DEBUG ===");
            Debug.LogWarning($"OfflineManager.Instance: {(Instance != null ? "EXISTS" : "NULL")}");
            Debug.LogWarning($"Stored Time: {storedOfflineTime / 3600:F2}h");
            Debug.LogWarning($"Max Time: {maxOfflineTime / 3600:F0}h");
            Debug.LogWarning($"Efficiency: {efficiencyRatio * 100:F0}%");
            Debug.LogWarning($"Boost Active: {isBoostActive}");
            Debug.LogWarning($"OfflineProgressPopup.Instance: {(OfflineProgressPopup.Instance != null ? "EXISTS" : "NULL")}");
            Debug.LogWarning("============================");
        }
    }

    void TestOfflinePopup()
    {
        if (SaveManager.Instance == null) return;

        // Save current game
        SaveManager.Instance.SaveGame();
        Debug.Log("[OfflineManager CHEAT] Game saved");

        // Wait a frame then show popup as if we were offline
        StartCoroutine(ShowTestPopup());
    }

    System.Collections.IEnumerator ShowTestPopup()
    {
        yield return null;

        // Simulate 6 hours offline
        double offlineSeconds = 21600; // 6 hours
        double previousStored = storedOfflineTime;
        AccumulateOfflineTime(offlineSeconds);
        double accumulated = storedOfflineTime - previousStored;

        if (OfflineProgressPopup.Instance != null)
        {
            OfflineProgressPopup.Instance.ShowOfflineProgress(offlineSeconds, accumulated, storedOfflineTime);
            Debug.Log($"[OfflineManager CHEAT] Showing offline popup: {offlineSeconds / 3600}h offline, {accumulated / 3600:F2}h accumulated");
        }
    }
    #endif

    void OnApplicationQuit()
    {
        // SaveManager handles saving the game with saveTime
        // No need for separate PlayerPrefs tracking
    }

    void OnApplicationPause(bool pause)
    {
        // SaveManager handles saving the game with saveTime
        // No need for separate PlayerPrefs tracking
    }

    public void AccumulateOfflineTime(double seconds)
    {
        // Apply efficiency ratio and cap at max time
        double effectiveTime = seconds * efficiencyRatio;
        storedOfflineTime = Math.Min(storedOfflineTime + effectiveTime, maxOfflineTime);
    }

    public bool CanStartBoost(double multiplier)
    {
        if (isBoostActive) return false;
        if (multiplier < 1 || multiplier > 20) return false;
        if (storedOfflineTime <= 0) return false;

        double duration = CalculateBoostDuration(multiplier);
        return duration > 0;
    }

    public void StartBoost(double multiplier)
    {
        if (!CanStartBoost(multiplier)) return;

        isBoostActive = true;
        currentMultiplier = multiplier;
        boostRemainingTime = CalculateBoostDuration(multiplier);

        Debug.Log($"[OfflineManager] Boost x{multiplier} started. Duration: {boostRemainingTime / 60:F2} min");
    }

    public void StopBoost()
    {
        if (!isBoostActive) return;

        isBoostActive = false;
        currentMultiplier = 1.0;
        boostRemainingTime = 0;

        Debug.Log("[OfflineManager] Boost stopped");
    }

    public double CalculateBoostDuration(double multiplier)
    {
        // boostDuration = (storedTime * efficiencyRatio) / multiplier
        // But storedTime is already after efficiency, so:
        // boostDuration = storedTime / multiplier
        return storedOfflineTime / multiplier;
    }

    public double GetMaxTimeUpgradeCost()
    {
        return MAX_TIME_BASE_COST + (maxTimeUpgradeLevel * MAX_TIME_COST_INCREASE);
    }

    public double GetEfficiencyUpgradeCost()
    {
        return EFFICIENCY_BASE_COST + (efficiencyUpgradeLevel * EFFICIENCY_COST_INCREASE);
    }

    public bool CanUpgradeMaxTime()
    {
        return storedOfflineTime >= GetMaxTimeUpgradeCost();
    }

    public bool CanUpgradeEfficiency()
    {
        if (efficiencyUpgradeLevel >= EFFICIENCY_MAX_LEVEL) return false;
        return storedOfflineTime >= GetEfficiencyUpgradeCost();
    }

    public void UpgradeMaxTime()
    {
        if (!CanUpgradeMaxTime()) return;

        double cost = GetMaxTimeUpgradeCost();
        storedOfflineTime -= cost;
        maxTimeUpgradeLevel++;

        // Increase max time by 6 hours per level (24h → 30h → 36h...)
        maxOfflineTime = 86400 + (maxTimeUpgradeLevel * 21600); // 21600 = 6 hours

        Debug.Log($"[OfflineManager] Max Time upgraded to level {maxTimeUpgradeLevel}. New cap: {maxOfflineTime / 3600}h. Cost: {cost / 3600}h");

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
    }

    public void UpgradeEfficiency()
    {
        if (!CanUpgradeEfficiency()) return;

        double cost = GetEfficiencyUpgradeCost();
        storedOfflineTime -= cost;
        efficiencyUpgradeLevel++;

        // Increase efficiency by 5% per level (50% → 55% → 60%...)
        efficiencyRatio = 0.5 + (efficiencyUpgradeLevel * 0.05);

        Debug.Log($"[OfflineManager] Efficiency upgraded to level {efficiencyUpgradeLevel}. New ratio: {efficiencyRatio * 100}%. Cost: {cost / 3600}h");

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
    }

    // For GameManager to apply boost multiplier to production
    public double GetActiveMultiplier()
    {
        return isBoostActive ? currentMultiplier : 1.0;
    }
}
