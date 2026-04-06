using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.MovieRecorder
{
    internal sealed class AddMovieRecorderHandler
    {
        private readonly AddMovieRecorderUseCase _useCase;

        public AddMovieRecorderHandler(AddMovieRecorderUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.RecorderMovieAdd, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            try
            {
                var body = await context.ReadBodyAsync();
                if (string.IsNullOrEmpty(body))
                {
                    var errorJson = JsonUtility.ToJson(new ErrorResponse("Request body is required."));
                    await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                    return;
                }

                var request = JsonUtility.FromJson<AddMovieRecorderRequest>(body);
                if (string.IsNullOrEmpty(request.name))
                {
                    var errorJson = JsonUtility.ToJson(new ErrorResponse("name is required."));
                    await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                    return;
                }

                if (string.IsNullOrEmpty(request.outputPath))
                {
                    var errorJson = JsonUtility.ToJson(new ErrorResponse("outputPath is required."));
                    await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                    return;
                }

                var name = await _useCase.ExecuteAsync(
                    request.name, request.outputPath,
                    request.encoder ?? string.Empty,
                    request.encodingQuality ?? string.Empty,
                    cancellationToken);
                var json = JsonUtility.ToJson(new AddMovieRecorderResponse(name));
                await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
            }
            catch (InvalidOperationException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
            }
            catch (NotSupportedException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
            }
        }
    }
}
