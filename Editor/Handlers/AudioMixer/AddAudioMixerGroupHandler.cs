using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.AudioMixer
{
    internal sealed class AddAudioMixerGroupHandler
    {
        private readonly AddAudioMixerGroupUseCase _useCase;

        public AddAudioMixerGroupHandler(AddAudioMixerGroupUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.AudioMixerAddGroup, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(AddAudioMixerGroupRequest.assetPath)}, and {nameof(AddAudioMixerGroupRequest.groupName)} are required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<AddAudioMixerGroupRequest>(body);

            if (string.IsNullOrEmpty(request.assetPath))
            {
                await WriteRequiredErrorAsync(context, nameof(AddAudioMixerGroupRequest.assetPath));
                return;
            }

            if (string.IsNullOrEmpty(request.groupName))
            {
                await WriteRequiredErrorAsync(context, nameof(AddAudioMixerGroupRequest.groupName));
                return;
            }

            var response = await _useCase.ExecuteAsync(request.assetPath, request.groupName, request.parentGroup,
                cancellationToken);
            var json = JsonUtility.ToJson(response);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }

        private static Task WriteRequiredErrorAsync(IRequestContext context, string fieldName)
        {
            var errorJson = JsonUtility.ToJson(new ErrorResponse($"{fieldName} is required."));
            return context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
        }
    }
}
