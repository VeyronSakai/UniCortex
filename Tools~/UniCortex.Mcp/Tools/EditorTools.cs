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
     Description("Get the Unity Editor state (play mode, paused). Works while paused."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetEditorStatusAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.GetEditorStatusAsync, cancellationToken);

    [McpServerTool(Name = "pause_editor", ReadOnly = false),
     Description("Pause the Unity Editor. Advance frames with step_editor."), UsedImplicitly]
    public ValueTask<CallToolResult> PauseEditorAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.PauseAsync, cancellationToken);

    [McpServerTool(Name = "unpause_editor", ReadOnly = false),
     Description("Unpause the Unity Editor."), UsedImplicitly]
    public ValueTask<CallToolResult> UnpauseEditorAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.UnpauseAsync, cancellationToken);

    [McpServerTool(Name = "step_editor", ReadOnly = false),
     Description("Advance the Unity Editor by one frame while paused (see pause_editor)."),
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
     Description("Run File/Save to save the active stage (Scene, Prefab, Timeline, etc.)."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SaveAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.SaveAsync, cancellationToken);

    [McpServerTool(Name = "reload_domain", ReadOnly = false),
     Description("Request a domain reload (script recompilation)."), UsedImplicitly]
    public ValueTask<CallToolResult> ReloadDomainAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.ReloadDomainAsync, cancellationToken);

    [McpServerTool(Name = "get_active_platform", ReadOnly = true),
     Description("Get the active build target platform (e.g. StandaloneOSX, Android, iOS)."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetActivePlatformAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, editorUseCase.GetActivePlatformAsync, cancellationToken);

    [McpServerTool(Name = "switch_platform", ReadOnly = false),
     Description(
         "Switch the active build target platform. Assets are reimported and scripts recompiled, which can " +
         "take minutes. Not available in Play Mode; the platform module must be installed."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SwitchPlatformAsync(
        [Description(
            "UnityEditor.BuildTarget name (case-insensitive), " +
            "e.g. StandaloneOSX, StandaloneWindows64, Android, iOS, WebGL.")]
        string buildTarget,
        CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => editorUseCase.SwitchPlatformAsync(buildTarget, ct), cancellationToken);
}
