using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ComponentUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> AddAsync(int instanceId, string componentType,
        CancellationToken cancellationToken)
    {
        var request = new AddComponentRequest { instanceId = instanceId, componentType = componentType };
        await client.PostAsync(ApiRoutes.ComponentAdd, request, cancellationToken);
        return $"Component '{componentType}' added successfully.";
    }

    public async ValueTask<string> RemoveAsync(int instanceId, string componentType,
        int componentIndex = 0, CancellationToken cancellationToken = default)
    {
        var request = new RemoveComponentRequest
        {
            instanceId = instanceId, componentType = componentType, componentIndex = componentIndex
        };
        await client.PostAsync(ApiRoutes.ComponentRemove, request, cancellationToken);
        return $"Component '{componentType}' removed successfully.";
    }

    public ValueTask<string> GetPropertiesAsync(int instanceId, string componentType,
        int componentIndex = 0, CancellationToken cancellationToken = default)
    {
        var route =
            $"{ApiRoutes.ComponentProperties}?instanceId={instanceId}&componentType={Uri.EscapeDataString(componentType)}&componentIndex={componentIndex}";
        return client.GetStringAsync(route, cancellationToken);
    }

    public async ValueTask<string> SetPropertyAsync(int instanceId, string componentType,
        string propertyPath, string value, CancellationToken cancellationToken = default)
    {
        var request = new SetComponentPropertyRequest
        {
            instanceId = instanceId,
            componentType = componentType,
            propertyPath = propertyPath,
            value = value
        };
        await client.PostAsync(ApiRoutes.ComponentSetProperty, request, cancellationToken);
        return $"Property '{propertyPath}' set to '{value}' successfully.";
    }
}
