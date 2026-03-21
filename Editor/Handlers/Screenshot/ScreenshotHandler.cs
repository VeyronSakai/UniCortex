using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Screenshot
{
    internal sealed class ScreenshotHandler
    {
        private readonly CaptureScreenshotUseCase _useCase;

        public ScreenshotHandler(CaptureScreenshotUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.ScreenshotCapture, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var pngData = await _useCase.ExecuteAsync(cancellationToken);
            var json = JsonUtility.ToJson(new CaptureScreenshotResponse(Convert.ToBase64String(pngData)));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
