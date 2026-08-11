namespace UniCortex.Core.Domains.CodeGraph;

public sealed record SymbolLocation(string File, int StartLine, int EndLine);

public sealed class CodeSymbolNode
{
    public required string Id { get; init; }
    public required CodeSymbolKind Kind { get; init; }
    public required string Name { get; init; }
    public required string Assembly { get; init; }
    public string? Namespace { get; init; }
    public string? ParentId { get; init; }
    public string? Modifiers { get; init; }

    /// <summary>Declaration locations. More than one entry for partial types/methods.</summary>
    public List<SymbolLocation> Locations { get; } = [];

    /// <summary>For types: display name of the base class (null for interfaces/enums or when the base is System.Object).</summary>
    public string? BaseClassName { get; set; }

    /// <summary>For types: display names of directly implemented interfaces.</summary>
    public List<string> Interfaces { get; } = [];

    /// <summary>Unity base class category: "MonoBehaviour", "ScriptableObject", "EditorWindow" or "Editor".</summary>
    public string? UnityRole { get; set; }

    /// <summary>True for Unity message methods (Awake, Update, OnEnable, ...) that are invoked by the engine.</summary>
    public bool IsUnityMessage { get; set; }

    /// <summary>True for fields serialized by Unity ([SerializeField] or public fields on Unity types).</summary>
    public bool IsSerialized { get; set; }
}
