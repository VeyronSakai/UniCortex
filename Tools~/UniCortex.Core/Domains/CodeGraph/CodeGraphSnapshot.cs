namespace UniCortex.Core.Domains.CodeGraph;

/// <summary>An immutable index of the project's C# code as a symbol graph.</summary>
public sealed class CodeGraphSnapshot
{
    public string ProjectPath { get; }
    public IReadOnlyDictionary<string, CodeSymbolNode> Nodes { get; }
    public IReadOnlyList<CodeEdge> Edges { get; }
    public IReadOnlyList<SourceFileRecord> Files { get; }
    public DateTimeOffset IndexedAt { get; }
    public TimeSpan IndexDuration { get; }
    public bool UnityReferencesResolved { get; }

    private readonly ILookup<string, CodeEdge> _edgesByTarget;
    private readonly ILookup<string, CodeEdge> _edgesBySource;
    private readonly Dictionary<string, SourceFileRecord> _filesByRelativePath;

    public CodeGraphSnapshot(
        string projectPath,
        IReadOnlyDictionary<string, CodeSymbolNode> nodes,
        IReadOnlyList<CodeEdge> edges,
        IReadOnlyList<SourceFileRecord> files,
        DateTimeOffset indexedAt,
        TimeSpan indexDuration,
        bool unityReferencesResolved)
    {
        ProjectPath = projectPath;
        Nodes = nodes;
        Edges = edges;
        Files = files;
        IndexedAt = indexedAt;
        IndexDuration = indexDuration;
        UnityReferencesResolved = unityReferencesResolved;
        _edgesByTarget = edges.ToLookup(e => e.ToId);
        _edgesBySource = edges.ToLookup(e => e.FromId);
        _filesByRelativePath = files
            .GroupBy(f => f.RelativePath)
            .ToDictionary(g => g.Key, g => g.First());
    }

    public IEnumerable<CodeEdge> IncomingEdges(string nodeId) => _edgesByTarget[nodeId];

    public IEnumerable<CodeEdge> OutgoingEdges(string nodeId) => _edgesBySource[nodeId];

    public SourceFileRecord? FindFile(string relativePath)
        => _filesByRelativePath.GetValueOrDefault(relativePath);

    /// <summary>
    /// Resolves a user-supplied symbol query to matching nodes. Accepts an exact id
    /// (e.g. "Ns.Type.Method(int)"), a parameterless form ("Ns.Type.Method"), or a
    /// suffix ("Type.Method", "Method"). Falls back to case-insensitive matching when
    /// the case-sensitive pass finds nothing.
    /// </summary>
    public IReadOnlyList<CodeSymbolNode> FindSymbols(string query)
    {
        if (Nodes.TryGetValue(query, out var exact))
        {
            return [exact];
        }

        var matches = MatchAll(query, StringComparison.Ordinal);
        if (matches.Count == 0)
        {
            matches = MatchAll(query, StringComparison.OrdinalIgnoreCase);
        }

        return matches;
    }

    private List<CodeSymbolNode> MatchAll(string query, StringComparison comparison)
    {
        var result = new List<CodeSymbolNode>();
        foreach (var node in Nodes.Values)
        {
            if (MatchesQuery(node.Id, query, comparison))
            {
                result.Add(node);
            }
        }

        return result;
    }

    private static bool MatchesQuery(string id, string query, StringComparison comparison)
    {
        if (MatchesQueryCore(id, query, comparison))
        {
            return true;
        }

        // Generic-insensitive pass: ids include type parameters ("Ns.Repository<T>.Map<T2>(T2)"),
        // but queries like "Repository.Map" must still resolve.
        var strippedId = StripGenericArguments(id);
        var strippedQuery = StripGenericArguments(query);
        if (strippedId.Length == id.Length && strippedQuery.Length == query.Length)
        {
            return false;
        }

        return MatchesQueryCore(strippedId, strippedQuery, comparison);
    }

    private static bool MatchesQueryCore(string id, string query, StringComparison comparison)
    {
        if (string.Equals(id, query, comparison))
        {
            return true;
        }

        // Suffix match on the full id, e.g. "Type.Method(int)" against "Ns.Type.Method(int)".
        if (id.EndsWith("." + query, comparison))
        {
            return true;
        }

        // Match ignoring the parameter list, e.g. "Ns.Type.Method" or "Method" against "Ns.Type.Method(int)".
        var parenIndex = id.IndexOf('(');
        if (parenIndex < 0)
        {
            return false;
        }

        var withoutParameters = id[..parenIndex];
        return string.Equals(withoutParameters, query, comparison)
               || withoutParameters.EndsWith("." + query, comparison);
    }

    /// <summary>Removes generic argument lists ("&lt;T, T2&gt;") from a display name.</summary>
    public static string StripGenericArguments(string value)
    {
        if (!value.Contains('<'))
        {
            return value;
        }

        var builder = new System.Text.StringBuilder(value.Length);
        var depth = 0;
        foreach (var character in value)
        {
            switch (character)
            {
                case '<':
                    depth++;
                    continue;
                case '>' when depth > 0:
                    depth--;
                    continue;
                default:
                    if (depth == 0)
                    {
                        builder.Append(character);
                    }

                    continue;
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// True when <paramref name="type"/> derives from (or implements) a type whose display name
    /// or simple name equals <paramref name="baseName"/>. Follows the in-index inheritance chain
    /// and also matches the Unity role (e.g. "MonoBehaviour").
    /// </summary>
    public bool DerivesFrom(CodeSymbolNode type, string baseName)
    {
        if (string.Equals(type.UnityRole, baseName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var visited = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<CodeSymbolNode>();
        queue.Enqueue(type);
        visited.Add(type.Id);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var baseNames = current.Interfaces.ToList();
            if (current.BaseClassName is not null)
            {
                baseNames.Add(current.BaseClassName);
            }

            foreach (var candidate in baseNames)
            {
                if (NameMatches(candidate, baseName))
                {
                    return true;
                }

                // Base names use the same display format as node ids, so in-index bases resolve directly.
                if (Nodes.TryGetValue(candidate, out var baseNode) && visited.Add(baseNode.Id))
                {
                    queue.Enqueue(baseNode);
                }
            }
        }

        return false;
    }

    private static bool NameMatches(string candidate, string baseName)
    {
        if (string.Equals(candidate, baseName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var simple = SimpleName(candidate);
        return string.Equals(simple, baseName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Extracts the simple name: strips namespace qualifiers and generic type arguments.</summary>
    public static string SimpleName(string displayName)
    {
        var name = displayName;
        var genericIndex = name.IndexOf('<');
        if (genericIndex >= 0)
        {
            name = name[..genericIndex];
        }

        var dotIndex = name.LastIndexOf('.');
        return dotIndex >= 0 ? name[(dotIndex + 1)..] : name;
    }
}
