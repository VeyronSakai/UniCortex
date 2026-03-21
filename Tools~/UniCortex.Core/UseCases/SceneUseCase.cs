using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class SceneUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> CreateAsync(string scenePath, CancellationToken cancellationToken)
    {
        var request = new CreateSceneRequest { scenePath = scenePath };
        await client.PostAsync(ApiRoutes.SceneCreate, request, cancellationToken);
        return $"Scene created: {scenePath}";
    }

    public async ValueTask<string> OpenAsync(string scenePath, CancellationToken cancellationToken)
    {
        var request = new OpenSceneRequest { scenePath = scenePath };
        await client.PostAsync(ApiRoutes.SceneOpen, request, cancellationToken);
        return $"Scene opened: {scenePath}";
    }

    public async ValueTask<string> SaveAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync(ApiRoutes.SceneSave, cancellationToken);
        return "Scene saved successfully.";
    }

    public ValueTask<string> GetHierarchyAsync(CancellationToken cancellationToken)
    {
        return client.GetStringAsync(ApiRoutes.SceneHierarchy, cancellationToken);
    }
}
