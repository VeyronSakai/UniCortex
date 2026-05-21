using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.ProjectSettings
{
    internal sealed class GetProjectSettingsHandler
    {
        private readonly GetProjectSettingsUseCase _useCase;

        public GetProjectSettingsHandler(GetProjectSettingsUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.ProjectSettingsGet, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var category = context.GetQueryParameter("category");
            if (string.IsNullOrEmpty(category))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse("category query parameter is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var result = await _useCase.ExecuteAsync(category, cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
