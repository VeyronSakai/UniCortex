using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Asset
{
    internal sealed class AssetInfoHandler
    {
        private readonly GetAssetInfoUseCase _useCase;

        public AssetInfoHandler(GetAssetInfoUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.AssetInfo, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var assetPath = context.GetQueryParameter("assetPath");
            if (string.IsNullOrEmpty(assetPath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("assetPath query parameter is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var result = await _useCase.ExecuteAsync(assetPath, cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(200, json);
        }
    }
}
