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
}

public class InputPointerCommands(InputUseCase inputUseCase)
{
    /// <summary>Click (or tap) at a position: press, then release after one frame. Simulates the Input System Mouse device in Play Mode. Requires com.unity.inputsystem. Specify either --x and --y, or --instance-id (requires com.unity.ugui).</summary>
    /// <param name="x">X coordinate in screen pixels (Screen.width space). Origin (0,0) is at the bottom-left of the Game View. Increases to the right. Note: capture_game_view images are at the Game View resolution with a top-left origin, so a pixel (px, py) in the image corresponds to x = px, y = imageHeight - py.</param>
    /// <param name="y">Y coordinate in screen pixels (Screen.height space). Origin (0,0) is at the bottom-left of the Game View. Increases upward. This is the inverse of typical image coordinates where Y increases downward.</param>
    /// <param name="instanceId">instanceId of the UI GameObject. Its center is used.</param>
    /// <param name="button">Mouse button: "left" (default), "right", or "middle".</param>
    [Command("click")]
    public async Task Click(float? x = null, float? y = null, int? instanceId = null,
        string button = MouseButton.Left, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(await inputUseCase.ClickPointerAsync(x, y, instanceId, button, cancellationToken));
    }

    /// <summary>Drag (or swipe) in one call: press at the start, move to the end with one move per frame, and release. Simulates the Input System Mouse device in Play Mode. Requires com.unity.inputsystem. Specify either --x and --y, or --instance-id (requires com.unity.ugui). Specify the end with --to-x and --to-y, or --to-instance-id.</summary>
    /// <param name="x">X coordinate in screen pixels (Screen.width space). Origin (0,0) is at the bottom-left of the Game View. Increases to the right. Note: capture_game_view images are at the Game View resolution with a top-left origin, so a pixel (px, py) in the image corresponds to x = px, y = imageHeight - py.</param>
    /// <param name="y">Y coordinate in screen pixels (Screen.height space). Origin (0,0) is at the bottom-left of the Game View. Increases upward. This is the inverse of typical image coordinates where Y increases downward.</param>
    /// <param name="instanceId">instanceId of the UI GameObject. Its center is used.</param>
    /// <param name="toX">--to-x, End X coordinate of the drag. Must be given together with --to-y.</param>
    /// <param name="toY">--to-y, End Y coordinate of the drag. Must be given together with --to-x.</param>
    /// <param name="toInstanceId">instanceId of the UI GameObject whose center is the end of the drag.</param>
    /// <param name="button">Mouse button: "left" (default), "right", or "middle".</param>
    /// <param name="frames">Number of frames to move from the start to the end, one move per frame (default 10).</param>
    /// <param name="holdFrames">Number of frames to keep the button pressed before moving (default 0).</param>
    [Command("drag")]
    public async Task Drag(float? x = null, float? y = null, int? instanceId = null,
        float? toX = null, float? toY = null, int? toInstanceId = null, string button = MouseButton.Left,
        int? frames = null, int? holdFrames = null, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(await inputUseCase.DragPointerAsync(x, y, instanceId, toX, toY, toInstanceId, button,
            frames, holdFrames, cancellationToken));
    }

    /// <summary>Move the pointer to a position without changing the button state. Simulates the Input System Mouse device in Play Mode. Requires com.unity.inputsystem. Specify either --x and --y, or --instance-id (requires com.unity.ugui).</summary>
    /// <param name="x">X coordinate in screen pixels (Screen.width space). Origin (0,0) is at the bottom-left of the Game View. Increases to the right. Note: capture_game_view images are at the Game View resolution with a top-left origin, so a pixel (px, py) in the image corresponds to x = px, y = imageHeight - py.</param>
    /// <param name="y">Y coordinate in screen pixels (Screen.height space). Origin (0,0) is at the bottom-left of the Game View. Increases upward. This is the inverse of typical image coordinates where Y increases downward.</param>
    /// <param name="instanceId">instanceId of the UI GameObject. Its center is used.</param>
    [Command("move")]
    public async Task Move(float? x = null, float? y = null, int? instanceId = null,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(await inputUseCase.MovePointerAsync(x, y, instanceId, cancellationToken));
    }

    /// <summary>Press a button at a position and keep it pressed until release. Simulates the Input System Mouse device in Play Mode. Requires com.unity.inputsystem. Specify either --x and --y, or --instance-id (requires com.unity.ugui).</summary>
    /// <param name="x">X coordinate in screen pixels (Screen.width space). Origin (0,0) is at the bottom-left of the Game View. Increases to the right. Note: capture_game_view images are at the Game View resolution with a top-left origin, so a pixel (px, py) in the image corresponds to x = px, y = imageHeight - py.</param>
    /// <param name="y">Y coordinate in screen pixels (Screen.height space). Origin (0,0) is at the bottom-left of the Game View. Increases upward. This is the inverse of typical image coordinates where Y increases downward.</param>
    /// <param name="instanceId">instanceId of the UI GameObject. Its center is used.</param>
    /// <param name="button">Mouse button: "left" (default), "right", or "middle".</param>
    [Command("press")]
    public async Task Press(float? x = null, float? y = null, int? instanceId = null,
        string button = MouseButton.Left, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(await inputUseCase.PressPointerAsync(x, y, instanceId, button, cancellationToken));
    }

    /// <summary>Release a button at a position. Simulates the Input System Mouse device in Play Mode. Requires com.unity.inputsystem. Specify either --x and --y, or --instance-id (requires com.unity.ugui).</summary>
    /// <param name="x">X coordinate in screen pixels (Screen.width space). Origin (0,0) is at the bottom-left of the Game View. Increases to the right. Note: capture_game_view images are at the Game View resolution with a top-left origin, so a pixel (px, py) in the image corresponds to x = px, y = imageHeight - py.</param>
    /// <param name="y">Y coordinate in screen pixels (Screen.height space). Origin (0,0) is at the bottom-left of the Game View. Increases upward. This is the inverse of typical image coordinates where Y increases downward.</param>
    /// <param name="instanceId">instanceId of the UI GameObject. Its center is used.</param>
    /// <param name="button">Mouse button: "left" (default), "right", or "middle".</param>
    [Command("release")]
    public async Task Release(float? x = null, float? y = null, int? instanceId = null,
        string button = MouseButton.Left, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(await inputUseCase.ReleasePointerAsync(x, y, instanceId, button, cancellationToken));
    }

    /// <summary>List the uGUI objects in the Game View that can be pressed now, with their rects in Game View coordinates, in Play Mode. Requires com.unity.ugui.</summary>
    [Command("targets")]
    public async Task Targets(CancellationToken cancellationToken = default)
    {
        var json = await inputUseCase.GetPointerTargetsAsync(cancellationToken);
        Console.WriteLine(json);
    }
}
