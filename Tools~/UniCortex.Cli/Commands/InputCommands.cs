using ConsoleAppFramework;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Cli.Commands;

#pragma warning disable CS1573 // Parameter has no matching param tag
public class InputCommands(InputUseCase inputUseCase)
{
    /// <summary>Send a keyboard event via Input System in Play Mode. Requires com.unity.inputsystem.</summary>
    /// <param name="key">Input System Key enum name. Available keys: A-Z, Digit0-Digit9, F1-F12, Space, Enter, Tab, Backspace, Delete, Insert, Escape, ContextMenu, LeftArrow, RightArrow, UpArrow, DownArrow, PageUp, PageDown, Home, End, LeftShift, RightShift, LeftCtrl, RightCtrl, LeftAlt, RightAlt, LeftMeta, RightMeta, Backquote, Quote, Semicolon, Comma, Period, Slash, Backslash, LeftBracket, RightBracket, Minus, Equals, CapsLock, NumLock, ScrollLock, PrintScreen, Pause, Numpad0-Numpad9, NumpadEnter, NumpadDivide, NumpadMultiply, NumpadPlus, NumpadMinus, NumpadPeriod, NumpadEquals, OEM1-OEM5, IMESelected.</param>
    /// <param name="eventType">Event type: "press" (default) or "release".</param>
    [Command("send-key")]
    public async Task SendKey([Argument] string key, string eventType = InputEventType.Press,
        CancellationToken cancellationToken = default)
    {
        var message = await inputUseCase.SendKeyEventAsync(key, eventType, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Send a mouse event via Input System in Play Mode. Requires com.unity.inputsystem. Specify either --x and --y, or one of --target-instance-id or --target-path (requires com.unity.ugui).</summary>
    /// <param name="x">X coordinate in screen pixels (Screen.width space). Origin (0,0) is at the bottom-left of the Game View. Increases to the right. Note: capture_game_view images are at the Game View resolution with a top-left origin, so a pixel (px, py) in the image corresponds to x = px, y = imageHeight - py.</param>
    /// <param name="y">Y coordinate in screen pixels (Screen.height space). Origin (0,0) is at the bottom-left of the Game View. Increases upward. This is the inverse of typical image coordinates where Y increases downward.</param>
    /// <param name="targetInstanceId">instanceId of the UI GameObject to send the event to. The event is sent to its center.</param>
    /// <param name="targetPath">Hierarchy path of the UI GameObject to send the event to (e.g. "Canvas/Menu/StartButton"). The event is sent to its center.</param>
    /// <param name="button">Mouse button: "left" (default), "right", or "middle".</param>
    /// <param name="eventType">Event type: "click" (default, press then release after one frame), "press", "release", or "move" (position only, no button).</param>
    [Command("send-mouse")]
    public async Task SendMouse(float? x = null, float? y = null, int? targetInstanceId = null,
        string? targetPath = null, string button = MouseButton.Left, string eventType = InputEventType.Click,
        CancellationToken cancellationToken = default)
    {
        var message = await inputUseCase.SendMouseEventAsync(x, y, targetInstanceId, targetPath, button, eventType,
            cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>List the uGUI objects in the Game View that receive pointer events, with their centers in Game View coordinates, in Play Mode. Requires com.unity.ugui.</summary>
    [Command("pointer-targets")]
    public async Task PointerTargets(CancellationToken cancellationToken = default)
    {
        var json = await inputUseCase.GetPointerTargetsAsync(cancellationToken);
        Console.WriteLine(json);
    }
}
