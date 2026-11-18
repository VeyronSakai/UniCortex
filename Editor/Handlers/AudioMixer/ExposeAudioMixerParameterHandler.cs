using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.AudioMixer
{
    internal sealed class ExposeAudioMixerParameterHandler
    {
        private readonly ExposeAudioMixerParameterUseCase _useCase;

        public ExposeAudioMixerParameterHandler(ExposeAudioMixerParameterUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.AudioMixerExposeParameter, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(ExposeAudioMixerParameterRequest.assetPath)}, {nameof(ExposeAudioMixerParameterRequest.group)}, {nameof(ExposeAudioMixerParameterRequest.parameter)}, and {nameof(ExposeAudioMixerParameterRequest.exposedName)} are required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<ExposeAudioMixerParameterRequest>(body);

            if (string.IsNullOrEmpty(request.assetPath))
            {
                await WriteRequiredErrorAsync(context, nameof(ExposeAudioMixerParameterRequest.assetPath));
                return;
            }

            if (string.IsNullOrEmpty(request.group))
            {
                await WriteRequiredErrorAsync(context, nameof(ExposeAudioMixerParameterRequest.group));
                return;
            }

            if (string.IsNullOrEmpty(request.parameter))
            {
                await WriteRequiredErrorAsync(context, nameof(ExposeAudioMixerParameterRequest.parameter));
                return;
            }

            if (string.IsNullOrEmpty(request.exposedName))
            {
                await WriteRequiredErrorAsync(context, nameof(ExposeAudioMixerParameterRequest.exposedName));
                return;
            }

            await _useCase.ExecuteAsync(request.assetPath, request.group, request.parameter, request.exposedName,
                cancellationToken);
            var json = JsonUtility.ToJson(new ExposeAudioMixerParameterResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }

        private static Task WriteRequiredErrorAsync(IRequestContext context, string fieldName)
        {
            var errorJson = JsonUtility.ToJson(new ErrorResponse($"{fieldName} is required."));
            return context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
        }
    }
}
