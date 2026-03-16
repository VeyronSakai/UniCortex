using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class TestCommands(TestUseCase testService)
{
    /// <summary>Run Unity Test Runner tests and wait for completion.</summary>
    /// <param name="testMode">Test mode: "EditMode" or "PlayMode".</param>
    /// <param name="testNames">Specific test names to run.</param>
    /// <param name="groupNames">Test group names to run.</param>
    /// <param name="categoryNames">Test category names to run.</param>
    /// <param name="assemblyNames">Test assembly names to run.</param>
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
