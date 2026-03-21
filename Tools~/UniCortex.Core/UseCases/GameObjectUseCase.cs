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
        var request = new ModifyGameObjectRequest
        {
            instanceId = instanceId,
            name = name,
            activeSelf = activeSelf,
            tag = tag,
            layer = layer,
            parentInstanceId = parentInstanceId
        };
        await client.PostAsync<ModifyGameObjectRequest, ModifyGameObjectResponse>(ApiRoutes.GameObjectModify, request,
            cancellationToken);
        return "GameObject modified successfully.";
    }
}
