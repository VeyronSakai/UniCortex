using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameObject
{
    internal sealed class GameObjectInfoHandler
    {
        private readonly GetGameObjectInfoUseCase _useCase;

        public GameObjectInfoHandler(GetGameObjectInfoUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.GameObjectInfo, HandleAsync);
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

            var result = await _useCase.ExecuteAsync(instanceId, cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(200, json);
        }
    }
}
