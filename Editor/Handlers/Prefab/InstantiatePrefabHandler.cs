using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Prefab
{
    internal sealed class InstantiatePrefabHandler
    {
        private readonly InstantiatePrefabUseCase _useCase;

        public InstantiatePrefabHandler(InstantiatePrefabUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.PrefabInstantiate, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("assetPath is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<InstantiatePrefabRequest>(body);

            if (string.IsNullOrEmpty(request.assetPath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("assetPath is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var result = await _useCase.ExecuteAsync(request.assetPath, cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(200, json);
        }
    }
}
