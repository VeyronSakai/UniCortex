using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class GameObjectTools(GameObjectUseCase gameObjectUseCase)
{
    [McpServerTool(Name = "find_game_objects", ReadOnly = true),
     Description(
         "Find GameObjects in the current scene by name, tag, or component type. " +
         "Supports Unity Search style query syntax: plain text for name (partial match), " +
         "t:Type for component type, tag:partial or tag=exact for tag, id:N for instance ID, " +
         "layer:N for layer, path:A/B for hierarchy path, is:root/child/leaf/static for state filters. " +
         "Multiple tokens can be combined: 'Camera t:Camera layer:0'."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> FindGameObjectsAsync(
        [Description(
            "Search query. Examples: 'Main Camera', 't:Camera', 'tag=Player', 'id:12345', 'is:root', 'path:Canvas/Button'. " +
            "Multiple tokens can be combined: 'Camera t:Camera layer:0'.")]
        string query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(query))
            {
                return ToolErrorHandling.CreateErrorResult(
                    new ArgumentException("query is required. Use get_hierarchy to list all GameObjects."));
            }

            var json = await gameObjectUseCase.FindAsync(query, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = json }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "create_game_object", ReadOnly = false),
     Description("Create a new empty GameObject in the current scene."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> CreateGameObjectAsync(
        [Description("Name of the GameObject to create.")] string name,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await gameObjectUseCase.CreateAsync(name, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = json }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "delete_game_object", ReadOnly = false), Description("Remove a GameObject from the current scene by its instance ID. Supports Undo."), UsedImplicitly]
    public async ValueTask<CallToolResult> DeleteGameObjectAsync(
        [Description("The instance ID of the GameObject to delete.")]
        int instanceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await gameObjectUseCase.DeleteAsync(instanceId, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "modify_game_object", ReadOnly = false),
     Description(
         "Modify a GameObject's properties (name, active state, tag, layer, parent). Only specified fields are changed."),
     UsedImplicitly]
    public async ValueTask<CallToolResult> ModifyGameObjectAsync(
        [Description("The instance ID of the GameObject to modify.")]
        int instanceId,
        [Description("New name for the GameObject.")] string? name = null,
        [Description("Set active state (true/false).")] bool? activeSelf = null,
        [Description("New tag for the GameObject.")] string? tag = null,
        [Description("New layer index.")] int? layer = null,
        [Description("Instance ID of the new parent. Use 0 to move to root.")]
        int? parentInstanceId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await gameObjectUseCase.ModifyAsync(instanceId, name, activeSelf, tag, layer,
                parentInstanceId, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
