using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.AudioMixer
{
    internal sealed class AudioMixerHandler
    {
        private readonly GetAudioMixerUseCase _useCase;

        public AudioMixerHandler(GetAudioMixerUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.AudioMixer, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var assetPath = context.GetQueryParameter(nameof(GetAudioMixerRequest.assetPath));
            if (string.IsNullOrEmpty(assetPath))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(GetAudioMixerRequest.assetPath)} query parameter is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var response = await _useCase.ExecuteAsync(assetPath, cancellationToken);
            var json = JsonUtility.ToJson(response);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
