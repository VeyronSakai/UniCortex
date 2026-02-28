using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameObject
{
    internal sealed class GetGameObjectsHandler
    {
        private readonly GetGameObjectsUseCase _useCase;

        public GetGameObjectsHandler(GetGameObjectsUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.GameObjects, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var query = context.GetQueryParameter("query");

            var result = await _useCase.ExecuteAsync(query, cancellationToken);
            var json = JsonUtility.ToJson(new GetGameObjectsResponse(result));
            await context.WriteResponseAsync(200, json);
        }
    }
}
