using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameView
{
    internal sealed class StartGameViewRecordHandler
    {
        private readonly StartRecordingUseCase _useCase;

        public StartGameViewRecordHandler(StartRecordingUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.GameViewRecordStart, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            try
            {
                var body = await context.ReadBodyAsync();

                var fps = 30;
                var outputPath = "";

                if (!string.IsNullOrEmpty(body))
                {
                    var request = JsonUtility.FromJson<StartRecordingRequest>(body);
                    if (request.fps > 0)
                    {
                        fps = request.fps;
                    }

                    if (!string.IsNullOrEmpty(request.outputPath))
                    {
                        outputPath = request.outputPath;
                    }
                }

                await _useCase.ExecuteAsync(fps, outputPath, cancellationToken);
                var json = JsonUtility.ToJson(new StartRecordingResponse(true));
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
