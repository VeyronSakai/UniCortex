using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class RecordingUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> StartAsync(int fps, string? outputPath,
        CancellationToken cancellationToken = default)
    {
        var request = new StartRecordingRequest
        {
            fps = fps,
            outputPath = outputPath ?? ""
        };
        await client.PostAsync<StartRecordingRequest, StartRecordingResponse>(
            ApiRoutes.GameViewRecordStart, request, cancellationToken);
        return "Recording started.";
    }

    public async ValueTask<string> StopAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.PostAsync<StartRecordingRequest, StopRecordingResponse>(
            ApiRoutes.GameViewRecordStop, cancellationToken: cancellationToken);
        return response.outputPath;
    }
}
