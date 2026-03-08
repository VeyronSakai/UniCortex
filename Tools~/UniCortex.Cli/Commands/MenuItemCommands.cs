using ConsoleAppFramework;
using UniCortex.Core.Services;

namespace UniCortex.Cli.Commands;

public class MenuItemCommands(MenuItemService menuItemService)
{
    /// <summary>Execute a Unity Editor menu item by its path.</summary>
    [Command("execute")]
    public async Task Execute(string menuPath, CancellationToken cancellationToken = default)
    {
        var message = await menuItemService.ExecuteAsync(menuPath, cancellationToken);
        Console.WriteLine(message);
    }
}
