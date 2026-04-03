using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class EditorTools(EditorUseCase editorUseCase)
{
    [McpServerTool(Name = "ping_editor", ReadOnly = true), Description("Check connectivity with the Unity Editor."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> PingEditorAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await editorUseCase.PingAsync(cancellationToken);
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
            var message = await editorUseCase.EnterPlayModeAsync(cancellationToken);
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
            var message = await editorUseCase.ExitPlayModeAsync(cancellationToken);
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
            var message = await editorUseCase.GetEditorStatusAsync(cancellationToken);
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
            var message = await editorUseCase.PauseAsync(cancellationToken);
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
            var message = await editorUseCase.UnpauseAsync(cancellationToken);
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
            var message = await editorUseCase.StepAsync(cancellationToken);
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
            var message = await editorUseCase.UndoAsync(cancellationToken);
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
            var message = await editorUseCase.RedoAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "set_time_scale", ReadOnly = false),
     Description("Set the Time.timeScale value in Unity. Useful for slow motion (< 1), fast forward (> 1), or pausing time (0)."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> SetTimeScaleAsync(
        [Description("The time scale value to set. 1 = normal speed, 0.5 = half speed, 2 = double speed, 0 = paused.")]
        float timeScale,
        CancellationToken cancellationToken)
    {
        try
        {
            var message = await editorUseCase.SetTimeScaleAsync(timeScale, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "get_time_scale", ReadOnly = true),
     Description("Get the current Time.timeScale value in Unity."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> GetTimeScaleAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await editorUseCase.GetTimeScaleAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "save", ReadOnly = false),
     Description("Execute File/Save to save the currently active stage in the Unity Editor. Supports any asset that File/Save can handle, such as Scenes, Prefabs, Timelines, etc."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await editorUseCase.SaveAsync(cancellationToken);
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
            var message = await editorUseCase.ReloadDomainAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
