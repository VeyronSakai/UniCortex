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
     Description("Create a new ScriptableObject .asset file at the given path."),
     UsedImplicitly]
    public ValueTask<CallToolResult> CreateScriptableObjectAsync(
        [Description("Fully qualified ScriptableObject subclass (e.g. \"MyNamespace.MyScriptableObject\").")]
        string typeName,
        [Description("Assembly of the type (e.g. \"Assembly-CSharp\").")]
        string assemblyName,
        [Description("Asset path to save to (e.g. \"Assets/Data/MyData.asset\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => scriptableObjectUseCase.CreateAsync(typeName, assemblyName, assetPath, ct), cancellationToken);

    [McpServerTool(Name = "get_scriptable_object_properties", ReadOnly = true),
     Description(
         "Get the list of serialized properties (path, type, value) of a ScriptableObject .asset file."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetScriptableObjectPropertiesAsync(
        [Description("ScriptableObject asset path (e.g. \"Assets/Data/MyData.asset\").")]
        string assetPath,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => scriptableObjectUseCase.GetPropertiesAsync(assetPath, ct), cancellationToken);

    [McpServerTool(Name = "set_scriptable_object_property", ReadOnly = false),
     Description(
         "Set a serialized property of a ScriptableObject .asset file. Supports Undo."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetScriptableObjectPropertyAsync(
        [Description("ScriptableObject asset path (e.g. \"Assets/Data/MyData.asset\").")]
        string assetPath,
        [Description("Serialized property path (e.g. \"m_Speed\" or \"items.Array.data[0].name\").")]
        string propertyPath,
        [Description(PropertyValueDescriptions.Value)]
        string value,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => scriptableObjectUseCase.SetPropertyAsync(assetPath, propertyPath, value, ct),
            cancellationToken);
}
