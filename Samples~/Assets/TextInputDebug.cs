using UnityEngine;
using UnityEngine.InputSystem;

// Logs the text input of every keyboard, including keyboards added later such as the virtual keyboard of UniCortex.
// Registered with RuntimeInitializeOnLoadMethod instead of MonoBehaviour event functions, so that it does not depend
// on their execution order.
public static class TextInputDebug
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        foreach (var device in InputSystem.devices)
        {
            if (device is Keyboard keyboard)
            {
                keyboard.onTextInput += OnTextInput;
            }
        }

        InputSystem.onDeviceChange += OnDeviceChange;
        Application.quitting += Shutdown;
    }

    // Called when Play Mode exits in the Editor, so that the handlers are not registered twice when Play Mode is
    // entered again without a domain reload.
    private static void Shutdown()
    {
        Application.quitting -= Shutdown;
        InputSystem.onDeviceChange -= OnDeviceChange;

        foreach (var device in InputSystem.devices)
        {
            if (device is Keyboard keyboard)
            {
                keyboard.onTextInput -= OnTextInput;
            }
        }
    }

    // Subscribes to a keyboard added later before its first text event is processed.
    private static void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Keyboard keyboard && change == InputDeviceChange.Added)
        {
            keyboard.onTextInput += OnTextInput;
        }
    }

    private static void OnTextInput(char character)
    {
        Debug.Log($"[TextInputDebug] Text input: {character}");
    }
}
