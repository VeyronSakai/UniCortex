using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ConsoleCommands(ConsoleUseCase consoleService)
{
    /// <summary>Get console log entries from the Unity Editor.</summary>
    [Command("logs")]
    public async Task Logs(int? count = null, bool? stackTrace = null, bool? log = null,
        bool? warning = null, bool? error = null, CancellationToken cancellationToken = default)
    {
        var json = await consoleService.GetLogsAsync(count, stackTrace, log, warning, error,
            cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Clear all console logs in the Unity Editor.</summary>
    [Command("clear")]
    public async Task Clear(CancellationToken cancellationToken = default)
    {
        var message = await consoleService.ClearAsync(cancellationToken);
        Console.WriteLine(message);
    }
}
