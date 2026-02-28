using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Component
{
    internal sealed class ComponentPropertiesHandler
    {
        private readonly GetComponentPropertiesUseCase _useCase;

        public ComponentPropertiesHandler(GetComponentPropertiesUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.ComponentProperties, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var instanceIdParam = context.GetQueryParameter("instanceId");
            if (string.IsNullOrEmpty(instanceIdParam) || !int.TryParse(instanceIdParam, out var instanceId))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("instanceId query parameter is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var componentType = context.GetQueryParameter("componentType");
            if (string.IsNullOrEmpty(componentType))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("componentType query parameter is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var componentIndex = 0;
            var componentIndexParam = context.GetQueryParameter("componentIndex");
            if (!string.IsNullOrEmpty(componentIndexParam) && int.TryParse(componentIndexParam, out var parsed))
            {
                componentIndex = parsed;
            }

            var result = await _useCase.ExecuteAsync(instanceId, componentType, componentIndex, cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(200, json);
        }
    }
}
