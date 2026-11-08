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
    private const string InstanceIdDescription = "instanceId of the GameObject.";

    private const string ComponentTypeDescription =
        "Fully qualified component type (e.g. \"UnityEngine.Rigidbody\").";

    private const string AssemblyNameDescription =
        "Assembly of the type (e.g. \"UnityEngine.PhysicsModule\" or \"Assembly-CSharp\").";

    private const string ComponentIndexDescription =
        "Index among components of the same type.";

    [McpServerTool(Name = "add_component", ReadOnly = false),
     Description("Add a component to a GameObject."), UsedImplicitly]
    public ValueTask<CallToolResult> AddComponentAsync(
        [Description(InstanceIdDescription)] int instanceId,
        [Description(ComponentTypeDescription)] string componentType,
        [Description(AssemblyNameDescription)] string assemblyName,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => componentUseCase.AddAsync(instanceId, componentType, assemblyName, ct), cancellationToken);

    [McpServerTool(Name = "remove_component", ReadOnly = false),
     Description("Remove a component from a GameObject."), UsedImplicitly]
    public ValueTask<CallToolResult> RemoveComponentAsync(
        [Description(InstanceIdDescription)] int instanceId,
        [Description(ComponentTypeDescription)] string componentType,
        [Description(AssemblyNameDescription)] string assemblyName,
        [Description(ComponentIndexDescription)]
        int componentIndex = 0,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => componentUseCase.RemoveAsync(instanceId, componentType, assemblyName, componentIndex, ct),
            cancellationToken);

    [McpServerTool(Name = "get_component_properties", ReadOnly = true),
     Description("Get serialized properties of a component on a GameObject."), UsedImplicitly]
    public ValueTask<CallToolResult> GetComponentPropertiesAsync(
        [Description(InstanceIdDescription)] int instanceId,
        [Description(ComponentTypeDescription)] string componentType,
        [Description(AssemblyNameDescription)] string assemblyName,
        [Description(ComponentIndexDescription)]
        int componentIndex = 0,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => componentUseCase.GetPropertiesAsync(instanceId, componentType, assemblyName, componentIndex, ct),
            cancellationToken);

    [McpServerTool(Name = "set_component_property", ReadOnly = false),
     Description("Set a serialized property of a component, including object references " +
                 "(e.g. PlayableDirector.m_PlayableAsset)."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetComponentPropertyAsync(
        [Description(InstanceIdDescription)] int instanceId,
        [Description(ComponentTypeDescription)] string componentType,
        [Description(AssemblyNameDescription)] string assemblyName,
        [Description("Serialized property path (e.g. m_LocalPosition.x).")]
        string propertyPath,
        [Description(PropertyValueDescriptions.Value)]
        string value,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => componentUseCase.SetPropertyAsync(instanceId, componentType, assemblyName, propertyPath, value, ct),
            cancellationToken);
}
