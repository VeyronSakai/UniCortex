using System.Net;
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
        catch (HttpRequestException ex) when (ex.StatusCode != HttpStatusCode.RequestTimeout)
        {
            // Server disrupted (e.g., domain reload during PlayMode entry)
        }
        catch (JsonException)
        {
            // Empty response body (e.g., PlayMode test triggers domain reload before response is sent)
        }

        // Domain reload can disrupt the POST /tests/run response path.
        // For transport-level failures other than explicit cancellation (408),
        // poll GET /tests/result until the stored result becomes available.
        response ??= await client.GetAsync<GetTestResultRequest, RunTestsResponse>(ApiRoutes.TestsResult,
            cancellationToken: cancellationToken);

        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
