using UniCortex.Core.Domains.CodeGraph;
using UniCortex.Core.Domains.Interfaces;

namespace UniCortex.Core.Infrastructures.CodeGraph;

/// <summary>
/// Caches the code graph in memory and rebuilds it when the discovered source files
/// (paths, timestamps, sizes) or the precompiled reference assemblies change. Registered
/// as a singleton so the index is reused across tool calls within one server process.
/// </summary>
public sealed class CodeGraphStore(IUnityProjectPathProvider projectPathProvider) : ICodeGraphStore
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private Task<CodeGraphSnapshot>? _buildTask;
    private string? _projectPath;
    private IReadOnlyList<SourceFileRecord>? _sourceFiles;
    private string? _referenceSignature;

    public async ValueTask<CodeGraphSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var projectPath = projectPathProvider.GetProjectPath();

        Task<CodeGraphSnapshot> buildTask;
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var source = UnitySourceLocator.Locate(projectPath);
            var referenceSignature = ComputeReferenceSignature(projectPath);

            // Once a build completed, compare against the files it actually indexed so
            // files that failed to read are retried on the next query.
            var indexedFiles = _buildTask is { IsCompletedSuccessfully: true } completed
                ? completed.Result.Files
                : _sourceFiles;

            if (_buildTask is null or { IsFaulted: true } or { IsCanceled: true }
                || _projectPath != projectPath
                || _referenceSignature != referenceSignature
                || indexedFiles is null
                || !indexedFiles.SequenceEqual(source.Files))
            {
                var builder = new RoslynCodeGraphBuilder();
                // The build result is shared by every waiting caller, so it must not be
                // tied to the first caller's cancellation token: a caller giving up only
                // abandons its wait, never the shared build.
                _buildTask = Task.Run(() => builder.Build(projectPath, source, CancellationToken.None));
                _projectPath = projectPath;
                _sourceFiles = source.Files;
                _referenceSignature = referenceSignature;
            }

            buildTask = _buildTask;
        }
        finally
        {
            _gate.Release();
        }

        return await buildTask.WaitAsync(cancellationToken);
    }

    /// <summary>
    /// Lightweight signature of Library/ScriptAssemblies so a Unity recompile (which changes
    /// the reference assemblies used for semantic resolution) triggers a re-index.
    /// </summary>
    private static string ComputeReferenceSignature(string projectPath)
    {
        var directory = Path.Combine(projectPath, "Library", "ScriptAssemblies");
        if (!Directory.Exists(directory))
        {
            return "";
        }

        try
        {
            long maxTicks = 0;
            var count = 0;
            foreach (var dll in Directory.EnumerateFiles(directory, "*.dll"))
            {
                count++;
                var ticks = File.GetLastWriteTimeUtc(dll).Ticks;
                if (ticks > maxTicks)
                {
                    maxTicks = ticks;
                }
            }

            return $"{count}:{maxTicks}";
        }
        catch (Exception)
        {
            return "";
        }
    }
}
