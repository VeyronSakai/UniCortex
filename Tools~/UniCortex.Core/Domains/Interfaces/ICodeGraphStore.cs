using UniCortex.Core.Domains.CodeGraph;

namespace UniCortex.Core.Domains.Interfaces;

public interface ICodeGraphStore
{
    /// <summary>
    /// Returns an up-to-date code graph for the Unity project, indexing (or re-indexing)
    /// the C# sources when needed.
    /// </summary>
    ValueTask<CodeGraphSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}
