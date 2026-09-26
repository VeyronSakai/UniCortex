using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Timeline
{
    internal sealed class SetTimelineClipPropertyHandler
    {
        private readonly SetTimelineClipPropertyUseCase _useCase;

        public SetTimelineClipPropertyHandler(SetTimelineClipPropertyUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.TimelineSetClipProperty, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(SetTimelineClipPropertyRequest.instanceId)}, {nameof(SetTimelineClipPropertyRequest.trackIndex)}, {nameof(SetTimelineClipPropertyRequest.clipIndex)}, {nameof(SetTimelineClipPropertyRequest.propertyPath)}, and {nameof(SetTimelineClipPropertyRequest.value)} are required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<SetTimelineClipPropertyRequest>(body);

            if (request.instanceId == 0)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(SetTimelineClipPropertyRequest.instanceId)} is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            if (string.IsNullOrEmpty(request.propertyPath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(SetTimelineClipPropertyRequest.propertyPath)} is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            if (request.value == null)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(SetTimelineClipPropertyRequest.value)} is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            try
            {
                await _useCase.ExecuteAsync(request.instanceId, request.trackIndex, request.clipIndex,
                    request.propertyPath, request.value, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var json = JsonUtility.ToJson(new SetTimelineClipPropertyResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
