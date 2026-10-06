using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Input
{
    internal sealed class ClickMouseHandler
    {
        private readonly ClickMouseUseCase _useCase;

        public ClickMouseHandler(ClickMouseUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.InputMouseClick, HandleAsync);
        }

        // Non-nullable fields for JsonUtility (see PointerRequestParser).
        [Serializable]
        private class RawClickMouseRequest
        {
            public float x;
            public float y;
            public int instanceId;
            public string button;
            public float holdDuration;
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                await WriteErrorAsync(context, PointerRequestParser.PositionRequiredMessage);
                return;
            }

            var request = JsonUtility.FromJson<RawClickMouseRequest>(body);
            var position = PointerRequestParser.ParsePosition(body, request.x, request.y, request.instanceId,
                "x", "y", "instanceId", out var error);
            if (position == null)
            {
                await WriteErrorAsync(context, error);
                return;
            }

            var button = string.IsNullOrEmpty(request.button) ? MouseButton.Left : request.button;

            MouseResponse response;
            try
            {
                response = await _useCase.ExecuteAsync(position, button, request.holdDuration, cancellationToken);
            }
            catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException
                                           or ArgumentException)
            {
                await WriteErrorAsync(context, ex.Message);
                return;
            }

            await context.WriteResponseAsync(HttpStatusCodes.Ok, JsonUtility.ToJson(response));
        }

        private static Task WriteErrorAsync(IRequestContext context, string message)
        {
            var errorJson = JsonUtility.ToJson(new ErrorResponse(message));
            return context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
        }
    }
}
