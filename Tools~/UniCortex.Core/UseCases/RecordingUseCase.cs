using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class RecordingUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> ConfigureAsync(
        string? outputPath, string? source, string? cameraSource,
        string? cameraTag, bool captureUI,
        int outputWidth, int outputHeight, string? outputFormat,
        CancellationToken cancellationToken = default)
    {
        var request = new ConfigureRecorderRequest
        {
            outputPath = outputPath ?? "",
            source = source ?? "GameView",
            cameraSource = cameraSource ?? "",
            cameraTag = cameraTag ?? "",
            captureUI = captureUI,
            outputWidth = outputWidth,
            outputHeight = outputHeight,
            outputFormat = outputFormat ?? "MP4"
        };
        await client.PostAsync<ConfigureRecorderRequest, ConfigureRecorderResponse>(
            ApiRoutes.GameViewRecorderSettings, request, cancellationToken);
        return "Recorder settings configured.";
    }

    public async ValueTask<GetRecorderSettingsResponse> GetSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        return await client.GetAsync<GetRecorderSettingsRequest, GetRecorderSettingsResponse>(
            ApiRoutes.GameViewRecorderSettings, cancellationToken: cancellationToken);
    }

    public async ValueTask<string> StartAsync(
        int fps, string? frameRatePlayback, string? recordMode,
        float startTime, float endTime,
        int startFrame, int endFrame, int frameNumber,
        CancellationToken cancellationToken = default)
    {
        var request = new StartRecordingRequest
        {
            fps = fps,
            frameRatePlayback = frameRatePlayback ?? "Constant",
            recordMode = recordMode ?? "Manual",
            startTime = startTime,
            endTime = endTime,
            startFrame = startFrame,
            endFrame = endFrame,
            frameNumber = frameNumber
        };
        await client.PostAsync<StartRecordingRequest, StartRecordingResponse>(
            ApiRoutes.GameViewRecorderStart, request, cancellationToken);
        return "Recording started.";
    }

    public async ValueTask<string> StopAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.PostAsync<StopRecordingRequest, StopRecordingResponse>(
            ApiRoutes.GameViewRecorderStop, cancellationToken: cancellationToken);
        return response.outputPath;
    }
}
