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

        try
        {
            // POST /tests/run only starts the run. Never resend it: once the server has received the request,
            // a resent one would start the run twice, or be rejected with 400 if the Editor is in Play Mode by then.
            await client.PostAsync<RunTestsRequest, RunTestsAcceptedResponse>(ApiRoutes.TestsRun, request,
                cancellationToken, resendAfterDisconnect: false);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is null or HttpStatusCode.ServiceUnavailable)
        {
            // The server was stopped (e.g. by the domain reload for entering Play Mode) before it could answer.
            // The server answers 503 in that case, or the connection is dropped without a status code.
            // The run has started and stores its result, so fall through to polling.
            // Other status codes (e.g. 400 in Play Mode) are real errors and are rethrown.
        }

        // GET /tests/result answers with an empty body while the run is in progress; the client retries it
        // (also across domain reloads) until the result is stored.
        var response = await client.GetAsync<GetTestResultRequest, RunTestsResponse>(ApiRoutes.TestsResult,
            cancellationToken: cancellationToken);

        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
