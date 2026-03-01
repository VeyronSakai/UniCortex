using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.ScriptableObject
{
    internal sealed class CreateScriptableObjectHandler
    {
        private readonly CreateScriptableObjectUseCase _useCase;

        public CreateScriptableObjectHandler(CreateScriptableObjectUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.ScriptableObjectCreate, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("type and assetPath are required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<CreateScriptableObjectRequest>(body);

            if (string.IsNullOrEmpty(request.type))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("type is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            if (string.IsNullOrEmpty(request.assetPath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("assetPath is required."));
                await context.WriteResponseAsync(400, errorJson);
                return;
            }

            await _useCase.ExecuteAsync(request.type, request.assetPath, cancellationToken);
            var json = JsonUtility.ToJson(new CreateScriptableObjectResponse(true));
            await context.WriteResponseAsync(200, json);
        }
    }
}
