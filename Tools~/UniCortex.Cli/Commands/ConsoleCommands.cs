using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ConsoleCommands(ConsoleUseCase consoleUseCase)
{
    /// <summary>Get console log entries from the Unity Editor. All levels are included unless --info, --warning, or --error is given.</summary>
    /// <param name="count">Number of log entries to retrieve.</param>
    /// <param name="stackTrace">Include stack traces in the output.</param>
    /// <param name="info">Include info-level entries (Debug.Log).</param>
    /// <param name="warning">Include warning-level entries.</param>
    /// <param name="error">Include error-level entries.</param>
    [Command("logs")]
    public async Task Logs(int? count = null, bool stackTrace = false, bool info = false,
        bool warning = false, bool error = false, CancellationToken cancellationToken = default)
    {
        // No level flag fetches every level; otherwise only the given levels are fetched.
        var anyLevel = info || warning || error;
        var json = await consoleUseCase.GetLogsAsync(count, stackTrace ? true : null,
            anyLevel ? info : null, anyLevel ? warning : null, anyLevel ? error : null, cancellationToken);
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
