using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ConsoleCommands(ConsoleUseCase consoleUseCase)
{
    /// <summary>Get console log entries from the Unity Editor.</summary>
    /// <param name="count">Number of log entries to retrieve.</param>
    /// <param name="stackTrace">Include stack traces in the output.</param>
    /// <param name="noLog">Exclude log-level entries.</param>
    /// <param name="noWarning">Exclude warning-level entries.</param>
    /// <param name="noError">Exclude error-level entries.</param>
    [Command("logs")]
    public async Task Logs(int? count = null, bool stackTrace = false, bool noLog = false,
        bool noWarning = false, bool noError = false, CancellationToken cancellationToken = default)
    {
        // Unset flags are sent as null so the Unity side applies its defaults.
        var json = await consoleUseCase.GetLogsAsync(count, stackTrace ? true : null, noLog ? false : null,
            noWarning ? false : null, noError ? false : null, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Clear all console logs in the Unity Editor.</summary>
    [Command("clear")]
    public async Task Clear(CancellationToken cancellationToken = default)
    {
        var message = await consoleUseCase.ClearAsync(cancellationToken);
        Console.WriteLine(message);
    }
}
