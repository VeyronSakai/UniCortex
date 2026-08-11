// Response DTOs for the code graph use case. These are Core-local (never sent to the
// Unity Editor), serialized with System.Text.Json using camelCase public fields to match
// the JSON shape of the shared Editor DTOs.

namespace UniCortex.Core.Domains.CodeGraph;

public class CodeSymbolSummary
{
    public string id = "";
    public string kind = "";
    public string assembly = "";
    public string? file;
    public int line;
    public string? modifiers;
    public string? unityRole;
    public bool? isUnityMessage;
    public bool? isSerialized;
}

public class GetCodeMapResponse
{
    public string projectPath = "";
    public int fileCount;
    public int symbolCount;
    public int edgeCount;
    public string indexedAt = "";
    public long indexMilliseconds;
    public bool unityReferencesResolved;
    public string? note;
    public List<AssemblySummary> assemblies = [];
    public List<NamespaceSummary> namespaces = [];
    public UnitySummary unity = new();
    public List<HotspotSummary> hotspots = [];
}

public class AssemblySummary
{
    public string name = "";
    public int fileCount;
    public int typeCount;
}

public class NamespaceSummary
{
    public string name = "";
    public int typeCount;
}

public class UnitySummary
{
    public int monoBehaviourCount;
    public int scriptableObjectCount;
    public int editorWindowCount;
    public int editorCount;
    public int unityMessageMethodCount;
    public int serializedFieldCount;
}

public class HotspotSummary
{
    public string id = "";
    public string kind = "";
    public int incomingReferences;
}

public class SearchSymbolsResponse
{
    public int totalMatches;
    public bool? truncated;
    public List<CodeSymbolSummary> symbols = [];
}

public class GetCodeSnippetResponse
{
    public string id = "";
    public string kind = "";
    public string assembly = "";
    public List<SnippetPart> parts = [];
}

public class SnippetPart
{
    public string file = "";
    public int startLine;
    public int endLine;
    public string code = "";
}

public class FindReferencesResponse
{
    public string symbol = "";
    public int totalReferences;
    public bool? truncated;
    public Dictionary<string, List<ReferenceSite>> references = [];
}

public class ReferenceSite
{
    public string from = "";
    public string file = "";
    public int line;
}

public class TraceCallGraphResponse
{
    public string symbol = "";
    public string direction = "";
    public int maxDepth;
    public int totalNodes;
    public bool? truncated;
    public TraceNode root = new();
}

public class TraceNode
{
    public string id = "";
    public string? file;
    public int? line;

    /// <summary>Number of distinct call sites collapsed into this child (present when more than one).</summary>
    public int? callSites;

    /// <summary>True when this link comes from a name-matched (not semantically resolved) call.</summary>
    public bool? unresolved;

    /// <summary>True when the call targets a base/interface member that this symbol overrides or implements.</summary>
    public bool? viaBase;

    public bool? cycle;
    public List<TraceNode>? children;
}
