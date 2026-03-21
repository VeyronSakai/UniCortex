using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ComponentUseCase(IUnityEditorClient client)
{
    public ValueTask<string> AddAsync(int instanceId, string componentType,
        CancellationToken cancellationToken)
    {
        var request = new AddComponentRequest { instanceId = instanceId, componentType = componentType };
        return client.PostAsync(ApiRoutes.ComponentAdd, request,
            $"Component '{componentType}' added successfully.", cancellationToken);
    }

    public ValueTask<string> RemoveAsync(int instanceId, string componentType,
        int componentIndex = 0, CancellationToken cancellationToken = default)
    {
        var request = new RemoveComponentRequest
        {
            instanceId = instanceId, componentType = componentType, componentIndex = componentIndex
        };
        return client.PostAsync(ApiRoutes.ComponentRemove, request,
            $"Component '{componentType}' removed successfully.", cancellationToken);
    }

    public ValueTask<string> GetPropertiesAsync(int instanceId, string componentType,
        int componentIndex = 0, CancellationToken cancellationToken = default)
    {
        var route =
            $"{ApiRoutes.ComponentProperties}?instanceId={instanceId}&componentType={Uri.EscapeDataString(componentType)}&componentIndex={componentIndex}";
        return client.GetStringAsync(route, cancellationToken);
    }

    public ValueTask<string> SetPropertyAsync(int instanceId, string componentType,
        string propertyPath, string value, CancellationToken cancellationToken = default)
    {
        var request = new SetComponentPropertyRequest
        {
            instanceId = instanceId,
            componentType = componentType,
            propertyPath = propertyPath,
            value = value
        };
        return client.PostAsync(ApiRoutes.ComponentSetProperty, request,
            $"Property '{propertyPath}' set to '{value}' successfully.", cancellationToken);
    }
}
