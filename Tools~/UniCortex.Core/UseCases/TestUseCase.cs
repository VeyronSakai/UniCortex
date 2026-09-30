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
            // POST /tests/run only starts the run. Do not resend it after a disconnect: the server may have started
            // the run, and a resent request would start it twice, or be rejected with 400 if the Editor is in
            // Play Mode by then. It is still resent when the server certainly did not run it (connection refused
            // or 503; see HttpRequestHandler).
            await client.PostAsync<RunTestsRequest, RunTestsAcceptedResponse>(ApiRoutes.TestsRun, request,
                cancellationToken, resendAfterDisconnect: false);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is null)
        {
            // The connection was dropped (or the response was empty) without a status code, so it is unknown
            // whether the run has started. It usually has, since the server writes its response before it stops;
            // fall through to polling. Status codes (e.g. 400 in Play Mode) are real errors and are rethrown.
        }

        // GET /tests/result answers with an empty body while the run is in progress; the client retries it
        // (also across domain reloads) until the result is stored.
        var response = await client.GetAsync<GetTestResultRequest, RunTestsResponse>(ApiRoutes.TestsResult,
            cancellationToken: cancellationToken);

        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
