using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class ProjectSettingsTools(ProjectSettingsUseCase projectSettingsUseCase, IAsyncOperationSequencer sequencer)
{
    [McpServerTool(Name = "get_project_settings", ReadOnly = true),
     Description("Get serialized properties of a ProjectSettings category (e.g. Player, Quality, Editor, Physics, Tags, Time, Audio, Graphics)."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetProjectSettingsAsync(
        [Description("The ProjectSettings category name (e.g. Player, Quality, Editor, Physics, Physics2D, Tags, Time, Audio, Graphics, Input, VFX, Navigation, Memory). Call list_project_settings_categories for the full list.")]
        string category,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => projectSettingsUseCase.GetAsync(category, ct), cancellationToken);

    [McpServerTool(Name = "set_project_setting", ReadOnly = false),
     Description("Set a serialized property on a ProjectSettings category. Uses SerializedProperty API with automatic Undo."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SetProjectSettingAsync(
        [Description("The ProjectSettings category name (e.g. Player, Quality, Time). Call list_project_settings_categories for the full list.")]
        string category,
        [Description("The serialized property path (e.g. m_TimeScale). Call get_project_settings to discover available paths.")]
        string propertyPath,
        [Description("The value as a string. Type is auto-detected from the property.")]
        string value,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            ct => projectSettingsUseCase.SetAsync(category, propertyPath, value, ct), cancellationToken);

    [McpServerTool(Name = "list_project_settings_categories", ReadOnly = true),
     Description("List the available ProjectSettings category names and their backing asset paths."),
     UsedImplicitly]
    public ValueTask<CallToolResult> ListProjectSettingsCategoriesAsync(
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteTextAsync(sequencer,
            projectSettingsUseCase.ListCategoriesAsync, cancellationToken);
}
