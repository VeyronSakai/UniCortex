using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.ScriptableObject
{
    internal sealed class ScriptableObjectPropertiesHandler
    {
        private readonly GetScriptableObjectPropertiesUseCase _useCase;

        public ScriptableObjectPropertiesHandler(GetScriptableObjectPropertiesUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.ScriptableObjectProperties, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var assetPath = context.GetQueryParameter("assetPath");
            if (string.IsNullOrEmpty(assetPath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("assetPath query parameter is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var result = await _useCase.ExecuteAsync(assetPath, cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
