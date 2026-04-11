using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class CustomToolCommands(CustomToolUseCase customToolUseCase)
{
    /// <summary>List all custom tools registered in the Unity Editor.</summary>
    [Command("list")]
    public async Task List(CancellationToken cancellationToken)
    {
        var response = await customToolUseCase.ListAsync(cancellationToken);
        if (response.tools == null || response.tools.Count == 0)
        {
            Console.WriteLine("No custom tools registered.");
            return;
        }

        foreach (var tool in response.tools)
        {
            Console.WriteLine($"{tool.name} - {tool.description}");
        }
    }

    /// <summary>Execute a custom tool by name.</summary>
    [Command("execute")]
    public async Task Execute(string name, string? arguments = null, CancellationToken cancellationToken = default)
    {
        var result = await customToolUseCase.ExecuteAsync(name, arguments, cancellationToken);
        Console.WriteLine(result);
    }
}
