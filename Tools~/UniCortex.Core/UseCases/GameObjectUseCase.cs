using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class GameObjectUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> FindAsync(string? query, CancellationToken cancellationToken)
    {
        var request = !string.IsNullOrEmpty(query) ? new FindGameObjectsRequest { query = query } : null;
        var response = await client.GetAsync<FindGameObjectsRequest, FindGameObjectsResponse>(
            ApiRoutes.GameObjects, request, cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> CreateAsync(string name, CancellationToken cancellationToken)
    {
        var request = new CreateGameObjectRequest { name = name };
        var response = await client.PostAsync<CreateGameObjectRequest, CreateGameObjectResponse>(
            ApiRoutes.GameObjectCreate, request, cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> DeleteAsync(int instanceId, CancellationToken cancellationToken)
    {
        var request = new DeleteGameObjectRequest { instanceId = instanceId };
        await client.PostAsync<DeleteGameObjectRequest, DeleteGameObjectResponse>(ApiRoutes.GameObjectDelete, request,
            cancellationToken);
        return $"GameObject {instanceId} deleted.";
    }

    public async ValueTask<string> ModifyAsync(int instanceId, string? name = null, bool? activeSelf = null,
        string? tag = null, int? layer = null, int? parentInstanceId = null,
        CancellationToken cancellationToken = default)
    {
        // Use Dictionary instead of the shared ModifyGameObjectRequest DTO because
        // Unity's JsonUtility does not support Nullable<T>. The shared DTO uses
        // non-nullable value types (bool, int), so serializing it would always
        // include default values (false, 0) for unset fields. The Unity-side handler
        // detects field presence via string matching, which would misinterpret these
        // defaults as intentionally provided values.
        var fields = new Dictionary<string, object> { ["instanceId"] = instanceId };
        if (name != null) fields["name"] = name;
        if (activeSelf.HasValue) fields["activeSelf"] = activeSelf.Value;
        if (tag != null) fields["tag"] = tag;
        if (layer.HasValue) fields["layer"] = layer.Value;
        if (parentInstanceId.HasValue) fields["parentInstanceId"] = parentInstanceId.Value;

        await client.PostAsync<Dictionary<string, object>, ModifyGameObjectResponse>(ApiRoutes.GameObjectModify, fields,
            cancellationToken);
        return "GameObject modified successfully.";
    }
}
