using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameObject
{
    internal sealed class FindGameObjectsHandler
    {
        private readonly FindGameObjectsUseCase _useCase;

        public FindGameObjectsHandler(FindGameObjectsUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.GameObjectFind, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var name = context.GetQueryParameter("name");
            var tag = context.GetQueryParameter("tag");
            var componentType = context.GetQueryParameter("componentType");

            var result = await _useCase.ExecuteAsync(name, tag, componentType, cancellationToken);
            var json = JsonUtility.ToJson(new FindGameObjectsResponse(result));
            await context.WriteResponseAsync(200, json);
        }
    }
}
