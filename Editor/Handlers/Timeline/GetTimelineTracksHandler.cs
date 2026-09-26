using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Timeline
{
    internal sealed class GetTimelineTracksHandler
    {
        private readonly GetTimelineTracksUseCase _useCase;

        public GetTimelineTracksHandler(GetTimelineTracksUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.TimelineTracks, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var error = TimelineQueryParameters.ParseTarget(context, out var instanceId, out var assetPath);
            if (error != null)
            {
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest,
                    JsonUtility.ToJson(new ErrorResponse(error)));
                return;
            }

            GetTimelineTracksResponse result;
            try
            {
                result = await _useCase.ExecuteAsync(instanceId, assetPath, cancellationToken);
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
