using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class MenuItemCommands(MenuItemUseCase menuItemUseCase)
{
    /// <summary>Execute a Unity Editor menu item by its path.</summary>
    /// <param name="menuPath">Menu item path (e.g. "GameObject/3D Object/Cube").</param>
    [Command("execute")]
    public async Task Execute([Argument] string menuPath, CancellationToken cancellationToken = default)
    {
        var message = await menuItemUseCase.ExecuteAsync(menuPath, cancellationToken);
        Console.WriteLine(message);
    }
}
