using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

[McpServerToolType, UsedImplicitly]
public class CodeGraphTools(CodeGraphUseCase codeGraphUseCase)
{
    [McpServerTool(Name = "get_code_map", ReadOnly = true),
     Description("Get an overview of the Unity project's C# code: assemblies, namespaces, " +
                 "Unity type counts (MonoBehaviour, ScriptableObject, ...) and the most referenced " +
                 "symbols (hotspots). Built from a Roslyn index of the project sources; works even " +
                 "while the Unity Editor is closed. Prefer the code graph tools over grep/file " +
                 "reads when exploring code structure."), UsedImplicitly]
    public ValueTask<CallToolResult> GetCodeMapAsync(CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteLocalTextAsync(codeGraphUseCase.GetCodeMapAsync, cancellationToken);

    [McpServerTool(Name = "search_symbols", ReadOnly = true),
     Description("Search C# symbols (types and members) in the Unity project by name pattern, " +
                 "optionally filtered by kind, file path and base type. Returns compact rows with " +
                 "symbol id, kind, assembly and location. Cheaper and more precise than grep for " +
                 "finding declarations."), UsedImplicitly]
    public ValueTask<CallToolResult> SearchSymbolsAsync(
        [Description("Name pattern. Plain text matches as substring (case-insensitive); " +
                     "'*' and '?' wildcards match against both the simple name and the full symbol id " +
                     "(e.g. \"On*\", \"*UseCase\", \"MyNamespace.*\").")]
        string namePattern,
        [Description("Comma-separated symbol kinds to include: Class, Struct, Interface, Enum, " +
                     "Delegate, Method, Constructor, Property, Field, Event. All kinds when omitted.")]
        string? kinds = null,
        [Description("Project-relative file path pattern ('*'/'?' wildcards or substring), " +
                     "e.g. \"Assets/Scripts/*\".")]
        string? filePattern = null,
        [Description("Only types deriving from or implementing this base, e.g. \"MonoBehaviour\", " +
                     "\"ScriptableObject\" or a project type name.")]
        string? baseType = null,
        [Description("Maximum number of results (default 50, max 500).")]
        int limit = 50,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteLocalTextAsync(
            ct => codeGraphUseCase.SearchSymbolsAsync(namePattern, kinds, filePattern, baseType, limit, ct),
            cancellationToken);

    [McpServerTool(Name = "get_code_snippet", ReadOnly = true),
     Description("Get the source code of a C# symbol (type or member) by its id. Returns only the " +
                 "declaration's lines instead of the whole file, so it is the token-efficient " +
                 "alternative to reading files. Partial types return every declaration part."), UsedImplicitly]
    public ValueTask<CallToolResult> GetCodeSnippetAsync(
        [Description("Symbol id from search_symbols (e.g. \"MyNamespace.MyType.MyMethod(int)\"). " +
                     "Unique suffixes like \"MyType.MyMethod\" are accepted.")]
        string symbol,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteLocalTextAsync(
            ct => codeGraphUseCase.GetCodeSnippetAsync(symbol, ct), cancellationToken);

    [McpServerTool(Name = "find_symbol_references", ReadOnly = true),
     Description("Find every indexed reference to a C# symbol, grouped by relation: calls, " +
                 "unresolvedCalls (name-matched), uses (type/member usages), inheritedBy, " +
                 "implementedBy and overriddenBy. Each entry has the referencing symbol and the " +
                 "file:line of the reference site."), UsedImplicitly]
    public ValueTask<CallToolResult> FindSymbolReferencesAsync(
        [Description("Symbol id from search_symbols. Unique suffixes are accepted.")]
        string symbol,
        [Description("Maximum number of reference sites (default 100, max 1000).")]
        int limit = 100,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteLocalTextAsync(
            ct => codeGraphUseCase.FindReferencesAsync(symbol, limit, ct), cancellationToken);

    [McpServerTool(Name = "trace_call_graph", ReadOnly = true),
     Description("Trace the call graph from a method: 'callers' walks up (who ends up invoking " +
                 "this, including calls to base/interface members it overrides or implements), " +
                 "'callees' walks down (what it invokes). Returns a tree with call sites, useful " +
                 "for impact analysis before refactoring."), UsedImplicitly]
    public ValueTask<CallToolResult> TraceCallGraphAsync(
        [Description("Symbol id of the method to trace, from search_symbols. Unique suffixes are accepted.")]
        string symbol,
        [Description("'callers' (default) or 'callees'.")]
        string direction = "callers",
        [Description("Traversal depth 1-5 (default 3).")]
        int maxDepth = 3,
        CancellationToken cancellationToken = default)
        => McpToolExecution.ExecuteLocalTextAsync(
            ct => codeGraphUseCase.TraceCallGraphAsync(symbol, direction, maxDepth, ct), cancellationToken);
}
