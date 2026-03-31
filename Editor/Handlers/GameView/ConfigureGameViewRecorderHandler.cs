using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameView
{
    internal sealed class ConfigureGameViewRecorderHandler
    {
        private readonly ConfigureRecorderUseCase _useCase;

        public ConfigureGameViewRecorderHandler(ConfigureRecorderUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.GameViewRecorderSettings, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            try
            {
                var body = await context.ReadBodyAsync();

                var outputPath = "";
                var source = "GameView";
                var cameraSource = "";
                var cameraTag = "";
                var captureUI = false;
                var outputWidth = 0;
                var outputHeight = 0;
                var outputFormat = "MP4";

                if (!string.IsNullOrEmpty(body))
                {
                    var request = JsonUtility.FromJson<ConfigureRecorderRequest>(body);
                    if (!string.IsNullOrEmpty(request.outputPath))
                        outputPath = request.outputPath;
                    if (!string.IsNullOrEmpty(request.source))
                        source = request.source;
                    if (!string.IsNullOrEmpty(request.cameraSource))
                        cameraSource = request.cameraSource;
                    if (!string.IsNullOrEmpty(request.cameraTag))
                        cameraTag = request.cameraTag;
                    captureUI = request.captureUI;
                    outputWidth = request.outputWidth;
                    outputHeight = request.outputHeight;
                    if (!string.IsNullOrEmpty(request.outputFormat))
                        outputFormat = request.outputFormat;
                }

                await _useCase.ExecuteAsync(outputPath, source, cameraSource, cameraTag,
                    captureUI, outputWidth, outputHeight, outputFormat, cancellationToken);
                var json = JsonUtility.ToJson(new ConfigureRecorderResponse(true));
                await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
            }
            catch (NotSupportedException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
            }
        }
    }
}
