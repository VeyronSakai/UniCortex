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
    private const string KeysDescription =
        "Input System Key enum names of the keys to press together, e.g. [\"Space\"], or [\"LeftCtrl\", \"S\"] for Ctrl+S. " +
        "Available keys: A-Z, Digit0-Digit9, F1-F12, Space, Enter, Tab, Backspace, Delete, Insert, Escape, " +
        "ContextMenu, LeftArrow, RightArrow, UpArrow, DownArrow, PageUp, PageDown, Home, End, " +
        "LeftShift, RightShift, LeftCtrl, RightCtrl, LeftAlt, RightAlt, LeftMeta, RightMeta, " +
        "Backquote, Quote, Semicolon, Comma, Period, Slash, Backslash, LeftBracket, RightBracket, Minus, Equals, " +
        "CapsLock, NumLock, ScrollLock, PrintScreen, Pause, Numpad0-Numpad9, NumpadEnter, NumpadDivide, " +
        "NumpadMultiply, NumpadPlus, NumpadMinus, NumpadPeriod, NumpadEquals, OEM1-OEM5, IMESelected.";

    [McpServerTool(Name = "press_key", ReadOnly = false),
     Description(
         "Press keys of the keyboard via Unity Input System (com.unity.inputsystem) in Play Mode: " +
         "press all the keys in the same frame, keep them pressed for holdDuration seconds, and release them " +
         "in the same frame. Pass several keys for a combination such as Ctrl+S. " +
         "Use holdDuration to keep a key pressed, e.g. a movement key. " +
         "Returns after the release has been processed. " +
         "Uses InputSystem.QueueEvent() to simulate device-level input. " +
         "Triggers Input System actions (InputAction, PlayerInput) and Keyboard.current key states. " +
         "Does NOT type text or edit text fields (InputField, TMP_InputField, UI Toolkit TextField), " +
         "because they read typed characters and editing keys from IMGUI events, not from key states; " +
         "use type_text to type text. " +
         "Requires the Input System package to be installed. " +
         "Does NOT work with legacy UnityEngine.Input.GetKey()."),
     UsedImplicitly]
    public ValueTask<CallToolResult> PressKeyAsync(
        [Description(KeysDescription)] string[] keys,
        [Description("Seconds to keep the keys pressed before releasing them (default 0: release in the next frame).")]
        float? holdDuration = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.PressKeyAsync(keys, holdDuration, ct), cancellationToken);

    [McpServerTool(Name = "type_text", ReadOnly = false),
     Description(
         "Type text into the focused text field in Play Mode, e.g. a uGUI InputField, a TextMeshPro TMP_InputField " +
         "or a UI Toolkit TextField. Focus the field first, e.g. with click_mouse. " +
         "Any characters can be typed, including upper and lower case letters, symbols and Japanese. " +
         "All the characters are typed in the same frame. Returns after they have been processed. " +
         "Also raises Keyboard.onTextInput of Unity Input System for each character. " +
         "Does not change key states (Keyboard.current, InputAction); use press_key for that. " +
         "Editing and submitting keys such as Backspace and Enter are not supported. " +
         "Requires the Input System package to be installed."),
     UsedImplicitly]
    public ValueTask<CallToolResult> TypeTextAsync(
        [Description("Text to type, e.g. \"Hello\" or \"こんにちは\".")] string text,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.TypeTextAsync(text, ct), cancellationToken);

    private const string MouseToolDescription =
        "Uses InputSystem.QueueEvent() to simulate the Mouse device of Unity Input System (com.unity.inputsystem) " +
        "in Play Mode, so uGUI (EventSystem) receives it as a pointer, the same as a tap on a touch screen. " +
        "The UI of touch-screen games can also be operated this way. " +
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
        "instanceId of the UI GameObject (a RectTransform under a Canvas), e.g. from get_ui_pointer_targets. Its center is used. Cannot be combined with x/y.";

    private const string ButtonDescription =
        $"Mouse button: \"{MouseButton.Left}\" (default), \"{MouseButton.Right}\", or \"{MouseButton.Middle}\". Use the default for a tap.";

    [McpServerTool(Name = "click_mouse", ReadOnly = false),
     Description("Click (or tap) at a position: press, keep it pressed for holdDuration seconds, and release. " +
                 "Use holdDuration for a long press. Returns after the release has been processed. " +
                 MouseToolDescription),
     UsedImplicitly]
    public ValueTask<CallToolResult> ClickMouseAsync(
        [Description(XDescription)] float? x = null,
        [Description(YDescription)] float? y = null,
        [Description(InstanceIdDescription)] int? instanceId = null,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        [Description("Seconds to keep the button pressed before releasing it, e.g. for a long press (default 0: release in the next frame).")]
        float? holdDuration = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.ClickMouseAsync(x, y, instanceId, button, holdDuration, ct), cancellationToken);

    [McpServerTool(Name = "drag_mouse", ReadOnly = false),
     Description(
         "Drag (or swipe) in one call: press at the start, move along a straight line to the end over duration seconds " +
         "(at most one move per frame), and release at the end. Returns after the release has been processed. " +
         "Because the movement is spread over frames, ScrollRect inertia, swipe detection and the EventSystem drag threshold " +
         "behave as with a real drag. " + MouseToolDescription),
     UsedImplicitly]
    public ValueTask<CallToolResult> DragMouseAsync(
        [Description("Start X coordinate of the drag, in screen pixels (Screen.width space). Origin (0,0) is at the bottom-left of the Game View. Increases to the right. Note: capture_game_view images are at the Game View resolution with a top-left origin, so a pixel (px, py) in the image corresponds to x = px, y = imageHeight - py. Must be given together with fromY, and not with fromInstanceId.")]
        float? fromX = null,
        [Description("Start Y coordinate of the drag, in screen pixels (Screen.height space). Origin (0,0) is at the bottom-left of the Game View. Increases upward. Must be given together with fromX, and not with fromInstanceId.")]
        float? fromY = null,
        [Description("instanceId of the UI GameObject (a RectTransform under a Canvas) whose center is the start of the drag, e.g. from get_ui_pointer_targets. Cannot be combined with fromX/fromY.")]
        int? fromInstanceId = null,
        [Description("End X coordinate of the drag, in the same space as fromX. Must be given together with toY, and not with toInstanceId.")]
        float? toX = null,
        [Description("End Y coordinate of the drag, in the same space as fromY. Must be given together with toX, and not with toInstanceId.")]
        float? toY = null,
        [Description("instanceId of the UI GameObject whose center is the end of the drag. Cannot be combined with toX/toY.")]
        int? toInstanceId = null,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        [Description("Seconds to move from the start to the end (default 0.2). With 0, it moves to the end in one frame.")]
        float? duration = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.DragMouseAsync(fromX, fromY, fromInstanceId, toX, toY, toInstanceId, button, duration,
                ct),
            cancellationToken);

    [McpServerTool(Name = "move_mouse", ReadOnly = false),
     Description("Move the mouse to a position without pressing a button, e.g. for hover. " +
                 MouseToolDescription),
     UsedImplicitly]
    public ValueTask<CallToolResult> MoveMouseAsync(
        [Description(XDescription)] float? x = null,
        [Description(YDescription)] float? y = null,
        [Description(InstanceIdDescription)] int? instanceId = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.MoveMouseAsync(x, y, instanceId, ct), cancellationToken);

    [McpServerTool(Name = "get_ui_pointer_targets", ReadOnly = true),
     Description(
         "List the uGUI objects in the Game View that can be pressed now, in Play Mode. " +
         "An object is listed when it is active, is under a Canvas, has an enabled component handling pointer events " +
         "(e.g. Button, Toggle, Slider, ScrollRect, EventTrigger, custom drag or long-press components), " +
         "is interactable (Selectable.IsInteractable()), and is the topmost EventSystem raycast hit at its center " +
         "(not covered by other UI and not off-screen). " +
         "Each item has the Hierarchy path, instanceId, and rect in Game View coordinates (same as x/y of the mouse tools). " +
         "Pass instanceId to click_mouse (or the other mouse tools) to press one. " +
         "Requires the uGUI package (com.unity.ugui) and an EventSystem in the scene."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetUIPointerTargetsAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            inputUseCase.GetUIPointerTargetsAsync, cancellationToken);
}
