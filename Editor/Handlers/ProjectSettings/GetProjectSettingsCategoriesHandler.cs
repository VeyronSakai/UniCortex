using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.ProjectSettings
{
    internal sealed class GetProjectSettingsCategoriesHandler
    {
        private readonly GetProjectSettingsCategoriesUseCase _useCase;

        public GetProjectSettingsCategoriesHandler(GetProjectSettingsCategoriesUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.ProjectSettingsCategories, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var result = await _useCase.ExecuteAsync(cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
