using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Input
{
    internal sealed class PressKeyHandler
    {
        private readonly PressKeyUseCase _useCase;

        public PressKeyHandler(PressKeyUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.InputKeyPress, HandleAsync);
        }

        // Non-nullable fields for JsonUtility, which does not support nullable types.
        [Serializable]
        private class RawPressKeyRequest
        {
            public string[] keys;
            public float holdDuration;
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                await WriteErrorAsync(context, PressKeyUseCase.KeysRequiredMessage);
                return;
            }

            var request = JsonUtility.FromJson<RawPressKeyRequest>(body);

            try
            {
                await _useCase.ExecuteAsync(request.keys, request.holdDuration, cancellationToken);
            }
            catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException
                                           or ArgumentException)
            {
                await WriteErrorAsync(context, ex.Message);
                return;
            }

            await context.WriteResponseAsync(HttpStatusCodes.Ok, JsonUtility.ToJson(new PressKeyResponse(true)));
        }

        private static Task WriteErrorAsync(IRequestContext context, string message)
        {
            var errorJson = JsonUtility.ToJson(new ErrorResponse(message));
            return context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
        }
    }
}
