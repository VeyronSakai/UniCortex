using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class RecordingUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> AddAsync(
        string name, string outputPath, string? encoder = null, string? encodingQuality = null,
        CancellationToken cancellationToken = default)
    {
        var request = new AddRecorderRequest
        {
            name = name,
            outputPath = outputPath,
            encoder = encoder ?? string.Empty,
            encodingQuality = encodingQuality ?? string.Empty
        };
        var response = await client.PostAsync<AddRecorderRequest, AddRecorderResponse>(
            ApiRoutes.RecorderAdd, request, cancellationToken);
        return response.name;
    }

    public async ValueTask<GetRecorderListResponse> GetListAsync(
        CancellationToken cancellationToken = default)
    {
        return await client.GetAsync<GetRecorderListRequest, GetRecorderListResponse>(
            ApiRoutes.RecorderList, cancellationToken: cancellationToken);
    }

    public async ValueTask RemoveAsync(int index, CancellationToken cancellationToken = default)
    {
        var request = new RemoveRecorderRequest { index = index };
        await client.PostAsync<RemoveRecorderRequest, RemoveRecorderResponse>(
            ApiRoutes.RecorderRemove, request, cancellationToken);
    }

    public async ValueTask StartAsync(
        int index, int fps = RecorderFps.Default,
        CancellationToken cancellationToken = default)
    {
        var request = new StartRecordingRequest
        {
            index = index,
            fps = fps
        };
        await client.PostAsync<StartRecordingRequest, StartRecordingResponse>(
            ApiRoutes.RecorderStart, request, cancellationToken);
    }

    public async ValueTask<string> StopAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.PostAsync<StopRecordingRequest, StopRecordingResponse>(
            ApiRoutes.RecorderStop, cancellationToken: cancellationToken);
        return response.outputPath;
    }
}
