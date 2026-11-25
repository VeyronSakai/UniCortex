using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Prefab
{
    internal sealed class UnpackPrefabInstanceHandler
    {
        private readonly UnpackPrefabInstanceUseCase _useCase;

        public UnpackPrefabInstanceHandler(UnpackPrefabInstanceUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.PrefabUnpack, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            var request = string.IsNullOrEmpty(body) ? null : JsonUtility.FromJson<UnpackPrefabInstanceRequest>(body);

            if (request == null || request.instanceId == 0)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("instanceId is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            // An unknown mode or a GameObject that is not the outermost root of a prefab instance throws
            // ArgumentException, which the router answers with 400.
            await _useCase.ExecuteAsync(request.instanceId, request.mode, cancellationToken);
            var json = JsonUtility.ToJson(new UnpackPrefabInstanceResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
