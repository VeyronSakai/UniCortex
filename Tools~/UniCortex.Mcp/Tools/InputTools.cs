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
        "Input System Key enum names pressed together, e.g. [\"Space\"] or [\"LeftCtrl\", \"S\"] for Ctrl+S. " +
        "Names include A-Z, Digit0-Digit9, F1-F12, Enter, Escape, Tab, Backspace, Delete, " +
        "LeftArrow/RightArrow/UpArrow/DownArrow, LeftShift, LeftCtrl, LeftAlt, LeftMeta, Numpad0-Numpad9, NumpadEnter.";

    [McpServerTool(Name = "press_key", ReadOnly = false),
     Description(
         "Press keys together in Play Mode via Unity Input System, hold them for holdDuration seconds, then release. " +
         "Returns after the release is processed. Triggers InputAction/PlayerInput and Keyboard.current, " +
         "but not legacy UnityEngine.Input or text fields (use type_text to type text). " +
         "Requires com.unity.inputsystem."),
     UsedImplicitly]
    public ValueTask<CallToolResult> PressKeyAsync(
        [Description(KeysDescription)] string[] keys,
        [Description("Seconds to hold the keys, e.g. for a movement key (default 0: release in the next frame).")]
        float? holdDuration = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.PressKeyAsync(keys, holdDuration, ct), cancellationToken);

    [McpServerTool(Name = "type_text", ReadOnly = false),
     Description(
         "Type text into the focused text field in Play Mode (uGUI InputField, TMP_InputField or UI Toolkit TextField); " +
         "focus it first, e.g. with click_mouse. Any characters, including Japanese, are typed in one frame; " +
         "returns after they are processed. Also raises Keyboard.onTextInput. Does not change key states " +
         "(use press_key) and does not support Backspace or Enter. Requires com.unity.inputsystem."),
     UsedImplicitly]
    public ValueTask<CallToolResult> TypeTextAsync(
        [Description("Text to type, e.g. \"Hello\" or \"こんにちは\".")] string text,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.TypeTextAsync(text, ct), cancellationToken);

    private const string MouseToolDescription =
        "Simulates the Input System Mouse in Play Mode; uGUI receives it as a pointer, like a touch-screen tap. " +
        "Triggers InputAction/PlayerInput and Mouse.current, not legacy UnityEngine.Input. " +
        "Give each position as screen coordinates or as an instanceId (the center of a UI element), not both. " +
        "With an instanceId the event still goes through the EventSystem raycast, " +
        "so an element covered by other UI does not receive it. " +
        "Requires com.unity.inputsystem (and com.unity.ugui for an instanceId).";

    private const string XDescription =
        "Screen X in pixels, origin at the bottom-left of the Game View. " +
        "A capture_game_view image pixel (px, py) is x = px, y = imageHeight - py. Use with y, not instanceId.";

    private const string YDescription =
        "Screen Y in pixels, origin at the bottom-left (increases upward). Use with x, not instanceId.";

    private const string InstanceIdDescription =
        "instanceId of a UI GameObject under a Canvas, e.g. from get_ui_pointer_targets; its center is used. " +
        "Not with x/y.";

    private const string ButtonDescription =
        $"Mouse button: \"{MouseButton.Left}\", \"{MouseButton.Right}\" or \"{MouseButton.Middle}\".";

    [McpServerTool(Name = "click_mouse", ReadOnly = false),
     Description("Click (or tap): press, hold for holdDuration seconds, and release. " +
                 "Returns after the release is processed. " +
                 MouseToolDescription),
     UsedImplicitly]
    public ValueTask<CallToolResult> ClickMouseAsync(
        [Description(XDescription)] float? x = null,
        [Description(YDescription)] float? y = null,
        [Description(InstanceIdDescription)] int? instanceId = null,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        [Description("Seconds to hold the button, e.g. for a long press (default 0: release in the next frame).")]
        float? holdDuration = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.ClickMouseAsync(x, y, instanceId, button, holdDuration, ct), cancellationToken);

    [McpServerTool(Name = "drag_mouse", ReadOnly = false),
     Description(
         "Drag (or swipe) in one call: press at the start, move in a straight line to the end over duration seconds " +
         "(at most one move per frame), and release. Returns after the release is processed. " +
         "ScrollRect inertia, swipe detection and the drag threshold behave as with a real drag. " +
         MouseToolDescription),
     UsedImplicitly]
    public ValueTask<CallToolResult> DragMouseAsync(
        [Description("Start X in screen pixels, origin at the bottom-left of the Game View. " +
                     "A capture_game_view image pixel (px, py) is x = px, y = imageHeight - py. " +
                     "Use with fromY, not fromInstanceId.")]
        float? fromX = null,
        [Description("Start Y in screen pixels (increases upward). Use with fromX, not fromInstanceId.")]
        float? fromY = null,
        [Description("instanceId of a UI GameObject whose center is the start, e.g. from get_ui_pointer_targets. " +
                     "Not with fromX/fromY.")]
        int? fromInstanceId = null,
        [Description("End X, same space as fromX. Use with toY, not toInstanceId.")]
        float? toX = null,
        [Description("End Y, same space as fromY. Use with toX, not toInstanceId.")]
        float? toY = null,
        [Description("instanceId of a UI GameObject whose center is the end. Not with toX/toY.")]
        int? toInstanceId = null,
        [Description(ButtonDescription)] string button = MouseButton.Left,
        [Description("Seconds to move from the start to the end (default 0.2; 0 moves in one frame).")]
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
         "List the uGUI objects that can be pressed now in Play Mode: active, interactable, with an enabled " +
         "pointer-event handler (e.g. Button, Toggle, Slider, ScrollRect, EventTrigger), and the topmost raycast hit " +
         "at their center (not covered or off-screen). Each item has the Hierarchy path, instanceId and rect in " +
         "screen coordinates (same as x/y of the mouse tools); pass the instanceId to click_mouse. " +
         "Requires com.unity.ugui and an EventSystem."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetUIPointerTargetsAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            inputUseCase.GetUIPointerTargetsAsync, cancellationToken);
}
