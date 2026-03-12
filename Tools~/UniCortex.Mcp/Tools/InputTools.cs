using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class InputTools(InputUseCase inputService)
{
    [McpServerTool(Name = "send_key_event", ReadOnly = false),
     Description(
         "Send a keyboard event to the Game View in Play Mode. " +
         "Uses EditorWindow.SendEvent() which works with OnGUI and UGUI EventSystem, " +
         "but does NOT trigger UnityEngine.Input.GetKey() or Input.GetMouseButton() (OS-level input)."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> SendKeyEventAsync(
        [Description("Key name for Event.KeyboardEvent() (e.g. \"space\", \"return\", \"a\", \"up\"). " +
                     "Modifiers: \"#a\" (Shift+A), \"^a\" (Ctrl+A), \"%a\" (Alt+A).")]
        string keyName,
        [Description("Event type: \"keyDown\" (default) or \"keyUp\".")]
        string eventType = "keyDown",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await inputService.SendKeyEventAsync(keyName, eventType, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "send_mouse_event", ReadOnly = false),
     Description(
         "Send a mouse event to the Game View in Play Mode. " +
         "Uses EditorWindow.SendEvent() which works with OnGUI and UGUI EventSystem, " +
         "but does NOT trigger UnityEngine.Input.GetMouseButton() (OS-level input)."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> SendMouseEventAsync(
        [Description("X coordinate in Game View local space.")]
        float x,
        [Description("Y coordinate in Game View local space.")]
        float y,
        [Description("Mouse button: 0=left (default), 1=right, 2=middle.")]
        int button = 0,
        [Description("Event type: \"mouseDown\" (default) or \"mouseUp\".")]
        string eventType = "mouseDown",
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
