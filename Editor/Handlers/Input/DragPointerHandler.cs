using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Input
{
    internal sealed class DragPointerHandler
    {
        private const string StartRequiredMessage = "Specify either fromX and fromY, or fromInstanceId.";

        private readonly DragPointerUseCase _useCase;

        public DragPointerHandler(DragPointerUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.InputPointerDrag, HandleAsync);
        }

        // Non-nullable fields for JsonUtility (see PointerRequestParser).
        [Serializable]
        private class RawDragPointerRequest
        {
            public float fromX;
            public float fromY;
            public int fromInstanceId;
            public float toX;
            public float toY;
            public int toInstanceId;
            public string button;
            public float duration;
            public float holdDuration;
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                await WriteErrorAsync(context, StartRequiredMessage);
                return;
            }

            var request = JsonUtility.FromJson<RawDragPointerRequest>(body);

            var start = PointerRequestParser.ParsePosition(body, request.fromX, request.fromY,
                request.fromInstanceId, "fromX", "fromY", "fromInstanceId", out var startError);
            if (start == null)
            {
                await WriteErrorAsync(context, startError);
                return;
            }

            var end = PointerRequestParser.ParsePosition(body, request.toX, request.toY, request.toInstanceId,
                "toX", "toY", "toInstanceId", out var endError);
            if (end == null)
            {
                await WriteErrorAsync(context, endError);
                return;
            }

            var button = string.IsNullOrEmpty(request.button) ? MouseButton.Left : request.button;
            var duration = PointerRequestParser.HasField(body, "duration")
                ? request.duration
                : DragPointerUseCase.DefaultDuration;

            DragPointerResponse response;
            try
            {
                response = await _useCase.ExecuteAsync(start, end, button, duration, request.holdDuration,
                    cancellationToken);
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
