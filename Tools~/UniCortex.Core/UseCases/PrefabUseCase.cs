using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class PrefabUseCase(IUnityEditorClient client)
{
    public ValueTask<string> CreateAsync(int instanceId, string assetPath,
        CancellationToken cancellationToken)
    {
        var request = new CreatePrefabRequest { instanceId = instanceId, assetPath = assetPath };
        return client.PostAsync(ApiRoutes.PrefabCreate, request, $"Prefab created at: {assetPath}",
            cancellationToken);
    }

    public ValueTask<string> InstantiateAsync(string assetPath, CancellationToken cancellationToken)
    {
        var request = new InstantiatePrefabRequest { assetPath = assetPath };
        return client.PostAndReadAsync(ApiRoutes.PrefabInstantiate, request, cancellationToken);
    }
}
