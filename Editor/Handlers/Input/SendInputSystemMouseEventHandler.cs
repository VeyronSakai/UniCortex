using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Input
{
    internal sealed class SendInputSystemMouseEventHandler
    {
        private readonly SendInputSystemMouseEventUseCase _useCase;

        public SendInputSystemMouseEventHandler(SendInputSystemMouseEventUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.InputSystemMouse, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("x and y are required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<SendInputSystemMouseEventRequest>(body);

            var eventType = string.IsNullOrEmpty(request.eventType) ? "press" : request.eventType;

            try
            {
                await _useCase.ExecuteAsync(request.x, request.y, request.button, eventType, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }
            catch (NotSupportedException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var json = JsonUtility.ToJson(new SendInputSystemMouseEventResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
