using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemDebug : MonoBehaviour
{
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard[Key.A].wasPressedThisFrame)
        {
            Debug.Log("[InputSystemDebug] A key pressed");
        }

        if (keyboard[Key.A].wasReleasedThisFrame)
        {
            Debug.Log("[InputSystemDebug] A key released");
        }

        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Debug.Log($"[InputSystemDebug] Left mouse pressed at {mouse.position.ReadValue()}");
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            Debug.Log($"[InputSystemDebug] Left mouse released at {mouse.position.ReadValue()}");
        }
    }
}
