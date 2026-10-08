using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemDebug : MonoBehaviour
{
    private void OnEnable()
    {
        foreach (var device in InputSystem.devices)
        {
            if (device is Keyboard keyboard)
            {
                keyboard.onTextInput += OnTextInput;
            }
        }

        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;

        foreach (var device in InputSystem.devices)
        {
            if (device is Keyboard keyboard)
            {
                keyboard.onTextInput -= OnTextInput;
            }
        }
    }

    // Subscribes to keyboards added later, e.g. the virtual keyboard of UniCortex, before their first text event.
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Keyboard keyboard && change == InputDeviceChange.Added)
        {
            keyboard.onTextInput += OnTextInput;
        }
    }

    private static void OnTextInput(char character)
    {
        Debug.Log($"[InputSystemDebug] Text input: {character}");
    }

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
