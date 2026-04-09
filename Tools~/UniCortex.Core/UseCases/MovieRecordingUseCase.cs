using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class MovieRecordingUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> AddAsync(
        string name, string outputPath,
        string encoder = MovieRecorderEncoderType.UnityMediaEncoder,
        string encodingQuality = MovieRecorderEncodingQuality.Low,
        bool captureAudio = false,
        CancellationToken cancellationToken = default)
    {
        var request = new AddMovieRecorderRequest
        {
            name = name,
            outputPath = outputPath,
            encoder = encoder,
            encodingQuality = encodingQuality,
            captureAudio = captureAudio
        };
        var response = await client.PostAsync<AddMovieRecorderRequest, AddMovieRecorderResponse>(
            ApiRoutes.RecorderMovieAdd, request, cancellationToken);
        return response.name;
    }

    public async ValueTask<GetRecorderListResponse> GetListAsync(
        CancellationToken cancellationToken = default)
    {
        return await client.GetAsync<GetRecorderListRequest, GetRecorderListResponse>(
            ApiRoutes.RecorderAllList, cancellationToken: cancellationToken);
    }

    public async ValueTask RemoveAsync(int index, CancellationToken cancellationToken = default)
    {
        var request = new RemoveMovieRecorderRequest { index = index };
        await client.PostAsync<RemoveMovieRecorderRequest, RemoveMovieRecorderResponse>(
            ApiRoutes.RecorderMovieRemove, request, cancellationToken);
    }

    public async ValueTask StartAsync(
        int index, int fps = RecorderFps.Default,
        CancellationToken cancellationToken = default)
    {
        var request = new StartMovieRecordingRequest
        {
            index = index,
            fps = fps
        };
        await client.PostAsync<StartMovieRecordingRequest, StartMovieRecordingResponse>(
            ApiRoutes.RecorderMovieStart, request, cancellationToken);
    }

    public async ValueTask<string> StopAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.PostAsync<StopMovieRecordingRequest, StopMovieRecordingResponse>(
            ApiRoutes.RecorderMovieStop, cancellationToken: cancellationToken);
        return response.outputPath;
    }
}
