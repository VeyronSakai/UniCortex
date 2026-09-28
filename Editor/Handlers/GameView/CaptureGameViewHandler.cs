using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameView
{
    internal sealed class CaptureGameViewHandler
    {
        private readonly CaptureGameViewUseCase _useCase;

        public CaptureGameViewHandler(CaptureGameViewUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.GameViewCapture, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            try
            {
                var pngData = await _useCase.ExecuteAsync(cancellationToken);
                var json = JsonUtility.ToJson(new CaptureGameViewResponse(Convert.ToBase64String(pngData)));
                await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
            }
            catch (InvalidOperationException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
            }
        }
    }
}
