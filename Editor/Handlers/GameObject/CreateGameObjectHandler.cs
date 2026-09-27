using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameObject
{
    internal sealed class CreateGameObjectHandler
    {
        private readonly CreateGameObjectUseCase _useCase;

        public CreateGameObjectHandler(CreateGameObjectUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.GameObjectCreate, HandleAsync);
        }

        // JsonUtility does not support Nullable<T>, so use a non-nullable helper for deserialization.
        // Field presence is detected via string matching where the default value is meaningful.
        [Serializable]
        private class RawCreateRequest
        {
            public string name;
            public int parentInstanceId;
            public int siblingIndex;
            public bool rectTransform;
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("name is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<RawCreateRequest>(body);

            if (string.IsNullOrEmpty(request.name))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("name is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var siblingIndex = body.Contains("\"siblingIndex\"") ? (int?)request.siblingIndex : null;
            if (siblingIndex < 0)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("siblingIndex must be 0 or greater."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var result = await _useCase.ExecuteAsync(request.name, request.parentInstanceId, siblingIndex,
                request.rectTransform, cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
