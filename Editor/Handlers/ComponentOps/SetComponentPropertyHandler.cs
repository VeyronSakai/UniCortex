using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.ComponentOps
{
    internal sealed class SetComponentPropertyHandler
    {
        private readonly SetComponentPropertyUseCase _useCase;

        public SetComponentPropertyHandler(SetComponentPropertyUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.ComponentSetProperty, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(
                    new ErrorResponse("instanceId, componentType, propertyPath, and value are required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<SetComponentPropertyRequest>(body);

            if (request.instanceId == 0)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("instanceId is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            if (string.IsNullOrEmpty(request.componentType))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("componentType is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            if (string.IsNullOrEmpty(request.propertyPath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("propertyPath is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            if (request.value == null)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("value is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            await _useCase.ExecuteAsync(request.instanceId, request.componentType, request.propertyPath,
                request.value, cancellationToken);
            var json = JsonUtility.ToJson(new SetComponentPropertyResponse(true));
            await context.WriteResponseAsync(200, json);
        }
    }
}
