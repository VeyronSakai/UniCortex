using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class InputTools(InputUseCase inputUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "send_key_event", ReadOnly = false),
     Description(
         "Send a keyboard event via Unity Input System (com.unity.inputsystem) in Play Mode. " +
         "Uses InputSystem.QueueEvent() to simulate device-level input. " +
         "Triggers Input System actions (InputAction, PlayerInput) and Keyboard.current key states. " +
         "Requires the Input System package to be installed. " +
         "Does NOT work with legacy UnityEngine.Input.GetKey()."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SendKeyEventAsync(
        [Description(
            "Input System Key enum name. Available keys: " +
            // Letters
            KeyName.A + ", " + KeyName.B + ", " + KeyName.C + ", " + KeyName.D + ", " +
            KeyName.E + ", " + KeyName.F + ", " + KeyName.G + ", " + KeyName.H + ", " +
            KeyName.I + ", " + KeyName.J + ", " + KeyName.K + ", " + KeyName.L + ", " +
            KeyName.M + ", " + KeyName.N + ", " + KeyName.O + ", " + KeyName.P + ", " +
            KeyName.Q + ", " + KeyName.R + ", " + KeyName.S + ", " + KeyName.T + ", " +
            KeyName.U + ", " + KeyName.V + ", " + KeyName.W + ", " + KeyName.X + ", " +
            KeyName.Y + ", " + KeyName.Z + ", " +
            // Digits
            KeyName.Digit0 + "-" + KeyName.Digit9 + ", " +
            // Function keys
            KeyName.F1 + "-" + KeyName.F12 + ", " +
            // Editing
            KeyName.Space + ", " + KeyName.Enter + ", " + KeyName.Tab + ", " +
            KeyName.Backspace + ", " + KeyName.Delete + ", " + KeyName.Insert + ", " +
            KeyName.Escape + ", " + KeyName.ContextMenu + ", " +
            // Navigation
            KeyName.LeftArrow + ", " + KeyName.RightArrow + ", " +
            KeyName.UpArrow + ", " + KeyName.DownArrow + ", " +
            KeyName.PageUp + ", " + KeyName.PageDown + ", " +
            KeyName.Home + ", " + KeyName.End + ", " +
            // Modifiers
            KeyName.LeftShift + ", " + KeyName.RightShift + ", " +
            KeyName.LeftCtrl + ", " + KeyName.RightCtrl + ", " +
            KeyName.LeftAlt + ", " + KeyName.RightAlt + ", " +
            KeyName.LeftMeta + ", " + KeyName.RightMeta + ", " +
            // Punctuation and symbols
            KeyName.Backquote + ", " + KeyName.Quote + ", " + KeyName.Semicolon + ", " +
            KeyName.Comma + ", " + KeyName.Period + ", " + KeyName.Slash + ", " +
            KeyName.Backslash + ", " + KeyName.LeftBracket + ", " + KeyName.RightBracket + ", " +
            KeyName.Minus + ", " + KeyName.Equals + ", " +
            // Lock and toggle keys
            KeyName.CapsLock + ", " + KeyName.NumLock + ", " + KeyName.ScrollLock + ", " +
            KeyName.PrintScreen + ", " + KeyName.Pause + ", " +
            // Numpad
            KeyName.Numpad0 + "-" + KeyName.Numpad9 + ", " +
            KeyName.NumpadEnter + ", " + KeyName.NumpadDivide + ", " +
            KeyName.NumpadMultiply + ", " + KeyName.NumpadPlus + ", " +
            KeyName.NumpadMinus + ", " + KeyName.NumpadPeriod + ", " + KeyName.NumpadEquals + ", " +
            // OEM and IME
            KeyName.OEM1 + "-" + KeyName.OEM5 + ", " + KeyName.IMESelected)]
        string key,
        [Description($"Event type: \"{InputEventType.Press}\" (default) or \"{InputEventType.Release}\".")]
        string eventType = InputEventType.Press,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.SendKeyEventAsync(key, eventType, ct), cancellationToken);

    private const string PointerToolDescription =
        "Uses InputSystem.QueueEvent() to simulate the Mouse device of Unity Input System (com.unity.inputsystem) " +
        "in Play Mode, so uGUI (EventSystem) receives it as a pointer, the same as a tap on a touch screen. " +
        "Triggers Input System actions (InputAction, PlayerInput) and Mouse.current states. " +
        "Specify the position either with x and y, or with instanceId to use the center of a UI element; " +
        "exactly one of them is required. " +
        "With a target, the event still goes through the EventSystem raycast like a real tap, " +
        "so a target covered by other UI does not receive it. " +
        "Requires the Input System package to be installed (and com.unity.ugui for a target). " +
        "Does NOT work with legacy UnityEngine.Input.GetMouseButton().";

    private const string XDescription =
        "X coordinate in screen pixels (Screen.width space). Origin (0,0) is at the bottom-left of the Game View. Increases to the right. Note: capture_game_view images are at the Game View resolution with a top-left origin, so a pixel (px, py) in the image corresponds to x = px, y = imageHeight - py. Must be given together with y, and not with instanceId.";

    private const string YDescription =
        "Y coordinate in screen pixels (Screen.height space). Origin (0,0) is at the bottom-left of the Game View. Increases upward. This is the inverse of typical image coordinates where Y increases downward. Must be given together with x, and not with instanceId.";

    private const string InstanceIdDescription =
        "instanceId of the UI GameObject (a RectTransform under a Canvas), e.g. from get_pointer_targets. Its center is used. Cannot be combined with x/y.";

    private const string ButtonDescription =
        $"Mouse button: \"{MouseButton.Left}\" (default), \"{MouseButton.Right}\", or \"{MouseButton.Middle}\". Use the default for a tap.";

    [McpServerTool(Name = "click_pointer", ReadOnly = false),
     Description("Click (or tap) at a position: press, then release after one frame. " + PointerToolDescription),
     UsedImplicitly]
    public ValueTask<CallToolResult> ClickPointerAsync(
        [Description(XDescription)] float? x = null,
        [Description(YDescription)] float? y = null,
        [Description(InstanceIdDescription)] int? instanceId = null,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.ClickPointerAsync(x, y, instanceId, button, ct), cancellationToken);

    [McpServerTool(Name = "drag_pointer", ReadOnly = false),
     Description(
         "Drag (or swipe) in one call: press at the start, move along a straight line to the end with one move per frame, " +
         "and release at the end. Returns after the release has been processed. " +
         "Because the movement is spread over frames, ScrollRect inertia, swipe detection and the EventSystem drag threshold " +
         "behave as with a real drag. " + PointerToolDescription),
     UsedImplicitly]
    public ValueTask<CallToolResult> DragPointerAsync(
        [Description("Start of the drag. " + XDescription)] float? x = null,
        [Description("Start of the drag. " + YDescription)] float? y = null,
        [Description("Start of the drag. " + InstanceIdDescription)] int? instanceId = null,
        [Description("End X coordinate of the drag, in the same space as x. Must be given together with toY, and not with toInstanceId.")]
        float? toX = null,
        [Description("End Y coordinate of the drag, in the same space as y. Must be given together with toX, and not with toInstanceId.")]
        float? toY = null,
        [Description("instanceId of the UI GameObject whose center is the end of the drag. Cannot be combined with toX/toY.")]
        int? toInstanceId = null,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        [Description("Number of frames to move from the start to the end, with one move per frame (default 10, at least 1).")]
        int? frames = null,
        [Description("Number of frames to keep the button pressed at the start before moving, e.g. for long-press-then-drag (default 0).")]
        int? holdFrames = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.DragPointerAsync(x, y, instanceId, toX, toY, toInstanceId, button, frames,
                holdFrames, ct),
            cancellationToken);

    [McpServerTool(Name = "move_pointer", ReadOnly = false),
     Description("Move the pointer to a position without changing the button state, e.g. for hover, " +
                 "or to move while a button is pressed with press_pointer. " + PointerToolDescription),
     UsedImplicitly]
    public ValueTask<CallToolResult> MovePointerAsync(
        [Description(XDescription)] float? x = null,
        [Description(YDescription)] float? y = null,
        [Description(InstanceIdDescription)] int? instanceId = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.MovePointerAsync(x, y, instanceId, ct), cancellationToken);

    [McpServerTool(Name = "press_pointer", ReadOnly = false),
     Description("Press a button at a position and keep it pressed until release_pointer. " +
                 "Use click_pointer or drag_pointer unless you need full control. " + PointerToolDescription),
     UsedImplicitly]
    public ValueTask<CallToolResult> PressPointerAsync(
        [Description(XDescription)] float? x = null,
        [Description(YDescription)] float? y = null,
        [Description(InstanceIdDescription)] int? instanceId = null,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.PressPointerAsync(x, y, instanceId, button, ct), cancellationToken);

    [McpServerTool(Name = "release_pointer", ReadOnly = false),
     Description("Release a button at a position, e.g. after press_pointer. " + PointerToolDescription),
     UsedImplicitly]
    public ValueTask<CallToolResult> ReleasePointerAsync(
        [Description(XDescription)] float? x = null,
        [Description(YDescription)] float? y = null,
        [Description(InstanceIdDescription)] int? instanceId = null,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.ReleasePointerAsync(x, y, instanceId, button, ct), cancellationToken);

    [McpServerTool(Name = "get_pointer_targets", ReadOnly = true),
     Description(
         "List the uGUI objects in the Game View that can be pressed now, in Play Mode. " +
         "An object is listed when it is active, is under a Canvas, has an enabled component handling pointer events " +
         "(e.g. Button, Toggle, Slider, ScrollRect, EventTrigger, custom drag or long-press components), " +
         "is interactable (Selectable.IsInteractable()), and is the topmost EventSystem raycast hit at its center " +
         "(not covered by other UI and not off-screen). " +
         "Each item has the Hierarchy path, instanceId, and rect in Game View coordinates (same as x/y of the pointer tools). " +
         "Pass instanceId to click_pointer (or the other pointer tools) to press one. " +
         "Requires the uGUI package (com.unity.ugui) and an EventSystem in the scene."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetPointerTargetsAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            inputUseCase.GetPointerTargetsAsync, cancellationToken);
}
