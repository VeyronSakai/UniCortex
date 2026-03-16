using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class EditorTools(EditorUseCase editorService)
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

    [McpServerTool(Name = "get_editor_status", ReadOnly = true),
     Description(
         "Get the current state of the Unity Editor (play mode, paused). Works even when the editor is paused."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> GetEditorStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await editorService.GetEditorStatusAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "pause_editor", ReadOnly = false),
     Description("Pause the Unity Editor. Use with step_editor for frame-by-frame control."), UsedImplicitly]
    public async ValueTask<CallToolResult> PauseEditorAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await editorService.PauseAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "unpause_editor", ReadOnly = false),
     Description("Unpause the Unity Editor. Works even when the editor is paused."), UsedImplicitly]
    public async ValueTask<CallToolResult> UnpauseEditorAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await editorService.UnpauseAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "step_editor", ReadOnly = false),
     Description(
         "Advance the Unity Editor by one frame while paused. Use with pause_editor for frame-by-frame control of Play Mode."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> StepEditorAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await editorService.StepAsync(cancellationToken);
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
