using ConsoleAppFramework;
using UniCortex.Core.Services;

namespace UniCortex.Cli.Commands;

public class AssetCommands(AssetService assetService)
{
    /// <summary>Refresh the Unity Asset Database.</summary>
    [Command("refresh")]
    public async Task Refresh(CancellationToken cancellationToken = default)
    {
        var message = await assetService.RefreshAsync(cancellationToken);
        Console.WriteLine(message);
    }
}
