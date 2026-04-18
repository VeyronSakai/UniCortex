using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ProjectViewCommands(ProjectViewUseCase projectViewUseCase)
{
    /// <summary>Select an asset in the Unity Project view, focus the window, and ping it.</summary>
    [Command("select")]
    public async Task Select([Argument] string assetPath, CancellationToken cancellationToken = default)
    {
        var message = await projectViewUseCase.SelectAsync(assetPath, cancellationToken);
        Console.WriteLine(message);
    }
}
