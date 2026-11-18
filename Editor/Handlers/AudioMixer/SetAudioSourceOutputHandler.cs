using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.AudioMixer
{
    internal sealed class SetAudioSourceOutputHandler
    {
        private readonly SetAudioSourceOutputUseCase _useCase;

        public SetAudioSourceOutputHandler(SetAudioSourceOutputUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.AudioSourceSetOutput, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(SetAudioSourceOutputRequest.instanceId)}, {nameof(SetAudioSourceOutputRequest.assetPath)}, and {nameof(SetAudioSourceOutputRequest.group)} are required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<SetAudioSourceOutputRequest>(body);

            if (request.instanceId == 0)
            {
                await WriteRequiredErrorAsync(context, nameof(SetAudioSourceOutputRequest.instanceId));
                return;
            }

            if (string.IsNullOrEmpty(request.assetPath))
            {
                await WriteRequiredErrorAsync(context, nameof(SetAudioSourceOutputRequest.assetPath));
                return;
            }

            if (string.IsNullOrEmpty(request.group))
            {
                await WriteRequiredErrorAsync(context, nameof(SetAudioSourceOutputRequest.group));
                return;
            }

            await _useCase.ExecuteAsync(request.instanceId, request.componentIndex, request.assetPath, request.group,
                cancellationToken);
            var json = JsonUtility.ToJson(new SetAudioSourceOutputResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }

        private static Task WriteRequiredErrorAsync(IRequestContext context, string fieldName)
        {
            var errorJson = JsonUtility.ToJson(new ErrorResponse($"{fieldName} is required."));
            return context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
        }
    }
}
