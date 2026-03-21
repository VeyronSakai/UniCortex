using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class PrefabUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> CreateAsync(int instanceId, string assetPath,
        CancellationToken cancellationToken)
    {
        var request = new CreatePrefabRequest { instanceId = instanceId, assetPath = assetPath };
        await client.PostAsync(ApiRoutes.PrefabCreate, request, cancellationToken);
        return $"Prefab created at: {assetPath}";
    }

    public ValueTask<string> InstantiateAsync(string assetPath, CancellationToken cancellationToken)
    {
        var request = new InstantiatePrefabRequest { assetPath = assetPath };
        return client.PostAsync(ApiRoutes.PrefabInstantiate, request, cancellationToken);
    }
}
