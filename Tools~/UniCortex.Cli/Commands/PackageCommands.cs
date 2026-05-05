using System.Text.Json;
using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class PackageCommands(PackageManagerUseCase packageManagerUseCase)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true
    };

    /// <summary>List packages in the Unity project.</summary>
    /// <param name="offlineMode">Use the local package cache. Defaults to true.</param>
    /// <param name="includeIndirectDependencies">Include indirect dependencies. Defaults to false.</param>
    [Command("list")]
    public async Task List(
        bool offlineMode = true,
        bool includeIndirectDependencies = false,
        CancellationToken cancellationToken = default)
    {
        var response = await packageManagerUseCase.ListAsync(
            offlineMode,
            includeIndirectDependencies,
            cancellationToken);
        Console.WriteLine(JsonSerializer.Serialize(response, s_jsonOptions));
    }

    /// <summary>Search the Unity package registry for a package.</summary>
    /// <param name="packageIdOrName">The package name or ID to search for.</param>
    /// <param name="offlineMode">Use the local package cache. Defaults to false.</param>
    [Command("search")]
    public async Task Search(
        [Argument] string packageIdOrName,
        bool offlineMode = false,
        CancellationToken cancellationToken = default)
    {
        var response = await packageManagerUseCase.SearchAsync(packageIdOrName, offlineMode, cancellationToken);
        Console.WriteLine(JsonSerializer.Serialize(response, s_jsonOptions));
    }

    /// <summary>Add a package dependency using a Unity Package Manager identifier.</summary>
    /// <param name="identifier">Identifier accepted by PackageManager.Client.Add, such as com.foo@1.2.3, a Git URL, or file:/path.</param>
    [Command("add")]
    public async Task Add([Argument] string identifier, CancellationToken cancellationToken = default)
    {
        var package = await packageManagerUseCase.AddAsync(identifier, cancellationToken);
        Console.WriteLine($"Package added: {package.packageId}");
    }

    /// <summary>Remove a direct package dependency by package name.</summary>
    /// <param name="packageName">The package name to remove, such as com.unity.timeline.</param>
    [Command("remove")]
    public async Task Remove([Argument] string packageName, CancellationToken cancellationToken = default)
    {
        await packageManagerUseCase.RemoveAsync(packageName, cancellationToken);
        Console.WriteLine($"Package removed: {packageName}");
    }

    /// <summary>Force Unity Package Manager to resolve packages.</summary>
    [Command("resolve")]
    public async Task Resolve(CancellationToken cancellationToken = default)
    {
        await packageManagerUseCase.ResolveAsync(cancellationToken);
        Console.WriteLine("Packages resolved.");
    }
}
