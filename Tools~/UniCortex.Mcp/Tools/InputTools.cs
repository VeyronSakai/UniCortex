using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class InputTools(InputUseCase inputService)
{
    [McpServerTool(Name = "send_key_event", ReadOnly = false),
     Description(
         "Send a keyboard event via Unity Input System (com.unity.inputsystem) in Play Mode. " +
         "Uses InputState.Change() to simulate device-level input. " +
         "Triggers Input System actions (InputAction, PlayerInput) and Keyboard.current key states. " +
         "Requires the Input System package to be installed. " +
         "Does NOT work with legacy UnityEngine.Input.GetKey()."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> SendKeyEventAsync(
        [Description(
            "Input System Key enum name. Available keys: " +
            KeyName.A + ", " + KeyName.B + ", " + KeyName.C + ", " + KeyName.D + ", " +
            KeyName.E + ", " + KeyName.F + ", " + KeyName.G + ", " + KeyName.H + ", " +
            KeyName.I + ", " + KeyName.J + ", " + KeyName.K + ", " + KeyName.L + ", " +
            KeyName.M + ", " + KeyName.N + ", " + KeyName.O + ", " + KeyName.P + ", " +
            KeyName.Q + ", " + KeyName.R + ", " + KeyName.S + ", " + KeyName.T + ", " +
            KeyName.U + ", " + KeyName.V + ", " + KeyName.W + ", " + KeyName.X + ", " +
            KeyName.Y + ", " + KeyName.Z + ", " +
            KeyName.Digit0 + "-" + KeyName.Digit9 + ", " +
            KeyName.F1 + "-" + KeyName.F12 + ", " +
            KeyName.Space + ", " + KeyName.Return + ", " + KeyName.Escape + ", " +
            KeyName.Tab + ", " + KeyName.Backspace + ", " + KeyName.Delete + ", " +
            KeyName.LeftArrow + ", " + KeyName.RightArrow + ", " + KeyName.UpArrow + ", " + KeyName.DownArrow + ", " +
            KeyName.LeftShift + ", " + KeyName.RightShift + ", " +
            KeyName.LeftCtrl + ", " + KeyName.RightCtrl + ", " +
            KeyName.LeftAlt + ", " + KeyName.RightAlt +
            ". Any valid Input System Key enum name is also accepted.")]
        string key,
        [Description($"Event type: \"{InputEventType.Press}\" (default) or \"{InputEventType.Release}\".")]
        string eventType = InputEventType.Press,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await inputService.SendKeyEventAsync(key, eventType, cancellationToken);
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
         "Uses InputState.Change() to simulate device-level input. " +
         "Triggers Input System actions (InputAction, PlayerInput) and Mouse.current states. " +
         "Requires the Input System package to be installed. " +
         "Does NOT work with legacy UnityEngine.Input.GetMouseButton()."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> SendMouseEventAsync(
        [Description("X coordinate in screen space.")]
        float x,
        [Description("Y coordinate in screen space.")]
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
                await inputService.SendMouseEventAsync(x, y, button, eventType, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
