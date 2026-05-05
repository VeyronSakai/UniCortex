using System.ComponentModel;
using System.Text.Json;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class PackageManagerTools(PackageManagerUseCase packageManagerUseCase, IAsyncOperationSequencer sequencer)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true
    };

    [McpServerTool(Name = "list_unity_packages", ReadOnly = true),
     Description("List packages in the Unity project. Defaults to offline mode and direct dependencies only."),
     UsedImplicitly]
    public ValueTask<CallToolResult> ListPackagesAsync(
        [Description("Use the local package cache. Defaults to true.")]
        bool offlineMode = true,
        [Description("Include indirect dependencies. Defaults to false.")]
        bool includeIndirectDependencies = false,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            var response = await packageManagerUseCase.ListAsync(
                offlineMode,
                includeIndirectDependencies,
                ct);
            return McpToolExecution.CreateTextResult(JsonSerializer.Serialize(response, s_jsonOptions));
        }, cancellationToken);

    [McpServerTool(Name = "search_unity_package", ReadOnly = true),
     Description("Search the Unity package registry for a package by name or package ID."),
     UsedImplicitly]
    public ValueTask<CallToolResult> SearchPackageAsync(
        [Description("The package name or ID to search for.")]
        string packageIdOrName,
        [Description("Use the local package cache. Defaults to false.")]
        bool offlineMode = false,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            var response = await packageManagerUseCase.SearchAsync(packageIdOrName, offlineMode, ct);
            return McpToolExecution.CreateTextResult(JsonSerializer.Serialize(response, s_jsonOptions));
        }, cancellationToken);

    [McpServerTool(Name = "add_unity_package", ReadOnly = false),
     Description("Add a Unity package dependency using a PackageManager.Client.Add identifier, such as com.foo@1.2.3, a Git URL, or file:/path."),
     UsedImplicitly]
    public ValueTask<CallToolResult> AddPackageAsync(
        [Description("Identifier accepted by PackageManager.Client.Add, such as com.foo@1.2.3, a Git URL, or file:/path.")]
        string identifier,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            var package = await packageManagerUseCase.AddAsync(identifier, ct);
            return McpToolExecution.CreateTextResult(JsonSerializer.Serialize(package, s_jsonOptions));
        }, cancellationToken);

    [McpServerTool(Name = "remove_unity_package", ReadOnly = false),
     Description("Remove a direct Unity package dependency by package name."),
     UsedImplicitly]
    public ValueTask<CallToolResult> RemovePackageAsync(
        [Description("The package name to remove, such as com.unity.timeline.")]
        string packageName,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            await packageManagerUseCase.RemoveAsync(packageName, ct);
            return McpToolExecution.CreateTextResult($"Package removed: {packageName}");
        }, cancellationToken);

    [McpServerTool(Name = "resolve_unity_packages", ReadOnly = false),
     Description("Force Unity Package Manager to resolve packages."),
     UsedImplicitly]
    public ValueTask<CallToolResult> ResolvePackagesAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteAsync(sequencer, async ct =>
        {
            await packageManagerUseCase.ResolveAsync(ct);
            return McpToolExecution.CreateTextResult("Packages resolved.");
        }, cancellationToken);
}
