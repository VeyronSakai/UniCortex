using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ProfilerUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> FocusWindowAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<FocusProfilerWindowRequest, FocusProfilerWindowResponse>(
            ApiRoutes.ProfilerFocus, cancellationToken: cancellationToken);
        return "Profiler window focused successfully.";
    }

    public async ValueTask<string> GetStatusAsync(CancellationToken cancellationToken)
    {
        var response = await GetStatusResponseAsync(cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<GetProfilerStatusResponse> GetStatusResponseAsync(CancellationToken cancellationToken)
    {
        return await client.GetAsync<GetProfilerStatusRequest, GetProfilerStatusResponse>(
            ApiRoutes.ProfilerStatus, cancellationToken: cancellationToken);
    }

    public async ValueTask<string> StartRecordingAsync(bool profileEditor = false,
        CancellationToken cancellationToken = default)
    {
        await client.PostAsync<StartProfilerRecordingRequest, StartProfilerRecordingResponse>(
            ApiRoutes.ProfilerStartRecording,
            new StartProfilerRecordingRequest { profileEditor = profileEditor },
            cancellationToken);
        return profileEditor
            ? "Profiler recording started for the Editor."
            : "Profiler recording started.";
    }

    public async ValueTask<string> StopRecordingAsync(CancellationToken cancellationToken = default)
    {
        await client.PostAsync<StopProfilerRecordingRequest, StopProfilerRecordingResponse>(
            ApiRoutes.ProfilerStopRecording, cancellationToken: cancellationToken);
        return "Profiler recording stopped successfully.";
    }
}
