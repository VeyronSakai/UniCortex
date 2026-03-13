using ConsoleAppFramework;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Cli.Commands;

#pragma warning disable CS1573 // Parameter has no matching param tag
public class InputCommands(InputUseCase inputService)
{
    /// <summary>Send a keyboard event via Input System in Play Mode. Requires com.unity.inputsystem.</summary>
    /// <param name="key">Input System Key enum name. Available keys: A-Z, Digit0-Digit9, F1-F12, Space, Return, Escape, Tab, Backspace, Delete, LeftArrow, RightArrow, UpArrow, DownArrow, LeftShift, RightShift, LeftCtrl, RightCtrl, LeftAlt, RightAlt. Any valid Input System Key enum name is also accepted.</param>
    /// <param name="eventType">Event type: "press" (default) or "release".</param>
    [Command("send-key")]
    public async Task SendKey(string key, string eventType = InputEventType.Press,
        CancellationToken cancellationToken = default)
    {
        var message = await inputService.SendKeyEventAsync(key, eventType, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Send a mouse event via Input System in Play Mode. Requires com.unity.inputsystem.</summary>
    /// <param name="x">X coordinate in screen space.</param>
    /// <param name="y">Y coordinate in screen space.</param>
    /// <param name="button">Mouse button: "left" (default), "right", or "middle".</param>
    /// <param name="eventType">Event type: "press" (default), "release", or "move" (position only, no button).</param>
    [Command("send-mouse")]
    public async Task SendMouse(float x, float y, string button = MouseButton.Left,
        string eventType = InputEventType.Press,
        CancellationToken cancellationToken = default)
    {
        var message = await inputService.SendMouseEventAsync(x, y, button, eventType, cancellationToken);
        Console.WriteLine(message);
    }
}
