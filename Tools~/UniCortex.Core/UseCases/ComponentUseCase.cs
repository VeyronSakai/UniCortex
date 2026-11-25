using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Extensions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ComponentUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> AddAsync(int instanceId, string componentType, string assemblyName,
        CancellationToken cancellationToken)
    {
        var request = new AddComponentRequest
        {
            instanceId = instanceId, componentType = componentType, assemblyName = assemblyName
        };
        await client.PostAsync<AddComponentRequest, AddComponentResponse>(ApiRoutes.ComponentAdd, request,
            cancellationToken);
        return $"Component '{componentType}' added successfully.";
    }

    public async ValueTask<string> RemoveAsync(int instanceId, string componentType, string assemblyName,
        int componentIndex = 0, CancellationToken cancellationToken = default)
    {
        var request = new RemoveComponentRequest
        {
            instanceId = instanceId,
            componentType = componentType,
            assemblyName = assemblyName,
            componentIndex = componentIndex
        };
        await client.PostAsync<RemoveComponentRequest, RemoveComponentResponse>(ApiRoutes.ComponentRemove, request,
            cancellationToken);
        return $"Component '{componentType}' removed successfully.";
    }

    public async ValueTask<string> GetPropertiesAsync(int instanceId, string componentType, string assemblyName,
        int componentIndex = 0, string? propertyPath = null, int? depth = null,
        CancellationToken cancellationToken = default)
    {
        var request = new GetComponentPropertiesRequest
        {
            instanceId = instanceId,
            componentType = componentType,
            assemblyName = assemblyName,
            componentIndex = componentIndex,
            // A null propertyPath or depth is left out of the query.
            propertyPath = propertyPath!,
            depth = depth
        };
        var response = await client.GetAsync<GetComponentPropertiesRequest, GetComponentPropertiesResponse>(
            ApiRoutes.ComponentProperties, request, cancellationToken);
        response.properties.OmitEmptyReferences();
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> SetPropertyAsync(int instanceId, string componentType, string assemblyName,
        string propertyPath, string value, CancellationToken cancellationToken = default)
    {
        var request = new SetComponentPropertyRequest
        {
            instanceId = instanceId,
            componentType = componentType,
            assemblyName = assemblyName,
            propertyPath = propertyPath,
            value = value
        };
        await client.PostAsync<SetComponentPropertyRequest, SetComponentPropertyResponse>(
            ApiRoutes.ComponentSetProperty, request, cancellationToken);
        return $"Property '{propertyPath}' set to '{value}' successfully.";
    }
}
