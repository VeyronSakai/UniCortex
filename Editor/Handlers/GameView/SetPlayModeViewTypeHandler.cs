using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameView
{
    internal sealed class SetPlayModeViewTypeHandler
    {
        private readonly SetPlayModeViewTypeUseCase _useCase;

        public SetPlayModeViewTypeHandler(SetPlayModeViewTypeUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.GameViewViewType, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            var request = string.IsNullOrEmpty(body) ? null : JsonUtility.FromJson<SetPlayModeViewTypeRequest>(body);

            if (request == null || string.IsNullOrEmpty(request.viewType))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("viewType is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            SetPlayModeViewTypeResponse result;
            try
            {
                result = await _useCase.ExecuteAsync(request.viewType, cancellationToken);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
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
