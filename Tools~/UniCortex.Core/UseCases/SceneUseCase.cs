using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class SceneUseCase(IUnityEditorClient client)
{
    public ValueTask<string> CreateAsync(string scenePath, CancellationToken cancellationToken)
    {
        var request = new CreateSceneRequest { scenePath = scenePath };
        return client.PostAsync(ApiRoutes.SceneCreate, request, $"Scene created: {scenePath}",
            cancellationToken);
    }

    public ValueTask<string> OpenAsync(string scenePath, CancellationToken cancellationToken)
    {
        var request = new OpenSceneRequest { scenePath = scenePath };
        return client.PostAsync(ApiRoutes.SceneOpen, request, $"Scene opened: {scenePath}",
            cancellationToken);
    }

    public ValueTask<string> SaveAsync(CancellationToken cancellationToken)
    {
        return client.PostEmptyAsync(ApiRoutes.SceneSave, "Scene saved successfully.", cancellationToken);
    }

    public ValueTask<string> GetHierarchyAsync(CancellationToken cancellationToken)
    {
        return client.GetStringAsync(ApiRoutes.SceneHierarchy, cancellationToken);
    }
}
