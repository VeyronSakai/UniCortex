using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class SceneUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> CreateAsync(string scenePath, CancellationToken cancellationToken)
    {
        var request = new CreateSceneRequest { scenePath = scenePath };
        await client.PostAsync<CreateSceneRequest, CreateSceneResponse>(ApiRoutes.SceneCreate, request, cancellationToken);
        return $"Scene created: {scenePath}";
    }

    public async ValueTask<string> OpenAsync(string scenePath, CancellationToken cancellationToken)
    {
        var request = new OpenSceneRequest { scenePath = scenePath };
        await client.PostAsync<OpenSceneRequest, OpenSceneResponse>(ApiRoutes.SceneOpen, request, cancellationToken);
        return $"Scene opened: {scenePath}";
    }

    public async ValueTask<string> SaveAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<SaveSceneRequest, SaveSceneResponse>(ApiRoutes.SceneSave,
            cancellationToken: cancellationToken);
        return "Scene saved successfully.";
    }

    public async ValueTask<string> GetHierarchyAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync<GetHierarchyRequest, GetHierarchyResponse>(
            ApiRoutes.Hierarchy, cancellationToken: cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
