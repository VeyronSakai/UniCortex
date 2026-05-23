using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ProjectSettingsUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> GetAsync(string category, CancellationToken cancellationToken = default)
    {
        var request = new GetProjectSettingsRequest { category = category };
        var response = await client.GetAsync<GetProjectSettingsRequest, GetProjectSettingsResponse>(
            ApiRoutes.ProjectSettingsGet, request, cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> SetAsync(string category, string propertyPath, string value,
        CancellationToken cancellationToken = default)
    {
        var request = new SetProjectSettingRequest
        {
            category = category, propertyPath = propertyPath, value = value
        };
        await client.PostAsync<SetProjectSettingRequest, SetProjectSettingResponse>(
            ApiRoutes.ProjectSettingsSet, request, cancellationToken);
        return $"Setting '{propertyPath}' in '{category}' set to '{value}' successfully.";
    }

    public async ValueTask<string> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var response = await client
            .GetAsync<GetProjectSettingsCategoriesRequest, GetProjectSettingsCategoriesResponse>(
                ApiRoutes.ProjectSettingsCategories, cancellationToken: cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
