using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Timeline
{
    internal sealed class GetTimelineInfoHandler
    {
        private readonly GetTimelineInfoUseCase _useCase;

        public GetTimelineInfoHandler(GetTimelineInfoUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.TimelineInfo, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var instanceIdParam = context.GetQueryParameter("instanceId");
            if (string.IsNullOrEmpty(instanceIdParam) || !int.TryParse(instanceIdParam, out var instanceId))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("instanceId query parameter is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            try
            {
                var result = await _useCase.ExecuteAsync(instanceId, cancellationToken);
                var json = JsonUtility.ToJson(result);
                await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
            }
            catch (InvalidOperationException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
            }
            catch (NotSupportedException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
            }
        }
    }
}
