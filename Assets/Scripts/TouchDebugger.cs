using UnityEngine;
using UnityEngine.InputSystem;

public class TouchDebugger : MonoBehaviour
{
    void Update()
    {
        // Old Input System (for "Both" mode)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Debug.Log($"[TouchDebugger OLD] Touch detected: {touch.position}, phase: {touch.phase}");
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"[TouchDebugger OLD] Mouse click: {Input.mousePosition}");
        }

        // New Input System
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.isPressed)
            {
                Debug.Log($"[TouchDebugger NEW] Touch: {touch.position.ReadValue()}");
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log($"[TouchDebugger NEW] Mouse: {Mouse.current.position.ReadValue()}");
        }
    }
}
