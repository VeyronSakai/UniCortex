using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class EditorTools(EditorUseCase editorUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "ping_editor", ReadOnly = true), Description("Check connectivity with the Unity Editor."),
     UsedImplicitly]
    public ValueTask<CallToolResult> PingEditorAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.PingAsync, cancellationToken);

    [McpServerTool(Name = "enter_play_mode", ReadOnly = false), Description("Start Play Mode in the Unity Editor."),
     UsedImplicitly]
    public ValueTask<CallToolResult> EnterPlayModeAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.EnterPlayModeAsync, cancellationToken);

    [McpServerTool(Name = "exit_play_mode", ReadOnly = false), Description("Stop Play Mode in the Unity Editor."),
     UsedImplicitly]
    public ValueTask<CallToolResult> ExitPlayModeAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.ExitPlayModeAsync, cancellationToken);

    [McpServerTool(Name = "get_editor_status", ReadOnly = true),
     Description(
         "Get the current state of the Unity Editor (play mode, paused). Works even when the editor is paused."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetEditorStatusAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.GetEditorStatusAsync, cancellationToken);

    [McpServerTool(Name = "pause_editor", ReadOnly = false),
     Description("Pause the Unity Editor. Use with step_editor for frame-by-frame control."), UsedImplicitly]
    public ValueTask<CallToolResult> PauseEditorAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.PauseAsync, cancellationToken);

    [McpServerTool(Name = "unpause_editor", ReadOnly = false),
     Description("Unpause the Unity Editor. Works even when the editor is paused."), UsedImplicitly]
    public ValueTask<CallToolResult> UnpauseEditorAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.UnpauseAsync, cancellationToken);

    [McpServerTool(Name = "step_editor", ReadOnly = false),
     Description(
         "Advance the Unity Editor by one frame while paused. Use with pause_editor for frame-by-frame control of Play Mode."),
     UsedImplicitly]
    public ValueTask<CallToolResult> StepEditorAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.StepAsync, cancellationToken);

    [McpServerTool(Name = "undo", ReadOnly = false), Description("Perform Undo in the Unity Editor."), UsedImplicitly]
    public ValueTask<CallToolResult> UndoAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.UndoAsync, cancellationToken);

    [McpServerTool(Name = "redo", ReadOnly = false), Description("Perform Redo in the Unity Editor."), UsedImplicitly]
    public ValueTask<CallToolResult> RedoAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.RedoAsync, cancellationToken);

    [McpServerTool(Name = "save", ReadOnly = false),
     Description("Execute File/Save to save the currently active stage in the Unity Editor. Supports any asset that File/Save can handle, such as Scenes, Prefabs, Timelines, etc."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SaveAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.SaveAsync, cancellationToken);

    [McpServerTool(Name = "reload_domain", ReadOnly = false),
     Description("Request a domain reload (script recompilation) in the Unity Editor."), UsedImplicitly]
    public ValueTask<CallToolResult> ReloadDomainAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.ReloadDomainAsync, cancellationToken);
}
