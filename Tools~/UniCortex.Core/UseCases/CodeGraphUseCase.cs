using System.Text.Json;
using System.Text.RegularExpressions;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.CodeGraph;
using UniCortex.Core.Domains.Interfaces;

namespace UniCortex.Core.UseCases;

/// <summary>
/// Query layer over the Roslyn code graph. Unlike the other use cases this one does not
/// talk to the Unity Editor HTTP server — it reads the project's C# sources directly, so
/// it works even while the Unity Editor is closed.
/// </summary>
public class CodeGraphUseCase(ICodeGraphStore store)
{
    private const int MaxTraceNodes = 200;

    public async ValueTask<string> GetCodeMapAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await store.GetSnapshotAsync(cancellationToken);
        var typeNodes = snapshot.Nodes.Values.Where(n => IsType(n.Kind)).ToList();

        var typeCountsByAssembly = typeNodes
            .GroupBy(n => n.Assembly)
            .ToDictionary(g => g.Key, g => g.Count());
        var assemblies = snapshot.Files
            .GroupBy(f => f.AssemblyName)
            .Select(g => new AssemblySummary
            {
                name = g.Key,
                fileCount = g.Count(),
                typeCount = typeCountsByAssembly.GetValueOrDefault(g.Key)
            })
            .OrderByDescending(a => a.typeCount)
            .ThenBy(a => a.name, StringComparer.Ordinal)
            .ToList();

        var namespaces = typeNodes
            .GroupBy(n => n.Namespace ?? "(global)")
            .Select(g => new NamespaceSummary { name = g.Key, typeCount = g.Count() })
            .OrderByDescending(n => n.typeCount)
            .ThenBy(n => n.name, StringComparer.Ordinal)
            .Take(30)
            .ToList();

        var unity = new UnitySummary
        {
            monoBehaviourCount = typeNodes.Count(n => n.UnityRole == "MonoBehaviour"),
            scriptableObjectCount = typeNodes.Count(n => n.UnityRole == "ScriptableObject"),
            editorWindowCount = typeNodes.Count(n => n.UnityRole == "EditorWindow"),
            editorCount = typeNodes.Count(n => n.UnityRole == "Editor"),
            unityMessageMethodCount = snapshot.Nodes.Values.Count(n => n.IsUnityMessage),
            serializedFieldCount = snapshot.Nodes.Values.Count(n => n.IsSerialized)
        };

        var hotspots = snapshot.Edges
            .GroupBy(e => e.ToId)
            .Select(g => (Id: g.Key, Count: g.Count()))
            .Where(h => snapshot.Nodes.ContainsKey(h.Id))
            .OrderByDescending(h => h.Count)
            .ThenBy(h => h.Id, StringComparer.Ordinal)
            .Take(15)
            .Select(h => new HotspotSummary
            {
                id = h.Id,
                kind = snapshot.Nodes[h.Id].Kind.ToString(),
                incomingReferences = h.Count
            })
            .ToList();

        var response = new GetCodeMapResponse
        {
            projectPath = snapshot.ProjectPath,
            fileCount = snapshot.Files.Count,
            symbolCount = snapshot.Nodes.Count,
            edgeCount = snapshot.Edges.Count,
            indexedAt = snapshot.IndexedAt.ToString("O"),
            indexMilliseconds = (long)snapshot.IndexDuration.TotalMilliseconds,
            unityReferencesResolved = snapshot.UnityReferencesResolved,
            note = snapshot.UnityReferencesResolved
                ? null
                : "Unity assemblies were not located; calls into Unity APIs and MonoBehaviour "
                  + "detection fall back to name-based matching.",
            assemblies = assemblies,
            namespaces = namespaces,
            unity = unity,
            hotspots = hotspots
        };

        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> SearchSymbolsAsync(
        string namePattern, string? kinds, string? filePattern, string? baseType, int limit,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(namePattern))
        {
            throw new ArgumentException("namePattern must not be empty. Use '*' to match everything.");
        }

        var snapshot = await store.GetSnapshotAsync(cancellationToken);
        var nameMatcher = BuildMatcher(namePattern);
        var fileMatcher = string.IsNullOrWhiteSpace(filePattern) ? null : BuildMatcher(filePattern);
        var kindFilter = ParseKinds(kinds);
        limit = Math.Clamp(limit, 1, 500);

        var matches = new List<CodeSymbolNode>();
        foreach (var node in snapshot.Nodes.Values)
        {
            if (kindFilter is not null && !kindFilter.Contains(node.Kind))
            {
                continue;
            }

            if (!nameMatcher(node.Name) && !nameMatcher(node.Id))
            {
                continue;
            }

            if (fileMatcher is not null && !node.Locations.Any(l => fileMatcher(l.File)))
            {
                continue;
            }

            if (baseType is not null && (!IsType(node.Kind) || !snapshot.DerivesFrom(node, baseType)))
            {
                continue;
            }

            matches.Add(node);
        }

        matches.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));

        var response = new SearchSymbolsResponse
        {
            totalMatches = matches.Count,
            truncated = matches.Count > limit ? true : null,
            symbols = matches.Take(limit).Select(ToSummary).ToList()
        };

        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> GetCodeSnippetAsync(
        string symbol, CancellationToken cancellationToken = default)
    {
        var snapshot = await store.GetSnapshotAsync(cancellationToken);
        var node = ResolveSingle(snapshot, symbol);

        var parts = new List<SnippetPart>();
        foreach (var location in node.Locations)
        {
            var record = snapshot.FindFile(location.File);
            var fullPath = record?.FullPath ?? Path.Combine(snapshot.ProjectPath, location.File);

            // Guard against the file changing between indexing and this read: slicing new
            // content with stale line numbers would silently return the wrong code.
            if (record is not null)
            {
                var info = new FileInfo(fullPath);
                if (!info.Exists
                    || info.LastWriteTimeUtc.Ticks != record.LastWriteUtcTicks
                    || info.Length != record.Length)
                {
                    throw new InvalidOperationException(
                        $"Source file '{location.File}' changed after indexing. Retry the query to re-index.");
                }
            }

            string[] lines;
            try
            {
                lines = File.ReadAllLines(fullPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read source file '{location.File}': {ex.Message}");
            }

            var startIndex = Math.Clamp(location.StartLine - 1, 0, Math.Max(lines.Length - 1, 0));
            var endIndex = Math.Clamp(location.EndLine, startIndex, lines.Length);
            parts.Add(new SnippetPart
            {
                file = location.File,
                startLine = location.StartLine,
                endLine = location.EndLine,
                code = string.Join(Environment.NewLine, lines[startIndex..endIndex])
            });
        }

        var response = new GetCodeSnippetResponse
        {
            id = node.Id,
            kind = node.Kind.ToString(),
            assembly = node.Assembly,
            parts = parts
        };

        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> FindReferencesAsync(
        string symbol, int limit, CancellationToken cancellationToken = default)
    {
        var snapshot = await store.GetSnapshotAsync(cancellationToken);
        var node = ResolveSingle(snapshot, symbol);
        limit = Math.Clamp(limit, 1, 1000);

        var references = new Dictionary<string, List<ReferenceSite>>();
        var total = 0;
        foreach (var edge in snapshot.IncomingEdges(node.Id))
        {
            total++;
            var groupName = edge.Kind switch
            {
                CodeEdgeKind.Calls => "calls",
                CodeEdgeKind.CallsBase => "calls",
                CodeEdgeKind.CallsUnresolved => "unresolvedCalls",
                CodeEdgeKind.Uses => "uses",
                CodeEdgeKind.Inherits => "inheritedBy",
                CodeEdgeKind.Implements => "implementedBy",
                CodeEdgeKind.ImplementsMember => "implementedBy",
                CodeEdgeKind.Overrides => "overriddenBy",
                _ => "other"
            };

            if (!references.TryGetValue(groupName, out var sites))
            {
                sites = [];
                references[groupName] = sites;
            }

            sites.Add(new ReferenceSite { from = edge.FromId, file = edge.File, line = edge.Line });
        }

        // Enforce the limit by trimming the largest groups first, so small but important
        // categories (e.g. inheritedBy) are not silently dropped by edge order.
        var truncated = total > limit;
        var remaining = total;
        while (remaining > limit)
        {
            var largest = references.Values.MaxBy(sites => sites.Count)!;
            largest.RemoveAt(largest.Count - 1);
            remaining--;
        }

        foreach (var emptyGroup in references.Where(kv => kv.Value.Count == 0).Select(kv => kv.Key).ToList())
        {
            references.Remove(emptyGroup);
        }

        var response = new FindReferencesResponse
        {
            symbol = node.Id,
            totalReferences = total,
            truncated = truncated ? true : null,
            references = references
        };

        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> TraceCallGraphAsync(
        string symbol, string direction, int maxDepth, CancellationToken cancellationToken = default)
    {
        var callers = direction.ToLowerInvariant() switch
        {
            "callers" => true,
            "callees" => false,
            _ => throw new ArgumentException($"direction must be 'callers' or 'callees', got '{direction}'.")
        };

        var snapshot = await store.GetSnapshotAsync(cancellationToken);
        var node = ResolveSingle(snapshot, symbol);
        maxDepth = Math.Clamp(maxDepth, 1, 5);

        var nodeCount = 0;
        var truncated = false;
        var declaration = node.Locations.FirstOrDefault();
        var root = new TraceNode { id = node.Id, file = declaration?.File, line = declaration?.StartLine };
        nodeCount++;
        Expand(root, 0, new HashSet<string>(StringComparer.Ordinal) { node.Id });

        var response = new TraceCallGraphResponse
        {
            symbol = node.Id,
            direction = callers ? "callers" : "callees",
            maxDepth = maxDepth,
            totalNodes = nodeCount,
            truncated = truncated ? true : null,
            root = root
        };

        return JsonSerializer.Serialize(response, JsonOptions.Default);

        void Expand(TraceNode current, int depth, HashSet<string> path)
        {
            if (depth >= maxDepth)
            {
                return;
            }

            foreach (var (childId, site, unresolved, viaBase, callSites) in GetNeighbors(current.id))
            {
                if (nodeCount >= MaxTraceNodes)
                {
                    truncated = true;
                    return;
                }

                var child = new TraceNode
                {
                    id = childId,
                    file = site.File,
                    line = site.Line,
                    callSites = callSites > 1 ? callSites : null,
                    unresolved = unresolved ? true : null,
                    viaBase = viaBase ? true : null
                };
                nodeCount++;
                current.children ??= [];
                current.children.Add(child);

                if (!path.Add(childId))
                {
                    child.cycle = true;
                    continue;
                }

                Expand(child, depth + 1, path);
                path.Remove(childId);
            }
        }

        IEnumerable<(string Id, (string File, int Line) Site, bool Unresolved, bool ViaBase, int CallSites)>
            GetNeighbors(string id)
        {
            var collected = new Dictionary<string, ((string, int) Site, bool Unresolved, bool ViaBase, int Count)>();

            void Collect(IEnumerable<CodeEdge> edges, Func<CodeEdge, string> selectId, bool viaBase)
            {
                foreach (var edge in edges)
                {
                    if (edge.Kind is not (CodeEdgeKind.Calls or CodeEdgeKind.CallsBase or CodeEdgeKind.CallsUnresolved))
                    {
                        continue;
                    }

                    // base.Foo() is non-virtual and can never dispatch to an override,
                    // so it is not a caller "via base".
                    if (viaBase && edge.Kind == CodeEdgeKind.CallsBase)
                    {
                        continue;
                    }

                    var neighborId = selectId(edge);
                    var unresolved = edge.Kind == CodeEdgeKind.CallsUnresolved;
                    if (collected.TryGetValue(neighborId, out var existing))
                    {
                        collected[neighborId] = (existing.Site, existing.Unresolved && unresolved,
                            existing.ViaBase && viaBase, existing.Count + 1);
                    }
                    else
                    {
                        collected[neighborId] = ((edge.File, edge.Line), unresolved, viaBase, 1);
                    }
                }
            }

            if (callers)
            {
                Collect(snapshot.IncomingEdges(id), e => e.FromId, viaBase: false);

                // Calls to any base/interface member this symbol (transitively) overrides or
                // implements may dispatch here at runtime, so their callers are included too.
                var visitedBases = new HashSet<string>(StringComparer.Ordinal) { id };
                var pending = new Queue<string>();
                pending.Enqueue(id);
                while (pending.Count > 0)
                {
                    foreach (var outgoing in snapshot.OutgoingEdges(pending.Dequeue()))
                    {
                        if (outgoing.Kind is not (CodeEdgeKind.Overrides or CodeEdgeKind.ImplementsMember)
                            || !visitedBases.Add(outgoing.ToId))
                        {
                            continue;
                        }

                        Collect(snapshot.IncomingEdges(outgoing.ToId), e => e.FromId, viaBase: true);
                        pending.Enqueue(outgoing.ToId);
                    }
                }
            }
            else
            {
                Collect(snapshot.OutgoingEdges(id), e => e.ToId, viaBase: false);
            }

            return collected.Select(kv => (kv.Key, kv.Value.Site, kv.Value.Unresolved, kv.Value.ViaBase, kv.Value.Count));
        }
    }

    // ----- helpers -----

    private static bool IsType(CodeSymbolKind kind)
        => kind is CodeSymbolKind.Class or CodeSymbolKind.Struct or CodeSymbolKind.Interface
            or CodeSymbolKind.Enum or CodeSymbolKind.Delegate;

    private static CodeSymbolNode ResolveSingle(CodeGraphSnapshot snapshot, string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("symbol must not be empty.");
        }

        var matches = snapshot.FindSymbols(symbol.Trim());
        switch (matches.Count)
        {
            case 1:
                return matches[0];
            case 0:
                throw new InvalidOperationException(
                    $"Symbol not found: '{symbol}'. Use search_symbols to find the exact symbol id.");
            default:
                var candidates = string.Join(", ", matches.Take(10).Select(m => $"'{m.Id}'"));
                var suffix = matches.Count > 10 ? $" and {matches.Count - 10} more" : "";
                throw new InvalidOperationException(
                    $"Symbol '{symbol}' is ambiguous ({matches.Count} matches): {candidates}{suffix}. "
                    + "Pass a more specific symbol id.");
        }
    }

    private static CodeSymbolSummary ToSummary(CodeSymbolNode node)
    {
        var location = node.Locations.FirstOrDefault();
        return new CodeSymbolSummary
        {
            id = node.Id,
            kind = node.Kind.ToString(),
            assembly = node.Assembly,
            file = location?.File,
            line = location?.StartLine ?? 0,
            modifiers = node.Modifiers,
            unityRole = node.UnityRole,
            isUnityMessage = node.IsUnityMessage ? true : null,
            isSerialized = node.IsSerialized ? true : null
        };
    }

    private static Func<string, bool> BuildMatcher(string pattern)
    {
        if (pattern.Contains('*') || pattern.Contains('?'))
        {
            var regex = new Regex(
                "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return value => regex.IsMatch(value);
        }

        return value => value.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static HashSet<CodeSymbolKind>? ParseKinds(string? kinds)
    {
        if (string.IsNullOrWhiteSpace(kinds))
        {
            return null;
        }

        var result = new HashSet<CodeSymbolKind>();
        foreach (var part in kinds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!Enum.TryParse<CodeSymbolKind>(part, ignoreCase: true, out var kind))
            {
                var validKinds = string.Join(", ", Enum.GetNames<CodeSymbolKind>());
                throw new ArgumentException($"Unknown symbol kind '{part}'. Valid kinds: {validKinds}.");
            }

            result.Add(kind);
        }

        return result;
    }
}
