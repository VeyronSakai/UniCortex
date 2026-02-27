using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.ComponentOps
{
    internal sealed class AddComponentHandler
    {
        private readonly AddComponentUseCase _useCase;

        public AddComponentHandler(AddComponentUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.ComponentAdd, HandleAsync);
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

            var request = JsonUtility.FromJson<AddComponentRequest>(body);

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

            await _useCase.ExecuteAsync(request.instanceId, request.componentType, cancellationToken);
            var json = JsonUtility.ToJson(new AddComponentResponse(true));
            await context.WriteResponseAsync(200, json);
        }
    }
}
