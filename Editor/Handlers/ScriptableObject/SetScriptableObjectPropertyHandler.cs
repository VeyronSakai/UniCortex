using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.ScriptableObject
{
    internal sealed class SetScriptableObjectPropertyHandler
    {
        private readonly SetScriptableObjectPropertyUseCase _useCase;

        public SetScriptableObjectPropertyHandler(SetScriptableObjectPropertyUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.ScriptableObjectProperty, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(
                    new ErrorResponse("assetPath, propertyPath, and value are required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<SetScriptableObjectPropertyRequest>(body);

            if (string.IsNullOrEmpty(request.assetPath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("assetPath is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            if (string.IsNullOrEmpty(request.propertyPath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("propertyPath is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            if (request.value == null)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("value is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            await _useCase.ExecuteAsync(request.assetPath, request.propertyPath, request.value, cancellationToken);
            var json = JsonUtility.ToJson(new SetScriptableObjectPropertyResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
