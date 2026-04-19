using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class GameViewTools(GameViewUseCase gameViewUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "focus_game_view", ReadOnly = false),
     Description(
         "Switch focus to the Game View window in the Unity Editor. " +
         "Useful before capture_screenshot to ensure the Game View is captured."),
     UsedImplicitly]
    public ValueTask<CallToolResult> FocusGameViewAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.FocusAsync, cancellationToken);

    [McpServerTool(Name = "get_game_view_size", ReadOnly = true),
     Description("Get the current Game View size (width and height in pixels) in the Unity Editor."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetGameViewSizeAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.GetSizeAsync, cancellationToken);

    [McpServerTool(Name = "get_game_view_size_list", ReadOnly = true),
     Description(
         "Get the list of available Game View sizes (built-in and custom) in the Unity Editor. " +
         "Returns each size with its index, name, width, height, and type. " +
         "Use the index with set_game_view_size to select a resolution."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetGameViewSizeListAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer, gameViewUseCase.GetSizeListAsync, cancellationToken);

    [McpServerTool(Name = "set_game_view_size", ReadOnly = false),
     Description(
         "Set the Game View resolution in the Unity Editor by selecting a size from the available list. " +
         "Use get_game_view_size_list to get available sizes and their indices."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetGameViewSizeAsync(
        [Description("Index of the size from get_game_view_size_list.")]
        int index,
        CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameViewUseCase.SetSizeAsync(index, ct), cancellationToken);
}
