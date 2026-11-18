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

    public async ValueTask<string> DiscardChangesAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<DiscardSceneChangesRequest, DiscardSceneChangesResponse>(
            ApiRoutes.SceneDiscard, cancellationToken: cancellationToken);
        return "Scene changes discarded.";
    }

    public async ValueTask<string> SaveAsAsync(string scenePath, CancellationToken cancellationToken)
    {
        var request = new SaveSceneAsRequest { scenePath = scenePath };
        await client.PostAsync<SaveSceneAsRequest, SaveSceneAsResponse>(ApiRoutes.SceneSaveAs, request,
            cancellationToken);
        return $"Active scene saved as: {scenePath}";
    }

    public async ValueTask<string> GetHierarchyAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync<GetHierarchyRequest, GetHierarchyResponse>(
            ApiRoutes.Hierarchy, cancellationToken: cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
