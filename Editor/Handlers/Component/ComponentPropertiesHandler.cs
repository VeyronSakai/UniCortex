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
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var componentType = context.GetQueryParameter("componentType");
            if (string.IsNullOrEmpty(componentType))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("componentType query parameter is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var assemblyName = context.GetQueryParameter("assemblyName");
            if (string.IsNullOrEmpty(assemblyName))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("assemblyName query parameter is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var componentIndex = 0;
            var componentIndexParam = context.GetQueryParameter("componentIndex");
            if (!string.IsNullOrEmpty(componentIndexParam) && int.TryParse(componentIndexParam, out var parsed))
            {
                componentIndex = parsed;
            }

            // Without depth, every level of arrays and nested serializable values is listed.
            int? depth = null;
            var depthParam = context.GetQueryParameter("depth");
            if (!string.IsNullOrEmpty(depthParam))
            {
                if (!int.TryParse(depthParam, out var parsedDepth))
                {
                    var errorJson = JsonUtility.ToJson(new ErrorResponse($"depth must be an integer (got '{depthParam}')."));
                    await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                    return;
                }

                depth = parsedDepth;
            }

            var propertyPath = context.GetQueryParameter("propertyPath");

            // A negative depth or an unknown propertyPath throws ArgumentException, which the router answers with 400.
            var result = await _useCase.ExecuteAsync(instanceId, componentType, assemblyName, componentIndex,
                propertyPath, depth, cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
