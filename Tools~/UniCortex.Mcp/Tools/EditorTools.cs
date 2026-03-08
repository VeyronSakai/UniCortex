using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Services;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class EditorTools(EditorService editorService)
{
    [McpServerTool(Name = "ping_editor", ReadOnly = true), Description("Check connectivity with the Unity Editor."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> PingEditorAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await editorService.PingAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "enter_play_mode", ReadOnly = false), Description("Start Play Mode in the Unity Editor."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> EnterPlayModeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await editorService.EnterPlayModeAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "exit_play_mode", ReadOnly = false), Description("Stop Play Mode in the Unity Editor."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> ExitPlayModeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await editorService.ExitPlayModeAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "undo", ReadOnly = false), Description("Perform Undo in the Unity Editor."), UsedImplicitly]
    public async ValueTask<CallToolResult> UndoAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await editorService.UndoAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "redo", ReadOnly = false), Description("Perform Redo in the Unity Editor."), UsedImplicitly]
    public async ValueTask<CallToolResult> RedoAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await editorService.RedoAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "reload_domain", ReadOnly = false),
     Description("Request a domain reload (script recompilation) in the Unity Editor."), UsedImplicitly]
    public async ValueTask<CallToolResult> ReloadDomainAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await editorService.ReloadDomainAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
