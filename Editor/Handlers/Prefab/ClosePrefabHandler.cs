using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Prefab
{
    internal sealed class ClosePrefabHandler
    {
        private readonly ClosePrefabUseCase _useCase;

        public ClosePrefabHandler(ClosePrefabUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.PrefabClose, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            await _useCase.ExecuteAsync(cancellationToken);
            var json = JsonUtility.ToJson(new ClosePrefabResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
