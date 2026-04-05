using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Recorder
{
    internal sealed class AddRecorderHandler
    {
        private readonly AddRecorderUseCase _useCase;

        public AddRecorderHandler(AddRecorderUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.RecorderAdd, HandleAsync);
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

                var request = JsonUtility.FromJson<AddRecorderRequest>(body);
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
                    string.IsNullOrEmpty(request.encoder) ? RecorderEncoderType.UnityMediaEncoder : request.encoder,
                    string.IsNullOrEmpty(request.encodingQuality) ? RecorderEncodingQuality.Low : request.encodingQuality,
                    cancellationToken);
                var json = JsonUtility.ToJson(new AddRecorderResponse(name));
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
