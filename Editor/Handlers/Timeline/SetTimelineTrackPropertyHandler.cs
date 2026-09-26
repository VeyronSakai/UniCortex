using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Timeline
{
    internal sealed class SetTimelineTrackPropertyHandler
    {
        private readonly SetTimelineTrackPropertyUseCase _useCase;

        public SetTimelineTrackPropertyHandler(SetTimelineTrackPropertyUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.TimelineSetTrackProperty, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(SetTimelineTrackPropertyRequest.instanceId)}, {nameof(SetTimelineTrackPropertyRequest.trackIndex)}, {nameof(SetTimelineTrackPropertyRequest.propertyPath)}, and {nameof(SetTimelineTrackPropertyRequest.value)} are required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<SetTimelineTrackPropertyRequest>(body);

            if (request.instanceId == 0)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(SetTimelineTrackPropertyRequest.instanceId)} is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            if (string.IsNullOrEmpty(request.propertyPath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(SetTimelineTrackPropertyRequest.propertyPath)} is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            if (request.value == null)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(SetTimelineTrackPropertyRequest.value)} is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            try
            {
                await _useCase.ExecuteAsync(request.instanceId, request.trackIndex, request.propertyPath,
                    request.value, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var json = JsonUtility.ToJson(new SetTimelineTrackPropertyResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
