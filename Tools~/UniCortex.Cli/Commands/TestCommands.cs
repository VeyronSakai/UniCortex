using ConsoleAppFramework;
using UniCortex.Core.Services;

namespace UniCortex.Cli.Commands;

public class TestCommands(TestService testService)
{
    /// <summary>Run Unity Test Runner tests and wait for completion.</summary>
    [Command("run")]
    public async Task Run(string? testMode = null, string[]? testNames = null,
        string[]? groupNames = null, string[]? categoryNames = null,
        string[]? assemblyNames = null, CancellationToken cancellationToken = default)
    {
        var json = await testService.RunAsync(testMode, testNames, groupNames, categoryNames,
            assemblyNames, cancellationToken);
        Console.WriteLine(json);
    }
}
