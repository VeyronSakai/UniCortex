using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameView
{
    internal sealed class GetGameViewRecorderSettingsHandler
    {
        private readonly GetRecorderSettingsUseCase _useCase;

        public GetGameViewRecorderSettingsHandler(GetRecorderSettingsUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.GameViewRecorderSettings, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _useCase.ExecuteAsync(cancellationToken);
                var json = JsonUtility.ToJson(response);
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
