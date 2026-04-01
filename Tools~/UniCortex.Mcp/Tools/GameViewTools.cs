using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class GameViewTools(GameViewUseCase gameViewUseCase)
{
    [McpServerTool(Name = "focus_game_view", ReadOnly = false),
     Description(
         "Switch focus to the Game View window in the Unity Editor. " +
         "Useful before capture_screenshot to ensure the Game View is captured."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> FocusGameViewAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await gameViewUseCase.FocusAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "get_game_view_size", ReadOnly = true),
     Description("Get the current Game View size (width and height in pixels) in the Unity Editor."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> GetGameViewSizeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await gameViewUseCase.GetSizeAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "get_game_view_size_list", ReadOnly = true),
     Description(
         "Get the list of available Game View sizes (built-in and custom) in the Unity Editor. " +
         "Returns each size with its index, name, width, height, and type. " +
         "Use the index with set_game_view_size to select a resolution."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> GetGameViewSizeListAsync(CancellationToken cancellationToken)
    {
        try
        {
            var message = await gameViewUseCase.GetSizeListAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "set_game_view_size", ReadOnly = false),
     Description(
         "Set the Game View resolution in the Unity Editor. " +
         "Preferred: specify 'index' from get_game_view_size_list to select an existing size. " +
         "Alternative: specify 'width' and 'height' to set a custom resolution (creates a new entry if no match found)."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> SetGameViewSizeAsync(
        [Description("Index of the size from get_game_view_size_list. Takes priority over width/height.")]
        int? index = null,
        [Description("Width in pixels (used when index is not specified)")]
        int? width = null,
        [Description("Height in pixels (used when index is not specified)")]
        int? height = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string message;
            if (index.HasValue)
            {
                message = await gameViewUseCase.SetSizeByIndexAsync(index.Value, cancellationToken);
            }
            else if (width.HasValue && height.HasValue)
            {
                message = await gameViewUseCase.SetSizeAsync(width.Value, height.Value, cancellationToken);
            }
            else
            {
                return new CallToolResult
                {
                    IsError = true,
                    Content = [new TextContentBlock { Text = "Either 'index' or both 'width' and 'height' must be specified." }]
                };
            }

            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
