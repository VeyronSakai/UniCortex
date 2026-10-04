using System.Threading.Tasks;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface ICompilationPipeline
    {
        // Must be called on the main thread.
        // The returned task completes with true when the compilation succeeded and the domain is about to be
        // reloaded, or with false when the compilation failed with errors (the domain is not reloaded).
        Task<bool> RequestScriptCompilation();
    }
}
