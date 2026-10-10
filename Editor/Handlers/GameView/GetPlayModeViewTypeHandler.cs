using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameView
{
    internal sealed class GetPlayModeViewTypeHandler
    {
        private readonly GetPlayModeViewTypeUseCase _useCase;

        public GetPlayModeViewTypeHandler(GetPlayModeViewTypeUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.GameViewViewType, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            GetPlayModeViewTypeResponse result;
            try
            {
                result = await _useCase.ExecuteAsync(cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
