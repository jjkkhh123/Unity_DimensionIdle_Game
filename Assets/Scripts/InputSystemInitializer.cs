using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ensures Input System is properly initialized for UI Toolkit.
/// This fixes a known issue where UI Toolkit doesn't receive input
/// when Active Input Handling is set to "Both" mode.
/// </summary>
public class InputSystemInitializer : MonoBehaviour
{
    void Awake()
    {
        // Force Input System initialization by accessing devices
        // This ensures UI Toolkit can properly receive input events

        if (Mouse.current != null)
        {
            Debug.Log("[InputSystemInitializer] Mouse device initialized");
        }

        if (Touchscreen.current != null)
        {
            Debug.Log("[InputSystemInitializer] Touchscreen device initialized");
        }
        else
        {
            // On non-touch devices, this is expected
            Debug.Log("[InputSystemInitializer] No touchscreen detected (expected on PC)");
        }
    }
}
