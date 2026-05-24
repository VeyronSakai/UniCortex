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
    private const string ComponentTypeDescription =
        "The fully-qualified component type name including namespace (e.g. \"UnityEngine.Rigidbody\").";

    private const string AssemblyNameDescription =
        "The name of the assembly that defines the type (e.g. \"UnityEngine.PhysicsModule\" or \"Assembly-CSharp\").";

    [McpServerTool(Name = "add_component", ReadOnly = false),
     Description("Add a component to a GameObject. Supports Undo."), UsedImplicitly]
    public ValueTask<CallToolResult> AddComponentAsync(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description(ComponentTypeDescription)] string componentType,
        [Description(AssemblyNameDescription)] string assemblyName,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => componentUseCase.AddAsync(instanceId, componentType, assemblyName, ct), cancellationToken);

    [McpServerTool(Name = "remove_component", ReadOnly = false),
     Description("Remove a component from a GameObject. Supports Undo."), UsedImplicitly]
    public ValueTask<CallToolResult> RemoveComponentAsync(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description(ComponentTypeDescription)] string componentType,
        [Description(AssemblyNameDescription)] string assemblyName,
        [Description("Index when multiple components of same type exist. Defaults to 0.")]
        int componentIndex = 0,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => componentUseCase.RemoveAsync(instanceId, componentType, assemblyName, componentIndex, ct),
            cancellationToken);

    [McpServerTool(Name = "get_component_properties", ReadOnly = true),
     Description("Get serialized properties of a component on a GameObject."), UsedImplicitly]
    public ValueTask<CallToolResult> GetComponentPropertiesAsync(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description(ComponentTypeDescription)] string componentType,
        [Description(AssemblyNameDescription)] string assemblyName,
        [Description("Index when multiple components of same type exist. Defaults to 0.")]
        int componentIndex = 0,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => componentUseCase.GetPropertiesAsync(instanceId, componentType, assemblyName, componentIndex, ct),
            cancellationToken);

    [McpServerTool(Name = "set_component_property", ReadOnly = false),
     Description("Set a serialized property on a component. Uses SerializedProperty API with automatic Undo."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetComponentPropertyAsync(
        [Description("The instance ID of the GameObject.")] int instanceId,
        [Description(ComponentTypeDescription)] string componentType,
        [Description(AssemblyNameDescription)] string assemblyName,
        [Description("The property path (e.g. m_LocalPosition.x).")]
        string propertyPath,
        [Description("The value as a string. Type is auto-detected from the property.")]
        string value,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => componentUseCase.SetPropertyAsync(instanceId, componentType, assemblyName, propertyPath, value, ct),
            cancellationToken);
}
