using System.Text.Json;
using ConsoleAppFramework;
using UniCortex.Core.Domains;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

internal sealed class CustomCommands(CustomToolUseCase customToolUseCase)
{
    [Command("list")]
    public async Task List(CancellationToken cancellationToken)
    {
        var manifest = await customToolUseCase.GetManifestAsync(cancellationToken);
        var commands = manifest.tools
            .Where(static tool => tool.exposeToCli && !string.IsNullOrWhiteSpace(tool.cliCommand))
            .OrderBy(static tool => tool.cliCommand, StringComparer.Ordinal)
            .Select(static tool => new CustomCommandListEntry(tool.cliCommand, tool.name, tool.description))
            .ToArray();

        Console.WriteLine(JsonSerializer.Serialize(commands, JsonOptions.Default));
    }

    private sealed class CustomCommandListEntry
    {
        public string command;
        public string toolName;
        public string description;

        public CustomCommandListEntry(string command, string toolName, string description)
        {
            this.command = command;
            this.toolName = toolName;
            this.description = description;
        }
    }
}
