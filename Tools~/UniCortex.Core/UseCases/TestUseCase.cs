using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class TestUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> RunAsync(string? testMode = null, string[]? testNames = null,
        string[]? groupNames = null, string[]? categoryNames = null, string[]? assemblyNames = null,
        CancellationToken cancellationToken = default)
    {
        await client.WaitForServerAsync(cancellationToken);

        var request = new RunTestsRequest(
            testMode ?? TestModes.EditMode,
            testNames != null ? new List<string>(testNames) : null,
            groupNames != null ? new List<string>(groupNames) : null,
            categoryNames != null ? new List<string>(categoryNames) : null,
            assemblyNames != null ? new List<string>(assemblyNames) : null);

        RunTestsResponse? response = null;
        try
        {
            response = await client.PostAsync<RunTestsRequest, RunTestsResponse>(ApiRoutes.TestsRun, request,
                cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.Message != ErrorMessages.RequestWasCancelled)
        {
            // Server disrupted (e.g., domain reload during PlayMode entry)
        }
        catch (JsonException)
        {
            // Empty response body (e.g., PlayMode test triggers domain reload before response is sent)
        }

        // PlayMode tests trigger a domain reload when entering play mode,
        // which disrupts the HTTP server. Poll GET /tests/result for results.
        // The HttpRequestHandler retries GET on Content-Length: 0, so this
        // automatically polls until results are stored in SessionState.
        response ??= await client.GetAsync<GetTestResultRequest, RunTestsResponse>(ApiRoutes.TestsResult,
            cancellationToken: cancellationToken);

        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
