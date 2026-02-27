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

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("name is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<CreateGameObjectRequest>(body);

            if (string.IsNullOrEmpty(request.name))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("name is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var result = await _useCase.ExecuteAsync(request.name, request.primitive, cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(200, json);
        }
    }
}
