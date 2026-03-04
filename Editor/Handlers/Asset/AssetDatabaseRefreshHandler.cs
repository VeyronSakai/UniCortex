using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Asset
{
    internal sealed class AssetDatabaseRefreshHandler
    {
        private readonly RefreshAssetDatabaseUseCase _useCase;

        public AssetDatabaseRefreshHandler(RefreshAssetDatabaseUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.AssetDatabaseRefresh, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            await _useCase.ExecuteAsync(cancellationToken);
            var json = JsonUtility.ToJson(new AssetDatabaseRefreshResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
