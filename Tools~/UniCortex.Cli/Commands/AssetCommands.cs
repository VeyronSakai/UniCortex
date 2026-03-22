using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class AssetCommands(AssetUseCase assetUseCase)
{
    /// <summary>Refresh the Unity Asset Database.</summary>
    [Command("refresh")]
    public async Task Refresh(CancellationToken cancellationToken = default)
    {
        var message = await assetUseCase.RefreshAsync(cancellationToken);
        Console.WriteLine(message);
    }
}
