using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameView
{
    internal sealed class FocusGameViewHandler
    {
        private readonly FocusGameViewUseCase _useCase;

        public FocusGameViewHandler(FocusGameViewUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.FocusGameView, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            try
            {
                await _useCase.ExecuteAsync(cancellationToken);
                var json = JsonUtility.ToJson(new FocusGameViewResponse(true));
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
