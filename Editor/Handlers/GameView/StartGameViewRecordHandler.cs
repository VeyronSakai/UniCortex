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
            router.Register(HttpMethodType.Post, ApiRoutes.GameViewRecorderStart, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            try
            {
                var fps = 30;
                var frameRatePlayback = "Constant";
                var recordMode = "Manual";
                float startTime = 0;
                float endTime = 0;
                var startFrame = 0;
                var endFrame = 0;
                var frameNumber = 0;

                var body = await context.ReadBodyAsync();
                if (!string.IsNullOrEmpty(body))
                {
                    var request = JsonUtility.FromJson<StartRecordingRequest>(body);
                    if (request.fps > 0)
                        fps = request.fps;
                    if (!string.IsNullOrEmpty(request.frameRatePlayback))
                        frameRatePlayback = request.frameRatePlayback;
                    if (!string.IsNullOrEmpty(request.recordMode))
                        recordMode = request.recordMode;
                    startTime = request.startTime;
                    endTime = request.endTime;
                    startFrame = request.startFrame;
                    endFrame = request.endFrame;
                    frameNumber = request.frameNumber;
                }

                await _useCase.ExecuteAsync(fps, frameRatePlayback, recordMode,
                    startTime, endTime, startFrame, endFrame, frameNumber, cancellationToken);
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
