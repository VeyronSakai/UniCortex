using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class GameObjectTools(GameObjectUseCase gameObjectUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "find_game_objects", ReadOnly = true),
     Description(
         "Find GameObjects in the current scene by name, tag, or component type. " +
         "Supports Unity Search style query syntax: plain text for name (partial match), " +
         "t:Type for component type, tag:partial or tag=exact for tag, id:N for instance ID, " +
         "layer:N for layer, path:A/B for hierarchy path, is:root/child/leaf/static for state filters. " +
         "Multiple tokens can be combined: 'Camera t:Camera layer:0'."),
     UsedImplicitly]
    public ValueTask<CallToolResult> FindGameObjectsAsync(
        [Description(
            "Search query. Examples: 'Main Camera', 't:Camera', 'tag=Player', 'id:12345', 'is:root', 'path:Canvas/Button'. " +
            "Multiple tokens can be combined: 'Camera t:Camera layer:0'.")]
        string query,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentException("query is required. Use get_hierarchy to list all GameObjects.");
            }

            return McpToolExecution.CreateTextResult(await gameObjectUseCase.FindAsync(query, ct));
        }, cancellationToken);

    [McpServerTool(Name = "create_game_object", ReadOnly = false),
     Description(
         "Create a new empty GameObject in the current scene, optionally under a parent at a specific sibling " +
         "position. For UI, the object can be created with a RectTransform. Supports Undo."),
     UsedImplicitly]
    public ValueTask<CallToolResult> CreateGameObjectAsync(
        [Description("Name of the GameObject to create.")] string name,
        [Description(
            "Instance ID of the parent GameObject. The new object's local transform is reset and it inherits the " +
            "parent's layer. If omitted, the object is created at the scene root.")]
        int? parentInstanceId = null,
        [Description(
            "Position among siblings (0 = first). For UI, sibling order determines draw order. " +
            "Values beyond the last sibling place it last. If omitted, the object is placed last.")]
        int? siblingIndex = null,
        [Description(
            "Create the object with a RectTransform for UI. Automatically applied when the parent has a RectTransform. " +
            "To adjust anchors (e.g. stretch to fill the parent), use set_component_property on the RectTransform.")]
        bool? useRectTransform = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameObjectUseCase.CreateAsync(name, parentInstanceId, siblingIndex, useRectTransform, ct), cancellationToken);

    [McpServerTool(Name = "delete_game_object", ReadOnly = false), Description("Remove a GameObject from the current scene by its instance ID. Supports Undo."), UsedImplicitly]
    public ValueTask<CallToolResult> DeleteGameObjectAsync(
        [Description("The instance ID of the GameObject to delete.")]
        int instanceId,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameObjectUseCase.DeleteAsync(instanceId, ct), cancellationToken);

    [McpServerTool(Name = "modify_game_object", ReadOnly = false),
     Description(
         "Modify a GameObject's properties (name, active state, tag, layer, parent, sibling order). " +
         "Only specified fields are changed. Supports Undo."),
     UsedImplicitly]
    public ValueTask<CallToolResult> ModifyGameObjectAsync(
        [Description("The instance ID of the GameObject to modify.")]
        int instanceId,
        [Description("New name for the GameObject.")] string? name = null,
        [Description("Set active state (true/false).")] bool? activeSelf = null,
        [Description("New tag for the GameObject.")] string? tag = null,
        [Description("New layer index.")] int? layer = null,
        [Description("Instance ID of the new parent. Use 0 to move to root.")]
        int? parentInstanceId = null,
        [Description(
            "New position among siblings (0 = first). Can be used without parentInstanceId to reorder within the " +
            "current parent, or together with it to place the object at a specific position under the new parent. " +
            "Values beyond the last sibling place it last.")]
        int? siblingIndex = null,
        [Description(
            "When changing the parent, keep the world position/rotation/scale (true, default) or keep the local " +
            "values (false). Use false when moving between parents with different scales (e.g. another Canvas) " +
            "or when the local values should be kept as-is.")]
        bool? worldPositionStays = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameObjectUseCase.ModifyAsync(instanceId, name, activeSelf, tag, layer, parentInstanceId,
                siblingIndex, worldPositionStays, ct),
            cancellationToken);

    [McpServerTool(Name = "duplicate_game_object", ReadOnly = false),
     Description(
         "Duplicate a GameObject in the current scene, deep-copying its children and components and placing the " +
         "copy as a sibling right after the original. Supports Undo."),
     UsedImplicitly]
    public ValueTask<CallToolResult> DuplicateGameObjectAsync(
        [Description("The instance ID of the GameObject to duplicate.")]
        int instanceId,
        [Description("Optional name for the duplicate. If omitted, a Unity-style unique name like 'Foo (1)' is assigned.")]
        string? name = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameObjectUseCase.DuplicateAsync(instanceId, name, ct), cancellationToken);
}
