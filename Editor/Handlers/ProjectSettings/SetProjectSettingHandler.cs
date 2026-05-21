using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.ProjectSettings
{
    internal sealed class SetProjectSettingHandler
    {
        private readonly SetProjectSettingUseCase _useCase;

        public SetProjectSettingHandler(SetProjectSettingUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.ProjectSettingsSet, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(
                    new ErrorResponse("category, propertyPath, and value are required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = JsonUtility.FromJson<SetProjectSettingRequest>(body);

            if (string.IsNullOrEmpty(request.category))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("category is required."));
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

            await _useCase.ExecuteAsync(request.category, request.propertyPath, request.value, cancellationToken);
            var json = JsonUtility.ToJson(new SetProjectSettingResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
