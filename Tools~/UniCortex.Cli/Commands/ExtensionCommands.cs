using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ExtensionCommands(ExtensionUseCase extensionUseCase)
{
    /// <summary>List all extensions registered in the Unity Editor.</summary>
    [Command("list")]
    public async Task List(CancellationToken cancellationToken)
    {
        var response = await extensionUseCase.ListAsync(cancellationToken);
        if (response.extensions == null || response.extensions.Count == 0)
        {
            Console.WriteLine("No extensions registered.");
            return;
        }

        foreach (var ext in response.extensions)
        {
            Console.WriteLine($"{ext.name} - {ext.description}");
        }
    }

    /// <summary>Execute an extension by name.</summary>
    [Command("execute")]
    public async Task Execute([Argument] string name, string? arguments = null,
        CancellationToken cancellationToken = default)
    {
        var result = await extensionUseCase.ExecuteAsync(name, arguments, cancellationToken);
        Console.WriteLine(result);
    }
}
