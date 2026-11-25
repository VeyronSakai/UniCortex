using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class PrefabUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> CreateAsync(int instanceId, string assetPath, bool connect = false,
        CancellationToken cancellationToken = default)
    {
        var request = new CreatePrefabRequest { instanceId = instanceId, assetPath = assetPath, connect = connect };
        var response = await client.PostAsync<CreatePrefabRequest, CreatePrefabResponse>(ApiRoutes.PrefabCreate,
            request, cancellationToken);

        var message = $"Prefab created at: {assetPath} (GUID: {response.guid}).";
        if (connect)
        {
            message += $" GameObject {instanceId} is now an instance of it.";
        }

        if (!string.IsNullOrEmpty(response.modifiedPrefabStagePath))
        {
            message += $" The Prefab open in Prefab Mode ({response.modifiedPrefabStagePath}) now holds that " +
                       "instance; save it with save (CLI: editor save) to keep the change, unless Auto Save is on.";
        }

        return message;
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

    public async ValueTask<string> CloseAsync(bool discardChanges = false,
        CancellationToken cancellationToken = default)
    {
        var request = new ClosePrefabRequest { discardChanges = discardChanges };
        await client.PostAsync<ClosePrefabRequest, ClosePrefabResponse>(ApiRoutes.PrefabClose, request,
            cancellationToken);
        return discardChanges ? "Prefab mode closed without saving its changes." : "Prefab mode closed.";
    }

    public async ValueTask<string> UnpackAsync(int instanceId, string? mode = null,
        CancellationToken cancellationToken = default)
    {
        var unpackMode = string.IsNullOrEmpty(mode) ? PrefabUnpackModes.OutermostRoot : mode;
        var request = new UnpackPrefabInstanceRequest { instanceId = instanceId, mode = unpackMode };
        await client.PostAsync<UnpackPrefabInstanceRequest, UnpackPrefabInstanceResponse>(ApiRoutes.PrefabUnpack,
            request, cancellationToken);
        return $"Prefab instance unpacked ({unpackMode}): {instanceId}";
    }
}
