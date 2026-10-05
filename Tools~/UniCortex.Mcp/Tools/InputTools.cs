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

    [McpServerTool(Name = "send_mouse_event", ReadOnly = false),
     Description(
         "Send a mouse event via Unity Input System (com.unity.inputsystem) in Play Mode. " +
         "Uses InputSystem.QueueEvent() to simulate device-level input. " +
         "Triggers Input System actions (InputAction, PlayerInput) and Mouse.current states. " +
         "Specify the position either with x and y, or with instanceId to send the event to the center of a UI element; " +
         "exactly one of them is required. " +
         "With a target, the event still goes through the EventSystem raycast like a real tap, " +
         "so a target covered by other UI does not receive it. " +
         "Requires the Input System package to be installed (and com.unity.ugui for a target). " +
         "Does NOT work with legacy UnityEngine.Input.GetMouseButton()."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SendMouseEventAsync(
        [Description("X coordinate in screen pixels (Screen.width space). Origin (0,0) is at the bottom-left of the Game View. Increases to the right. Note: capture_game_view images are at the Game View resolution with a top-left origin, so a pixel (px, py) in the image corresponds to x = px, y = imageHeight - py. Must be given together with y, and not with instanceId.")]
        float? x = null,
        [Description("Y coordinate in screen pixels (Screen.height space). Origin (0,0) is at the bottom-left of the Game View. Increases upward. This is the inverse of typical image coordinates where Y increases downward. Must be given together with x, and not with instanceId.")]
        float? y = null,
        [Description("instanceId of the UI GameObject (a RectTransform under a Canvas) to send the event to, e.g. from get_pointer_targets. The event is sent to its center. Cannot be combined with x/y.")]
        int? instanceId = null,
        [Description($"Mouse button: \"{MouseButton.Left}\" (default), \"{MouseButton.Right}\", or \"{MouseButton.Middle}\".")]
        string button = MouseButton.Left,
        [Description($"Event type: \"{InputEventType.Click}\" (default, press then release after one frame), \"{InputEventType.Press}\", \"{InputEventType.Release}\", or \"{InputEventType.Move}\" (position only, no button).")]
        string eventType = InputEventType.Click,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => inputUseCase.SendMouseEventAsync(x, y, instanceId, button, eventType, ct),
            cancellationToken);

    [McpServerTool(Name = "get_pointer_targets", ReadOnly = true),
     Description(
         "List the uGUI objects in the Game View that can be pressed now, in Play Mode. " +
         "An object is listed when it is active, is under a Canvas, has an enabled component handling pointer events " +
         "(e.g. Button, Toggle, Slider, ScrollRect, EventTrigger, custom drag or long-press components), " +
         "is interactable (Selectable.IsInteractable()), and is the topmost EventSystem raycast hit at its center " +
         "(not covered by other UI and not off-screen). " +
         "Each item has the Hierarchy path, instanceId, and rect in Game View coordinates (same as send_mouse_event x/y). " +
         "Pass instanceId to send_mouse_event to press one. " +
         "Requires the uGUI package (com.unity.ugui) and an EventSystem in the scene."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetPointerTargetsAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            inputUseCase.GetPointerTargetsAsync, cancellationToken);
}
