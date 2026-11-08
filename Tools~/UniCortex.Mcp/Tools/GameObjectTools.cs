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
         "Find GameObjects in all loaded scenes (DontDestroyOnLoad included in Play Mode; only the Prefab contents " +
         "in Prefab Mode) with the Hierarchy search syntax: words match names (partial, case-insensitive, all must " +
         "match), t:Type matches a component type (derived included; multiple t: are OR), and ref:<instanceId>: " +
         "finds objects referencing it. No wildcards. " +
         "Results include activeSelf, tag, layer, components and sceneName."),
     UsedImplicitly]
    public ValueTask<CallToolResult> FindGameObjectsAsync(
        [Description("Search query, e.g. 'Main Camera', 't:Camera', 'Button t:Image', 'ref:12345:'.")]
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
         "Create an empty GameObject in the current scene, optionally under a parent at a sibling index " +
         "and with a RectTransform for UI."),
     UsedImplicitly]
    public ValueTask<CallToolResult> CreateGameObjectAsync(
        [Description("Name of the GameObject.")] string name,
        [Description(
            "instanceId of the parent (local transform is reset, parent's layer inherited). Omit for the scene root.")]
        int? parentInstanceId = null,
        [Description(
            "Position among siblings (0 = first; omitted or out of range = last). For UI it sets the draw order.")]
        int? siblingIndex = null,
        [Description(
            "Use a RectTransform for UI (automatic when the parent has one). " +
            "Set anchors with set_component_property on the RectTransform.")]
        bool? useRectTransform = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameObjectUseCase.CreateAsync(name, parentInstanceId, siblingIndex, useRectTransform, ct), cancellationToken);

    [McpServerTool(Name = "delete_game_object", ReadOnly = false), Description("Delete a GameObject from the current scene."), UsedImplicitly]
    public ValueTask<CallToolResult> DeleteGameObjectAsync(
        [Description("instanceId of the GameObject.")]
        int instanceId,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameObjectUseCase.DeleteAsync(instanceId, ct), cancellationToken);

    [McpServerTool(Name = "modify_game_object", ReadOnly = false),
     Description(
         "Change a GameObject's name, active state, tag, layer, parent or sibling order; " +
         "only the given fields change."),
     UsedImplicitly]
    public ValueTask<CallToolResult> ModifyGameObjectAsync(
        [Description("instanceId of the GameObject.")]
        int instanceId,
        [Description("New name.")] string? name = null,
        [Description("Active state.")] bool? activeSelf = null,
        [Description("New tag.")] string? tag = null,
        [Description("New layer index.")] int? layer = null,
        [Description("instanceId of the new parent; 0 for the scene root.")]
        int? parentInstanceId = null,
        [Description(
            "New position among siblings (0 = first; out of range = last), " +
            "under the current parent or under parentInstanceId.")]
        int? siblingIndex = null,
        [Description(
            "When reparenting, keep the world transform (true, default) or the local values (false, " +
            "e.g. when moving between Canvases with different scales).")]
        bool? worldPositionStays = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameObjectUseCase.ModifyAsync(instanceId, name, activeSelf, tag, layer, parentInstanceId,
                siblingIndex, worldPositionStays, ct),
            cancellationToken);

    [McpServerTool(Name = "duplicate_game_object", ReadOnly = false),
     Description(
         "Duplicate a GameObject like the Editor's Duplicate (Ctrl+D): children and components are copied, " +
         "Prefab instances stay linked with their overrides, and the copy is placed right after the original."),
     UsedImplicitly]
    public ValueTask<CallToolResult> DuplicateGameObjectAsync(
        [Description("instanceId of the GameObject.")]
        int instanceId,
        [Description("Name of the copy. Omit for a unique name like 'Foo (1)'.")]
        string? name = null,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => gameObjectUseCase.DuplicateAsync(instanceId, name, ct), cancellationToken);
}
