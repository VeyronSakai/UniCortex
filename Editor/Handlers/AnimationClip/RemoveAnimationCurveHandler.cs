using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.AnimationClip
{
    internal sealed class RemoveAnimationCurveHandler
    {
        private readonly RemoveAnimationCurveUseCase _useCase;

        public RemoveAnimationCurveHandler(RemoveAnimationCurveUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.AnimationClipRemoveCurve, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(RemoveAnimationCurveRequest.assetPath)}, {nameof(RemoveAnimationCurveRequest.componentType)}, {nameof(RemoveAnimationCurveRequest.assemblyName)}, and {nameof(RemoveAnimationCurveRequest.propertyName)} are required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<RemoveAnimationCurveRequest>(body);

            var missingField = AnimationCurveRequestValidator.FindMissingField(request.assetPath,
                request.componentType, request.assemblyName, request.propertyName);
            if (missingField != null)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse($"{missingField} is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            await _useCase.ExecuteAsync(request.assetPath, request.path, request.componentType,
                request.assemblyName, request.propertyName, cancellationToken);
            var json = JsonUtility.ToJson(new RemoveAnimationCurveResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
