using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Component
{
    internal sealed class RemoveComponentHandler
    {
        private readonly RemoveComponentUseCase _useCase;

        public RemoveComponentHandler(RemoveComponentUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.ComponentRemove, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("instanceId and componentType are required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<RemoveComponentRequest>(body);

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

            await _useCase.ExecuteAsync(request.instanceId, request.componentType, request.componentIndex,
                cancellationToken);
            var json = JsonUtility.ToJson(new RemoveComponentResponse(true));
            await context.WriteResponseAsync(200, json);
        }
    }
}
