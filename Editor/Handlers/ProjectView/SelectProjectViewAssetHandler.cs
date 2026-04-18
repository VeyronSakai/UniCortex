using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.ProjectView
{
    internal sealed class SelectProjectViewAssetHandler
    {
        private readonly SelectProjectViewAssetUseCase _useCase;

        public SelectProjectViewAssetHandler(SelectProjectViewAssetUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.ProjectViewSelect, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("assetPath is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<SelectProjectViewAssetRequest>(body);

            if (string.IsNullOrEmpty(request.assetPath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("assetPath is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            await _useCase.ExecuteAsync(request.assetPath, cancellationToken);
            var json = JsonUtility.ToJson(new SelectProjectViewAssetResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
