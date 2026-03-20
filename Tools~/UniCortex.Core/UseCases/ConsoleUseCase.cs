using UniCortex.Core.Infrastructures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ConsoleUseCase(UnityEditorClient client)
{
    public async ValueTask<string> GetLogsAsync(int? count = null, bool? stackTrace = null,
        bool? log = null, bool? warning = null, bool? error = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (count.HasValue) queryParams.Add($"count={count.Value}");
        if (stackTrace.HasValue) queryParams.Add($"stackTrace={stackTrace.Value.ToString().ToLowerInvariant()}");
        if (log.HasValue) queryParams.Add($"log={log.Value.ToString().ToLowerInvariant()}");
        if (warning.HasValue) queryParams.Add($"warning={warning.Value.ToString().ToLowerInvariant()}");
        if (error.HasValue) queryParams.Add($"error={error.Value.ToString().ToLowerInvariant()}");

        var route = ApiRoutes.ConsoleLogs;
        if (queryParams.Count > 0)
        {
            route = $"{route}?{string.Join("&", queryParams)}";
        }

        return await client.GetStringAsync(route, cancellationToken);
    }

    public ValueTask<string> ClearAsync(CancellationToken cancellationToken)
    {
        return client.PostEmptyAsync(ApiRoutes.ConsoleClear, "Console logs cleared successfully.",
            cancellationToken);
    }
}
