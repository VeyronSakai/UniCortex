using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Input
{
    internal sealed class MovePointerHandler
    {
        private readonly MovePointerUseCase _useCase;

        public MovePointerHandler(MovePointerUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.InputPointerMove, HandleAsync);
        }

        // Non-nullable fields for JsonUtility (see PointerRequestParser).
        [Serializable]
        private class RawMovePointerRequest
        {
            public float x;
            public float y;
            public int instanceId;
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                await WriteErrorAsync(context, PointerRequestParser.PositionRequiredMessage);
                return;
            }

            var request = JsonUtility.FromJson<RawMovePointerRequest>(body);
            var position = PointerRequestParser.ParsePosition(body, request.x, request.y, request.instanceId,
                "x", "y", "instanceId", out var error);
            if (position == null)
            {
                await WriteErrorAsync(context, error);
                return;
            }

            PointerResponse response;
            try
            {
                response = await _useCase.ExecuteAsync(position, cancellationToken);
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
