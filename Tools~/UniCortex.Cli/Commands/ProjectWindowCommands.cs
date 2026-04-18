using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ProjectWindowCommands(ProjectWindowUseCase projectWindowUseCase)
{
    /// <summary>Select an asset in the Unity Project Window, focus the window, and ping it.</summary>
    [Command("select")]
    public async Task Select([Argument] string assetPath, CancellationToken cancellationToken = default)
    {
        var message = await projectWindowUseCase.SelectAsync(assetPath, cancellationToken);
        Console.WriteLine(message);
    }
}
