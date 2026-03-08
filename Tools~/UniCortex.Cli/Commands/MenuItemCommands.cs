using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class MenuItemCommands(MenuItemUseCase menuItemService)
{
    /// <summary>Execute a Unity Editor menu item by its path.</summary>
    [Command("execute")]
    public async Task Execute(string menuPath, CancellationToken cancellationToken = default)
    {
        var message = await menuItemService.ExecuteAsync(menuPath, cancellationToken);
        Console.WriteLine(message);
    }
}
