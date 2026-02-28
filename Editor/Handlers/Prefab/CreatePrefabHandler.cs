using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Prefab
{
    internal sealed class CreatePrefabHandler
    {
        private readonly CreatePrefabUseCase _useCase;

        public CreatePrefabHandler(CreatePrefabUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.PrefabCreate, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("instanceId and assetPath are required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<CreatePrefabRequest>(body);

            if (request.instanceId == 0)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("instanceId is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            if (string.IsNullOrEmpty(request.assetPath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("assetPath is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            await _useCase.ExecuteAsync(request.instanceId, request.assetPath, cancellationToken);
            var json = JsonUtility.ToJson(new CreatePrefabResponse(true));
            await context.WriteResponseAsync(200, json);
        }
    }
}
