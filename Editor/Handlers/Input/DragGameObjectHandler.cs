using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Input
{
    internal sealed class DragGameObjectHandler
    {
        private readonly DragPointerUseCase _useCase;

        public DragGameObjectHandler(DragPointerUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.InputGameObjectDrag, HandleAsync);
        }

        // JsonUtility does not support Nullable<T>, so frames and holdFrames are read into non-nullable fields.
        [Serializable]
        private class RawDragGameObjectRequest
        {
            public int fromInstanceId;
            public int toInstanceId;
            public string button;
            public int frames;
            public int holdFrames;
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            var request = string.IsNullOrEmpty(body)
                ? new RawDragGameObjectRequest()
                : JsonUtility.FromJson<RawDragGameObjectRequest>(body);
            var error = PointerRequestParser.ValidateInstanceId(request.fromInstanceId, "fromInstanceId")
                        ?? PointerRequestParser.ValidateInstanceId(request.toInstanceId, "toInstanceId");
            if (error != null)
            {
                await WriteErrorAsync(context, error);
                return;
            }

            var start = new PointerPosition.Target(request.fromInstanceId);
            var end = new PointerPosition.Target(request.toInstanceId);
            var button = string.IsNullOrEmpty(request.button) ? MouseButton.Left : request.button;
            var frames = PointerRequestParser.HasField(body, "frames")
                ? request.frames
                : DragPointerUseCase.DefaultFrames;

            DragPointerResponse response;
            try
            {
                response = await _useCase.ExecuteAsync(start, end, button, frames, request.holdFrames,
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
