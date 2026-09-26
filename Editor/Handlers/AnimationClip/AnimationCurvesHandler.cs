using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.AnimationClip
{
    internal sealed class AnimationCurvesHandler
    {
        private readonly GetAnimationCurvesUseCase _useCase;

        public AnimationCurvesHandler(GetAnimationCurvesUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.AnimationClipCurves, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var assetPath = context.GetQueryParameter(nameof(GetAnimationCurvesRequest.assetPath));
            if (string.IsNullOrEmpty(assetPath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(GetAnimationCurvesRequest.assetPath)} query parameter is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var result = await _useCase.ExecuteAsync(assetPath, cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
