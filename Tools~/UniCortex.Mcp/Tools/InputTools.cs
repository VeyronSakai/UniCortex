using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class InputTools(InputUseCase inputUseCase)
{
    [McpServerTool(Name = "send_key_event", ReadOnly = false),
     Description(
         "Send a keyboard event via Unity Input System (com.unity.inputsystem) in Play Mode. " +
         "Uses InputSystem.QueueEvent() to simulate device-level input. " +
         "Triggers Input System actions (InputAction, PlayerInput) and Keyboard.current key states. " +
         "Requires the Input System package to be installed. " +
         "Does NOT work with legacy UnityEngine.Input.GetKey()."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> SendKeyEventAsync(
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
    {
        try
        {
            var message = await inputUseCase.SendKeyEventAsync(key, eventType, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "send_mouse_event", ReadOnly = false),
     Description(
         "Send a mouse event via Unity Input System (com.unity.inputsystem) in Play Mode. " +
         "Uses InputSystem.QueueEvent() to simulate device-level input. " +
         "Triggers Input System actions (InputAction, PlayerInput) and Mouse.current states. " +
         "Requires the Input System package to be installed. " +
         "Does NOT work with legacy UnityEngine.Input.GetMouseButton()."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> SendMouseEventAsync(
        [Description("X coordinate in screen pixels. Origin (0,0) is at the bottom-left of the Game View. Increases to the right. Range depends on Game View resolution (e.g. 0-800 for 800px width).")]
        float x,
        [Description("Y coordinate in screen pixels. Origin (0,0) is at the bottom-left of the Game View. Increases upward. Range depends on Game View resolution (e.g. 0-600 for 600px height).")]
        float y,
        [Description($"Mouse button: \"{MouseButton.Left}\" (default), \"{MouseButton.Right}\", or \"{MouseButton.Middle}\".")]
        string button = MouseButton.Left,
        [Description($"Event type: \"{InputEventType.Press}\" (default), \"{InputEventType.Release}\", or \"{InputEventType.Move}\" (position only, no button).")]
        string eventType = InputEventType.Press,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message =
                await inputUseCase.SendMouseEventAsync(x, y, button, eventType, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
