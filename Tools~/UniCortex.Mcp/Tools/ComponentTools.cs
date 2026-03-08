using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Services;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class ComponentTools(ComponentService componentService)
{
    [McpServerTool(Name = "add_component", ReadOnly = false),
     Description("Add a component to a GameObject. Supports Undo."), UsedImplicitly]
    public async ValueTask<CallToolResult> AddComponentAsync(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description("The fully-qualified component type name including namespace (e.g. UnityEngine.Rigidbody, UnityEngine.BoxCollider).")]
        string componentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await componentService.AddAsync(instanceId, componentType, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "remove_component", ReadOnly = false),
     Description("Remove a component from a GameObject. Supports Undo."), UsedImplicitly]
    public async ValueTask<CallToolResult> RemoveComponentAsync(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description("The fully-qualified component type name including namespace (e.g. UnityEngine.Rigidbody).")]
        string componentType,
        [Description("Index when multiple components of same type exist. Defaults to 0.")]
        int componentIndex = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await componentService.RemoveAsync(instanceId, componentType, componentIndex,
                cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "get_component_properties", ReadOnly = true),
     Description("Get serialized properties of a component on a GameObject."), UsedImplicitly]
    public async ValueTask<CallToolResult> GetComponentPropertiesAsync(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description("The fully-qualified component type name including namespace (e.g. UnityEngine.Transform, UnityEngine.Rigidbody).")]
        string componentType,
        [Description("Index when multiple components of same type exist. Defaults to 0.")]
        int componentIndex = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await componentService.GetPropertiesAsync(instanceId, componentType, componentIndex,
                cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = json }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "set_component_property", ReadOnly = false),
     Description("Set a serialized property on a component. Uses SerializedProperty API with automatic Undo."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> SetComponentPropertyAsync(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description("The fully-qualified component type name including namespace (e.g. UnityEngine.Transform).")] string componentType,
        [Description("The property path (e.g. m_LocalPosition.x).")]
        string propertyPath,
        [Description("The value as a string. Type is auto-detected from the property.")]
        string value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await componentService.SetPropertyAsync(instanceId, componentType, propertyPath,
                value, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
