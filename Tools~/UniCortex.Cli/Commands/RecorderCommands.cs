using System.Text.Json;
using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class RecorderAllCommands(MovieRecordingUseCase movieRecordingUseCase)
{
    /// <summary>Get the list of all configured recorders and their settings. Requires com.unity.recorder.</summary>
    [Command("list")]
    public async Task List(CancellationToken cancellationToken = default)
    {
        var response = await movieRecordingUseCase.GetListAsync(cancellationToken);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        });
        Console.WriteLine(json);
    }
}
