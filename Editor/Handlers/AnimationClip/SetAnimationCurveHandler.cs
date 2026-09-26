using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.AnimationClip
{
    internal sealed class SetAnimationCurveHandler
    {
        private readonly SetAnimationCurveUseCase _useCase;

        public SetAnimationCurveHandler(SetAnimationCurveUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.AnimationClipSetCurve, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(SetAnimationCurveRequest.assetPath)}, {nameof(SetAnimationCurveRequest.componentType)}, {nameof(SetAnimationCurveRequest.assemblyName)}, {nameof(SetAnimationCurveRequest.propertyName)}, and {nameof(SetAnimationCurveRequest.keys)} are required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<SetAnimationCurveRequest>(body);

            var missingField = AnimationCurveRequestValidator.FindMissingField(request.assetPath,
                request.componentType, request.assemblyName, request.propertyName);
            if (missingField != null)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse($"{missingField} is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            if (request.keys == null || request.keys.Count == 0)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(SetAnimationCurveRequest.keys)} must contain at least one key."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            await _useCase.ExecuteAsync(request.assetPath, request.animatorRelativePath, request.componentType,
                request.assemblyName, request.propertyName, request.keys, cancellationToken);
            var json = JsonUtility.ToJson(new SetAnimationCurveResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
