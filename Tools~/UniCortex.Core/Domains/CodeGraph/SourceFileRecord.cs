namespace UniCortex.Core.Domains.CodeGraph;

/// <summary>A C# source file discovered in the Unity project.</summary>
public sealed record SourceFileRecord(
    string FullPath,
    string RelativePath,
    string AssemblyName,
    long LastWriteUtcTicks,
    long Length);

/// <summary>The set of C# sources belonging to a Unity project (Assets, embedded and local file: packages).</summary>
public sealed class UnityProjectSource
{
    public required IReadOnlyList<SourceFileRecord> Files { get; init; }

    /// <summary>Names of assemblies that are compiled from the discovered sources (asmdef names + Assembly-CSharp*).</summary>
    public required IReadOnlySet<string> AssemblyNames { get; init; }
}
