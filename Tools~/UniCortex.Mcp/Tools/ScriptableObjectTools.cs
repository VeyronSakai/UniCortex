using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class ScriptableObjectTools(
    ScriptableObjectUseCase scriptableObjectUseCase,
    IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "create_scriptable_object", ReadOnly = false),
     Description(
         "Create a new ScriptableObject .asset file at the given path. Specify the ScriptableObject subclass by its full namespace-qualified name (e.g. \"MyNamespace.MyScriptableObject\")."),
     UsedImplicitly]
    public ValueTask<CallToolResult> CreateScriptableObjectAsync(
        [Description(
            "The full namespace-qualified type name of the ScriptableObject subclass (e.g. \"MyNamespace.MyScriptableObject\"). When the same type name exists in multiple loaded assemblies, pass an assembly-qualified name to disambiguate (e.g. \"MyNamespace.MyScriptableObject, Assembly-CSharp\").")]
        string typeName,
        [Description(
            "The asset path where the ScriptableObject should be saved (e.g. \"Assets/Data/MyData.asset\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => scriptableObjectUseCase.CreateAsync(typeName, assetPath, ct), cancellationToken);

    [McpServerTool(Name = "get_scriptable_object_properties", ReadOnly = true),
     Description(
         "Get the list of serialized properties (path, type, value) of a ScriptableObject .asset file."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetScriptableObjectPropertiesAsync(
        [Description("The asset path of the ScriptableObject (e.g. \"Assets/Data/MyData.asset\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => scriptableObjectUseCase.GetPropertiesAsync(assetPath, ct), cancellationToken);

    [McpServerTool(Name = "set_scriptable_object_property", ReadOnly = false),
     Description(
         "Set a serialized property value on a ScriptableObject .asset file. Property path and value format follow the same conventions as set_component_property."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetScriptableObjectPropertyAsync(
        [Description("The asset path of the ScriptableObject (e.g. \"Assets/Data/MyData.asset\").")]
        string assetPath,
        [Description("The property path to set (e.g. \"m_Speed\" or \"items.Array.data[0].name\").")]
        string propertyPath,
        [Description("The new value as a string (e.g. \"42\", \"true\", \"(1, 0, 0)\").")]
        string value,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => scriptableObjectUseCase.SetPropertyAsync(assetPath, propertyPath, value, ct),
            cancellationToken);
}
