using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Input
{
    internal sealed class SendInputSystemKeyEventHandler
    {
        private readonly SendInputSystemKeyEventUseCase _useCase;

        public SendInputSystemKeyEventHandler(SendInputSystemKeyEventUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.InputSystemKey, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("key is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<SendInputSystemKeyEventRequest>(body);

            if (string.IsNullOrEmpty(request.key))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("key is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var eventType = string.IsNullOrEmpty(request.eventType) ? "press" : request.eventType;

            try
            {
                await _useCase.ExecuteAsync(request.key, eventType, cancellationToken);
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

            var json = JsonUtility.ToJson(new SendInputSystemKeyEventResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
