using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class CodeCommands(CodeGraphUseCase codeGraphUseCase)
{
    /// <summary>Print an overview of the project's C# code (assemblies, namespaces, Unity types, hotspots).</summary>
    [Command("map")]
    public async Task Map(CancellationToken cancellationToken = default)
    {
        var json = await codeGraphUseCase.GetCodeMapAsync(cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Search C# symbols by name pattern ('*'/'?' wildcards or substring).</summary>
    /// <param name="namePattern">Name pattern (e.g. "On*", "*UseCase", "Player").</param>
    /// <param name="kinds">Comma-separated kinds: Class, Struct, Interface, Enum, Delegate, Method, Constructor, Property, Field, Event.</param>
    /// <param name="filePattern">Project-relative file path pattern (e.g. "Assets/Scripts/*").</param>
    /// <param name="baseType">Only types deriving from this base (e.g. "MonoBehaviour").</param>
    /// <param name="limit">Maximum number of results.</param>
    [Command("search")]
    public async Task Search(
        [Argument] string namePattern,
        string? kinds = null,
        string? filePattern = null,
        string? baseType = null,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var json = await codeGraphUseCase.SearchSymbolsAsync(
            namePattern, kinds, filePattern, baseType, limit, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Print the source code of a symbol by its id.</summary>
    /// <param name="symbol">Symbol id (e.g. "MyNamespace.MyType.MyMethod(int)"). Unique suffixes are accepted.</param>
    [Command("snippet")]
    public async Task Snippet([Argument] string symbol, CancellationToken cancellationToken = default)
    {
        var json = await codeGraphUseCase.GetCodeSnippetAsync(symbol, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Find references to a symbol, grouped by relation (calls, uses, inheritedBy, ...).</summary>
    /// <param name="symbol">Symbol id. Unique suffixes are accepted.</param>
    /// <param name="limit">Maximum number of reference sites.</param>
    [Command("refs")]
    public async Task Refs([Argument] string symbol, int limit = 100, CancellationToken cancellationToken = default)
    {
        var json = await codeGraphUseCase.FindReferencesAsync(symbol, limit, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Trace the call graph from a method (callers or callees).</summary>
    /// <param name="symbol">Symbol id of the method. Unique suffixes are accepted.</param>
    /// <param name="direction">"callers" or "callees".</param>
    /// <param name="maxDepth">Traversal depth 1-5.</param>
    [Command("trace")]
    public async Task Trace(
        [Argument] string symbol,
        string direction = "callers",
        int maxDepth = 3,
        CancellationToken cancellationToken = default)
    {
        var json = await codeGraphUseCase.TraceCallGraphAsync(symbol, direction, maxDepth, cancellationToken);
        Console.WriteLine(json);
    }
}
