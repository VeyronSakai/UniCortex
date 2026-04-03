using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class PrefabUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> CreateAsync(int instanceId, string assetPath,
        CancellationToken cancellationToken)
    {
        var request = new CreatePrefabRequest { instanceId = instanceId, assetPath = assetPath };
        await client.PostAsync<CreatePrefabRequest, CreatePrefabResponse>(ApiRoutes.PrefabCreate, request,
            cancellationToken);
        return $"Prefab created at: {assetPath}";
    }

    public async ValueTask<string> InstantiateAsync(string assetPath, CancellationToken cancellationToken)
    {
        var request = new InstantiatePrefabRequest { assetPath = assetPath };
        var response = await client.PostAsync<InstantiatePrefabRequest, InstantiatePrefabResponse>(
            ApiRoutes.PrefabInstantiate, request, cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> OpenAsync(string assetPath, CancellationToken cancellationToken)
    {
        var request = new OpenPrefabRequest { assetPath = assetPath };
        await client.PostAsync<OpenPrefabRequest, OpenPrefabResponse>(ApiRoutes.PrefabOpen, request,
            cancellationToken);
        return $"Prefab opened: {assetPath}";
    }

    public async ValueTask<string> CloseAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<object, ClosePrefabResponse>(ApiRoutes.PrefabClose, null, cancellationToken);
        return "Prefab mode closed.";
    }

}
