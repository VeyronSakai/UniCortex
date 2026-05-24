using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class ComponentTools(ComponentUseCase componentUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "add_component", ReadOnly = false),
     Description("Add a component to a GameObject. Supports Undo."), UsedImplicitly]
    public ValueTask<CallToolResult> AddComponentAsync(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description("The fully-qualified component type name (e.g. \"UnityEngine.Rigidbody\"). When the same type name exists in multiple loaded assemblies, pass an assembly-qualified name to disambiguate (e.g. \"MyNamespace.Foo, Assembly-CSharp\").")]
        string componentType,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => componentUseCase.AddAsync(instanceId, componentType, ct), cancellationToken);

    [McpServerTool(Name = "remove_component", ReadOnly = false),
     Description("Remove a component from a GameObject. Supports Undo."), UsedImplicitly]
    public ValueTask<CallToolResult> RemoveComponentAsync(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description("The fully-qualified component type name (e.g. \"UnityEngine.Rigidbody\"). When the same type name exists in multiple loaded assemblies, pass an assembly-qualified name to disambiguate (e.g. \"MyNamespace.Foo, Assembly-CSharp\").")]
        string componentType,
        [Description("Index when multiple components of same type exist. Defaults to 0.")]
        int componentIndex = 0,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => componentUseCase.RemoveAsync(instanceId, componentType, componentIndex, ct),
            cancellationToken);

    [McpServerTool(Name = "get_component_properties", ReadOnly = true),
     Description("Get serialized properties of a component on a GameObject."), UsedImplicitly]
    public ValueTask<CallToolResult> GetComponentPropertiesAsync(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description("The fully-qualified component type name (e.g. \"UnityEngine.Transform\"). When the same type name exists in multiple loaded assemblies, pass an assembly-qualified name to disambiguate (e.g. \"MyNamespace.Foo, Assembly-CSharp\").")]
        string componentType,
        [Description("Index when multiple components of same type exist. Defaults to 0.")]
        int componentIndex = 0,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => componentUseCase.GetPropertiesAsync(instanceId, componentType, componentIndex, ct),
            cancellationToken);

    [McpServerTool(Name = "set_component_property", ReadOnly = false),
     Description("Set a serialized property on a component. Uses SerializedProperty API with automatic Undo."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetComponentPropertyAsync(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description("The fully-qualified component type name (e.g. \"UnityEngine.Transform\"). When the same type name exists in multiple loaded assemblies, pass an assembly-qualified name to disambiguate (e.g. \"MyNamespace.Foo, Assembly-CSharp\").")]
        string componentType,
        [Description("The property path (e.g. m_LocalPosition.x).")]
        string propertyPath,
        [Description("The value as a string. Type is auto-detected from the property.")]
        string value,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => componentUseCase.SetPropertyAsync(instanceId, componentType, propertyPath, value, ct),
            cancellationToken);
}
