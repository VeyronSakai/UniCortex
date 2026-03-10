using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class SceneTools(SceneUseCase sceneService)
{
    [McpServerTool(Name = "create_scene", ReadOnly = false),
     Description("Create a new empty scene and save it at the specified asset path."), UsedImplicitly]
    public async ValueTask<CallToolResult> CreateSceneAsync(
        [Description("The asset path where the scene should be saved (e.g. \"Assets/Scenes/NewScene.unity\").")]
        string scenePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await sceneService.CreateAsync(scenePath, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "open_scene", ReadOnly = false),
     Description("Open a scene in the Unity Editor by its asset path."), UsedImplicitly]
    public async ValueTask<CallToolResult> OpenSceneAsync(
        [Description("The asset path of the scene to open (e.g. \"Assets/Scenes/Main.unity\").")]
        string scenePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await sceneService.OpenAsync(scenePath, cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "save_scene", ReadOnly = false),
     Description("Save all open scenes in the Unity Editor."), UsedImplicitly]
    public async ValueTask<CallToolResult> SaveSceneAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await sceneService.SaveAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = message }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }

    [McpServerTool(Name = "get_scene_hierarchy", ReadOnly = true),
     Description("Get the GameObject hierarchy of the current scene in the Unity Editor."), UsedImplicitly]
    public async ValueTask<CallToolResult> GetSceneHierarchyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await sceneService.GetHierarchyAsync(cancellationToken);
            return new CallToolResult { Content = [new TextContentBlock { Text = json }] };
        }
        catch (Exception ex)
        {
            return ToolErrorHandling.CreateErrorResult(ex);
        }
    }
}
