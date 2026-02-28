using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Scene
{
    internal sealed class SceneHierarchyHandler
    {
        private readonly GetSceneHierarchyUseCase _useCase;

        public SceneHierarchyHandler(GetSceneHierarchyUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.SceneHierarchy, HandleSceneHierarchyAsync);
        }

        private async Task HandleSceneHierarchyAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var result = await _useCase.ExecuteAsync(cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(200, json);
        }
    }
}
