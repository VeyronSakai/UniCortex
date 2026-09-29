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
            // Never resend: once the server has received the request, a resent one would arrive after the
            // domain reload for Play Mode and be rejected with 400 because the Editor is now in Play Mode.
            response = await client.PostAsync<RunTestsRequest, RunTestsResponse>(ApiRoutes.TestsRun, request,
                cancellationToken, resendAfterDisconnect: false);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is null or HttpStatusCode.RequestTimeout)
        {
            // Server disrupted (e.g., domain reload during PlayMode entry). The server answers 408 when it is
            // stopped while the request is in flight, or the connection is dropped without a status code.
            // The test run keeps going across the reload and stores its result, so fall through to polling.
            // Other status codes (e.g., 400 in Play Mode) are real errors and are rethrown.
        }
        catch (JsonException)
        {
            // Empty response body (e.g., PlayMode test triggers domain reload before response is sent)
        }

        // Domain reload can disrupt the POST /tests/run response path.
        // Poll GET /tests/result until the stored result becomes available.
        response ??= await client.GetAsync<GetTestResultRequest, RunTestsResponse>(ApiRoutes.TestsResult,
            cancellationToken: cancellationToken);

        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
