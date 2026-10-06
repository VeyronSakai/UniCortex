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

    private const string InputDescription =
        "Uses InputSystem.QueueEvent() to simulate the Mouse device of Unity Input System (com.unity.inputsystem) " +
        "in Play Mode, so uGUI (EventSystem) receives it as a pointer, the same as a tap on a touch screen. " +
        "Triggers Input System actions (InputAction, PlayerInput) and Mouse.current states. " +
        "Requires the Input System package to be installed. " +
        "Does NOT work with legacy UnityEngine.Input.GetMouseButton().";

    private const string PointerDescription =
        "The position is given in Game View coordinates. " +
        "To operate a GameObject by its instanceId instead, use the *_game_object tools. " + InputDescription;

    private const string GameObjectDescription =
        "The position is the center of a GameObject given by its instanceId, e.g. from get_pointer_targets. " +
        "Currently only uGUI elements (a RectTransform under a Canvas) are supported. " +
        "The event still goes through the EventSystem raycast like a real tap, " +
        "so a GameObject covered by other UI does not receive it. " +
        "To use Game View coordinates instead, use the *_pointer tools. " +
        "Also requires the uGUI package (com.unity.ugui). " + InputDescription;

    private const string CoordinatesDescription =
        "in screen pixels (Screen.width / Screen.height space). Origin (0,0) is at the bottom-left of the Game View; X increases to the right and Y increases upward. Note: capture_game_view images are at the Game View resolution with a top-left origin, so a pixel (px, py) in the image corresponds to x = px, y = imageHeight - py.";

    private const string InstanceIdDescription =
        "instanceId of the GameObject, e.g. from get_pointer_targets. Its center is used. Currently only uGUI elements (a RectTransform under a Canvas) are supported.";

    private const string ButtonDescription =
        $"Mouse button: \"{MouseButton.Left}\" (default), \"{MouseButton.Right}\", or \"{MouseButton.Middle}\". Use the default for a tap.";

    private const string FramesDescription =
        "Number of frames to move from the start to the end, with one move per frame (default 10, at least 1).";

    private const string HoldFramesDescription =
        "Number of frames to keep the button pressed at the start before moving, e.g. for long-press-then-drag (default 0).";

    private const string ClickDescription = "Click (or tap): press, then release after one frame. ";

    private const string DragDescription =
        "Drag (or swipe) in one call: press at the start, move along a straight line to the end with one move per frame, " +
        "and release at the end. Returns after the release has been processed. " +
        "Because the movement is spread over frames, ScrollRect inertia, swipe detection and the EventSystem drag threshold " +
        "behave as with a real drag. ";

    private const string MoveDescription =
        "Move the pointer without changing the button state, e.g. for hover, or to move while a button is pressed. ";

    private const string PressDescription =
        "Press a button and keep it pressed until it is released. Use click or drag unless you need full control. ";

    private const string ReleaseDescription = "Release a button, e.g. after press. ";

    [McpServerTool(Name = "click_pointer", ReadOnly = false),
     Description(ClickDescription + PointerDescription), UsedImplicitly]
    public ValueTask<CallToolResult> ClickPointerAsync(
        [Description("X coordinate " + CoordinatesDescription)] float x,
        [Description("Y coordinate " + CoordinatesDescription)] float y,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.ClickPointerAsync(x, y, button, ct), cancellationToken);

    [McpServerTool(Name = "drag_pointer", ReadOnly = false),
     Description(DragDescription + PointerDescription), UsedImplicitly]
    public ValueTask<CallToolResult> DragPointerAsync(
        [Description("Start X coordinate " + CoordinatesDescription)] float fromX,
        [Description("Start Y coordinate " + CoordinatesDescription)] float fromY,
        [Description("End X coordinate, in the same space as fromX.")] float toX,
        [Description("End Y coordinate, in the same space as fromY.")] float toY,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        [Description(FramesDescription)] int? frames = null,
        [Description(HoldFramesDescription)] int? holdFrames = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.DragPointerAsync(fromX, fromY, toX, toY, button, frames, holdFrames, ct),
            cancellationToken);

    [McpServerTool(Name = "move_pointer", ReadOnly = false),
     Description(MoveDescription + PointerDescription), UsedImplicitly]
    public ValueTask<CallToolResult> MovePointerAsync(
        [Description("X coordinate " + CoordinatesDescription)] float x,
        [Description("Y coordinate " + CoordinatesDescription)] float y,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.MovePointerAsync(x, y, ct), cancellationToken);

    [McpServerTool(Name = "press_pointer", ReadOnly = false),
     Description(PressDescription + PointerDescription), UsedImplicitly]
    public ValueTask<CallToolResult> PressPointerAsync(
        [Description("X coordinate " + CoordinatesDescription)] float x,
        [Description("Y coordinate " + CoordinatesDescription)] float y,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.PressPointerAsync(x, y, button, ct), cancellationToken);

    [McpServerTool(Name = "release_pointer", ReadOnly = false),
     Description(ReleaseDescription + PointerDescription), UsedImplicitly]
    public ValueTask<CallToolResult> ReleasePointerAsync(
        [Description("X coordinate " + CoordinatesDescription)] float x,
        [Description("Y coordinate " + CoordinatesDescription)] float y,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.ReleasePointerAsync(x, y, button, ct), cancellationToken);

    [McpServerTool(Name = "click_game_object", ReadOnly = false),
     Description(ClickDescription + GameObjectDescription), UsedImplicitly]
    public ValueTask<CallToolResult> ClickGameObjectAsync(
        [Description(InstanceIdDescription)] int instanceId,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.ClickGameObjectAsync(instanceId, button, ct), cancellationToken);

    [McpServerTool(Name = "drag_game_object", ReadOnly = false),
     Description(DragDescription + GameObjectDescription), UsedImplicitly]
    public ValueTask<CallToolResult> DragGameObjectAsync(
        [Description("Start of the drag. " + InstanceIdDescription)] int fromInstanceId,
        [Description("End of the drag. " + InstanceIdDescription)] int toInstanceId,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        [Description(FramesDescription)] int? frames = null,
        [Description(HoldFramesDescription)] int? holdFrames = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.DragGameObjectAsync(fromInstanceId, toInstanceId, button, frames, holdFrames, ct),
            cancellationToken);

    [McpServerTool(Name = "move_game_object", ReadOnly = false),
     Description(MoveDescription + GameObjectDescription), UsedImplicitly]
    public ValueTask<CallToolResult> MoveGameObjectAsync(
        [Description(InstanceIdDescription)] int instanceId,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.MoveGameObjectAsync(instanceId, ct), cancellationToken);

    [McpServerTool(Name = "press_game_object", ReadOnly = false),
     Description(PressDescription + GameObjectDescription), UsedImplicitly]
    public ValueTask<CallToolResult> PressGameObjectAsync(
        [Description(InstanceIdDescription)] int instanceId,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.PressGameObjectAsync(instanceId, button, ct), cancellationToken);

    [McpServerTool(Name = "release_game_object", ReadOnly = false),
     Description(ReleaseDescription + GameObjectDescription), UsedImplicitly]
    public ValueTask<CallToolResult> ReleaseGameObjectAsync(
        [Description(InstanceIdDescription)] int instanceId,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.ReleaseGameObjectAsync(instanceId, button, ct), cancellationToken);

    [McpServerTool(Name = "get_pointer_targets", ReadOnly = true),
     Description(
         "List the uGUI objects in the Game View that can be pressed now, in Play Mode. " +
         "An object is listed when it is active, is under a Canvas, has an enabled component handling pointer events " +
         "(e.g. Button, Toggle, Slider, ScrollRect, EventTrigger, custom drag or long-press components), " +
         "is interactable (Selectable.IsInteractable()), and is the topmost EventSystem raycast hit at its center " +
         "(not covered by other UI and not off-screen). " +
         "Each item has the Hierarchy path, instanceId, and rect in Game View coordinates (same as x/y of the *_pointer tools). " +
         "Pass instanceId to click_game_object (or the other *_game_object tools) to press one. " +
         "Requires the uGUI package (com.unity.ugui) and an EventSystem in the scene."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetPointerTargetsAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            inputUseCase.GetPointerTargetsAsync, cancellationToken);
}
