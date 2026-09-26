using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Timeline
{
    internal sealed class GetTimelineTrackPropertiesHandler
    {
        private readonly GetTimelineTrackPropertiesUseCase _useCase;

        public GetTimelineTrackPropertiesHandler(GetTimelineTrackPropertiesUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.TimelineTrackProperties, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var trackIndex = 0;
            var error = TimelineQueryParameters.ParseTarget(context, out var instanceId, out var assetPath)
                        ?? TimelineQueryParameters.ParseIndex(context,
                            nameof(GetTimelineTrackPropertiesRequest.trackIndex), out trackIndex);
            if (error != null)
            {
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest,
                    JsonUtility.ToJson(new ErrorResponse(error)));
                return;
            }

            GetTimelineTrackPropertiesResponse result;
            try
            {
                result = await _useCase.ExecuteAsync(instanceId, assetPath, trackIndex, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest,
                    JsonUtility.ToJson(new ErrorResponse(ex.Message)));
                return;
            }

            await context.WriteResponseAsync(HttpStatusCodes.Ok, JsonUtility.ToJson(result));
        }
    }
}
