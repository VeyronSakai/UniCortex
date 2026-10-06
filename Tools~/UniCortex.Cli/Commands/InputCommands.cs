using ConsoleAppFramework;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Cli.Commands;

#pragma warning disable CS1573 // Parameter has no matching param tag
public class InputKeyCommands(InputUseCase inputUseCase)
{
    /// <summary>Press keys of the keyboard via Input System in Play Mode: press all the keys in the same frame, keep them pressed for --hold-duration seconds, and release them. Requires com.unity.inputsystem.</summary>
    /// <param name="keys">Comma-separated Input System Key enum names of the keys to press together, e.g. Space, or LeftCtrl,S for Ctrl+S. Available keys: A-Z, Digit0-Digit9, F1-F12, Space, Enter, Tab, Backspace, Delete, Insert, Escape, ContextMenu, LeftArrow, RightArrow, UpArrow, DownArrow, PageUp, PageDown, Home, End, LeftShift, RightShift, LeftCtrl, RightCtrl, LeftAlt, RightAlt, LeftMeta, RightMeta, Backquote, Quote, Semicolon, Comma, Period, Slash, Backslash, LeftBracket, RightBracket, Minus, Equals, CapsLock, NumLock, ScrollLock, PrintScreen, Pause, Numpad0-Numpad9, NumpadEnter, NumpadDivide, NumpadMultiply, NumpadPlus, NumpadMinus, NumpadPeriod, NumpadEquals, OEM1-OEM5, IMESelected.</param>
    /// <param name="holdDuration">Seconds to keep the keys pressed before releasing them (default 0: release in the next frame).</param>
    [Command("press")]
    public async Task Press([Argument] string[] keys, float? holdDuration = null,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(await inputUseCase.PressKeyAsync(keys, holdDuration, cancellationToken));
    }
}

public class InputMouseCommands(InputUseCase inputUseCase)
{
    /// <summary>Click (or tap) at a position: press, keep it pressed for --hold-duration seconds, and release. Simulates the Input System Mouse device in Play Mode. Requires com.unity.inputsystem. Specify either --x and --y, or --instance-id (requires com.unity.ugui).</summary>
    /// <param name="x">X coordinate in screen pixels (Screen.width space). Origin (0,0) is at the bottom-left of the Game View. Increases to the right. Note: capture_game_view images are at the Game View resolution with a top-left origin, so a pixel (px, py) in the image corresponds to x = px, y = imageHeight - py.</param>
    /// <param name="y">Y coordinate in screen pixels (Screen.height space). Origin (0,0) is at the bottom-left of the Game View. Increases upward. This is the inverse of typical image coordinates where Y increases downward.</param>
    /// <param name="instanceId">instanceId of the UI GameObject. Its center is used.</param>
    /// <param name="button">Mouse button: "left" (default), "right", or "middle".</param>
    /// <param name="holdDuration">Seconds to keep the button pressed before releasing it, e.g. for a long press (default 0: release in the next frame).</param>
    [Command("click")]
    public async Task Click(float? x = null, float? y = null, int? instanceId = null,
        string button = MouseButton.Left, float? holdDuration = null, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(await inputUseCase.ClickMouseAsync(x, y, instanceId, button, holdDuration,
            cancellationToken));
    }

    /// <summary>Drag (or swipe) in one call: press at the start, move to the end over --duration seconds (at most one move per frame), and release. Simulates the Input System Mouse device in Play Mode. Requires com.unity.inputsystem. Specify the start with --from-x and --from-y, or --from-instance-id (requires com.unity.ugui), and the end with --to-x and --to-y, or --to-instance-id.</summary>
    /// <param name="fromX">--from-x, Start X coordinate of the drag in screen pixels (Screen.width space). Origin (0,0) is at the bottom-left of the Game View. Increases to the right. Note: capture_game_view images are at the Game View resolution with a top-left origin, so a pixel (px, py) in the image corresponds to x = px, y = imageHeight - py. Must be given together with --from-y.</param>
    /// <param name="fromY">--from-y, Start Y coordinate of the drag in screen pixels (Screen.height space). Origin (0,0) is at the bottom-left of the Game View. Increases upward. Must be given together with --from-x.</param>
    /// <param name="fromInstanceId">instanceId of the UI GameObject whose center is the start of the drag.</param>
    /// <param name="toX">--to-x, End X coordinate of the drag. Must be given together with --to-y.</param>
    /// <param name="toY">--to-y, End Y coordinate of the drag. Must be given together with --to-x.</param>
    /// <param name="toInstanceId">instanceId of the UI GameObject whose center is the end of the drag.</param>
    /// <param name="button">Mouse button: "left" (default), "right", or "middle".</param>
    /// <param name="duration">Seconds to move from the start to the end (default 0.2).</param>
    [Command("drag")]
    public async Task Drag(float? fromX = null, float? fromY = null, int? fromInstanceId = null,
        float? toX = null, float? toY = null, int? toInstanceId = null, string button = MouseButton.Left,
        float? duration = null, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(await inputUseCase.DragMouseAsync(fromX, fromY, fromInstanceId, toX, toY, toInstanceId,
            button, duration, cancellationToken));
    }

    /// <summary>Move the mouse to a position without pressing a button, e.g. for hover. Simulates the Input System Mouse device in Play Mode. Requires com.unity.inputsystem. Specify either --x and --y, or --instance-id (requires com.unity.ugui).</summary>
    /// <param name="x">X coordinate in screen pixels (Screen.width space). Origin (0,0) is at the bottom-left of the Game View. Increases to the right. Note: capture_game_view images are at the Game View resolution with a top-left origin, so a pixel (px, py) in the image corresponds to x = px, y = imageHeight - py.</param>
    /// <param name="y">Y coordinate in screen pixels (Screen.height space). Origin (0,0) is at the bottom-left of the Game View. Increases upward. This is the inverse of typical image coordinates where Y increases downward.</param>
    /// <param name="instanceId">instanceId of the UI GameObject. Its center is used.</param>
    [Command("move")]
    public async Task Move(float? x = null, float? y = null, int? instanceId = null,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(await inputUseCase.MoveMouseAsync(x, y, instanceId, cancellationToken));
    }
}

public class InputPointerCommands(InputUseCase inputUseCase)
{
    /// <summary>List the uGUI objects in the Game View that can be pressed now, with their rects in Game View coordinates, in Play Mode. Requires com.unity.ugui.</summary>
    [Command("targets")]
    public async Task Targets(CancellationToken cancellationToken = default)
    {
        var json = await inputUseCase.GetPointerTargetsAsync(cancellationToken);
        Console.WriteLine(json);
    }
}
