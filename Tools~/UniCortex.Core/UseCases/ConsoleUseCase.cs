using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ConsoleUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> GetLogsAsync(int? count = null, bool? stackTrace = null,
        bool? log = null, bool? warning = null, bool? error = null,
        CancellationToken cancellationToken = default)
    {
        var request = new GetConsoleLogsRequest
        {
            count = count,
            stackTrace = stackTrace,
            log = log,
            warning = warning,
            error = error
        };
        var response = await client.GetAsync<GetConsoleLogsRequest, GetConsoleLogsResponse>(
            ApiRoutes.ConsoleLogs, request, cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> ClearAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<ClearConsoleRequest, ClearConsoleResponse>(ApiRoutes.ConsoleClear,
            cancellationToken: cancellationToken);
        return "Console logs cleared successfully.";
    }
}
